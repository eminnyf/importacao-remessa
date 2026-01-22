Imports System.Data
Imports Dapper
Imports RemessaImportacao.Infrastructure.Data

Namespace Repositories
    Public Class ImportacaoRepository
        Private ReadOnly _factory As DbConnectionFactory

        Public Sub New(factory As DbConnectionFactory)
            _factory = factory
        End Sub

        Public Function CriarImportacao(nomeArquivo As String) As Long
            Dim nome = If(nomeArquivo, "").Trim()
            If String.IsNullOrWhiteSpace(nome) Then
                nome = "arquivo_desconhecido.xlsx"
            End If

            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"INSERT INTO importacao (nome_arquivo, status)
 VALUES (@nome, 'PROCESSANDO')
 RETURNING id;"
                Return conn.ExecuteScalar(Of Long)(sql, New With {.nome = nome})
            End Using
        End Function

        Public Sub FinalizarImportacao(importacaoId As Long,
                                       totalLinhas As Integer,
                                       totalSucesso As Integer,
                                       totalInconsistencias As Integer,
                                       status As String)

            If importacaoId <= 0 Then
                Throw New ArgumentException("importacaoId inválido.", NameOf(importacaoId))
            End If

            Dim st = If(status, "").Trim().ToUpperInvariant()
            If String.IsNullOrWhiteSpace(st) Then
                st = "ERRO"
            End If

            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"UPDATE importacao
   SET total_linhas = @totalLinhas,
       total_sucesso = @totalSucesso,
       total_inconsistencias = @totalInconsistencias,
       status = @status
 WHERE id = @id;"
                conn.Execute(sql, New With {
                    .id = importacaoId,
                    .totalLinhas = totalLinhas,
                    .totalSucesso = totalSucesso,
                    .totalInconsistencias = totalInconsistencias,
                    .status = st
                })
            End Using
        End Sub

    End Class
End Namespace
