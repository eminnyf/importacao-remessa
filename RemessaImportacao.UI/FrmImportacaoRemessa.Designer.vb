<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmImportacaoRemessa
    Inherits System.Windows.Forms.Form

    'Descartar substituições de formulário para limpar a lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Exigido pelo Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'OBSERVAÇÃO: o procedimento a seguir é exigido pelo Windows Form Designer
    'Pode ser modificado usando o Windows Form Designer.  
    'Não o modifique usando o editor de códigos.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.txtArquivo = New System.Windows.Forms.Label()
        Me.txtCaminhoArquivo = New System.Windows.Forms.TextBox()
        Me.btnSelecionarArquivo = New System.Windows.Forms.Button()
        Me.btnProcessar = New System.Windows.Forms.Button()
        Me.txtLog = New System.Windows.Forms.TextBox()
        Me.pbProgresso = New System.Windows.Forms.ProgressBar()
        Me.grpImportacao = New System.Windows.Forms.GroupBox()
        Me.grpImportacao.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtArquivo
        '
        Me.txtArquivo.AutoSize = True
        Me.txtArquivo.Location = New System.Drawing.Point(33, 112)
        Me.txtArquivo.Name = "txtArquivo"
        Me.txtArquivo.Size = New System.Drawing.Size(134, 16)
        Me.txtArquivo.TabIndex = 0
        Me.txtArquivo.Text = "Arquivo de Remessa"
        Me.txtArquivo.UseWaitCursor = True
        '
        'txtCaminhoArquivo
        '
        Me.txtCaminhoArquivo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCaminhoArquivo.Location = New System.Drawing.Point(173, 109)
        Me.txtCaminhoArquivo.Name = "txtCaminhoArquivo"
        Me.txtCaminhoArquivo.ReadOnly = True
        Me.txtCaminhoArquivo.Size = New System.Drawing.Size(550, 22)
        Me.txtCaminhoArquivo.TabIndex = 1
        Me.txtCaminhoArquivo.UseWaitCursor = True
        '
        'btnSelecionarArquivo
        '
        Me.btnSelecionarArquivo.Location = New System.Drawing.Point(729, 109)
        Me.btnSelecionarArquivo.Name = "btnSelecionarArquivo"
        Me.btnSelecionarArquivo.Size = New System.Drawing.Size(83, 23)
        Me.btnSelecionarArquivo.TabIndex = 2
        Me.btnSelecionarArquivo.Text = "Selecionar"
        Me.btnSelecionarArquivo.UseVisualStyleBackColor = True
        Me.btnSelecionarArquivo.UseWaitCursor = True
        '
        'btnProcessar
        '
        Me.btnProcessar.Enabled = False
        Me.btnProcessar.Location = New System.Drawing.Point(361, 157)
        Me.btnProcessar.Name = "btnProcessar"
        Me.btnProcessar.Size = New System.Drawing.Size(180, 35)
        Me.btnProcessar.TabIndex = 3
        Me.btnProcessar.Text = "Iniciar Processamento"
        Me.btnProcessar.UseVisualStyleBackColor = True
        Me.btnProcessar.UseWaitCursor = True
        '
        'txtLog
        '
        Me.txtLog.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.txtLog.Location = New System.Drawing.Point(3, 246)
        Me.txtLog.Multiline = True
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ReadOnly = True
        Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLog.Size = New System.Drawing.Size(872, 200)
        Me.txtLog.TabIndex = 4
        Me.txtLog.UseWaitCursor = True
        '
        'pbProgresso
        '
        Me.pbProgresso.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pbProgresso.Location = New System.Drawing.Point(3, 223)
        Me.pbProgresso.Name = "pbProgresso"
        Me.pbProgresso.Size = New System.Drawing.Size(872, 23)
        Me.pbProgresso.TabIndex = 5
        Me.pbProgresso.UseWaitCursor = True
        '
        'grpImportacao
        '
        Me.grpImportacao.Controls.Add(Me.txtArquivo)
        Me.grpImportacao.Controls.Add(Me.pbProgresso)
        Me.grpImportacao.Controls.Add(Me.txtCaminhoArquivo)
        Me.grpImportacao.Controls.Add(Me.btnProcessar)
        Me.grpImportacao.Controls.Add(Me.txtLog)
        Me.grpImportacao.Controls.Add(Me.btnSelecionarArquivo)
        Me.grpImportacao.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpImportacao.Location = New System.Drawing.Point(0, 0)
        Me.grpImportacao.Name = "grpImportacao"
        Me.grpImportacao.Size = New System.Drawing.Size(878, 449)
        Me.grpImportacao.TabIndex = 6
        Me.grpImportacao.TabStop = False
        Me.grpImportacao.UseWaitCursor = True
        '
        'FrmImportacaoRemessa
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(878, 449)
        Me.Controls.Add(Me.grpImportacao)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(900, 500)
        Me.Name = "FrmImportacaoRemessa"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Importação de Remessa"
        Me.grpImportacao.ResumeLayout(False)
        Me.grpImportacao.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents txtArquivo As Label
    Friend WithEvents txtCaminhoArquivo As TextBox
    Friend WithEvents btnSelecionarArquivo As Button
    Friend WithEvents btnProcessar As Button
    Friend WithEvents txtLog As TextBox
    Friend WithEvents pbProgresso As ProgressBar
    Friend WithEvents grpImportacao As GroupBox
End Class
