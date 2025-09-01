<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormAnalisi
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
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

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata in Progettazione Windows Form.  
    'Non modificarla mediante l'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage_AnalisiPerAnno = New System.Windows.Forms.TabPage()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.dgvAnalisiAnno = New System.Windows.Forms.DataGridView()
        Me.PeriodoInizio = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PeriodoFine = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Mese = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Anno = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Entrate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Uscite = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Risparmio = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.lblTitolo = New System.Windows.Forms.Label()
        Me.lblSottotitolo = New System.Windows.Forms.Label()
        Me.btnGrafici = New System.Windows.Forms.Button()
        Me.cmbAnnoAnalisi = New System.Windows.Forms.ComboBox()
        Me.btnAggiornaAnalisiAnno = New System.Windows.Forms.Button()
        Me.lblRiepilogoAnno = New System.Windows.Forms.Label()
        Me.TabPage_AnalisiCategorie = New System.Windows.Forms.TabPage()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.dgvCategorie = New System.Windows.Forms.DataGridView()
        Me.Periodo = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Macrocategoria = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Categoria = New System.Windows.Forms.DataGridViewTextBoxColumn()
        'Me.Sottocategoria = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.EntrateCategoria = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.UsciteCategoria = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.RisparmioCategoria = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.tabGraficiCategorie = New System.Windows.Forms.TabControl()
        Me.Tab_Barre = New System.Windows.Forms.TabPage()
        Me.cartesianChartCategorieBarre = New LiveChartsCore.SkiaSharpView.WinForms.CartesianChart()
        Me.Tab_Torta = New System.Windows.Forms.TabPage()
        Me.pieChartCategorieTorta = New LiveChartsCore.SkiaSharpView.WinForms.PieChart()
        Me.Tab_Trend = New System.Windows.Forms.TabPage()
        Me.cartesianChartCategorieTrend = New LiveChartsCore.SkiaSharpView.WinForms.CartesianChart()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnAggiornaCategorie = New System.Windows.Forms.Button()
        'Me.cmbSottoCategoria = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmbCategoria = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbMacroCategoria = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbMeseCategoriee = New System.Windows.Forms.ComboBox()
        Me.cmbMeseCategorie = New System.Windows.Forms.Label()
        Me.cmbAnnoCategorie = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TabPage_ComparaAnni = New System.Windows.Forms.TabPage()
        Me.TabControl1.SuspendLayout()
        Me.TabPage_AnalisiPerAnno.SuspendLayout()
        Me.Panel3.SuspendLayout()
        CType(Me.dgvAnalisiAnno, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel2.SuspendLayout()
        Me.TabPage_AnalisiCategorie.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        CType(Me.dgvCategorie, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabGraficiCategorie.SuspendLayout()
        Me.Tab_Barre.SuspendLayout()
        Me.Tab_Torta.SuspendLayout()
        Me.Tab_Trend.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage_AnalisiPerAnno)
        Me.TabControl1.Controls.Add(Me.TabPage_AnalisiCategorie)
        Me.TabControl1.Controls.Add(Me.TabPage_ComparaAnni)
        Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControl1.Location = New System.Drawing.Point(0, 0)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(1578, 744)
        Me.TabControl1.TabIndex = 0
        '
        'TabPage_AnalisiPerAnno
        '
        Me.TabPage_AnalisiPerAnno.Controls.Add(Me.Panel3)
        Me.TabPage_AnalisiPerAnno.Controls.Add(Me.Panel2)
        Me.TabPage_AnalisiPerAnno.Controls.Add(Me.lblRiepilogoAnno)
        Me.TabPage_AnalisiPerAnno.Location = New System.Drawing.Point(4, 29)
        Me.TabPage_AnalisiPerAnno.Name = "TabPage_AnalisiPerAnno"
        Me.TabPage_AnalisiPerAnno.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage_AnalisiPerAnno.Size = New System.Drawing.Size(1570, 711)
        Me.TabPage_AnalisiPerAnno.TabIndex = 0
        Me.TabPage_AnalisiPerAnno.Text = "Analisi per ANNO"
        Me.TabPage_AnalisiPerAnno.UseVisualStyleBackColor = True
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.dgvAnalisiAnno)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel3.Location = New System.Drawing.Point(3, 103)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(1564, 565)
        Me.Panel3.TabIndex = 8
        '
        'dgvAnalisiAnno
        '
        Me.dgvAnalisiAnno.AllowUserToAddRows = False
        Me.dgvAnalisiAnno.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvAnalisiAnno.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvAnalisiAnno.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.PeriodoInizio, Me.PeriodoFine, Me.Mese, Me.Anno, Me.Entrate, Me.Uscite, Me.Risparmio})
        Me.dgvAnalisiAnno.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvAnalisiAnno.Location = New System.Drawing.Point(0, 0)
        Me.dgvAnalisiAnno.Name = "dgvAnalisiAnno"
        Me.dgvAnalisiAnno.ReadOnly = True
        Me.dgvAnalisiAnno.RowHeadersWidth = 62
        Me.dgvAnalisiAnno.RowTemplate.Height = 28
        Me.dgvAnalisiAnno.Size = New System.Drawing.Size(1564, 565)
        Me.dgvAnalisiAnno.TabIndex = 4
        '
        'PeriodoInizio
        '
        Me.PeriodoInizio.HeaderText = "Periodo Inizio"
        Me.PeriodoInizio.MinimumWidth = 8
        Me.PeriodoInizio.Name = "PeriodoInizio"
        Me.PeriodoInizio.ReadOnly = True
        '
        'PeriodoFine
        '
        Me.PeriodoFine.HeaderText = "Periodo Fine"
        Me.PeriodoFine.MinimumWidth = 8
        Me.PeriodoFine.Name = "PeriodoFine"
        Me.PeriodoFine.ReadOnly = True
        '
        'Mese
        '
        Me.Mese.HeaderText = "Mese"
        Me.Mese.MinimumWidth = 8
        Me.Mese.Name = "Mese"
        Me.Mese.ReadOnly = True
        '
        'Anno
        '
        Me.Anno.HeaderText = "Anno"
        Me.Anno.MinimumWidth = 8
        Me.Anno.Name = "Anno"
        Me.Anno.ReadOnly = True
        '
        'Entrate
        '
        DataGridViewCellStyle4.Format = "N2"
        DataGridViewCellStyle4.NullValue = Nothing
        Me.Entrate.DefaultCellStyle = DataGridViewCellStyle4
        Me.Entrate.HeaderText = "Entrate"
        Me.Entrate.MinimumWidth = 8
        Me.Entrate.Name = "Entrate"
        Me.Entrate.ReadOnly = True
        '
        'Uscite
        '
        DataGridViewCellStyle5.Format = "C2"
        DataGridViewCellStyle5.NullValue = Nothing
        Me.Uscite.DefaultCellStyle = DataGridViewCellStyle5
        Me.Uscite.HeaderText = "Uscite"
        Me.Uscite.MinimumWidth = 8
        Me.Uscite.Name = "Uscite"
        Me.Uscite.ReadOnly = True
        '
        'Risparmio
        '
        DataGridViewCellStyle6.Format = "N2"
        DataGridViewCellStyle6.NullValue = Nothing
        Me.Risparmio.DefaultCellStyle = DataGridViewCellStyle6
        Me.Risparmio.HeaderText = "Risparmio"
        Me.Risparmio.MinimumWidth = 8
        Me.Risparmio.Name = "Risparmio"
        Me.Risparmio.ReadOnly = True
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.lblTitolo)
        Me.Panel2.Controls.Add(Me.lblSottotitolo)
        Me.Panel2.Controls.Add(Me.btnGrafici)
        Me.Panel2.Controls.Add(Me.cmbAnnoAnalisi)
        Me.Panel2.Controls.Add(Me.btnAggiornaAnalisiAnno)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel2.Location = New System.Drawing.Point(3, 3)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(1564, 100)
        Me.Panel2.TabIndex = 7
        '
        'lblTitolo
        '
        Me.lblTitolo.AutoSize = True
        Me.lblTitolo.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.lblTitolo.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitolo.Location = New System.Drawing.Point(5, 0)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(384, 29)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "ANALISI FINANZIARIA PER ANNO"
        Me.lblTitolo.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'lblSottotitolo
        '
        Me.lblSottotitolo.AutoSize = True
        Me.lblSottotitolo.Location = New System.Drawing.Point(6, 29)
        Me.lblSottotitolo.Name = "lblSottotitolo"
        Me.lblSottotitolo.Size = New System.Drawing.Size(461, 20)
        Me.lblSottotitolo.TabIndex = 1
        Me.lblSottotitolo.Text = "Seleziona un anno per visualizzare dati filtrati e grafici automatici"
        Me.lblSottotitolo.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'btnGrafici
        '
        Me.btnGrafici.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnGrafici.Location = New System.Drawing.Point(1286, 12)
        Me.btnGrafici.Name = "btnGrafici"
        Me.btnGrafici.Size = New System.Drawing.Size(152, 55)
        Me.btnGrafici.TabIndex = 5
        Me.btnGrafici.Text = "Visualizza Grafici"
        Me.btnGrafici.UseVisualStyleBackColor = True
        '
        'cmbAnnoAnalisi
        '
        Me.cmbAnnoAnalisi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbAnnoAnalisi.FormattingEnabled = True
        Me.cmbAnnoAnalisi.Location = New System.Drawing.Point(5, 60)
        Me.cmbAnnoAnalisi.Name = "cmbAnnoAnalisi"
        Me.cmbAnnoAnalisi.Size = New System.Drawing.Size(121, 28)
        Me.cmbAnnoAnalisi.TabIndex = 2
        '
        'btnAggiornaAnalisiAnno
        '
        Me.btnAggiornaAnalisiAnno.Location = New System.Drawing.Point(135, 60)
        Me.btnAggiornaAnalisiAnno.Name = "btnAggiornaAnalisiAnno"
        Me.btnAggiornaAnalisiAnno.Size = New System.Drawing.Size(100, 30)
        Me.btnAggiornaAnalisiAnno.TabIndex = 3
        Me.btnAggiornaAnalisiAnno.Text = "Aggiorna"
        Me.btnAggiornaAnalisiAnno.UseVisualStyleBackColor = True
        '
        'lblRiepilogoAnno
        '
        Me.lblRiepilogoAnno.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblRiepilogoAnno.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.lblRiepilogoAnno.Location = New System.Drawing.Point(3, 668)
        Me.lblRiepilogoAnno.Name = "lblRiepilogoAnno"
        Me.lblRiepilogoAnno.Size = New System.Drawing.Size(1564, 40)
        Me.lblRiepilogoAnno.TabIndex = 6
        Me.lblRiepilogoAnno.Text = "Label1"
        Me.lblRiepilogoAnno.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TabPage_AnalisiCategorie
        '
        Me.TabPage_AnalisiCategorie.Controls.Add(Me.SplitContainer1)
        Me.TabPage_AnalisiCategorie.Controls.Add(Me.Panel1)
        Me.TabPage_AnalisiCategorie.Location = New System.Drawing.Point(4, 29)
        Me.TabPage_AnalisiCategorie.Name = "TabPage_AnalisiCategorie"
        Me.TabPage_AnalisiCategorie.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage_AnalisiCategorie.Size = New System.Drawing.Size(1570, 711)
        Me.TabPage_AnalisiCategorie.TabIndex = 1
        Me.TabPage_AnalisiCategorie.Text = "Analisi Categorie"
        Me.TabPage_AnalisiCategorie.UseVisualStyleBackColor = True
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(3, 63)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.dgvCategorie)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.tabGraficiCategorie)
        Me.SplitContainer1.Size = New System.Drawing.Size(1564, 645)
        Me.SplitContainer1.SplitterDistance = 647
        Me.SplitContainer1.TabIndex = 1
        '
        'dgvCategorie
        '
        Me.dgvCategorie.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvCategorie.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvCategorie.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Periodo, Me.Macrocategoria, Me.Categoria, Me.EntrateCategoria, Me.UsciteCategoria, Me.RisparmioCategoria})
        Me.dgvCategorie.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvCategorie.Location = New System.Drawing.Point(0, 0)
        Me.dgvCategorie.Name = "dgvCategorie"
        Me.dgvCategorie.RowHeadersWidth = 62
        Me.dgvCategorie.RowTemplate.Height = 28
        Me.dgvCategorie.Size = New System.Drawing.Size(647, 645)
        Me.dgvCategorie.TabIndex = 0
        '
        'Periodo
        '
        Me.Periodo.HeaderText = "Periodo"
        Me.Periodo.MinimumWidth = 8
        Me.Periodo.Name = "Periodo"
        '
        'Macrocategoria
        '
        Me.Macrocategoria.HeaderText = "Macrocategoria"
        Me.Macrocategoria.MinimumWidth = 8
        Me.Macrocategoria.Name = "Macrocategoria"
        '
        'Categoria
        '
        Me.Categoria.HeaderText = "Categoria"
        Me.Categoria.MinimumWidth = 8
        Me.Categoria.Name = "Categoria"
        '
        'EntrateCategoria
        '
        Me.EntrateCategoria.HeaderText = "Entrate"
        Me.EntrateCategoria.MinimumWidth = 8
        Me.EntrateCategoria.Name = "EntrateCategoria"
        '
        'UsciteCategoria
        '
        Me.UsciteCategoria.HeaderText = "Uscite"
        Me.UsciteCategoria.MinimumWidth = 8
        Me.UsciteCategoria.Name = "UsciteCategoria"
        '
        'RisparmioCategoria
        '
        Me.RisparmioCategoria.HeaderText = "Risparmio"
        Me.RisparmioCategoria.MinimumWidth = 8
        Me.RisparmioCategoria.Name = "RisparmioCategoria"
        '
        'tabGraficiCategorie
        '
        Me.tabGraficiCategorie.Controls.Add(Me.Tab_Barre)
        Me.tabGraficiCategorie.Controls.Add(Me.Tab_Torta)
        Me.tabGraficiCategorie.Controls.Add(Me.Tab_Trend)
        Me.tabGraficiCategorie.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tabGraficiCategorie.Location = New System.Drawing.Point(0, 0)
        Me.tabGraficiCategorie.Name = "tabGraficiCategorie"
        Me.tabGraficiCategorie.SelectedIndex = 0
        Me.tabGraficiCategorie.Size = New System.Drawing.Size(913, 645)
        Me.tabGraficiCategorie.TabIndex = 0
        '
        'Tab_Barre
        '
        Me.Tab_Barre.Controls.Add(Me.cartesianChartCategorieBarre)
        Me.Tab_Barre.Location = New System.Drawing.Point(4, 29)
        Me.Tab_Barre.Name = "Tab_Barre"
        Me.Tab_Barre.Padding = New System.Windows.Forms.Padding(3)
        Me.Tab_Barre.Size = New System.Drawing.Size(905, 612)
        Me.Tab_Barre.TabIndex = 0
        Me.Tab_Barre.Text = "Barre"
        Me.Tab_Barre.UseVisualStyleBackColor = True
        '
        'cartesianChartCategorieBarre
        '
        Me.cartesianChartCategorieBarre.Dock = System.Windows.Forms.DockStyle.Fill
        Me.cartesianChartCategorieBarre.Location = New System.Drawing.Point(3, 3)
        Me.cartesianChartCategorieBarre.MatchAxesScreenDataRatio = False
        Me.cartesianChartCategorieBarre.Name = "cartesianChartCategorieBarre"
        Me.cartesianChartCategorieBarre.Size = New System.Drawing.Size(899, 606)
        Me.cartesianChartCategorieBarre.TabIndex = 0
        '
        'Tab_Torta
        '
        Me.Tab_Torta.Controls.Add(Me.pieChartCategorieTorta)
        Me.Tab_Torta.Location = New System.Drawing.Point(4, 29)
        Me.Tab_Torta.Name = "Tab_Torta"
        Me.Tab_Torta.Padding = New System.Windows.Forms.Padding(3)
        Me.Tab_Torta.Size = New System.Drawing.Size(827, 612)
        Me.Tab_Torta.TabIndex = 1
        Me.Tab_Torta.Text = "Torta"
        Me.Tab_Torta.UseVisualStyleBackColor = True
        '
        'pieChartCategorieTorta
        '
        Me.pieChartCategorieTorta.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pieChartCategorieTorta.InitialRotation = 0R
        Me.pieChartCategorieTorta.IsClockwise = True
        Me.pieChartCategorieTorta.Location = New System.Drawing.Point(3, 3)
        Me.pieChartCategorieTorta.MaxAngle = 360.0R
        Me.pieChartCategorieTorta.MaxValue = Double.NaN
        Me.pieChartCategorieTorta.MinValue = 0R
        Me.pieChartCategorieTorta.Name = "pieChartCategorieTorta"
        Me.pieChartCategorieTorta.Size = New System.Drawing.Size(821, 606)
        Me.pieChartCategorieTorta.TabIndex = 0
        '
        'Tab_Trend
        '
        Me.Tab_Trend.Controls.Add(Me.cartesianChartCategorieTrend)
        Me.Tab_Trend.Location = New System.Drawing.Point(4, 29)
        Me.Tab_Trend.Name = "Tab_Trend"
        Me.Tab_Trend.Padding = New System.Windows.Forms.Padding(3)
        Me.Tab_Trend.Size = New System.Drawing.Size(827, 612)
        Me.Tab_Trend.TabIndex = 2
        Me.Tab_Trend.Text = "Trend"
        Me.Tab_Trend.UseVisualStyleBackColor = True
        '
        'cartesianChartCategorieTrend
        '
        Me.cartesianChartCategorieTrend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.cartesianChartCategorieTrend.Location = New System.Drawing.Point(3, 3)
        Me.cartesianChartCategorieTrend.MatchAxesScreenDataRatio = False
        Me.cartesianChartCategorieTrend.Name = "cartesianChartCategorieTrend"
        Me.cartesianChartCategorieTrend.Size = New System.Drawing.Size(821, 606)
        Me.cartesianChartCategorieTrend.TabIndex = 0
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.btnAggiornaCategorie)
        Me.Panel1.Controls.Add(Me.Label3)
        Me.Panel1.Controls.Add(Me.cmbCategoria)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.cmbMacroCategoria)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.cmbMeseCategoriee)
        Me.Panel1.Controls.Add(Me.cmbMeseCategorie)
        Me.Panel1.Controls.Add(Me.cmbAnnoCategorie)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(3, 3)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1564, 60)
        Me.Panel1.TabIndex = 0
        '
        'btnAggiornaCategorie
        '
        Me.btnAggiornaCategorie.Location = New System.Drawing.Point(1116, 10)
        Me.btnAggiornaCategorie.Name = "btnAggiornaCategorie"
        Me.btnAggiornaCategorie.Size = New System.Drawing.Size(137, 35)
        Me.btnAggiornaCategorie.TabIndex = 10
        Me.btnAggiornaCategorie.Text = "Aggiorna"
        Me.btnAggiornaCategorie.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(851, 10)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(114, 20)
        Me.Label3.TabIndex = 8
        'Me.Label3.Text = "Sottocategoria"
        '
        'cmbCategoria
        '
        Me.cmbCategoria.FormattingEnabled = True
        Me.cmbCategoria.Location = New System.Drawing.Point(720, 10)
        Me.cmbCategoria.Name = "cmbCategoria"
        Me.cmbCategoria.Size = New System.Drawing.Size(121, 28)
        Me.cmbCategoria.TabIndex = 7
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(632, 10)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(78, 20)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "Categoria"
        '
        'cmbMacroCategoria
        '
        Me.cmbMacroCategoria.FormattingEnabled = True
        Me.cmbMacroCategoria.Location = New System.Drawing.Point(501, 10)
        Me.cmbMacroCategoria.Name = "cmbMacroCategoria"
        Me.cmbMacroCategoria.Size = New System.Drawing.Size(121, 28)
        Me.cmbMacroCategoria.TabIndex = 5
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(372, 10)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(119, 20)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Macrocategoria"
        '
        'cmbMeseCategoriee
        '
        Me.cmbMeseCategoriee.FormattingEnabled = True
        Me.cmbMeseCategoriee.Location = New System.Drawing.Point(241, 10)
        Me.cmbMeseCategoriee.Name = "cmbMeseCategoriee"
        Me.cmbMeseCategoriee.Size = New System.Drawing.Size(121, 28)
        Me.cmbMeseCategoriee.TabIndex = 3
        '
        'cmbMeseCategorie
        '
        Me.cmbMeseCategorie.AutoSize = True
        Me.cmbMeseCategorie.Location = New System.Drawing.Point(193, 10)
        Me.cmbMeseCategorie.Name = "cmbMeseCategorie"
        Me.cmbMeseCategorie.Size = New System.Drawing.Size(48, 20)
        Me.cmbMeseCategorie.TabIndex = 2
        Me.cmbMeseCategorie.Text = "Mese"
        Me.cmbMeseCategorie.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'cmbAnnoCategorie
        '
        Me.cmbAnnoCategorie.FormattingEnabled = True
        Me.cmbAnnoCategorie.Location = New System.Drawing.Point(62, 10)
        Me.cmbAnnoCategorie.Name = "cmbAnnoCategorie"
        Me.cmbAnnoCategorie.Size = New System.Drawing.Size(121, 28)
        Me.cmbAnnoCategorie.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(10, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(47, 20)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Anno"
        '
        'TabPage_ComparaAnni
        '
        Me.TabPage_ComparaAnni.Location = New System.Drawing.Point(4, 29)
        Me.TabPage_ComparaAnni.Name = "TabPage_ComparaAnni"
        Me.TabPage_ComparaAnni.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage_ComparaAnni.Size = New System.Drawing.Size(1437, 711)
        Me.TabPage_ComparaAnni.TabIndex = 2
        Me.TabPage_ComparaAnni.Text = "Compara Anni"
        Me.TabPage_ComparaAnni.UseVisualStyleBackColor = True
        '
        'FormAnalisi
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1578, 744)
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "FormAnalisi"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Analisi Finanziaria"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage_AnalisiPerAnno.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        CType(Me.dgvAnalisiAnno, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.TabPage_AnalisiCategorie.ResumeLayout(False)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        CType(Me.dgvCategorie, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabGraficiCategorie.ResumeLayout(False)
        Me.Tab_Barre.ResumeLayout(False)
        Me.Tab_Torta.ResumeLayout(False)
        Me.Tab_Trend.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage_AnalisiPerAnno As TabPage
    Friend WithEvents TabPage_AnalisiCategorie As TabPage
    Friend WithEvents TabPage_ComparaAnni As TabPage
    Friend WithEvents lblTitolo As Label
    Friend WithEvents lblSottotitolo As Label
    Friend WithEvents cmbAnnoAnalisi As ComboBox
    Friend WithEvents btnAggiornaAnalisiAnno As Button
    Friend WithEvents dgvAnalisiAnno As DataGridView
    Friend WithEvents PeriodoInizio As DataGridViewTextBoxColumn
    Friend WithEvents PeriodoFine As DataGridViewTextBoxColumn
    Friend WithEvents Mese As DataGridViewTextBoxColumn
    Friend WithEvents Anno As DataGridViewTextBoxColumn
    Friend WithEvents Entrate As DataGridViewTextBoxColumn
    Friend WithEvents Uscite As DataGridViewTextBoxColumn
    Friend WithEvents Risparmio As DataGridViewTextBoxColumn
    Friend WithEvents btnGrafici As Button
    Friend WithEvents lblRiepilogoAnno As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Label3 As Label
    Friend WithEvents cmbCategoria As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents cmbMacroCategoria As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents cmbMeseCategoriee As ComboBox
    Friend WithEvents cmbMeseCategorie As Label
    Friend WithEvents cmbAnnoCategorie As ComboBox
    Friend WithEvents Label1 As Label
    Friend WithEvents btnAggiornaCategorie As Button
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents dgvCategorie As DataGridView
    Friend WithEvents Periodo As DataGridViewTextBoxColumn
    Friend WithEvents Macrocategoria As DataGridViewTextBoxColumn
    Friend WithEvents Categoria As DataGridViewTextBoxColumn
    'Friend WithEvents Sottocategoria As DataGridViewTextBoxColumn
    Friend WithEvents EntrateCategoria As DataGridViewTextBoxColumn
    Friend WithEvents UsciteCategoria As DataGridViewTextBoxColumn
    Friend WithEvents RisparmioCategoria As DataGridViewTextBoxColumn
    Friend WithEvents tabGraficiCategorie As TabControl
    Friend WithEvents Tab_Barre As TabPage
    Friend WithEvents cartesianChartCategorieBarre As LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
    Friend WithEvents Tab_Torta As TabPage
    Friend WithEvents pieChartCategorieTorta As LiveChartsCore.SkiaSharpView.WinForms.PieChart
    Friend WithEvents Tab_Trend As TabPage
    Friend WithEvents cartesianChartCategorieTrend As LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
    Friend WithEvents Panel3 As Panel
    Friend WithEvents Panel2 As Panel
End Class
