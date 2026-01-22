Imports System.Data
Imports Dapper
Imports RemessaImportacao.Infrastructure.Data

Namespace Repositories
    Public Class ContratoRepository
        Private ReadOnly _factory As DbConnectionFactory

        Public Sub New(factory As DbConnectionFactory)
            _factory = factory
        End Sub

        Public Function UpsertContrato(numeroContrato As String, clienteId As Long) As Long
            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"INSERT INTO contrato (numero_contrato, cliente_id, dt_atualizacao)
 VALUES (@num, @clienteId, NOW())
 ON CONFLICT (numero_contrato)
 DO UPDATE SET
   cliente_id = EXCLUDED.cliente_id,
   dt_atualizacao = NOW()
 RETURNING id;"

                Return conn.ExecuteScalar(Of Long)(sql, New With {.num = numeroContrato, .clienteId = clienteId})
            End Using
        End Function
    End Class
End Namespace
