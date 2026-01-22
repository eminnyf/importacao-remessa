Imports System.Configuration
Imports System.Data
Imports Npgsql

Namespace Data
    Public Class DbConnectionFactory
        Private ReadOnly _cs As String

        Public Sub New()
            _cs = ConfigurationManager.ConnectionStrings("RemessaDb")?.ConnectionString
            If String.IsNullOrWhiteSpace(_cs) Then
                Throw New InvalidOperationException("ConnectionString 'RemessaDb' não encontrada no App.config.")
            End If
        End Sub

        Public Function Create() As IDbConnection
            Return New NpgsqlConnection(_cs)
        End Function
    End Class
End Namespace
