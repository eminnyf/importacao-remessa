Imports System.IO
Imports System.Data
Imports System.Globalization
Imports System.Text
Imports ExcelDataReader
Imports RemessaImportacao.Domain.Dtos

Namespace Excel
    Public Class ExcelRemessaReader

        Private Const QtdColunasLayout As Integer = 14


        '  PUBLIC API

        ' Lê as linhas da planilha ou a primeira e devolve DTOs.
        ' usarMapaPorNome:=True  -> pega valores por nome do cabeçalho
        ' usarMapaPorNome:=False -> pega valores por índice fixo 
        Public Function LerLinhas(caminhoArquivo As String, Optional usarMapaPorNome As Boolean = False) As List(Of RemessaLinhaDto)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

            Dim resultado As New List(Of RemessaLinhaDto)()

            Using stream = File.Open(caminhoArquivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using reader = ExcelReaderFactory.CreateReader(stream)

                    Dim conf As New ExcelDataSetConfiguration With {
                        .ConfigureDataTable = Function(__) New ExcelDataTableConfiguration With {
                            .UseHeaderRow = True
                        }
                    }

                    Dim ds = reader.AsDataSet(conf)

                    Dim tabela As DataTable =
                        If(ds.Tables.Contains("Remessa"), ds.Tables("Remessa"), ds.Tables(0))

                    Dim linhaArquivo As Integer = 2

                    Dim mapa As Dictionary(Of String, Integer) = Nothing
                    If usarMapaPorNome Then
                        mapa = MontarMapaCabecalhos(tabela)
                    End If

                    For Each r As DataRow In tabela.Rows

                        If LinhaVazia(r) Then
                            linhaArquivo += 1
                            Continue For
                        End If

                        Dim dto As RemessaLinhaDto

                        If usarMapaPorNome Then
                            dto = MontarDtoPorNome(r, linhaArquivo, mapa)
                        Else
                            dto = MontarDtoPorIndice(r, linhaArquivo)
                        End If

                        resultado.Add(dto)
                        linhaArquivo += 1
                    Next

                End Using
            End Using

            Return resultado
        End Function


        ' Lê os cabeçalhos do arquivo para validar layout
        Public Function LerCabecalhos(caminhoArquivo As String) As List(Of String)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

            Dim headers As New List(Of String)()

            Using stream = File.Open(caminhoArquivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using reader = ExcelReaderFactory.CreateReader(stream)


                    Dim conf As New ExcelDataSetConfiguration With {
                        .ConfigureDataTable = Function(__) New ExcelDataTableConfiguration With {
                            .UseHeaderRow = False
                        }
                    }

                    Dim ds = reader.AsDataSet(conf)
                    Dim tabela As DataTable =
                        If(ds.Tables.Contains("Remessa"), ds.Tables("Remessa"), ds.Tables(0))

                    If tabela.Rows.Count = 0 Then Return headers

                    Dim primeiraLinha As DataRow = tabela.Rows(0)

                    For i As Integer = 0 To tabela.Columns.Count - 1
                        Dim h As String = ""
                        If Not primeiraLinha.IsNull(i) Then
                            h = primeiraLinha(i).ToString().Trim()
                        End If
                        headers.Add(h)
                    Next

                End Using
            End Using


            While headers.Count > 0 AndAlso String.IsNullOrWhiteSpace(headers(headers.Count - 1))
                headers.RemoveAt(headers.Count - 1)
            End While

            Return headers
        End Function

        Private Function MontarDtoPorIndice(r As DataRow, linhaArquivo As Integer) As RemessaLinhaDto
            ' Mantém o layout por índice (0..13)
            Dim dto As New RemessaLinhaDto() With {
                .LinhaArquivo = linhaArquivo,
                .CpfCnpj = NormalizarCpf(ValorTexto(r, 0)),
                .Contrato = ValorTexto(r, 1),
                .NumeroParcela = ValorInt(r, 2),
                .Vencimento = ValorDataObrigatoria(r, 3),
                .Valor = ValorDecimalObrigatorio(r, 4),
                .NomeCliente = ValorTexto(r, 5),
                .Telefone = ValorTexto(r, 6),
                .Celular = ValorTexto(r, 7),
                .Email = ValorTexto(r, 8),
                .DataNascimento = ValorDataOpcional(r, 9),
                .Logradouro = ValorTexto(r, 10),
                .Bairro = ValorTexto(r, 11),
                .Cidade = ValorTexto(r, 12),
                .Uf = ValorTexto(r, 13)
            }

            Return dto
        End Function


        Private Function MontarDtoPorNome(r As DataRow, linhaArquivo As Integer, mapa As Dictionary(Of String, Integer)) As RemessaLinhaDto
            Dim dto As New RemessaLinhaDto() With {
        .LinhaArquivo = linhaArquivo,
        .CpfCnpj = NormalizarCpf(ValorTexto(r, mapa, "cpf", "cpf_cnpj", "cpf/cnpj", "documento")),
        .Contrato = ValorTexto(r, mapa, "contrato", "numero_contrato"),
        .NumeroParcela = ValorInt(r, mapa, "parcela", "numero_parcela", "nr_parcela"),
        .Vencimento = ValorDataObrigatoria(r, mapa, "vencimento", "data_vencimento", "dt_vencimento"),
        .Valor = ValorDecimalObrigatorio(r, mapa, "valor", "valor_parcela", "vlr"),
        .NomeCliente = ValorTexto(r, mapa, "nome_cliente", "cliente", "nome"),
        .Telefone = ValorTexto(r, mapa, "telefone", "tel", "fone"),
        .Celular = ValorTexto(r, mapa, "celular", "cel", "mobile"),
        .Email = ValorTexto(r, mapa, "email", "e_mail", "e-mail", "mail"),
        .DataNascimento = ValorDataOpcional(r, mapa, "nascimento", "data_nascimento", "dt_nascimento"),
        .Logradouro = ValorTexto(r, mapa, "logradouro", "endereco", "endereço", "rua"),
        .Bairro = ValorTexto(r, mapa, "bairro", "district"),
        .Cidade = ValorTexto(r, mapa, "cidade", "city"),
        .Uf = ValorTexto(r, mapa, "uf", "estado", "state")
        }

            Return dto
        End Function

        '  MAPEAMENTO DE CABEÇALHOS

        Private Function MontarMapaCabecalhos(tabela As DataTable) As Dictionary(Of String, Integer)
            Dim map As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

            For i As Integer = 0 To tabela.Columns.Count - 1
                Dim rawName = If(tabela.Columns(i)?.ColumnName, "")
                Dim key = NormalizarChaveCabecalho(rawName)

                If String.IsNullOrWhiteSpace(key) Then Continue For
                If Not map.ContainsKey(key) Then
                    map.Add(key, i)
                End If
            Next

            Return map
        End Function

        Private Function GetIndex(mapa As Dictionary(Of String, Integer), ParamArray aliases As String()) As Integer
            If mapa Is Nothing OrElse aliases Is Nothing OrElse aliases.Length = 0 Then Return -1

            For Each a In aliases
                Dim key = NormalizarChaveCabecalho(a)
                If String.IsNullOrWhiteSpace(key) Then Continue For

                Dim idx As Integer
                If mapa.TryGetValue(key, idx) Then
                    Return idx
                End If
            Next

            Return -1
        End Function

        '  VALIDACOES
        Private Function LinhaVazia(r As DataRow) As Boolean
            For i As Integer = 0 To r.ItemArray.Length - 1
                If Not r.IsNull(i) AndAlso Not String.IsNullOrWhiteSpace(r(i).ToString()) Then
                    Return False
                End If
            Next
            Return True
        End Function

        Private Function ValorTexto(r As DataRow, idx As Integer) As String
            If idx < 0 OrElse idx >= r.ItemArray.Length OrElse r.IsNull(idx) Then Return ""

            Dim val = r(idx)

            If TypeOf val Is Date Then
                Return CDate(val).ToString("yyyy-MM-dd")
            End If

            Return val.ToString().Trim()
        End Function

        Private Function ValorTexto(r As DataRow, mapa As Dictionary(Of String, Integer), ParamArray aliases As String()) As String
            Dim idx = GetIndex(mapa, aliases)
            If idx < 0 Then Return ""
            Return ValorTexto(r, idx)
        End Function

        Private Function ValorInt(r As DataRow, idx As Integer) As Integer
            If idx < 0 OrElse idx >= r.ItemArray.Length OrElse r.IsNull(idx) Then Return 0

            Dim val = r(idx)

            If TypeOf val Is Integer Then Return CInt(val)
            If TypeOf val Is Long Then Return CInt(val)
            If TypeOf val Is Double Then Return CInt(Math.Truncate(CDbl(val)))
            If TypeOf val Is Decimal Then Return CInt(Math.Truncate(CDec(val)))

            Dim t = val.ToString().Trim()

            Dim n As Integer
            If Integer.TryParse(t, n) Then Return n

            Dim d As Double
            If Double.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then Return CInt(Math.Truncate(d))
            If Double.TryParse(t, NumberStyles.Any, New CultureInfo("pt-BR"), d) Then Return CInt(Math.Truncate(d))

            Return 0
        End Function

        Private Function ValorInt(r As DataRow, mapa As Dictionary(Of String, Integer), ParamArray aliases As String()) As Integer
            Dim idx = GetIndex(mapa, aliases)
            If idx < 0 Then Return 0
            Return ValorInt(r, idx)
        End Function


        Private Function ValorDecimalObrigatorio(r As DataRow, idx As Integer) As Decimal
            If idx < 0 OrElse idx >= r.ItemArray.Length OrElse r.IsNull(idx) Then Return 0D

            Dim val = r(idx)

            If TypeOf val Is Decimal Then Return CDec(val)
            If TypeOf val Is Double Then Return CDec(val)
            If TypeOf val Is Integer Then Return CDec(val)
            If TypeOf val Is Long Then Return CDec(val)

            Dim t = val.ToString().Trim()

            Dim v As Decimal
            If Decimal.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, v) Then Return v
            If Decimal.TryParse(t, NumberStyles.Any, New CultureInfo("pt-BR"), v) Then Return v

            Return 0D
        End Function

        Private Function ValorDecimalObrigatorio(r As DataRow, mapa As Dictionary(Of String, Integer), ParamArray aliases As String()) As Decimal
            Dim idx = GetIndex(mapa, aliases)
            If idx < 0 Then Return 0D
            Return ValorDecimalObrigatorio(r, idx)
        End Function

        Private Function ValorDataObrigatoria(r As DataRow, idx As Integer) As Date
            Dim opt = ValorDataOpcional(r, idx)
            If opt.HasValue Then Return opt.Value.Date
            Return Date.MinValue
        End Function

        Private Function ValorDataObrigatoria(r As DataRow, mapa As Dictionary(Of String, Integer), ParamArray aliases As String()) As Date
            Dim idx = GetIndex(mapa, aliases)
            If idx < 0 Then Return Date.MinValue
            Return ValorDataObrigatoria(r, idx)
        End Function

        Private Function ValorDataOpcional(r As DataRow, idx As Integer) As Date?
            If idx < 0 OrElse idx >= r.ItemArray.Length OrElse r.IsNull(idx) Then Return Nothing

            Dim val = r(idx)

            If TypeOf val Is Date Then Return CDate(val).Date

            If TypeOf val Is Double Then
                Try
                    Return Date.FromOADate(CDbl(val)).Date
                Catch
                    Return Nothing
                End Try
            End If

            Dim t = val.ToString().Trim()

            Dim d As Date
            If Date.TryParse(t, New CultureInfo("pt-BR"), DateTimeStyles.None, d) Then Return d.Date
            If Date.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return d.Date

            Return Nothing
        End Function

        Private Function ValorDataOpcional(r As DataRow, mapa As Dictionary(Of String, Integer), ParamArray aliases As String()) As Date?
            Dim idx = GetIndex(mapa, aliases)
            If idx < 0 Then Return Nothing
            Return ValorDataOpcional(r, idx)
        End Function

        Private Function NormalizarCpf(cpf As String) As String
            If String.IsNullOrWhiteSpace(cpf) Then Return ""
            Dim soNumeros = New String(cpf.Where(Function(c) Char.IsDigit(c)).ToArray())
            Return soNumeros
        End Function


        Private Function NormalizarChaveCabecalho(s As String) As String
            If String.IsNullOrWhiteSpace(s) Then Return ""

            Dim x = s.Trim().ToLowerInvariant()
            x = RemoverAcentos(x)

            Dim sb As New StringBuilder(x.Length)
            For Each ch In x
                If Char.IsLetterOrDigit(ch) Then
                    sb.Append(ch)
                ElseIf Char.IsWhiteSpace(ch) OrElse ch = "-"c OrElse ch = "/"c OrElse ch = "\"c OrElse ch = "."c Then
                    sb.Append("_"c)
                Else

                    sb.Append("_"c)
                End If
            Next

            Dim key = sb.ToString()

            ' compacta underlines repetidos
            While key.Contains("__")
                key = key.Replace("__", "_")
            End While

            key = key.Trim("_"c)

            Return key
        End Function


        Private Function RemoverAcentos(texto As String) As String
            If String.IsNullOrWhiteSpace(texto) Then Return ""

            Dim normalized = texto.Normalize(NormalizationForm.FormD)
            Dim sb As New StringBuilder()

            For Each c As Char In normalized
                Dim uc = Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                If uc <> Globalization.UnicodeCategory.NonSpacingMark Then
                    sb.Append(c)
                End If
            Next

            Return sb.ToString().Normalize(NormalizationForm.FormC)
        End Function

    End Class
End Namespace
