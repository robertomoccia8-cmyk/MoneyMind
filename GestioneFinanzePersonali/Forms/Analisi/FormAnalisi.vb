Imports System.Data.SQLite
Imports System.Globalization
Imports LiveChartsCore
Imports LiveChartsCore.SkiaSharpView
Imports LiveChartsCore.SkiaSharpView.WinForms

Public Class FormAnalisi

    Private Const GIORNO_PERIODO As Integer = 23

    ' === CONTROLLI TERZA TAB CONFRONTO ANNI ===
    Private cmbAnnoA As ComboBox
    Private cmbAnnoB As ComboBox
    Private btnConfronta As Button
    Private dgvConfrontoAnni As DataGridView
    Private cartesianChartConfronto As CartesianChart
    Private lblRiepilogoConfronto As Label
    Private chkPeriodoCustom As CheckBox
    Private dtpInizioA As DateTimePicker
    Private dtpFineA As DateTimePicker
    Private dtpInizioB As DateTimePicker
    Private dtpFineB As DateTimePicker



    ' === CARICAMENTO FORM ===
    Private Sub FormAnalisi_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CaricaAnniDisponibili()
        CaricaAnniCategorie()
        CaricaMacroCategorie()
        CaricaCategoriePrincipali()
        CaricaMesiCategorie()

        ' Inizializza terza tab
        ImpostaTabConfrontoAnni()
        CaricaAnniConfronto()

        If cmbAnnoAnalisi.Items.Count > 0 Then cmbAnnoAnalisi.SelectedIndex = 0
        If cmbAnnoCategorie.Items.Count > 0 Then cmbAnnoCategorie.SelectedIndex = 0
        If cmbMeseCategoriee.Items.Count > 0 Then cmbMeseCategoriee.SelectedIndex = 0
        If cmbMacroCategoria.Items.Count > 0 Then cmbMacroCategoria.SelectedIndex = 0
        If cmbCategoria.Items.Count > 0 Then cmbCategoria.SelectedIndex = 0
        'If cmbSottoCategoria.Items.Count > 0 Then cmbSottoCategoria.SelectedIndex = 0

        PopolaAnalisiPerAnno()
        AggiornaAnalisiCategorie()
    End Sub

    ' === SETUP TERZA TAB CONFRONTO ANNI ===
    Private Sub ImpostaTabConfrontoAnni()
        ' Assumi che il TabControl si chiami TabControl1 e che la terza tab esista già
        Dim tabConfronto As TabPage = TabControl1.TabPages(2) ' Terza tab (indice 2)
        tabConfronto.Text = "Confronto Anni"
        tabConfronto.BackColor = Color.White

        ' PRIMA: SplitContainer per tabella e grafico (occupa tutto lo spazio)
        Dim splitConfronto As New SplitContainer()
        splitConfronto.Dock = DockStyle.Fill
        splitConfronto.Orientation = Orientation.Horizontal
        splitConfronto.SplitterDistance = 350
        tabConfronto.Controls.Add(splitConfronto)

        ' DOPO: Pannello controlli superiori (sovrapposto sopra il SplitContainer) - PIÙ ALTO
        Dim pnlControlli As New Panel()
        pnlControlli.Dock = DockStyle.Top
        pnlControlli.Height = 100  ' Aumentato per la seconda riga
        pnlControlli.Padding = New Padding(10)
        pnlControlli.BackColor = Color.FromArgb(240, 240, 240)
        tabConfronto.Controls.Add(pnlControlli)

        ' === RIGA 1: CONTROLLI ANNI ===
        ' ComboBox Anno A
        Dim lblAnnoA As New Label()
        lblAnnoA.Text = "Anno A:"
        lblAnnoA.Location = New Point(10, 15)
        lblAnnoA.Size = New Size(50, 20)
        pnlControlli.Controls.Add(lblAnnoA)

        cmbAnnoA = New ComboBox()
        cmbAnnoA.DropDownStyle = ComboBoxStyle.DropDownList
        cmbAnnoA.Location = New Point(70, 12)
        cmbAnnoA.Size = New Size(80, 25)
        pnlControlli.Controls.Add(cmbAnnoA)

        ' ComboBox Anno B
        Dim lblAnnoB As New Label()
        lblAnnoB.Text = "Anno B:"
        lblAnnoB.Location = New Point(170, 15)
        lblAnnoB.Size = New Size(50, 20)
        pnlControlli.Controls.Add(lblAnnoB)

        cmbAnnoB = New ComboBox()
        cmbAnnoB.DropDownStyle = ComboBoxStyle.DropDownList
        cmbAnnoB.Location = New Point(230, 12)
        cmbAnnoB.Size = New Size(80, 25)
        pnlControlli.Controls.Add(cmbAnnoB)

        ' Bottone Confronta
        btnConfronta = New Button()
        btnConfronta.Text = "Confronta"
        btnConfronta.Location = New Point(330, 10)
        btnConfronta.Size = New Size(90, 30)
        btnConfronta.BackColor = Color.FromArgb(52, 152, 219)
        btnConfronta.ForeColor = Color.White
        btnConfronta.FlatStyle = FlatStyle.Flat
        AddHandler btnConfronta.Click, AddressOf btnConfronta_Click
        pnlControlli.Controls.Add(btnConfronta)

        ' Bottone Scambia Anni
        Dim btnScambia As New Button()
        btnScambia.Text = "↔"
        btnScambia.Location = New Point(440, 10)
        btnScambia.Size = New Size(30, 30)
        btnScambia.BackColor = Color.FromArgb(149, 165, 166)
        btnScambia.ForeColor = Color.White
        btnScambia.FlatStyle = FlatStyle.Flat
        AddHandler btnScambia.Click, AddressOf btnScambia_Click
        pnlControlli.Controls.Add(btnScambia)

        ' === RIGA 2: CONTROLLI PERIODO PERSONALIZZATO ===
        ' Checkbox per attivare periodo personalizzato
        chkPeriodoCustom = New CheckBox()
        chkPeriodoCustom.Text = "Periodi personalizzati"
        chkPeriodoCustom.Location = New Point(10, 50)
        chkPeriodoCustom.Size = New Size(150, 20)
        AddHandler chkPeriodoCustom.CheckedChanged, AddressOf chkPeriodoCustom_CheckedChanged
        pnlControlli.Controls.Add(chkPeriodoCustom)

        ' Periodo A: Data Inizio
        Dim lblPeriodoA As New Label()
        lblPeriodoA.Text = "A: Da"
        lblPeriodoA.Location = New Point(170, 52)
        lblPeriodoA.Size = New Size(30, 20)
        lblPeriodoA.Enabled = False
        pnlControlli.Controls.Add(lblPeriodoA)

        dtpInizioA = New DateTimePicker()
        dtpInizioA.Location = New Point(205, 50)
        dtpInizioA.Size = New Size(90, 25)
        dtpInizioA.Format = DateTimePickerFormat.Short
        dtpInizioA.Enabled = False
        pnlControlli.Controls.Add(dtpInizioA)

        Dim lblAA As New Label()
        lblAA.Text = "A"
        lblAA.Location = New Point(300, 52)
        lblAA.Size = New Size(10, 20)
        lblAA.Enabled = False
        pnlControlli.Controls.Add(lblAA)

        dtpFineA = New DateTimePicker()
        dtpFineA.Location = New Point(315, 50)
        dtpFineA.Size = New Size(90, 25)
        dtpFineA.Format = DateTimePickerFormat.Short
        dtpFineA.Enabled = False
        pnlControlli.Controls.Add(dtpFineA)

        ' Periodo B: Data Inizio
        Dim lblPeriodoB As New Label()
        lblPeriodoB.Text = "B: Da"
        lblPeriodoB.Location = New Point(415, 52)
        lblPeriodoB.Size = New Size(30, 20)
        lblPeriodoB.Enabled = False
        pnlControlli.Controls.Add(lblPeriodoB)

        dtpInizioB = New DateTimePicker()
        dtpInizioB.Location = New Point(450, 50)
        dtpInizioB.Size = New Size(90, 25)
        dtpInizioB.Format = DateTimePickerFormat.Short
        dtpInizioB.Enabled = False
        pnlControlli.Controls.Add(dtpInizioB)

        Dim lblBB As New Label()
        lblBB.Text = "A"
        lblBB.Location = New Point(545, 52)
        lblBB.Size = New Size(10, 20)
        lblBB.Enabled = False
        pnlControlli.Controls.Add(lblBB)

        dtpFineB = New DateTimePicker()
        dtpFineB.Location = New Point(560, 50)
        dtpFineB.Size = New Size(90, 25)
        dtpFineB.Format = DateTimePickerFormat.Short
        dtpFineB.Enabled = False
        pnlControlli.Controls.Add(dtpFineB)

        ' Panel superiore per DataGridView
        Dim pnlTabella As New Panel()
        pnlTabella.Dock = DockStyle.Fill
        pnlTabella.Padding = New Padding(10)
        splitConfronto.Panel1.Controls.Add(pnlTabella)

        ' DataGridView Confronto
        dgvConfrontoAnni = New DataGridView()
        dgvConfrontoAnni.Dock = DockStyle.Fill
        dgvConfrontoAnni.AutoGenerateColumns = False
        dgvConfrontoAnni.AllowUserToAddRows = False
        dgvConfrontoAnni.AllowUserToDeleteRows = False
        dgvConfrontoAnni.ReadOnly = True
        dgvConfrontoAnni.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvConfrontoAnni.BackgroundColor = Color.White
        dgvConfrontoAnni.GridColor = Color.FromArgb(189, 195, 199)
        dgvConfrontoAnni.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94)
        dgvConfrontoAnni.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvConfrontoAnni.EnableHeadersVisualStyles = False

        ' Colonne DataGridView - NUOVO ORDINE
        dgvConfrontoAnni.Columns.AddRange({
        New DataGridViewTextBoxColumn() With {.Name = "Mese", .HeaderText = "Mese", .Width = 80},
        New DataGridViewTextBoxColumn() With {.Name = "EntrateA", .HeaderText = "Entrate A", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "EntrateB", .HeaderText = "Entrate B", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "DiffEntrate", .HeaderText = "Diff Entrate", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "UsciteA", .HeaderText = "Uscite A", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "UsciteB", .HeaderText = "Uscite B", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "DiffUscite", .HeaderText = "Diff Uscite", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "RisparmioA", .HeaderText = "Risparmio A", .Width = 95, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "RisparmioB", .HeaderText = "Risparmio B", .Width = 95, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}},
        New DataGridViewTextBoxColumn() With {.Name = "DiffRisparmio", .HeaderText = "Diff Risparmio", .Width = 105, .DefaultCellStyle = New DataGridViewCellStyle() With {.Format = "N2"}}
    })

        pnlTabella.Controls.Add(dgvConfrontoAnni)

        ' Panel inferiore per grafico e riepilogo
        Dim pnlGrafico As New Panel()
        pnlGrafico.Dock = DockStyle.Fill
        pnlGrafico.Padding = New Padding(10)
        splitConfronto.Panel2.Controls.Add(pnlGrafico)

        ' Grafico confronto
        cartesianChartConfronto = New CartesianChart()
        cartesianChartConfronto.Dock = DockStyle.Fill
        cartesianChartConfronto.Margin = New Padding(0, 0, 0, 40)
        pnlGrafico.Controls.Add(cartesianChartConfronto)

        ' Label riepilogo
        lblRiepilogoConfronto = New Label()
        lblRiepilogoConfronto.Dock = DockStyle.Bottom
        lblRiepilogoConfronto.Height = 40
        lblRiepilogoConfronto.TextAlign = ContentAlignment.MiddleCenter
        lblRiepilogoConfronto.Font = New Font("Microsoft Sans Serif", 10, FontStyle.Bold)
        lblRiepilogoConfronto.Text = "Seleziona due anni diversi e clicca 'Confronta' per vedere il confronto"
        pnlGrafico.Controls.Add(lblRiepilogoConfronto)
    End Sub

    ' === EVENTI PER PERIODO PERSONALIZZATO ===
    Private Sub chkPeriodoCustom_CheckedChanged(sender As Object, e As EventArgs)
        Dim attivo As Boolean = chkPeriodoCustom.Checked

        ' Attiva/disattiva controlli anno vs controlli periodo
        cmbAnnoA.Enabled = Not attivo
        cmbAnnoB.Enabled = Not attivo

        ' Attiva controlli periodo custom
        dtpInizioA.Enabled = attivo
        dtpFineA.Enabled = attivo
        dtpInizioB.Enabled = attivo
        dtpFineB.Enabled = attivo

        ' Aggiorna label bottone e testo riepilogo
        If attivo Then
            btnConfronta.Text = "Confronta Periodi"
            lblRiepilogoConfronto.Text = "Seleziona le date dei due periodi e clicca 'Confronta Periodi'"
            dtpInizioA.Value = Date.Today.AddMonths(-12)
            dtpFineA.Value = Date.Today.AddMonths(-6)
            dtpInizioB.Value = Date.Today.AddMonths(-6)
            dtpFineB.Value = Date.Today
        Else
            btnConfronta.Text = "Confronta"
            lblRiepilogoConfronto.Text = "Seleziona due anni diversi e clicca 'Confronta' per vedere il confronto"
        End If
    End Sub

    ' === FUNZIONE PER CONFRONTO PERIODI PERSONALIZZATI ===
    Private Sub EseguiConfrontoPeriodi(inizioA As Date, fineA As Date, inizioB As Date, fineB As Date)
        dgvConfrontoAnni.Rows.Clear()
        Dim datiA = CalcolaDatiPeriodo(inizioA, fineA)
        Dim datiB = CalcolaDatiPeriodo(inizioB, fineB)
        dgvConfrontoAnni.Rows.Add("Periodo Totale",
        datiA.Item1, datiB.Item1, datiB.Item1 - datiA.Item1,
        datiA.Item2, datiB.Item2, datiB.Item2 - datiA.Item2,
        datiA.Item3, datiB.Item3, datiB.Item3 - datiA.Item3)
        ColoraCelleDifferenze()
        AggiornaGraficoConfrontoPeriodi(inizioA, fineA, inizioB, fineB, datiA, datiB)
        AggiornaRiepilogoConfrontoPeriodi(inizioA, fineA, inizioB, fineB,
        datiA.Item1, datiA.Item2, datiA.Item3,
        datiB.Item1, datiB.Item2, datiB.Item3)
    End Sub

    ' === CALCOLA DATI PER UN PERIODO SPECIFICO ===
    Private Function CalcolaDatiPeriodo(inizio As Date, fine As Date) As Tuple(Of Decimal, Decimal, Decimal)
        Dim entrate As Decimal = 0, uscite As Decimal = 0
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Const sql = "SELECT Importo FROM Transazioni WHERE Data >= @inizio AND Data <= @fine"
            Using cmd As New SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@inizio", inizio.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@fine", fine.ToString("yyyy-MM-dd"))
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim imp = Convert.ToDecimal(reader("Importo"))
                        If imp > 0 Then entrate += imp Else uscite += Math.Abs(imp)
                    End While
                End Using
            End Using
        End Using
        Return New Tuple(Of Decimal, Decimal, Decimal)(entrate, uscite, entrate - uscite)
    End Function

    ' === GRAFICO PER PERIODI PERSONALIZZATI ===
    Private Sub AggiornaGraficoConfrontoPeriodi(inizioA As Date, fineA As Date, inizioB As Date, fineB As Date, datiA As Tuple(Of Decimal, Decimal, Decimal), datiB As Tuple(Of Decimal, Decimal, Decimal))
        Dim periodi As New List(Of String) From {
        $"Periodo A ({inizioA.ToShortDateString()}-{fineA.ToShortDateString()})",
        $"Periodo B ({inizioB.ToShortDateString()}-{fineB.ToShortDateString()})"
    }

        Dim entrateA As New List(Of Double) From {Convert.ToDouble(datiA.Item1), 0}
        Dim entrateB As New List(Of Double) From {0, Convert.ToDouble(datiB.Item1)}
        Dim usciteA As New List(Of Double) From {Convert.ToDouble(datiA.Item2), 0}
        Dim usciteB As New List(Of Double) From {0, Convert.ToDouble(datiB.Item2)}

        cartesianChartConfronto.Series = New List(Of ISeries) From {
        New ColumnSeries(Of Double) With {.Name = $"Entrate Periodo A", .Values = entrateA},
        New ColumnSeries(Of Double) With {.Name = $"Entrate Periodo B", .Values = entrateB},
        New ColumnSeries(Of Double) With {.Name = $"Uscite Periodo A", .Values = usciteA},
        New ColumnSeries(Of Double) With {.Name = $"Uscite Periodo B", .Values = usciteB}
    }

        cartesianChartConfronto.XAxes = New List(Of Axis) From {
        New Axis With {.Labels = periodi}
    }
    End Sub

    ' === RIEPILOGO PER PERIODI PERSONALIZZATI ===
    Private Sub AggiornaRiepilogoConfrontoPeriodi(inizioA As Date, fineA As Date, inizioB As Date, fineB As Date, entA As Decimal, uscA As Decimal, rispA As Decimal, entB As Decimal, uscB As Decimal, rispB As Decimal)
        Dim diffEntrate = entB - entA
        Dim diffUscite = uscB - uscA
        Dim diffRisparmio = rispB - rispA

        Dim testo As String = $"Confronto {inizioA.ToShortDateString()}-{fineA.ToShortDateString()} vs {inizioB.ToShortDateString()}-{fineB.ToShortDateString()}: "

        If diffRisparmio >= 0 Then
            testo &= $"Periodo B migliore di {diffRisparmio.ToString("C2")}"
            lblRiepilogoConfronto.ForeColor = Color.DarkGreen
        Else
            testo &= $"Periodo A migliore di {Math.Abs(diffRisparmio).ToString("C2")}"
            lblRiepilogoConfronto.ForeColor = Color.DarkRed
        End If

        testo &= $" | Entrate: {If(diffEntrate >= 0, "+", "")}{diffEntrate.ToString("C2")} | Uscite: {If(diffUscite >= 0, "+", "")}{diffUscite.ToString("C2")}"

        lblRiepilogoConfronto.Text = testo
    End Sub

    Private Sub CaricaAnniConfronto()
        cmbAnnoA.Items.Clear()
        cmbAnnoB.Items.Clear()

        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim anni As New List(Of String)
            Dim sql As String = "SELECT DISTINCT strftime('%Y', Data) AS Anno FROM Transazioni ORDER BY Anno DESC"
            Using cmd As New SQLiteCommand(sql, conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        anni.Add(reader("Anno").ToString())
                    End While
                End Using
            End Using

            cmbAnnoA.Items.AddRange(anni.ToArray())
            cmbAnnoB.Items.AddRange(anni.ToArray())

            ' Seleziona automaticamente gli ultimi due anni se disponibili
            If anni.Count > 0 Then cmbAnnoA.SelectedIndex = 0
            If anni.Count > 1 Then cmbAnnoB.SelectedIndex = 1
        End Using
    End Sub

    Private Sub btnConfronta_Click(sender As Object, e As EventArgs)
        If chkPeriodoCustom.Checked Then
            ' Controllo validità date
            If dtpFineA.Value <= dtpInizioA.Value OrElse dtpFineB.Value <= dtpInizioB.Value Then
                MessageBox.Show("Le date di fine devono essere posteriori a quelle di inizio.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            ' Confronta periodi
            EseguiConfrontoPeriodi(dtpInizioA.Value, dtpFineA.Value, dtpInizioB.Value, dtpFineB.Value)
        Else
            ' Modalità confronto anni
            If cmbAnnoA.SelectedItem Is Nothing OrElse cmbAnnoB.SelectedItem Is Nothing Then
                MessageBox.Show("Seleziona entrambi gli anni per il confronto.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If cmbAnnoA.SelectedItem.ToString() = cmbAnnoB.SelectedItem.ToString() Then
                MessageBox.Show("Seleziona due anni diversi per il confronto.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            EseguiConfrontoAnni()
        End If
    End Sub

    Private Sub dgvCategorie_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCategorie.CellDoubleClick
        If e.RowIndex < 0 Then Exit Sub
        Dim anno = Integer.Parse(cmbAnnoCategorie.SelectedItem.ToString())
        Dim mese = Convert.ToInt32(dgvCategorie.Rows(e.RowIndex).Cells(0).Value)
        Dim macro = dgvCategorie.Rows(e.RowIndex).Cells(1).Value.ToString()
        Dim cat = dgvCategorie.Rows(e.RowIndex).Cells(2).Value.ToString()
        Dim periodo = CalcolaPeriodoMensile(anno, mese)
        Dim frm = New FormFinanza(periodo.DataInizio, periodo.DataFine, macro, cat)
        frm.ShowDialog()
    End Sub

    Private Sub btnScambia_Click(sender As Object, e As EventArgs)
        If cmbAnnoA.SelectedItem IsNot Nothing AndAlso cmbAnnoB.SelectedItem IsNot Nothing Then
            Dim tempIndex = cmbAnnoA.SelectedIndex
            cmbAnnoA.SelectedIndex = cmbAnnoB.SelectedIndex
            cmbAnnoB.SelectedIndex = tempIndex
        End If
    End Sub

    Private Sub EseguiConfrontoAnni()
        ' Leggi gli anni selezionati
        Dim annoA As Integer = Integer.Parse(cmbAnnoA.SelectedItem.ToString())
        Dim annoB As Integer = Integer.Parse(cmbAnnoB.SelectedItem.ToString())

        ' Pulisci la griglia
        dgvConfrontoAnni.Rows.Clear()

        ' Raccogli i dati mensili per entrambi gli anni
        Dim datiA = Enumerable.Range(1, 12).
        ToDictionary(Function(m) m, Function(m) CalcolaDatiPeriodo(
            CalcolaPeriodoMensile(annoA, m).DataInizio,
            CalcolaPeriodoMensile(annoA, m).DataFine))

        Dim datiB = Enumerable.Range(1, 12).
        ToDictionary(Function(m) m, Function(m) CalcolaDatiPeriodo(
            CalcolaPeriodoMensile(annoB, m).DataInizio,
            CalcolaPeriodoMensile(annoB, m).DataFine))

        ' Totali cumulati
        Dim totA_Entrate As Decimal = 0D, totA_Uscite As Decimal = 0D, totA_Risp As Decimal = 0D
        Dim totB_Entrate As Decimal = 0D, totB_Uscite As Decimal = 0D, totB_Risp As Decimal = 0D

        ' Popola riga per riga
        For mese As Integer = 1 To 12
            Dim nomeMese = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mese)

            Dim eA = datiA(mese).Item1
            Dim uA = datiA(mese).Item2
            Dim rA = datiA(mese).Item3

            Dim eB = datiB(mese).Item1
            Dim uB = datiB(mese).Item2
            Dim rB = datiB(mese).Item3

            Dim diffE = eB - eA
            Dim diffU = uB - uA
            Dim diffR = rB - rA

            dgvConfrontoAnni.Rows.Add(
            nomeMese,
            eA, eB, diffE,
            uA, uB, diffU,
            rA, rB, diffR
        )

            totA_Entrate += eA : totA_Uscite += uA : totA_Risp += rA
            totB_Entrate += eB : totB_Uscite += uB : totB_Risp += rB
        Next

        ColoraCelleDifferenze()
        AggiornaGraficoConfronto()

        AggiornaRiepilogoConfronto(
        annoA, annoB,
        totA_Entrate, totA_Uscite, totA_Risp,
        totB_Entrate, totB_Uscite, totB_Risp
    )
    End Sub

    Private Function CalcolaDatiMese(anno As Integer, mese As Integer) As Tuple(Of Decimal, Decimal, Decimal)
        ' Usa la nuova funzione helper per il calcolo corretto
        Dim periodo = CalcolaPeriodoMensile(anno, mese)
        Dim dataInizio = periodo.DataInizio
        Dim dataFine = periodo.DataFine

        ' Se il periodo non è ancora iniziato, ritorna zero
        If dataInizio > Date.Today Then
            Return New Tuple(Of Decimal, Decimal, Decimal)(0, 0, 0)
        End If

        Dim entrate As Decimal = 0, uscite As Decimal = 0

        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String = "SELECT Importo FROM Transazioni WHERE Data >= @inizio AND Data <= @fine"
            Using cmd As New SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@inizio", dataInizio.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@fine", dataFine.ToString("yyyy-MM-dd"))
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim imp As Decimal = Convert.ToDecimal(reader("Importo"))
                        If imp > 0 Then entrate += imp
                        If imp < 0 Then uscite += Math.Abs(imp)
                    End While
                End Using
            End Using
        End Using

        Dim risparmio As Decimal = entrate - uscite
        Return New Tuple(Of Decimal, Decimal, Decimal)(entrate, uscite, risparmio)
    End Function


    Private Sub ColoraCelleDifferenze()
        For Each row As DataGridViewRow In dgvConfrontoAnni.Rows
            If row.IsNewRow Then Continue For

            ' Colora differenze entrate (verde positivo, rosso negativo)
            Dim diffEntrate As Decimal = Convert.ToDecimal(row.Cells("DiffEntrate").Value)
            row.Cells("DiffEntrate").Style.BackColor = If(diffEntrate >= 0, Color.LightGreen, Color.LightCoral)

            ' Colora differenze uscite (rosso positivo, verde negativo - meno uscite è meglio)
            Dim diffUscite As Decimal = Convert.ToDecimal(row.Cells("DiffUscite").Value)
            row.Cells("DiffUscite").Style.BackColor = If(diffUscite <= 0, Color.LightGreen, Color.LightCoral)

            ' Colora differenze risparmio (verde positivo, rosso negativo)
            Dim diffRisparmio As Decimal = Convert.ToDecimal(row.Cells("DiffRisparmio").Value)
            row.Cells("DiffRisparmio").Style.BackColor = If(diffRisparmio >= 0, Color.LightGreen, Color.LightCoral)
        Next
    End Sub

    Private Sub AggiornaGraficoConfronto()
        Dim mesi As New List(Of String)
        Dim entrateA As New List(Of Double)
        Dim entrateB As New List(Of Double)
        Dim usciteA As New List(Of Double)
        Dim usciteB As New List(Of Double)
        Dim risparmioA As New List(Of Double)
        Dim risparmioB As New List(Of Double)

        For Each row As DataGridViewRow In dgvConfrontoAnni.Rows
            If row.IsNewRow Then Continue For
            mesi.Add(row.Cells("Mese").Value.ToString())
            entrateA.Add(Convert.ToDouble(row.Cells("EntrateA").Value))
            entrateB.Add(Convert.ToDouble(row.Cells("EntrateB").Value))
            usciteA.Add(Convert.ToDouble(row.Cells("UsciteA").Value))
            usciteB.Add(Convert.ToDouble(row.Cells("UsciteB").Value))
            risparmioA.Add(Convert.ToDouble(row.Cells("RisparmioA").Value))
            risparmioB.Add(Convert.ToDouble(row.Cells("RisparmioB").Value))
        Next

        cartesianChartConfronto.Series = New List(Of ISeries) From {
            New ColumnSeries(Of Double) With {.Name = $"Entrate {cmbAnnoA.SelectedItem}", .Values = entrateA},
            New ColumnSeries(Of Double) With {.Name = $"Entrate {cmbAnnoB.SelectedItem}", .Values = entrateB},
            New ColumnSeries(Of Double) With {.Name = $"Uscite {cmbAnnoA.SelectedItem}", .Values = usciteA},
            New ColumnSeries(Of Double) With {.Name = $"Uscite {cmbAnnoB.SelectedItem}", .Values = usciteB}
        }

        cartesianChartConfronto.XAxes = New List(Of Axis) From {
            New Axis With {.Labels = mesi}
        }
    End Sub

    Private Sub AggiornaRiepilogoConfronto(annoA As Integer, annoB As Integer, totEntrateA As Decimal, totUsciteA As Decimal, totRisparmioA As Decimal, totEntrateB As Decimal, totUsciteB As Decimal, totRisparmioB As Decimal)
        Dim diffEntrate = totEntrateB - totEntrateA
        Dim diffUscite = totUsciteB - totUsciteA
        Dim diffRisparmio = totRisparmioB - totRisparmioA

        Dim testo As String = $"Confronto {annoA} vs {annoB}: "

        If diffRisparmio >= 0 Then
            testo &= $"Risparmio migliorato di {diffRisparmio.ToString("C2")} "
            lblRiepilogoConfronto.ForeColor = Color.DarkGreen
        Else
            testo &= $"Risparmio peggiorato di {Math.Abs(diffRisparmio).ToString("C2")} "
            lblRiepilogoConfronto.ForeColor = Color.DarkRed
        End If

        testo &= $"| Entrate: {If(diffEntrate >= 0, "+", "")}{diffEntrate.ToString("C2")} | Uscite: {If(diffUscite >= 0, "+", "")}{diffUscite.ToString("C2")}"

        lblRiepilogoConfronto.Text = testo
    End Sub

    ' === ANALISI PER ANNO (codice esistente) ===
    Private Sub CaricaAnniDisponibili()
        cmbAnnoAnalisi.Items.Clear()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim anni As New List(Of String)
            Dim sql As String = "SELECT DISTINCT strftime('%Y', Data) AS Anno FROM Transazioni ORDER BY Anno DESC"
            Using cmd As New SQLiteCommand(sql, conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        anni.Add(reader("Anno").ToString())
                    End While
                End Using
            End Using
            cmbAnnoAnalisi.Items.AddRange(anni.ToArray())
        End Using
    End Sub

    Private Sub btnAggiornaAnalisiAnno_Click(sender As Object, e As EventArgs) Handles btnAggiornaAnalisiAnno.Click
        PopolaAnalisiPerAnno()
    End Sub

    Private Sub cmbAnnoAnalisi_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAnnoAnalisi.SelectedIndexChanged
        PopolaAnalisiPerAnno()
    End Sub

    Private Sub btnGrafici_Click(sender As Object, e As EventArgs) Handles btnGrafici.Click
        Dim annoScelto As Integer
        If cmbAnnoAnalisi.SelectedItem IsNot Nothing AndAlso Integer.TryParse(cmbAnnoAnalisi.SelectedItem.ToString(), annoScelto) Then
            Using fg As New FormGrafici(annoScelto)
                fg.ShowDialog()
            End Using
        Else
            MessageBox.Show("Seleziona un anno prima di visualizzare i grafici.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub PopolaAnalisiPerAnno()
        If cmbAnnoAnalisi.SelectedItem Is Nothing Then Exit Sub
        Dim annoScelto As Integer = Integer.Parse(cmbAnnoAnalisi.SelectedItem.ToString())
        dgvAnalisiAnno.Rows.Clear()

        For mese As Integer = 1 To 12
            Dim periodo = CalcolaPeriodoMensile(annoScelto, mese)
            Dim dataInizio = periodo.DataInizio
            Dim dataFine = periodo.DataFine
            If dataInizio > Date.Today Then Exit For

            Dim nomeMese As String = dataFine.ToString("MMMM", CultureInfo.CurrentCulture)
            Dim entrate As Decimal = 0D, uscite As Decimal = 0D

            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Const sql = "SELECT Importo FROM Transazioni WHERE Data >= @inizio AND Data <= @fine"
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@inizio", dataInizio.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@fine", dataFine.ToString("yyyy-MM-dd"))
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim imp = Convert.ToDecimal(reader("Importo"))
                            If imp > 0 Then entrate += imp Else uscite += Math.Abs(imp)
                        End While
                    End Using
                End Using
            End Using

            Dim risparmio = entrate - uscite
            dgvAnalisiAnno.Rows.Add(
            dataInizio.ToString("dd/MM/yyyy"),
            dataFine.ToString("dd/MM/yyyy"),
            nomeMese,
            dataFine.Year,
            entrate,
            uscite,
            risparmio
        )
        Next

        ' formattazione
        If dgvAnalisiAnno.Columns.Contains("Entrate") Then dgvAnalisiAnno.Columns("Entrate").DefaultCellStyle.Format = "N2"
        If dgvAnalisiAnno.Columns.Contains("Uscite") Then dgvAnalisiAnno.Columns("Uscite").DefaultCellStyle.Format = "N2"
        If dgvAnalisiAnno.Columns.Contains("Risparmio") Then dgvAnalisiAnno.Columns("Risparmio").DefaultCellStyle.Format = "N2"
    End Sub


    ' === SEZIONE ANALISI CATEGORIE (codice esistente) ===

    Private Sub CaricaAnniCategorie()
        cmbAnnoCategorie.Items.Clear()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String = "SELECT DISTINCT strftime('%Y', Data) FROM Transazioni ORDER BY 1 DESC"
            Using cmd As New SQLiteCommand(sql, conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        cmbAnnoCategorie.Items.Add(reader(0).ToString())
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub CaricaMesiCategorie()
        cmbMeseCategoriee.Items.Clear()
        cmbMeseCategoriee.Items.Add("Tutti")
        For m = 1 To 12
            cmbMeseCategoriee.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m))
        Next
    End Sub

    Private Sub CaricaMacroCategorie()
        cmbMacroCategoria.Items.Clear()
        cmbMacroCategoria.Items.Add("Tutte")
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql = "SELECT DISTINCT MacroCategoria FROM Pattern ORDER BY MacroCategoria"
            Using cmd As New SQLiteCommand(sql, conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        cmbMacroCategoria.Items.Add(reader("MacroCategoria").ToString())
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub CaricaCategoriePrincipali()
        cmbCategoria.Items.Clear()
        cmbCategoria.Items.Add("Tutte")
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String
            If cmbMacroCategoria.SelectedIndex > 0 Then
                sql = "SELECT DISTINCT Categoria FROM Pattern WHERE MacroCategoria=@macro"
            Else
                sql = "SELECT DISTINCT Categoria FROM Pattern"
            End If
            Using cmd As New SQLiteCommand(sql, conn)
                If cmbMacroCategoria.SelectedIndex > 0 Then cmd.Parameters.AddWithValue("@macro", cmbMacroCategoria.SelectedItem.ToString())
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        cmbCategoria.Items.Add(reader("Categoria").ToString())
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub CaricaSottoCategorie()
        ' Metodo rimosso - SottoCategoria non più utilizzata
    End Sub

    ' === EVENTI FILTRI ===
    Private Sub cmbAnnoCategorie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAnnoCategorie.SelectedIndexChanged
        AggiornaAnalisiCategorie()
    End Sub
    Private Sub cmbMeseCategoriee_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbMeseCategoriee.SelectedIndexChanged
        AggiornaAnalisiCategorie()
    End Sub
    Private Sub cmbMacroCategoria_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbMacroCategoria.SelectedIndexChanged
        CaricaCategoriePrincipali()
        'CaricaSottoCategorie()
        AggiornaAnalisiCategorie()
    End Sub
    Private Sub cmbCategoria_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbCategoria.SelectedIndexChanged
        'CaricaSottoCategorie()
        AggiornaAnalisiCategorie()
    End Sub
    Private Sub btnAggiornaCategorie_Click(sender As Object, e As EventArgs) Handles btnAggiornaCategorie.Click
        AggiornaAnalisiCategorie()
    End Sub

    Private Sub AggiornaAnalisiCategorie()
        If cmbAnnoCategorie.SelectedItem Is Nothing Then Exit Sub
        Dim anno = cmbAnnoCategorie.SelectedItem.ToString()
        Dim meseIndex = cmbMeseCategoriee.SelectedIndex
        Dim macro = If(cmbMacroCategoria.SelectedIndex > 0, cmbMacroCategoria.SelectedItem.ToString(), Nothing)
        Dim cat = If(cmbCategoria.SelectedIndex > 0, cmbCategoria.SelectedItem.ToString(), Nothing)
        ' RIMUOVI sotto

        Dim sql = "
        SELECT strftime('%m', Data) AS Mese, MacroCategoria, Categoria,
               SUM(CASE WHEN Importo > 0 THEN Importo ELSE 0 END) AS EntrateCategoria,
               SUM(CASE WHEN Importo < 0 THEN ABS(Importo) ELSE 0 END) AS UsciteCategoria
        FROM Transazioni
        WHERE strftime('%Y', Data) = @anno
    "
        Dim dataInizio As Date = Nothing, dataFine As Date = Nothing

        If meseIndex > 0 Then
            Dim periodo = CalcolaPeriodoMensile(CInt(anno), meseIndex)
            dataInizio = periodo.DataInizio
            dataFine = periodo.DataFine
            sql &= " AND Data >= @inizio AND Data <= @fine"
        End If
        If macro IsNot Nothing Then sql &= " AND MacroCategoria = @macro"
        If cat IsNot Nothing Then sql &= " AND Categoria = @cat"
        ' RIMUOVI filtro SottoCategoria
        sql &= " GROUP BY Mese, MacroCategoria, Categoria
             ORDER BY Mese, MacroCategoria, Categoria"

        dgvCategorie.Rows.Clear()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd As New SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@anno", anno)
                If meseIndex > 0 Then
                    cmd.Parameters.AddWithValue("@inizio", dataInizio.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@fine", dataFine.ToString("yyyy-MM-dd"))
                End If
                If macro IsNot Nothing Then cmd.Parameters.AddWithValue("@macro", macro)
                If cat IsNot Nothing Then cmd.Parameters.AddWithValue("@cat", cat)
                ' RIMUOVI parametro SottoCategoria
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim e = Convert.ToDecimal(reader("EntrateCategoria"))
                        Dim u = Convert.ToDecimal(reader("UsciteCategoria"))
                        dgvCategorie.Rows.Add(reader("Mese"), reader("MacroCategoria"),
                    reader("Categoria"), e, u, e - u)
                    End While
                End Using
            End Using
        End Using
        AggiornaGraficiCategorie()
    End Sub

    ''' <summary>
    ''' NUOVA FUNZIONE che usa GestoreStipendi
    ''' </summary>
    Private Function CalcolaPeriodoMensile(anno As Integer, mese As Integer) As (DataInizio As Date, DataFine As Date)
        Return GestoreStipendi.CalcolaPeriodoStipendiale(anno, mese)
    End Function


    Private Sub AggiornaGraficiCategorie()
        Dim months As New List(Of String)
        Dim entrateList As New List(Of Double)
        Dim usciteList As New List(Of Double)
        Dim risparmioList As New List(Of Double)

        For Each row As DataGridViewRow In dgvCategorie.Rows
            If row.IsNewRow Then Continue For
            months.Add(row.Cells(0).Value.ToString())
            entrateList.Add(Convert.ToDouble(row.Cells(4).Value))
            usciteList.Add(Convert.ToDouble(row.Cells(5).Value))
            risparmioList.Add(Convert.ToDouble(row.Cells(5).Value))
        Next

        ' BARRE: Andamento mensile
        cartesianChartCategorieBarre.Series = New List(Of ISeries) From {
            New ColumnSeries(Of Double) With {.Name = "Entrate", .Values = entrateList},
            New ColumnSeries(Of Double) With {.Name = "Uscite", .Values = usciteList}
        }
        cartesianChartCategorieBarre.XAxes = New List(Of Axis) From {
            New Axis With {.Labels = months}
        }

        ' TORTA: distribuzione uscite per categoria
        Dim pieSeriesList As New List(Of ISeries)
        For Each row As DataGridViewRow In dgvCategorie.Rows
            If row.IsNewRow Then Continue For
            Dim categoria As String = row.Cells(2).Value.ToString()
            Dim valore As Double = Convert.ToDouble(row.Cells(5).Value)
            pieSeriesList.Add(New PieSeries(Of Double) With {
                .Name = categoria,
                .Values = New List(Of Double) From {valore}
            })
        Next
        pieChartCategorieTorta.Series = pieSeriesList

        ' TREND: andamento risparmio
        cartesianChartCategorieTrend.Series = New List(Of ISeries) From {
            New LineSeries(Of Double) With {.Name = "Risparmio", .Values = risparmioList}
        }
        cartesianChartCategorieTrend.XAxes = New List(Of Axis) From {
            New Axis With {.Labels = months}
        }
    End Sub

End Class
