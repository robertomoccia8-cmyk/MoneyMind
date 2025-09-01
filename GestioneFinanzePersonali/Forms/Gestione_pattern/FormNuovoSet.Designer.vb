<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormNuovoSet
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblTitolo = New System.Windows.Forms.Label()
        Me.lblMacro = New System.Windows.Forms.Label()
        Me.txtMacroCategoria = New System.Windows.Forms.TextBox()
        Me.lblCategoria = New System.Windows.Forms.Label()
        Me.txtCategoria = New System.Windows.Forms.TextBox()
        Me.lblSotto = New System.Windows.Forms.Label()
        Me.lblParole = New System.Windows.Forms.Label()
        Me.txtParole = New System.Windows.Forms.TextBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnAnnulla = New System.Windows.Forms.Button()
        Me.SuspendLayout()

        'lblTitolo
        Me.lblTitolo.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitolo.Location = New System.Drawing.Point(12, 9)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(360, 30)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Crea Nuovo Set di Categorie"
        Me.lblTitolo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

        'lblMacro
        Me.lblMacro.Location = New System.Drawing.Point(15, 50)
        Me.lblMacro.Name = "lblMacro"
        Me.lblMacro.Size = New System.Drawing.Size(120, 23)
        Me.lblMacro.TabIndex = 1
        Me.lblMacro.Text = "Macro Categoria:"

        'txtMacroCategoria
        Me.txtMacroCategoria.Location = New System.Drawing.Point(141, 47)
        Me.txtMacroCategoria.Name = "txtMacroCategoria"
        Me.txtMacroCategoria.Size = New System.Drawing.Size(200, 27)
        Me.txtMacroCategoria.TabIndex = 2

        'lblCategoria
        Me.lblCategoria.Location = New System.Drawing.Point(15, 85)
        Me.lblCategoria.Name = "lblCategoria"
        Me.lblCategoria.Size = New System.Drawing.Size(120, 23)
        Me.lblCategoria.TabIndex = 3
        Me.lblCategoria.Text = "Categoria:"

        'txtCategoria
        Me.txtCategoria.Location = New System.Drawing.Point(141, 82)
        Me.txtCategoria.Name = "txtCategoria"
        Me.txtCategoria.Size = New System.Drawing.Size(200, 27)
        Me.txtCategoria.TabIndex = 4

        'lblSotto
        Me.lblSotto.Location = New System.Drawing.Point(15, 120)
        Me.lblSotto.Name = "lblSotto"
        Me.lblSotto.Size = New System.Drawing.Size(120, 23)
        Me.lblSotto.TabIndex = 5
        Me.lblSotto.Text = "Sotto Categoria:"

        'txtSottoCategoria
        'Me.txtSottoCategoria.Location = New System.Drawing.Point(141, 117)
        'Me.txtSottoCategoria.Name = "txtSottoCategoria"
        'Me.txtSottoCategoria.Size = New System.Drawing.Size(200, 27)
        'Me.txtSottoCategoria.TabIndex = 6

        'lblParole
        Me.lblParole.Location = New System.Drawing.Point(15, 155)
        Me.lblParole.Name = "lblParole"
        Me.lblParole.Size = New System.Drawing.Size(120, 23)
        Me.lblParole.TabIndex = 7
        Me.lblParole.Text = "Parole (separate da virgola):"

        'txtParole
        Me.txtParole.Location = New System.Drawing.Point(15, 178)
        Me.txtParole.Multiline = True
        Me.txtParole.Name = "txtParole"
        Me.txtParole.Size = New System.Drawing.Size(326, 80)
        Me.txtParole.TabIndex = 8
        ' Me.txtParole.PlaceholderText = "es: supermercato, spesa, alimentari"

        'btnOK
        Me.btnOK.BackColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(125, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.btnOK.ForeColor = System.Drawing.Color.White
        Me.btnOK.Location = New System.Drawing.Point(141, 275)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(100, 35)
        Me.btnOK.TabIndex = 9
        Me.btnOK.Text = "Crea"
        Me.btnOK.UseVisualStyleBackColor = False

        'btnAnnulla
        Me.btnAnnulla.BackColor = System.Drawing.Color.FromArgb(CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.btnAnnulla.ForeColor = System.Drawing.Color.White
        Me.btnAnnulla.Location = New System.Drawing.Point(247, 275)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = New System.Drawing.Size(100, 35)
        Me.btnAnnulla.TabIndex = 10
        Me.btnAnnulla.Text = "Annulla"
        Me.btnAnnulla.UseVisualStyleBackColor = False

        'FormNuovoSet
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(384, 331)
        Me.Controls.Add(Me.btnAnnulla)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.txtParole)
        Me.Controls.Add(Me.lblParole)
        'Me.Controls.Add(Me.txtSottoCategoria)
        Me.Controls.Add(Me.lblSotto)
        Me.Controls.Add(Me.txtCategoria)
        Me.Controls.Add(Me.lblCategoria)
        Me.Controls.Add(Me.txtMacroCategoria)
        Me.Controls.Add(Me.lblMacro)
        Me.Controls.Add(Me.lblTitolo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormNuovoSet"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Nuovo Set"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As Label
    Friend WithEvents lblMacro As Label
    Friend WithEvents txtMacroCategoria As TextBox
    Friend WithEvents lblCategoria As Label
    Friend WithEvents txtCategoria As TextBox
    Friend WithEvents lblSotto As Label
    Friend WithEvents lblParole As Label
    Friend WithEvents txtParole As TextBox
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAnnulla As Button
End Class
