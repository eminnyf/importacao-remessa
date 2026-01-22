Imports System.IO
Imports RemessaImportacao.Infrastructure.Data
Imports RemessaImportacao.Infrastructure.Excel
Imports RemessaImportacao.Infrastructure.Http
Imports RemessaImportacao.Infrastructure.Repositories
Imports RemessaImportacao.Application.Models
Imports RemessaImportacao.Domain.Dtos
Imports RemessaImportacao.Domain.Entities

Namespace Services
    Public Class ImportacaoService

        Private ReadOnly _factory As DbConnectionFactory
        Private ReadOnly _excelReader As ExcelRemessaReader
        Private ReadOnly _randomUser As RandomUserClient

        Private ReadOnly _layoutRepo As LayoutRepository
        Private ReadOnly _importRepo As ImportacaoRepository
        Private ReadOnly _clienteRepo As ClienteRepository
        Private ReadOnly _contratoRepo As ContratoRepository
        Private ReadOnly _parcelaRepo As ParcelaRepository
        Private ReadOnly _inconsRepo As InconsistenciaRepository

        Private _enderecoCache As Endereco = Nothing

        Public Sub New()
            _factory = New DbConnectionFactory()

            _excelReader = New ExcelRemessaReader()
            _randomUser = New RandomUserClient()

            _layoutRepo = New LayoutRepository(_factory)
            _importRepo = New ImportacaoRepository(_factory)
            _clienteRepo = New ClienteRepository(_factory)
            _contratoRepo = New ContratoRepository(_factory)
            _parcelaRepo = New ParcelaRepository(_factory)
            _inconsRepo = New InconsistenciaRepository(_factory)
        End Sub

        ' ignoraLayout = True -> segue mesmo com layout divergente (quando o usuário optar por prosseguir)
        Public Async Function ProcessarAsync(caminhoArquivo As String, Optional ignoraLayout As Boolean = False) As Task(Of ImportacaoResultado)
            Dim resultado As New ImportacaoResultado()

            Dim nomeArquivo = Path.GetFileName(caminhoArquivo)
            Dim importacaoId As Long = _importRepo.CriarImportacao(nomeArquivo)

            Try
                ' Etapa 1) Layout cadastrado + validação de layout quando necessário
                Dim layoutDb = ObterLayoutObrigatorio()
                If Not ignoraLayout Then
                    Dim divergencias = ValidarLayoutOuRetornarDivergencias(caminhoArquivo, layoutDb)

                    If divergencias.Count > 0 Then
                        PreencherResultadoLayoutInvalido(resultado, divergencias, importacaoId)
                        _importRepo.FinalizarImportacao(importacaoId, 0, 0, resultado.TotalInconsistencias, "LAYOUT_INVALIDO")
                        Return resultado
                    End If
                End If

                ' Etapa 2) Ler linhas do arquivo
                Dim linhas = LerLinhasArquivo(caminhoArquivo, usarMapaPorNome:=ignoraLayout)
                resultado.TotalLinhas = linhas.Count

                ' Etapa 3) Processar linhas
                Await ProcessarLinhasAsync(importacaoId, linhas, resultado)

                ' Etapa 4) Finalizar
                resultado.Status = "CONCLUIDO"
                _importRepo.FinalizarImportacao(importacaoId,
                                               resultado.TotalLinhas,
                                               resultado.TotalSucesso,
                                               resultado.TotalInconsistencias,
                                               "CONCLUIDO")

                Return resultado

            Catch
                resultado.Status = "ERRO"
                _importRepo.FinalizarImportacao(importacaoId,
                                               resultado.TotalLinhas,
                                               resultado.TotalSucesso,
                                               resultado.TotalInconsistencias,
                                               "ERRO")
                Throw
            End Try
        End Function

        ' ETAPAS (privados)

        Private Function ObterLayoutObrigatorio() As List(Of String)
            Dim layoutDb = _layoutRepo.ObterLayoutOrdenado()
            If layoutDb Is Nothing OrElse layoutDb.Count = 0 Then
                Throw New Exception("Layout não cadastrado no banco.")
            End If
            Return layoutDb
        End Function

        Private Function ValidarLayoutOuRetornarDivergencias(caminhoArquivo As String, layoutDb As List(Of String)) As List(Of String)
            Dim cabecalhos = _excelReader.LerCabecalhos(caminhoArquivo)
            Return CompararLayout(layoutDb, cabecalhos)
        End Function

        Private Sub PreencherResultadoLayoutInvalido(resultado As ImportacaoResultado, divergencias As List(Of String), importacaoId As Long)
            resultado.Status = "LAYOUT_INVALIDO"
            resultado.TotalLinhas = 0
            resultado.TotalSucesso = 0
            resultado.TotalInconsistencias = divergencias.Count

            For Each d In divergencias
                resultado.MensagensInconsistencias.Add("LAYOUT: " & d)
                _inconsRepo.Registrar(importacaoId, 0, d) 'linha 0 = divergência geral
            Next
        End Sub

        Private Function LerLinhasArquivo(caminhoArquivo As String, usarMapaPorNome As Boolean) As List(Of RemessaLinhaDto)
            Return _excelReader.LerLinhas(caminhoArquivo, usarMapaPorNome:=usarMapaPorNome)
        End Function

        Private Async Function ProcessarLinhasAsync(importacaoId As Long, linhas As List(Of RemessaLinhaDto), resultado As ImportacaoResultado) As Task
            For Each linha In linhas
                Try
                    Await ProcessarLinhaAsync(linha)
                    resultado.TotalSucesso += 1

                Catch exLinha As Exception
                    resultado.TotalInconsistencias += 1

                    Dim numLinha = linha.LinhaArquivo
                    Dim msgUsuario = MontarMensagemErroUsuario(numLinha, exLinha)

                    resultado.MensagensInconsistencias.Add(msgUsuario)
                    _inconsRepo.Registrar(importacaoId, numLinha, exLinha.Message)
                End Try
            Next
        End Function

        ' PROCESSAMENTO POR LINHA

        Private Async Function ProcessarLinhaAsync(linha As RemessaLinhaDto) As Task
            ' Regras mínimas
            If String.IsNullOrWhiteSpace(linha.CpfCnpj) Then Throw New Exception("CPF/CNPJ vazio.")

            ' CPF/CNPJ simples: apenas dígitos e tamanho 11/14
            linha.CpfCnpj = NormalizarCpfCnpjOuFalhar(linha.CpfCnpj)

            If String.IsNullOrWhiteSpace(linha.Contrato) Then Throw New Exception("Contrato vazio.")
            If linha.NumeroParcela <= 0 Then Throw New Exception("Número da parcela inválido.")
            If linha.Vencimento = Date.MinValue Then Throw New Exception("Vencimento inválido.")
            If linha.Valor <= 0D Then Throw New Exception("Valor inválido.")

            Dim ender As Endereco = ObterEnderecoDoArquivo(linha)
            If ender Is Nothing Then
                ender = Await ObterEnderecoSeguroAsync()
            End If

            ' UPSERT Cliente
            Dim clienteId = _clienteRepo.UpsertCliente(
                cpfCnpj:=linha.CpfCnpj,
                nome:=linha.NomeCliente,
                dataNascimento:=linha.DataNascimento,
                email:=linha.Email,
                telefone:=linha.Telefone,
                celular:=linha.Celular,
                logradouro:=ender.Logradouro,
                bairro:=ender.Bairro,
                cidade:=ender.Cidade,
                uf:=ender.Uf
            )

            ' UPSERT Contrato
            Dim contratoId = _contratoRepo.UpsertContrato(linha.Contrato, clienteId)

            ' UPSERT Parcela
            _parcelaRepo.UpsertParcela(contratoId, linha.NumeroParcela, linha.Vencimento, linha.Valor)
        End Function

        Private Function NormalizarCpfCnpjOuFalhar(valor As String) As String
            Dim digits = New String(valor.Where(AddressOf Char.IsDigit).ToArray())

            If Not (digits.Length = 11 OrElse digits.Length = 14) Then
                Throw New Exception("CPF/CNPJ inválido (deve ter 11 ou 14 dígitos).")
            End If

            Return digits
        End Function

        ' ENDEREÇO

        Private Function ObterEnderecoDoArquivo(linha As RemessaLinhaDto) As Endereco
            Dim temAlgo As Boolean =
                Not String.IsNullOrWhiteSpace(linha.Logradouro) OrElse
                Not String.IsNullOrWhiteSpace(linha.Bairro) OrElse
                Not String.IsNullOrWhiteSpace(linha.Cidade) OrElse
                Not String.IsNullOrWhiteSpace(linha.Uf)

            If Not temAlgo Then Return Nothing

            Return New Endereco With {
                .Logradouro = If(linha.Logradouro, "").Trim(),
                .Bairro = If(linha.Bairro, "").Trim(),
                .Cidade = If(linha.Cidade, "").Trim(),
                .Uf = If(linha.Uf, "").Trim(),
                .Origem = "arquivo"
            }
        End Function

        Private Async Function ObterEnderecoSeguroAsync() As Task(Of Endereco)
            If _enderecoCache IsNot Nothing Then
                Return _enderecoCache
            End If

            Try
                Dim ender = Await _randomUser.ObterEnderecoAsync()

                If ender Is Nothing OrElse String.IsNullOrWhiteSpace(ender.Logradouro) Then
                    Throw New Exception("API RandomUser retornou endereço vazio.")
                End If

                _enderecoCache = ender
                Return ender

            Catch
                Dim fallback As New Endereco With {
                    .Logradouro = "N/D",
                    .Bairro = "N/D",
                    .Cidade = "N/D",
                    .Uf = "NA",
                    .Origem = "fallback"
                }

                _enderecoCache = fallback
                Return fallback
            End Try
        End Function

        ' SUPORTE

        Private Function MontarMensagemErroUsuario(numLinha As Integer, ex As Exception) As String
            Dim msg = $"Linha {numLinha}: {ex.Message}"

            If ex.InnerException IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(ex.InnerException.Message) Then
                msg &= $" (Detalhe: {ex.InnerException.Message})"
            End If

            Return msg
        End Function

        Private Function CompararLayout(layoutDb As List(Of String), cabecalhosArquivo As List(Of String)) As List(Of String)
            Dim erros As New List(Of String)

            Dim db = layoutDb.Select(Function(x) x.Trim().ToLowerInvariant()).ToList()
            Dim arq = cabecalhosArquivo.Select(Function(x) x.Trim().ToLowerInvariant()).ToList()

            If db.Count <> arq.Count Then
                erros.Add($"Quantidade de colunas diferente. Esperado: {db.Count} | Arquivo: {arq.Count}")
            End If

            Dim n = Math.Min(db.Count, arq.Count)
            For i As Integer = 0 To n - 1
                If db(i) <> arq(i) Then
                    erros.Add($"Coluna {i + 1} diferente. Esperado: '{layoutDb(i)}' | Arquivo: '{cabecalhosArquivo(i)}'")
                End If
            Next

            Return erros
        End Function

    End Class
End Namespace
