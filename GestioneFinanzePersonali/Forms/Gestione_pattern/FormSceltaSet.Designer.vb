<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormSceltaSet
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
        Me.pnlSet1 = New System.Windows.Forms.Panel()
        Me.rbSet1 = New System.Windows.Forms.RadioButton()
        Me.lblInfoSet1 = New System.Windows.Forms.Label()
        Me.pnlSet2 = New System.Windows.Forms.Panel()
        Me.rbSet2 = New System.Windows.Forms.RadioButton()
        Me.lblInfoSet2 = New System.Windows.Forms.Label()
        Me.pnlPulsanti = New System.Windows.Forms.Panel()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnAnnulla = New System.Windows.Forms.Button()
        Me.pnlSet1.SuspendLayout()
        Me.pnlSet2.SuspendLayout()
        Me.pnlPulsanti.SuspendLayout()
        Me.SuspendLayout()

        'lblTitolo
        Me.lblTitolo.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTitolo.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitolo.Location = New System.Drawing.Point(0, 0)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(500, 50)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "🔗 Scegli il Set da Mantenere"
        Me.lblTitolo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

        'pnlSet1
        Me.pnlSet1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlSet1.Controls.Add(Me.lblInfoSet1)
        Me.pnlSet1.Controls.Add(Me.rbSet1)
        Me.pnlSet1.Location = New System.Drawing.Point(20, 70)
        Me.pnlSet1.Name = "pnlSet1"
        Me.pnlSet1.Size = New System.Drawing.Size(460, 120)
        Me.pnlSet1.TabIndex = 1

        'rbSet1
        Me.rbSet1.Checked = True
        Me.rbSet1.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.rbSet1.Location = New System.Drawing.Point(15, 15)
        Me.rbSet1.Name = "rbSet1"
        Me.rbSet1.Size = New System.Drawing.Size(430, 25)
        Me.rbSet1.TabIndex = 0
        Me.rbSet1.TabStop = True
        Me.rbSet1.Text = "Set 1"
        Me.rbSet1.UseVisualStyleBackColor = True

        'lblInfoSet1
        Me.lblInfoSet1.Location = New System.Drawing.Point(35, 45)
        Me.lblInfoSet1.Name = "lblInfoSet1"
        Me.lblInfoSet1.Size = New System.Drawing.Size(410, 65)
        Me.lblInfoSet1.TabIndex = 1
        Me.lblInfoSet1.Text = "Informazioni Set 1"

        'pnlSet2
        Me.pnlSet2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlSet2.Controls.Add(Me.lblInfoSet2)
        Me.pnlSet2.Controls.Add(Me.rbSet2)
        Me.pnlSet2.Location = New System.Drawing.Point(20, 210)
        Me.pnlSet2.Name = "pnlSet2"
        Me.pnlSet2.Size = New System.Drawing.Size(460, 120)
        Me.pnlSet2.TabIndex = 2

        'rbSet2
        Me.rbSet2.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.rbSet2.Location = New System.Drawing.Point(15, 15)
        Me.rbSet2.Name = "rbSet2"
        Me.rbSet2.Size = New System.Drawing.Size(430, 25)
        Me.rbSet2.TabIndex = 0
        Me.rbSet2.Text = "Set 2"
        Me.rbSet2.UseVisualStyleBackColor = True

        'lblInfoSet2
        Me.lblInfoSet2.Location = New System.Drawing.Point(35, 45)
        Me.lblInfoSet2.Name = "lblInfoSet2"
        Me.lblInfoSet2.Size = New System.Drawing.Size(410, 65)
        Me.lblInfoSet2.TabIndex = 1
        Me.lblInfoSet2.Text = "Informazioni Set 2"

        'pnlPulsanti
        Me.pnlPulsanti.Controls.Add(Me.btnAnnulla)
        Me.pnlPulsanti.Controls.Add(Me.btnOK)
        Me.pnlPulsanti.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlPulsanti.Location = New System.Drawing.Point(0, 350)
        Me.pnlPulsanti.Name = "pnlPulsanti"
        Me.pnlPulsanti.Size = New System.Drawing.Size(500, 60)
        Me.pnlPulsanti.TabIndex = 3

        'btnOK
        Me.btnOK.BackColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(125, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.btnOK.ForeColor = System.Drawing.Color.White
        Me.btnOK.Location = New System.Drawing.Point(270, 15)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(100, 35)
        Me.btnOK.TabIndex = 0
        Me.btnOK.Text = "✅ Unisci"
        Me.btnOK.UseVisualStyleBackColor = False

        'btnAnnulla
        Me.btnAnnulla.BackColor = System.Drawing.Color.FromArgb(CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.btnAnnulla.ForeColor = System.Drawing.Color.White
        Me.btnAnnulla.Location = New System.Drawing.Point(380, 15)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = New System.Drawing.Size(100, 35)
        Me.btnAnnulla.TabIndex = 1
        Me.btnAnnulla.Text = "❌ Annulla"
        Me.btnAnnulla.UseVisualStyleBackColor = False

        'FormSceltaSet
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(500, 410)
        Me.Controls.Add(Me.pnlPulsanti)
        Me.Controls.Add(Me.pnlSet2)
        Me.Controls.Add(Me.pnlSet1)
        Me.Controls.Add(Me.lblTitolo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormSceltaSet"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Unione Set"
        Me.pnlSet1.ResumeLayout(False)
        Me.pnlSet2.ResumeLayout(False)
        Me.pnlPulsanti.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents lblTitolo As Label
    Friend WithEvents pnlSet1 As Panel
    Friend WithEvents rbSet1 As RadioButton
    Friend WithEvents lblInfoSet1 As Label
    Friend WithEvents pnlSet2 As Panel
    Friend WithEvents rbSet2 As RadioButton
    Friend WithEvents lblInfoSet2 As Label
    Friend WithEvents pnlPulsanti As Panel
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAnnulla As Button
End Class
