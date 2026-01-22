Namespace Importacao
    Public Class InconsistenciaImportacao
        Public Property IdInconsistencia As Long
        Public Property IdImportacao As Long
        Public Property LinhaArquivo As Integer
        Public Property Campo As String
        Public Property Mensagem As String
        Public Property Severidade As String = "WARN"
    End Class
End Namespace
