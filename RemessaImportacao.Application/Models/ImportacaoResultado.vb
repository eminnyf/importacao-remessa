Namespace Models
    Public Class ImportacaoResultado
        Public Property TotalLinhas As Integer
        Public Property TotalSucesso As Integer
        Public Property TotalInconsistencias As Integer
        Public Property MensagensInconsistencias As New List(Of String)

        Public Property Status As String = "CONCLUIDO" 'CONCLUIDO | LAYOUT_INVALIDO | ERRO
    End Class
End Namespace
