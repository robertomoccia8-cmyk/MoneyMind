<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormFinanza
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
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

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata in Progettazione Windows Form.  
    'Non modificarla mediante l'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.panelInserimento = New System.Windows.Forms.Panel()
        Me.btnAnalisi = New System.Windows.Forms.Button()
        Me.btnEliminaRiga = New System.Windows.Forms.Button()
        Me.txtDescrizione = New System.Windows.Forms.TextBox()
        Me.lblDescrizione = New System.Windows.Forms.Label()
        Me.txtImporto = New System.Windows.Forms.TextBox()
        Me.lblImporto = New System.Windows.Forms.Label()
        Me.dtpData = New System.Windows.Forms.DateTimePicker()
        Me.lblData = New System.Windows.Forms.Label()
        Me.btnChiudiDettagli = New System.Windows.Forms.Button()
        Me.lblFiltroAttivo = New System.Windows.Forms.Label()
        Me.btnAggiungi = New System.Windows.Forms.Button()
        Me.btnClassifica = New System.Windows.Forms.Button()
        Me.btnImportaExcel = New System.Windows.Forms.Button()
        Me.btnGestionePattern = New System.Windows.Forms.Button()
        Me.dgvTransazioni = New System.Windows.Forms.DataGridView()
        Me.panelInfoMensile = New System.Windows.Forms.Panel()
        Me.lblInfoMensile = New System.Windows.Forms.Label()
        Me.lblFiltroParolaDettagliato = New System.Windows.Forms.Label()
        Me.Panel_centrale = New System.Windows.Forms.Panel()
        Me.btnConfiguraStipendi = New System.Windows.Forms.Button()
        Me.panelInserimento.SuspendLayout()
        CType(Me.dgvTransazioni, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panelInfoMensile.SuspendLayout()
        Me.Panel_centrale.SuspendLayout()
        Me.SuspendLayout()
        '
        'panelInserimento
        '
        Me.panelInserimento.Controls.Add(Me.btnAnalisi)
        Me.panelInserimento.Controls.Add(Me.btnEliminaRiga)
        Me.panelInserimento.Controls.Add(Me.txtDescrizione)
        Me.panelInserimento.Controls.Add(Me.lblDescrizione)
        Me.panelInserimento.Controls.Add(Me.txtImporto)
        Me.panelInserimento.Controls.Add(Me.lblImporto)
        Me.panelInserimento.Controls.Add(Me.dtpData)
        Me.panelInserimento.Controls.Add(Me.lblData)
        Me.panelInserimento.Dock = System.Windows.Forms.DockStyle.Top
        Me.panelInserimento.Location = New System.Drawing.Point(0, 0)
        Me.panelInserimento.Name = "panelInserimento"
        Me.panelInserimento.Size = New System.Drawing.Size(1428, 100)
        Me.panelInserimento.TabIndex = 1
        '
        'btnAnalisi
        '
        Me.btnAnalisi.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAnalisi.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnAnalisi.Location = New System.Drawing.Point(880, 50)
        Me.btnAnalisi.Name = "btnAnalisi"
        Me.btnAnalisi.Size = New System.Drawing.Size(160, 35)
        Me.btnAnalisi.TabIndex = 7
        Me.btnAnalisi.Text = "Analisi"
        Me.btnAnalisi.UseVisualStyleBackColor = True
        '
        'btnEliminaRiga
        '
        Me.btnEliminaRiga.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnEliminaRiga.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnEliminaRiga.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnEliminaRiga.Location = New System.Drawing.Point(710, 50)
        Me.btnEliminaRiga.Name = "btnEliminaRiga"
        Me.btnEliminaRiga.Size = New System.Drawing.Size(161, 35)
        Me.btnEliminaRiga.TabIndex = 6
        Me.btnEliminaRiga.Text = "Elimina Transazione"
        Me.btnEliminaRiga.UseVisualStyleBackColor = True
        '
        'txtDescrizione
        '
        Me.txtDescrizione.Location = New System.Drawing.Point(100, 47)
        Me.txtDescrizione.Name = "txtDescrizione"
        Me.txtDescrizione.Size = New System.Drawing.Size(400, 26)
        Me.txtDescrizione.TabIndex = 5
        '
        'lblDescrizione
        '
        Me.lblDescrizione.AutoSize = True
        Me.lblDescrizione.Location = New System.Drawing.Point(10, 50)
        Me.lblDescrizione.Name = "lblDescrizione"
        Me.lblDescrizione.Size = New System.Drawing.Size(96, 20)
        Me.lblDescrizione.TabIndex = 4
        Me.lblDescrizione.Text = "Descrizione:"
        '
        'txtImporto
        '
        Me.txtImporto.Location = New System.Drawing.Point(290, 12)
        Me.txtImporto.Name = "txtImporto"
        Me.txtImporto.Size = New System.Drawing.Size(100, 26)
        Me.txtImporto.TabIndex = 3
        Me.txtImporto.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblImporto
        '
        Me.lblImporto.AutoSize = True
        Me.lblImporto.Location = New System.Drawing.Point(220, 15)
        Me.lblImporto.Name = "lblImporto"
        Me.lblImporto.Size = New System.Drawing.Size(68, 20)
        Me.lblImporto.TabIndex = 2
        Me.lblImporto.Text = "Importo:"
        '
        'dtpData
        '
        Me.dtpData.Location = New System.Drawing.Point(70, 12)
        Me.dtpData.Name = "dtpData"
        Me.dtpData.Size = New System.Drawing.Size(130, 26)
        Me.dtpData.TabIndex = 1
        '
        'lblData
        '
        Me.lblData.AutoSize = True
        Me.lblData.Location = New System.Drawing.Point(10, 15)
        Me.lblData.Name = "lblData"
        Me.lblData.Size = New System.Drawing.Size(44, 20)
        Me.lblData.TabIndex = 0
        Me.lblData.Text = "Data"
        '
        'btnChiudiDettagli
        '
        Me.btnChiudiDettagli.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnChiudiDettagli.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnChiudiDettagli.Location = New System.Drawing.Point(1282, 28)
        Me.btnChiudiDettagli.Name = "btnChiudiDettagli"
        Me.btnChiudiDettagli.Size = New System.Drawing.Size(134, 30)
        Me.btnChiudiDettagli.TabIndex = 9
        Me.btnChiudiDettagli.Text = "Chiudi dettagli"
        Me.btnChiudiDettagli.UseVisualStyleBackColor = True
        '
        'lblFiltroAttivo
        '
        Me.lblFiltroAttivo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblFiltroAttivo.AutoSize = True
        Me.lblFiltroAttivo.ForeColor = System.Drawing.Color.DarkOrange
        Me.lblFiltroAttivo.Location = New System.Drawing.Point(12, 14)
        Me.lblFiltroAttivo.Name = "lblFiltroAttivo"
        Me.lblFiltroAttivo.Size = New System.Drawing.Size(0, 20)
        Me.lblFiltroAttivo.TabIndex = 8
        '
        'btnAggiungi
        '
        Me.btnAggiungi.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAggiungi.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnAggiungi.Location = New System.Drawing.Point(520, 10)
        Me.btnAggiungi.Name = "btnAggiungi"
        Me.btnAggiungi.Size = New System.Drawing.Size(180, 35)
        Me.btnAggiungi.TabIndex = 2
        Me.btnAggiungi.Text = "Aggiungi Transazione"
        Me.btnAggiungi.UseVisualStyleBackColor = True
        '
        'btnClassifica
        '
        Me.btnClassifica.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClassifica.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnClassifica.Location = New System.Drawing.Point(520, 50)
        Me.btnClassifica.Name = "btnClassifica"
        Me.btnClassifica.Size = New System.Drawing.Size(180, 35)
        Me.btnClassifica.TabIndex = 3
        Me.btnClassifica.Text = "Classifica Categorie"
        Me.btnClassifica.UseVisualStyleBackColor = True
        '
        'btnImportaExcel
        '
        Me.btnImportaExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnImportaExcel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnImportaExcel.Location = New System.Drawing.Point(710, 10)
        Me.btnImportaExcel.Name = "btnImportaExcel"
        Me.btnImportaExcel.Size = New System.Drawing.Size(160, 35)
        Me.btnImportaExcel.TabIndex = 4
        Me.btnImportaExcel.Text = "Importa da File"
        Me.btnImportaExcel.UseVisualStyleBackColor = True
        '
        'btnGestionePattern
        '
        Me.btnGestionePattern.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnGestionePattern.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnGestionePattern.Location = New System.Drawing.Point(880, 10)
        Me.btnGestionePattern.Name = "btnGestionePattern"
        Me.btnGestionePattern.Size = New System.Drawing.Size(160, 35)
        Me.btnGestionePattern.TabIndex = 5
        Me.btnGestionePattern.Text = "Gestione Pattern"
        Me.btnGestionePattern.UseVisualStyleBackColor = True
        '
        'dgvTransazioni
        '
        Me.dgvTransazioni.AllowUserToAddRows = False
        Me.dgvTransazioni.AllowUserToDeleteRows = False
        Me.dgvTransazioni.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvTransazioni.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells
        Me.dgvTransazioni.BackgroundColor = System.Drawing.Color.White
        Me.dgvTransazioni.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.dgvTransazioni.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvTransazioni.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvTransazioni.EnableHeadersVisualStyles = False
        Me.dgvTransazioni.Location = New System.Drawing.Point(0, 0)
        Me.dgvTransazioni.Name = "dgvTransazioni"
        Me.dgvTransazioni.ReadOnly = True
        Me.dgvTransazioni.RowHeadersWidth = 62
        Me.dgvTransazioni.RowTemplate.Height = 28
        Me.dgvTransazioni.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvTransazioni.Size = New System.Drawing.Size(1428, 624)
        Me.dgvTransazioni.TabIndex = 6
        '
        'panelInfoMensile
        '
        Me.panelInfoMensile.BackColor = System.Drawing.SystemColors.Info
        Me.panelInfoMensile.Controls.Add(Me.lblInfoMensile)
        Me.panelInfoMensile.Controls.Add(Me.btnChiudiDettagli)
        Me.panelInfoMensile.Controls.Add(Me.lblFiltroAttivo)
        Me.panelInfoMensile.Controls.Add(Me.lblFiltroParolaDettagliato)
        Me.panelInfoMensile.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelInfoMensile.Location = New System.Drawing.Point(0, 724)
        Me.panelInfoMensile.Name = "panelInfoMensile"
        Me.panelInfoMensile.Size = New System.Drawing.Size(1428, 70)
        Me.panelInfoMensile.TabIndex = 7
        '
        'lblInfoMensile
        '
        Me.lblInfoMensile.AutoSize = True
        Me.lblInfoMensile.Location = New System.Drawing.Point(12, 14)
        Me.lblInfoMensile.Name = "lblInfoMensile"
        Me.lblInfoMensile.Size = New System.Drawing.Size(0, 20)
        Me.lblInfoMensile.TabIndex = 7
        '
        'lblFiltroParolaDettagliato
        '
        Me.lblFiltroParolaDettagliato.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblFiltroParolaDettagliato.AutoSize = True
        Me.lblFiltroParolaDettagliato.ForeColor = System.Drawing.Color.DarkBlue
        Me.lblFiltroParolaDettagliato.Location = New System.Drawing.Point(12, 14)
        Me.lblFiltroParolaDettagliato.Name = "lblFiltroParolaDettagliato"
        Me.lblFiltroParolaDettagliato.Size = New System.Drawing.Size(0, 20)
        Me.lblFiltroParolaDettagliato.TabIndex = 10
        Me.lblFiltroParolaDettagliato.Visible = False
        '
        'Panel_centrale
        '
        Me.Panel_centrale.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.Panel_centrale.Controls.Add(Me.dgvTransazioni)
        Me.Panel_centrale.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel_centrale.Location = New System.Drawing.Point(0, 100)
        Me.Panel_centrale.Name = "Panel_centrale"
        Me.Panel_centrale.Size = New System.Drawing.Size(1428, 624)
        Me.Panel_centrale.TabIndex = 8
        '
        'btnConfiguraStipendi
        '
        Me.btnConfiguraStipendi.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnConfiguraStipendi.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnConfiguraStipendi.Location = New System.Drawing.Point(1050, 10)
        Me.btnConfiguraStipendi.Name = "btnConfiguraStipendi"
        Me.btnConfiguraStipendi.Size = New System.Drawing.Size(160, 35)
        Me.btnConfiguraStipendi.TabIndex = 9
        Me.btnConfiguraStipendi.Text = "Impostazioni"
        Me.btnConfiguraStipendi.UseVisualStyleBackColor = True
        '
        'FormFinanza
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1428, 794)
        Me.Controls.Add(Me.btnConfiguraStipendi)
        Me.Controls.Add(Me.Panel_centrale)
        Me.Controls.Add(Me.panelInfoMensile)
        Me.Controls.Add(Me.btnGestionePattern)
        Me.Controls.Add(Me.btnImportaExcel)
        Me.Controls.Add(Me.btnClassifica)
        Me.Controls.Add(Me.btnAggiungi)
        Me.Controls.Add(Me.panelInserimento)
        Me.Name = "FormFinanza"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Gestione Finanze Personali"
        Me.panelInserimento.ResumeLayout(False)
        Me.panelInserimento.PerformLayout()
        CType(Me.dgvTransazioni, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panelInfoMensile.ResumeLayout(False)
        Me.panelInfoMensile.PerformLayout()
        Me.Panel_centrale.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents panelInserimento As Panel
    Friend WithEvents lblData As Label
    Friend WithEvents txtDescrizione As TextBox
    Friend WithEvents lblDescrizione As Label
    Friend WithEvents txtImporto As TextBox
    Friend WithEvents lblImporto As Label
    Friend WithEvents dtpData As DateTimePicker
    Friend WithEvents btnAggiungi As Button
    Friend WithEvents btnEliminaRiga As Button
    Friend WithEvents btnClassifica As Button
    Friend WithEvents btnImportaExcel As Button
    Friend WithEvents btnGestionePattern As Button
    Friend WithEvents dgvTransazioni As DataGridView
    Friend WithEvents btnAnalisi As Button
    Friend WithEvents panelInfoMensile As Panel
    Friend WithEvents Panel_centrale As Panel
    Friend WithEvents lblFiltroAttivo As Label
    Friend WithEvents btnChiudiDettagli As Button
    Friend WithEvents lblInfoMensile As Label
    Friend WithEvents lblFiltroParolaDettagliato As Label
    Friend WithEvents btnConfiguraStipendi As Button
End Class
