<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormUnisciSet
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
        Me.lblOrigine = New System.Windows.Forms.Label()
        Me.lblInfoOrigine = New System.Windows.Forms.Label()
        Me.lblDestinazione = New System.Windows.Forms.Label()
        Me.cmbDestinazione = New System.Windows.Forms.ComboBox()
        Me.lblInfoDestinazione = New System.Windows.Forms.Label()
        Me.pnlPulsanti = New System.Windows.Forms.Panel()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnAnnulla = New System.Windows.Forms.Button()
        Me.lblWarning = New System.Windows.Forms.Label()
        Me.pnlPulsanti.SuspendLayout()
        Me.SuspendLayout()

        'lblTitolo
        Me.lblTitolo.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTitolo.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitolo.Location = New System.Drawing.Point(0, 0)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(550, 50)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "🔗 Unisci Set con Altro Set"
        Me.lblTitolo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

        'lblOrigine
        Me.lblOrigine.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblOrigine.Location = New System.Drawing.Point(20, 70)
        Me.lblOrigine.Name = "lblOrigine"
        Me.lblOrigine.Size = New System.Drawing.Size(510, 25)
        Me.lblOrigine.TabIndex = 1
        Me.lblOrigine.Text = "Set di Origine (da unire):"

        'lblInfoOrigine
        Me.lblInfoOrigine.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(248, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.lblInfoOrigine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInfoOrigine.Location = New System.Drawing.Point(20, 95)
        Me.lblInfoOrigine.Name = "lblInfoOrigine"
        Me.lblInfoOrigine.Size = New System.Drawing.Size(510, 80)
        Me.lblInfoOrigine.TabIndex = 2
        Me.lblInfoOrigine.Text = "Info Set Origine"

        'lblDestinazione
        Me.lblDestinazione.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblDestinazione.Location = New System.Drawing.Point(20, 195)
        Me.lblDestinazione.Name = "lblDestinazione"
        Me.lblDestinazione.Size = New System.Drawing.Size(510, 25)
        Me.lblDestinazione.TabIndex = 3
        Me.lblDestinazione.Text = "Set di Destinazione (dove unire):"

        'cmbDestinazione
        Me.cmbDestinazione.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDestinazione.Location = New System.Drawing.Point(20, 220)
        Me.cmbDestinazione.Name = "cmbDestinazione"
        Me.cmbDestinazione.Size = New System.Drawing.Size(510, 28)
        Me.cmbDestinazione.TabIndex = 4

        'lblInfoDestinazione
        Me.lblInfoDestinazione.BackColor = System.Drawing.Color.FromArgb(CType(CType(245, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(245, Byte), Integer))
        Me.lblInfoDestinazione.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInfoDestinazione.Location = New System.Drawing.Point(20, 255)
        Me.lblInfoDestinazione.Name = "lblInfoDestinazione"
        Me.lblInfoDestinazione.Size = New System.Drawing.Size(510, 80)
        Me.lblInfoDestinazione.TabIndex = 5
        Me.lblInfoDestinazione.Text = "Seleziona un set di destinazione..."

        'lblWarning
        Me.lblWarning.ForeColor = System.Drawing.Color.FromArgb(CType(CType(244, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(54, Byte), Integer))
        Me.lblWarning.Location = New System.Drawing.Point(20, 350)
        Me.lblWarning.Name = "lblWarning"
        Me.lblWarning.Size = New System.Drawing.Size(510, 40)
        Me.lblWarning.TabIndex = 6
        Me.lblWarning.Text = "⚠️ ATTENZIONE: Tutti i pattern e le transazioni del set di origine verranno spostate nel set di destinazione. Il set di origine verrà eliminato."

        'pnlPulsanti
        Me.pnlPulsanti.Controls.Add(Me.btnAnnulla)
        Me.pnlPulsanti.Controls.Add(Me.btnOK)
        Me.pnlPulsanti.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlPulsanti.Location = New System.Drawing.Point(0, 410)
        Me.pnlPulsanti.Name = "pnlPulsanti"
        Me.pnlPulsanti.Size = New System.Drawing.Size(550, 60)
        Me.pnlPulsanti.TabIndex = 7

        'btnOK
        Me.btnOK.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnOK.Enabled = False
        Me.btnOK.ForeColor = System.Drawing.Color.White
        Me.btnOK.Location = New System.Drawing.Point(320, 15)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(100, 35)
        Me.btnOK.TabIndex = 0
        Me.btnOK.Text = "🔗 Unisci"
        Me.btnOK.UseVisualStyleBackColor = False

        'btnAnnulla
        Me.btnAnnulla.BackColor = System.Drawing.Color.FromArgb(CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.btnAnnulla.ForeColor = System.Drawing.Color.White
        Me.btnAnnulla.Location = New System.Drawing.Point(430, 15)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = New System.Drawing.Size(100, 35)
        Me.btnAnnulla.TabIndex = 1
        Me.btnAnnulla.Text = "❌ Annulla"
        Me.btnAnnulla.UseVisualStyleBackColor = False

        'FormUnisciSet
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(550, 470)
        Me.Controls.Add(Me.pnlPulsanti)
        Me.Controls.Add(Me.lblWarning)
        Me.Controls.Add(Me.lblInfoDestinazione)
        Me.Controls.Add(Me.cmbDestinazione)
        Me.Controls.Add(Me.lblDestinazione)
        Me.Controls.Add(Me.lblInfoOrigine)
        Me.Controls.Add(Me.lblOrigine)
        Me.Controls.Add(Me.lblTitolo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormUnisciSet"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Unione Set"
        Me.pnlPulsanti.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents lblTitolo As Label
    Friend WithEvents lblOrigine As Label
    Friend WithEvents lblInfoOrigine As Label
    Friend WithEvents lblDestinazione As Label
    Friend WithEvents cmbDestinazione As ComboBox
    Friend WithEvents lblInfoDestinazione As Label
    Friend WithEvents pnlPulsanti As Panel
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAnnulla As Button
    Friend WithEvents lblWarning As Label
End Class
