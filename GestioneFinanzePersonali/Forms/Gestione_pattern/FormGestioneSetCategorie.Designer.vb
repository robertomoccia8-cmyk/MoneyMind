<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormGestioneSetCategorie
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
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.pnlLeft = New System.Windows.Forms.Panel()
        Me.treeViewSet = New System.Windows.Forms.TreeView()
        Me.pnlLeftButtons = New System.Windows.Forms.Panel()
        Me.btnStrumentiPulizia = New System.Windows.Forms.Button()
        Me.btnTrovaDuplicati = New System.Windows.Forms.Button()
        Me.btnNuovoSet = New System.Windows.Forms.Button()
        Me.btnResetFiltro = New System.Windows.Forms.Button()
        Me.btnRicerca = New System.Windows.Forms.Button()
        Me.txtRicerca = New System.Windows.Forms.TextBox()
        Me.lblTitolo = New System.Windows.Forms.Label()
        Me.tabControlDettagli = New System.Windows.Forms.TabControl()
        Me.tabDettaglioSet = New System.Windows.Forms.TabPage()
        Me.tabStatistiche = New System.Windows.Forms.TabPage()
        Me.tabAnalisiParole = New System.Windows.Forms.TabPage()
        Me.tabExportImport = New System.Windows.Forms.TabPage()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.pnlLeft.SuspendLayout()
        Me.pnlLeftButtons.SuspendLayout()
        Me.tabControlDettagli.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitMain
        '
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.Location = New System.Drawing.Point(0, 0)
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.pnlLeft)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.tabControlDettagli)
        Me.splitMain.Size = New System.Drawing.Size(1575, 800)
        Me.splitMain.SplitterDistance = 450
        Me.splitMain.TabIndex = 0
        '
        'pnlLeft
        '
        Me.pnlLeft.Controls.Add(Me.treeViewSet)
        Me.pnlLeft.Controls.Add(Me.pnlLeftButtons)
        Me.pnlLeft.Controls.Add(Me.btnResetFiltro)
        Me.pnlLeft.Controls.Add(Me.btnRicerca)
        Me.pnlLeft.Controls.Add(Me.txtRicerca)
        Me.pnlLeft.Controls.Add(Me.lblTitolo)
        Me.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlLeft.Location = New System.Drawing.Point(0, 0)
        Me.pnlLeft.Name = "pnlLeft"
        Me.pnlLeft.Size = New System.Drawing.Size(450, 800)
        Me.pnlLeft.TabIndex = 0
        '
        'treeViewSet
        '
        Me.treeViewSet.Dock = System.Windows.Forms.DockStyle.Fill
        Me.treeViewSet.Indent = 20
        Me.treeViewSet.ItemHeight = 20
        Me.treeViewSet.Location = New System.Drawing.Point(0, 126)
        Me.treeViewSet.Name = "treeViewSet"
        Me.treeViewSet.ShowNodeToolTips = True
        Me.treeViewSet.Size = New System.Drawing.Size(450, 594)
        Me.treeViewSet.TabIndex = 4
        '
        'pnlLeftButtons
        '
        Me.pnlLeftButtons.Controls.Add(Me.btnStrumentiPulizia)
        Me.pnlLeftButtons.Controls.Add(Me.btnTrovaDuplicati)
        Me.pnlLeftButtons.Controls.Add(Me.btnNuovoSet)
        Me.pnlLeftButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlLeftButtons.Location = New System.Drawing.Point(0, 720)
        Me.pnlLeftButtons.Name = "pnlLeftButtons"
        Me.pnlLeftButtons.Size = New System.Drawing.Size(450, 80)
        Me.pnlLeftButtons.TabIndex = 4
        '
        'btnStrumentiPulizia
        '
        Me.btnStrumentiPulizia.BackColor = System.Drawing.Color.FromArgb(CType(CType(244, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(54, Byte), Integer))
        Me.btnStrumentiPulizia.ForeColor = System.Drawing.Color.White
        Me.btnStrumentiPulizia.Location = New System.Drawing.Point(304, 10)
        Me.btnStrumentiPulizia.Name = "btnStrumentiPulizia"
        Me.btnStrumentiPulizia.Size = New System.Drawing.Size(135, 30)
        Me.btnStrumentiPulizia.TabIndex = 2
        Me.btnStrumentiPulizia.Text = "Pulizia"
        Me.btnStrumentiPulizia.UseVisualStyleBackColor = False
        '
        'btnTrovaDuplicati
        '
        Me.btnTrovaDuplicati.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnTrovaDuplicati.ForeColor = System.Drawing.Color.White
        Me.btnTrovaDuplicati.Location = New System.Drawing.Point(158, 10)
        Me.btnTrovaDuplicati.Name = "btnTrovaDuplicati"
        Me.btnTrovaDuplicati.Size = New System.Drawing.Size(135, 30)
        Me.btnTrovaDuplicati.TabIndex = 1
        Me.btnTrovaDuplicati.Text = "Duplicati"
        Me.btnTrovaDuplicati.UseVisualStyleBackColor = False
        '
        'btnNuovoSet
        '
        Me.btnNuovoSet.BackColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(125, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.btnNuovoSet.ForeColor = System.Drawing.Color.White
        Me.btnNuovoSet.Location = New System.Drawing.Point(11, 10)
        Me.btnNuovoSet.Name = "btnNuovoSet"
        Me.btnNuovoSet.Size = New System.Drawing.Size(135, 30)
        Me.btnNuovoSet.TabIndex = 0
        Me.btnNuovoSet.Text = "➕ Nuovo Set"
        Me.btnNuovoSet.UseVisualStyleBackColor = False
        '
        'btnResetFiltro
        '
        Me.btnResetFiltro.Dock = System.Windows.Forms.DockStyle.Top
        Me.btnResetFiltro.Location = New System.Drawing.Point(0, 96)
        Me.btnResetFiltro.Name = "btnResetFiltro"
        Me.btnResetFiltro.Size = New System.Drawing.Size(450, 30)
        Me.btnResetFiltro.TabIndex = 3
        Me.btnResetFiltro.Text = "Resetta Filtro"
        Me.btnResetFiltro.UseVisualStyleBackColor = True
        '
        'btnRicerca
        '
        Me.btnRicerca.Dock = System.Windows.Forms.DockStyle.Top
        Me.btnRicerca.Location = New System.Drawing.Point(0, 66)
        Me.btnRicerca.Name = "btnRicerca"
        Me.btnRicerca.Size = New System.Drawing.Size(450, 30)
        Me.btnRicerca.TabIndex = 2
        Me.btnRicerca.Text = "Ricerca"
        Me.btnRicerca.UseVisualStyleBackColor = True
        '
        'txtRicerca
        '
        Me.txtRicerca.Dock = System.Windows.Forms.DockStyle.Top
        Me.txtRicerca.Location = New System.Drawing.Point(0, 40)
        Me.txtRicerca.Name = "txtRicerca"
        Me.txtRicerca.Size = New System.Drawing.Size(450, 26)
        Me.txtRicerca.TabIndex = 1
        '
        'lblTitolo
        '
        Me.lblTitolo.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTitolo.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitolo.Location = New System.Drawing.Point(0, 0)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(450, 40)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Gestione Set Categorie"
        Me.lblTitolo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'tabControlDettagli
        '
        Me.tabControlDettagli.Controls.Add(Me.tabDettaglioSet)
        Me.tabControlDettagli.Controls.Add(Me.tabStatistiche)
        Me.tabControlDettagli.Controls.Add(Me.tabAnalisiParole)
        Me.tabControlDettagli.Controls.Add(Me.tabExportImport)
        Me.tabControlDettagli.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tabControlDettagli.Location = New System.Drawing.Point(0, 0)
        Me.tabControlDettagli.Name = "tabControlDettagli"
        Me.tabControlDettagli.SelectedIndex = 0
        Me.tabControlDettagli.Size = New System.Drawing.Size(1121, 800)
        Me.tabControlDettagli.TabIndex = 0
        '
        'tabDettaglioSet
        '
        Me.tabDettaglioSet.Location = New System.Drawing.Point(4, 29)
        Me.tabDettaglioSet.Name = "tabDettaglioSet"
        Me.tabDettaglioSet.Padding = New System.Windows.Forms.Padding(3)
        Me.tabDettaglioSet.Size = New System.Drawing.Size(1113, 767)
        Me.tabDettaglioSet.TabIndex = 0
        Me.tabDettaglioSet.Text = "Dettaglio Set"
        Me.tabDettaglioSet.UseVisualStyleBackColor = True
        '
        'tabStatistiche
        '
        Me.tabStatistiche.Location = New System.Drawing.Point(4, 29)
        Me.tabStatistiche.Name = "tabStatistiche"
        Me.tabStatistiche.Padding = New System.Windows.Forms.Padding(3)
        Me.tabStatistiche.Size = New System.Drawing.Size(1112, 767)
        Me.tabStatistiche.TabIndex = 1
        Me.tabStatistiche.Text = "Statistiche"
        Me.tabStatistiche.UseVisualStyleBackColor = True
        '
        'tabAnalisiParole
        '
        Me.tabAnalisiParole.Location = New System.Drawing.Point(4, 29)
        Me.tabAnalisiParole.Name = "tabAnalisiParole"
        Me.tabAnalisiParole.Padding = New System.Windows.Forms.Padding(3)
        Me.tabAnalisiParole.Size = New System.Drawing.Size(1112, 767)
        Me.tabAnalisiParole.TabIndex = 2
        Me.tabAnalisiParole.Text = "Analisi Parole"
        Me.tabAnalisiParole.UseVisualStyleBackColor = True
        '
        'tabExportImport
        '
        Me.tabExportImport.Location = New System.Drawing.Point(4, 29)
        Me.tabExportImport.Name = "tabExportImport"
        Me.tabExportImport.Padding = New System.Windows.Forms.Padding(3)
        Me.tabExportImport.Size = New System.Drawing.Size(1112, 767)
        Me.tabExportImport.TabIndex = 3
        Me.tabExportImport.Text = "Export/Import"
        Me.tabExportImport.UseVisualStyleBackColor = True
        '
        'FormGestioneSetCategorie
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1575, 800)
        Me.Controls.Add(Me.splitMain)
        Me.Name = "FormGestioneSetCategorie"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Dashboard Gestione Set Categorie"
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.pnlLeft.ResumeLayout(False)
        Me.pnlLeft.PerformLayout()
        Me.pnlLeftButtons.ResumeLayout(False)
        Me.tabControlDettagli.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents splitMain As SplitContainer
    Friend WithEvents pnlLeft As Panel
    Friend WithEvents lblTitolo As Label
    Friend WithEvents txtRicerca As TextBox
    Friend WithEvents btnRicerca As Button
    Friend WithEvents treeViewSet As TreeView
    Friend WithEvents pnlLeftButtons As Panel
    Friend WithEvents btnNuovoSet As Button
    Friend WithEvents btnTrovaDuplicati As Button
    Friend WithEvents btnStrumentiPulizia As Button
    Friend WithEvents tabControlDettagli As TabControl
    Friend WithEvents tabDettaglioSet As TabPage
    Friend WithEvents tabStatistiche As TabPage
    Friend WithEvents tabAnalisiParole As TabPage
    Friend WithEvents tabExportImport As TabPage
    Friend WithEvents btnResetFiltro As Button
End Class
