Namespace Dtos
    Public Class RemessaLinhaDto
        Public Property LinhaArquivo As Integer

        Public Property CpfCnpj As String
        Public Property Contrato As String
        Public Property NumeroParcela As Integer
        Public Property Vencimento As Date
        Public Property Valor As Decimal

        Public Property NomeCliente As String
        Public Property Telefone As String
        Public Property Celular As String
        Public Property Email As String
        Public Property DataNascimento As Date?

        Public Property Logradouro As String
        Public Property Bairro As String
        Public Property Cidade As String
        Public Property Uf As String
    End Class
End Namespace
