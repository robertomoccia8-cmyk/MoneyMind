<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormDuplicati
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
        Me.dgvDuplicati = New System.Windows.Forms.DataGridView()
        Me.pnlPulsanti = New System.Windows.Forms.Panel()
        Me.btnUnisciSelezionati = New System.Windows.Forms.Button()
        Me.btnIgnora = New System.Windows.Forms.Button()
        Me.btnChiudi = New System.Windows.Forms.Button()
        CType(Me.dgvDuplicati, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlPulsanti.SuspendLayout()
        Me.SuspendLayout()

        'lblTitolo
        Me.lblTitolo.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTitolo.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitolo.Location = New System.Drawing.Point(0, 0)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(884, 50)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "🔍 Set Duplicati o Simili Trovati"
        Me.lblTitolo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

        'dgvDuplicati
        Me.dgvDuplicati.AllowUserToAddRows = False
        Me.dgvDuplicati.AllowUserToDeleteRows = False
        Me.dgvDuplicati.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvDuplicati.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvDuplicati.Location = New System.Drawing.Point(0, 50)
        Me.dgvDuplicati.Name = "dgvDuplicati"
        Me.dgvDuplicati.ReadOnly = True
        Me.dgvDuplicati.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvDuplicati.Size = New System.Drawing.Size(884, 430)
        Me.dgvDuplicati.TabIndex = 1

        'pnlPulsanti
        Me.pnlPulsanti.Controls.Add(Me.btnChiudi)
        Me.pnlPulsanti.Controls.Add(Me.btnIgnora)
        Me.pnlPulsanti.Controls.Add(Me.btnUnisciSelezionati)
        Me.pnlPulsanti.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlPulsanti.Location = New System.Drawing.Point(0, 480)
        Me.pnlPulsanti.Name = "pnlPulsanti"
        Me.pnlPulsanti.Size = New System.Drawing.Size(884, 61)
        Me.pnlPulsanti.TabIndex = 2

        'btnUnisciSelezionati
        Me.btnUnisciSelezionati.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnUnisciSelezionati.ForeColor = System.Drawing.Color.White
        Me.btnUnisciSelezionati.Location = New System.Drawing.Point(20, 15)
        Me.btnUnisciSelezionati.Name = "btnUnisciSelezionati"
        Me.btnUnisciSelezionati.Size = New System.Drawing.Size(150, 35)
        Me.btnUnisciSelezionati.TabIndex = 0
        Me.btnUnisciSelezionati.Text = "🔗 Unisci Selezionati"
        Me.btnUnisciSelezionati.UseVisualStyleBackColor = False

        'btnIgnora
        Me.btnIgnora.BackColor = System.Drawing.Color.FromArgb(CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.btnIgnora.ForeColor = System.Drawing.Color.White
        Me.btnIgnora.Location = New System.Drawing.Point(190, 15)
        Me.btnIgnora.Name = "btnIgnora"
        Me.btnIgnora.Size = New System.Drawing.Size(100, 35)
        Me.btnIgnora.TabIndex = 1
        Me.btnIgnora.Text = "⏭️ Ignora"
        Me.btnIgnora.UseVisualStyleBackColor = False

        'btnChiudi
        Me.btnChiudi.BackColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(150, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.btnChiudi.ForeColor = System.Drawing.Color.White
        Me.btnChiudi.Location = New System.Drawing.Point(760, 15)
        Me.btnChiudi.Name = "btnChiudi"
        Me.btnChiudi.Size = New System.Drawing.Size(100, 35)
        Me.btnChiudi.TabIndex = 2
        Me.btnChiudi.Text = "✅ Chiudi"
        Me.btnChiudi.UseVisualStyleBackColor = False

        'FormDuplicati
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(884, 541)
        Me.Controls.Add(Me.dgvDuplicati)
        Me.Controls.Add(Me.pnlPulsanti)
        Me.Controls.Add(Me.lblTitolo)
        Me.Name = "FormDuplicati"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Gestione Duplicati"
        CType(Me.dgvDuplicati, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlPulsanti.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents lblTitolo As Label
    Friend WithEvents dgvDuplicati As DataGridView
    Friend WithEvents pnlPulsanti As Panel
    Friend WithEvents btnUnisciSelezionati As Button
    Friend WithEvents btnIgnora As Button
    Friend WithEvents btnChiudi As Button
End Class
