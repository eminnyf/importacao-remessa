Imports System.Data
Imports Dapper
Imports RemessaImportacao.Infrastructure.Data

Namespace Repositories
    Public Class ClienteRepository
        Private ReadOnly _factory As DbConnectionFactory

        Public Sub New(factory As DbConnectionFactory)
            _factory = factory
        End Sub

        'UPSERT por cpf_cnpj e retorna o id do cliente
        Public Function UpsertCliente(cpfCnpj As String,
                                     nome As String,
                                     dataNascimento As Date?,
                                     email As String,
                                     telefone As String,
                                     celular As String,
                                     logradouro As String,
                                     bairro As String,
                                     cidade As String,
                                     uf As String) As Long

            Using conn As IDbConnection = _factory.Create()
                Const sql As String =
"INSERT INTO cliente
 (cpf_cnpj, nome, data_nascimento, email, telefone, celular, logradouro, bairro, cidade, uf, dt_atualizacao)
 VALUES
 (@cpf, @nome, @nasc, @email, @tel, @cel, @log, @bairro, @cidade, @uf, NOW())
 ON CONFLICT (cpf_cnpj)
 DO UPDATE SET
   nome = EXCLUDED.nome,
   data_nascimento = EXCLUDED.data_nascimento,
   email = EXCLUDED.email,
   telefone = EXCLUDED.telefone,
   celular = EXCLUDED.celular,
   logradouro = EXCLUDED.logradouro,
   bairro = EXCLUDED.bairro,
   cidade = EXCLUDED.cidade,
   uf = EXCLUDED.uf,
   dt_atualizacao = NOW()
 RETURNING id;"

                Return conn.ExecuteScalar(Of Long)(sql, New With {
                    .cpf = cpfCnpj,
                    .nome = nome,
                    .nasc = dataNascimento,
                    .email = email,
                    .tel = telefone,
                    .cel = celular,
                    .log = logradouro,
                    .bairro = bairro,
                    .cidade = cidade,
                    .uf = uf
                })
            End Using
        End Function
    End Class
End Namespace
