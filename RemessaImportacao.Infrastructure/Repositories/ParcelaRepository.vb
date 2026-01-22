Imports System.Data
Imports Dapper
Imports RemessaImportacao.Infrastructure.Data

Namespace Repositories
    Public Class ParcelaRepository
        Private ReadOnly _factory As DbConnectionFactory

        Public Sub New(factory As DbConnectionFactory)
            _factory = factory
        End Sub

        Public Sub UpsertParcela(contratoId As Long, numeroParcela As Integer, vencimento As Date, valor As Decimal)
            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"INSERT INTO parcela (contrato_id, numero_parcela, data_vencimento, valor, dt_atualizacao)
 VALUES (@contratoId, @numParc, @venc, @valor, NOW())
 ON CONFLICT (contrato_id, numero_parcela)
 DO UPDATE SET
   data_vencimento = EXCLUDED.data_vencimento,
   valor = EXCLUDED.valor,
   dt_atualizacao = NOW();"

                conn.Execute(sql, New With {.contratoId = contratoId, .numParc = numeroParcela, .venc = vencimento, .valor = valor})
            End Using
        End Sub
    End Class
End Namespace
