Imports System.Data
Imports Dapper
Imports RemessaImportacao.Infrastructure.Data

Namespace Repositories
    Public Class InconsistenciaRepository
        Private ReadOnly _factory As DbConnectionFactory

        Public Sub New(factory As DbConnectionFactory)
            _factory = factory
        End Sub

        Public Sub RemoverPorImportacao(importacaoId As Long)
            If importacaoId <= 0 Then
                Throw New ArgumentException("importacaoId inválido.", NameOf(importacaoId))
            End If

            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"DELETE FROM inconsistencia
 WHERE importacao_id = @id;"
                conn.Execute(sql, New With {.id = importacaoId})
            End Using
        End Sub

        Public Sub Registrar(importacaoId As Long, linhaArquivo As Integer, descricao As String)
            If importacaoId <= 0 Then
                Throw New ArgumentException("importacaoId inválido.", NameOf(importacaoId))
            End If

            Dim desc = If(descricao, "").Trim()
            If String.IsNullOrWhiteSpace(desc) Then
                desc = "Inconsistência não especificada."
            End If

            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"INSERT INTO inconsistencia (importacao_id, linha_arquivo, descricao)
 VALUES (@id, @linha, @desc);"
                conn.Execute(sql, New With {.id = importacaoId, .linha = linhaArquivo, .desc = desc})
            End Using
        End Sub
    End Class
End Namespace
