Imports System.Data
Imports Dapper
Imports RemessaImportacao.Infrastructure.Data

Namespace Repositories
    Public Class LayoutRepository

        Private ReadOnly _factory As DbConnectionFactory

        Public Sub New(factory As DbConnectionFactory)
            _factory = factory
        End Sub

        Public Function ObterLayoutOrdenado() As List(Of String)
            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"SELECT nome_coluna
   FROM layout_arquivo
  ORDER BY ordem;"
                Return conn.Query(Of String)(sql).ToList()
            End Using
        End Function

    End Class
End Namespace
