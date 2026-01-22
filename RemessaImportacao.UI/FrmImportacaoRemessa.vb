Imports RemessaImportacao.Application.Models

Public Class FrmImportacaoRemessa

    Private Async Sub btnProcessar_Click(sender As Object, e As EventArgs) Handles btnProcessar.Click
        If Not ValidarArquivoSelecionado() Then Return

        TravarTela(True)

        Try
            AdicionarLog("Iniciando importação...")

            Dim service As New RemessaImportacao.Application.Services.ImportacaoService()

            ' 1) Primeira tentativa: valida layout
            Dim res = Await service.ProcessarAsync(txtCaminhoArquivo.Text)

            ' 2) Se layout inválido, pergunta se quer continuar
            If res.Status = "LAYOUT_INVALIDO" Then
                Dim msg = "O arquivo não está aderente ao layout esperado:" &
                      Environment.NewLine & Environment.NewLine &
                      String.Join(Environment.NewLine, res.MensagensInconsistencias) &
                      Environment.NewLine & Environment.NewLine &
                      "Deseja prosseguir mesmo assim?"

                Dim opt = MessageBox.Show(msg, "Layout divergente",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Warning)

                If opt = DialogResult.Yes Then
                    MessageBox.Show(
                    "Vou prosseguir mesmo com layout divergente." & Environment.NewLine &
                    "Se as colunas estiverem em posições diferentes, podem ocorrer inconsistências.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                    AdicionarLog("Usuário optou por prosseguir apesar do layout divergente.", "WARN")

                    ' 3) Reprocessa ignorando layout
                    res = Await service.ProcessarAsync(txtCaminhoArquivo.Text, ignoraLayout:=True)

                Else
                    AdicionarLog("Importação cancelada pelo usuário (layout divergente).", "INFO")

                    MessageBox.Show("Importação cancelada.", "Cancelado",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If
            End If

            ' 4) Mostra resultado somente uma vez (layout ok OU reprocessado)
            MostrarResultado(res)

        Catch ex As Exception
            AdicionarLog("Erro na importação: " & ex.Message, "ERRO")
            MessageBox.Show("Erro na importação: " & ex.Message,
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            TravarTela(False)
        End Try
    End Sub


    Private Sub btnSelecionarArquivo_Click(sender As Object, e As EventArgs) Handles btnSelecionarArquivo.Click
        Dim caminho = SelecionarArquivo()
        If String.IsNullOrWhiteSpace(caminho) Then Return

        txtCaminhoArquivo.Text = caminho
        AdicionarLog("Arquivo selecionado: " & System.IO.Path.GetFileName(caminho))

        btnProcessar.Enabled = True
    End Sub


    Private Function SelecionarArquivo() As String
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Arquivos Excel (*.xlsx)|*.xlsx"
            ofd.Title = "Selecione o arquivo de remessa"
            ofd.CheckFileExists = True
            ofd.Multiselect = False

            If ofd.ShowDialog() = DialogResult.OK Then
                Return ofd.FileName
            End If
        End Using

        Return ""
    End Function


    Private Function ValidarArquivoSelecionado() As Boolean
        If String.IsNullOrWhiteSpace(txtCaminhoArquivo.Text) Then
            MessageBox.Show("Selecione um arquivo antes de iniciar.",
                            "Atenção",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return False
        End If

        If Not System.IO.File.Exists(txtCaminhoArquivo.Text) Then
            MessageBox.Show("O arquivo selecionado não foi encontrado. Selecione novamente.",
                            "Atenção",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function


    Private Sub TravarTela(travar As Boolean)
        btnProcessar.Enabled = Not travar AndAlso Not String.IsNullOrWhiteSpace(txtCaminhoArquivo.Text)
        btnSelecionarArquivo.Enabled = Not travar
        Cursor = If(travar, Cursors.WaitCursor, Cursors.Default)
    End Sub


    Private Sub MostrarResultado(res As ImportacaoResultado)
        AdicionarLog($"Finalizado. Linhas: {res.TotalLinhas} | Sucesso: {res.TotalSucesso} | Inconsistências: {res.TotalInconsistencias}")

        If res.TotalInconsistencias > 0 Then
            MessageBox.Show(String.Join(Environment.NewLine, res.MensagensInconsistencias),
                            "Inconsistências encontradas",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
        Else
            MessageBox.Show("Importação concluída com sucesso!",
                            "Sucesso",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
        End If
    End Sub


    Private Sub AdicionarLog(mensagem As String, Optional tipo As String = "INFO")
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [{tipo}] {mensagem}{Environment.NewLine}")
    End Sub


    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles txtArquivo.Click
    End Sub

    Private Sub grpImportacao_Enter(sender As Object, e As EventArgs) Handles grpImportacao.Enter
    End Sub

End Class
