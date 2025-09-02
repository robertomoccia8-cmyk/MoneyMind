Imports System.Data.SQLite
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports GestioneFinanzePersonali.GptClassificatoreTransazioni
Imports GestioneFinanzePersonali.Models

Public Class FormRaffinaPattern

    ' VARIABILI PER INTELLIGENZA ARTIFICIALI TRANSAZIONI
    Private gptClassificatore As GptClassificatoreTransazioni
    
    ' Eventi per notificare cambiamenti ai pattern
    Public Shared Event PatternsChanged()
    Public Shared Event TransactionsClassified(count As Integer)
    Private btnAIAssistita As Button
    Private panelAI As Panel
    Private lblAIStatus As Label
    Private progBarAI As ProgressBar
    Private btnTrovaSimiliAI As Button
    Private btnCreaPatternAI As Button

    Private necessityOptions As List(Of String) = New List(Of String) From {"Base", "Importante", "Superflua"}
    Private seasonOptions As List(Of String) = New List(Of String) From {"Nessuna", "Invernale", "Primaverile", "Estiva", "Autunnale", "Annuale"}
    Private frequencyOptions As List(Of String) = New List(Of String) From {"Occasionale", "Ricorrente", "Annuale", "Mensile Fissa"}

    Private Sub PopolaComboNecessita()
        cmbNecessita.Items.Clear()
        cmbNecessita.Items.AddRange(necessityOptions.ToArray())
    End Sub

    Private Sub PopolaComboStagionalita()
        cmbStagionalita.Items.Clear()
        cmbStagionalita.Items.AddRange(seasonOptions.ToArray())
    End Sub

    Private transazioneSelezionataID As Integer = -1

    Private Sub FormRaffinaPattern_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 1) Carica le liste personalizzabili
        PatternOptionsManager.LoadOptions(necessityOptions, frequencyOptions, seasonOptions)

        ' 2) Inizializza il client AI
        gptClassificatore = New GptClassificatoreTransazioni()

        ' 3) Crea l'interfaccia TableLayoutPanel
        ImpostaInterfacciaNuova()

        ' 4) Carica i dati
        CaricaTransazioniNonClassificate()
        CaricaCombinazioniPattern()
        AdattaLarghezzaComboCombinazioni()
        PopolaComboNecessita()
        PopolaComboStagionalita()
        PopolaComboFrequenza()
    End Sub


    ' 🔹 Funzione helper per creare pulsanti uniformi
    Private Function CreaPulsanteAI(testo As String, colore As Color) As Button
        Dim btn As New Button With {
        .Text = testo,
        .Size = New Size(140, 35),
        .BackColor = colore,
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI", 9, FontStyle.Bold),
        .Margin = New Padding(0, 0, 10, 0),
        .Cursor = Cursors.Hand
    }
        btn.FlatAppearance.BorderSize = 0
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(colore, 0.1)
        Return btn
    End Function

    Private splitMain As SplitContainer
    ' Tutti i valori di Pattern.Parola (DISTINCT, ordinati)
    Private Function GetTutteLeParolePattern() As List(Of String)
        Dim result As New List(Of String)
        Try
            Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "SELECT DISTINCT Parola FROM Pattern WHERE Parola <> '' ORDER BY Parola"
                Using cmd As New SQLite.SQLiteCommand(sql, conn)
                    Using r = cmd.ExecuteReader()
                        While r.Read()
                            result.Add(r("Parola").ToString())
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore caricamento parole pattern: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        Return result
    End Function






    ' Dato un valore di Parola, recupera MacroCategoria/Categoria suggerite dal DB per auto-compilare i campi
    Private Function CaricaDettagliPattern(parola As String) As (Macro As String, Cat As String)
        Try
            Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "SELECT MacroCategoria, Categoria FROM Pattern WHERE Parola = @p LIMIT 1"
                Using cmd As New SQLite.SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@p", parola)
                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            Return (r("MacroCategoria").ToString(), r("Categoria").ToString())
                        End If
                    End Using
                End Using
            End Using
        Catch
        End Try
        Return ("", "")
    End Function
    Private Sub ImpostaInterfacciaNuova()
        Me.Text = "Raffina Pattern con AI"
        Me.Size = New Size(1400, 800)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(248, 249, 250)

        ' TableLayoutPanel principale a 4 quadranti
        Dim mainTable As New TableLayoutPanel With {
        .Dock = DockStyle.Fill,
        .RowCount = 2,
        .ColumnCount = 2,
        .BackColor = Color.Transparent
    }

        ' Imposta dimensioni righe e colonne
        mainTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 60)) ' Riga superiore: 60px
        mainTable.RowStyles.Add(New RowStyle(SizeType.Percent, 100))  ' Riga inferiore: resto
        mainTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45)) ' Colonna sinistra: 45%
        mainTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55)) ' Colonna destra: 55%

        Me.Controls.Add(mainTable)

        ' ═══════════════════════════════════════════════════════════
        ' QUADRANTE 1: TITOLO TRANSAZIONI (Alto-Sinistra)
        ' ═══════════════════════════════════════════════════════════
        Dim panelTitolo As New Panel With {
        .Dock = DockStyle.Fill,
        .BackColor = Color.FromArgb(52, 73, 94),
        .Padding = New Padding(15, 0, 0, 0)
    }

        Dim lblTitolo As New Label With {
        .Text = "Transazioni Non Classificate",
        .Dock = DockStyle.Fill,
        .Font = New Font("Segoe UI", 11, FontStyle.Bold),
        .ForeColor = Color.White,
        .TextAlign = ContentAlignment.MiddleLeft
    }
        panelTitolo.Controls.Add(lblTitolo)
        mainTable.Controls.Add(panelTitolo, 0, 0)

        ' ═══════════════════════════════════════════════════════════
        ' QUADRANTE 2: PULSANTI AI (Alto-Destra)
        ' ═══════════════════════════════════════════════════════════
        panelAI = New Panel With {
        .Dock = DockStyle.Fill,
        .BackColor = Color.FromArgb(41, 128, 185),
        .Padding = New Padding(10, 10, 15, 10)
    }

        Dim flowButtons As New FlowLayoutPanel With {
        .Dock = DockStyle.Fill,
        .FlowDirection = FlowDirection.LeftToRight,
        .WrapContents = False,
        .BackColor = Color.Transparent
    }

        ' Pulsanti AI
        btnAIAssistita = CreaPulsanteAI("AI Assistita", Color.FromArgb(52, 152, 219))
        AddHandler btnAIAssistita.Click, AddressOf BtnAIAssistita_Click
        flowButtons.Controls.Add(btnAIAssistita)

        btnTrovaSimiliAI = CreaPulsanteAI("Trova Simili", Color.FromArgb(46, 204, 113))
        AddHandler btnTrovaSimiliAI.Click, AddressOf BtnTrovaSimiliAI_Click
        flowButtons.Controls.Add(btnTrovaSimiliAI)

        btnCreaPatternAI = CreaPulsanteAI("✨ Crea Pattern", Color.FromArgb(155, 89, 182))
        AddHandler btnCreaPatternAI.Click, AddressOf BtnCreaPatternAI_Click
        flowButtons.Controls.Add(btnCreaPatternAI)

        ' ProgressBar AI
        progBarAI = New ProgressBar With {
        .Style = ProgressBarStyle.Marquee,
        .MarqueeAnimationSpeed = 30,
        .Size = New Size(120, 25),
        .Visible = False,
        .Margin = New Padding(10, 5, 0, 0)
    }
        flowButtons.Controls.Add(progBarAI)

        panelAI.Controls.Add(flowButtons)
        mainTable.Controls.Add(panelAI, 1, 0)

        ' ═══════════════════════════════════════════════════════════
        ' QUADRANTE 3: DATAGRIDVIEW (Basso-Sinistra)
        ' ═══════════════════════════════════════════════════════════
        Dim panelGrid As New Panel With {
        .Dock = DockStyle.Fill,
        .Padding = New Padding(15, 10, 10, 15),
        .BackColor = Color.White
    }

        dgvTransazioniNonClassificate = New DataGridView With {
        .Dock = DockStyle.Fill,
        .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        .MultiSelect = False,
        .ReadOnly = True,
        .AllowUserToAddRows = False,
        .AllowUserToDeleteRows = False,
        .RowHeadersVisible = False,
        .BackgroundColor = Color.White,
        .GridColor = Color.FromArgb(189, 195, 199),
        .BorderStyle = BorderStyle.None
    }
        panelGrid.Controls.Add(dgvTransazioniNonClassificate)
        mainTable.Controls.Add(panelGrid, 0, 1)

        ' ═══════════════════════════════════════════════════════════
        ' QUADRANTE 4: FORM PATTERN + STATUS (Basso-Destra)  
        ' ═══════════════════════════════════════════════════════════
        Dim panelDestra As New TableLayoutPanel With {
        .Dock = DockStyle.Fill,
        .RowCount = 3,
        .ColumnCount = 1,
        .BackColor = Color.FromArgb(248, 249, 250)
    }

        ' Suddividi il quadrante destro in 3 parti:
        panelDestra.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))  ' Titolo: 40px
        panelDestra.RowStyles.Add(New RowStyle(SizeType.Percent, 100))  ' Form: resto
        panelDestra.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))  ' Status: 35px

        ' Titolo "Crea Nuovo Pattern"
        Dim panelTitoloPattern As New Panel With {
        .Dock = DockStyle.Fill,
        .BackColor = Color.FromArgb(236, 240, 241),
        .Padding = New Padding(20, 0, 0, 0)
    }

        Dim lblTitoloPattern As New Label With {
        .Text = "Crea Nuovo Pattern",
        .Dock = DockStyle.Fill,
        .Font = New Font("Segoe UI", 11, FontStyle.Bold),
        .ForeColor = Color.FromArgb(52, 73, 94),
        .TextAlign = ContentAlignment.MiddleLeft
    }
        panelTitoloPattern.Controls.Add(lblTitoloPattern)
        panelDestra.Controls.Add(panelTitoloPattern, 0, 0)

        ' Panel scorrevole per i campi pattern
        Dim panelScroll As New Panel With {
        .Dock = DockStyle.Fill,
        .AutoScroll = True,
        .BackColor = Color.White,
        .Padding = New Padding(0, 10, 0, 10)
    }
        CreaControlliPattern(panelScroll)
        panelDestra.Controls.Add(panelScroll, 0, 1)

        ' Status Bar AI
        Dim panelStatus As New Panel With {
        .Dock = DockStyle.Fill,
        .BackColor = Color.FromArgb(250, 250, 252),
        .Padding = New Padding(20, 5, 20, 5)
    }

        lblAIStatus = New Label With {
        .Text = "✅ Pronto per AI",
        .Dock = DockStyle.Fill,
        .Font = New Font("Segoe UI", 9, FontStyle.Italic),
        .ForeColor = Color.FromArgb(39, 174, 96),
        .TextAlign = ContentAlignment.MiddleLeft
    }
        panelStatus.Controls.Add(lblAIStatus)
        panelDestra.Controls.Add(panelStatus, 0, 2)

        mainTable.Controls.Add(panelDestra, 1, 1)
    End Sub

    Private Sub CreaControlliPattern(parent As Panel)
        Dim yPos As Integer = 40
        Dim spacing As Integer = 60

        ' Parola
        CreaLabelEInput(parent, "Parola:", txtParola, 20, yPos)
        txtParola.BackColor = ColorTranslator.FromHtml("#FFF3CD")
        yPos += spacing

        ' MacroCategoria
        CreaLabelEInput(parent, "Macro Categoria:", txtMacroCategoria, 20, yPos)
        yPos += spacing

        ' Categoria
        CreaLabelEInput(parent, "Categoria:", txtCategoria, 20, yPos)
        yPos += spacing

        ' RIMUOVI COMPLETAMENTE SottoCategoria

        ' Necessità + combo + pulsante gestione
        CreaLabelECombo(parent, "Necessità:", cmbNecessita, 20, yPos)
        btnGestisciNecessita = New Button
        btnGestisciNecessita.Text = "Modifica voci"
        btnGestisciNecessita.Size = New Size(90, 30)
        btnGestisciNecessita.Location = New Point(240, yPos + 20)
        AddHandler btnGestisciNecessita.Click, AddressOf GestisciNecessita_Click
        parent.Controls.Add(btnGestisciNecessita)
        yPos += spacing

        ' Frequenza
        CreaLabelECombo(parent, "Frequenza:", cmbFrequenza, 20, yPos)
        btnGestisciFrequenza = New Button
        btnGestisciFrequenza.Text = "Modifica voci"
        btnGestisciFrequenza.Size = New Size(90, 30)
        btnGestisciFrequenza.Location = New Point(240, yPos + 20)
        AddHandler btnGestisciFrequenza.Click, AddressOf GestisciFrequenza_Click
        parent.Controls.Add(btnGestisciFrequenza)
        yPos += spacing

        ' Stagionalità + combo + pulsante gestione
        CreaLabelECombo(parent, "Stagionalità:", cmbStagionalita, 20, yPos)
        btnGestisciStagionalita = New Button
        btnGestisciStagionalita.Text = "Modifica voci"
        btnGestisciStagionalita.Size = New Size(90, 30)
        btnGestisciStagionalita.Location = New Point(240, yPos + 20)
        AddHandler btnGestisciStagionalita.Click, AddressOf GestisciStagionalita_Click
        parent.Controls.Add(btnGestisciStagionalita)
        yPos += spacing

        ' Peso
        CreaLabelEInput(parent, "Peso (0-100):", txtPeso, 20, yPos)
        txtPeso.TextAlign = HorizontalAlignment.Right
        txtPeso.Text = "50"
        yPos += spacing

        ' Separatore
        Dim separatore As New Label
        separatore.Text = StrDup(22, "─")
        separatore.ForeColor = ColorTranslator.FromHtml("#BDC3C7")
        separatore.Location = New Point(20, yPos)
        separatore.Size = New Size(300, 20)
        parent.Controls.Add(separatore)
        yPos += 40

        ' ComboBox combinazioni esistenti
        CreaLabelECombo(parent, "Usa combinazione esistente (auto-compila):", cmbCombinazioni, 20, yPos)
        cmbCombinazioni.BackColor = ColorTranslator.FromHtml("#E8F5E8")
        yPos += spacing + 20

        ' Pulsante Aggiungi Pattern
        btnAggiungiPattern = New Button
        btnAggiungiPattern.Text = "Aggiungi Pattern"
        btnAggiungiPattern.Size = New Size(180, 40)
        btnAggiungiPattern.Location = New Point(20, yPos)
        btnAggiungiPattern.BackColor = ColorTranslator.FromHtml("#2980B9")
        btnAggiungiPattern.ForeColor = Color.White
        btnAggiungiPattern.Font = New Font("Microsoft Sans Serif", 10, FontStyle.Bold)
        btnAggiungiPattern.Cursor = Cursors.Hand
        btnAggiungiPattern.FlatStyle = FlatStyle.Flat
        btnAggiungiPattern.FlatAppearance.BorderSize = 0
        parent.Controls.Add(btnAggiungiPattern)
        yPos += 60

        ' Pulsante Chiudi
        Dim btnChiudi As New Button
        btnChiudi.Text = "Chiudi"
        btnChiudi.Size = New Size(100, 35)
        btnChiudi.Location = New Point(210, yPos - 60)
        btnChiudi.BackColor = ColorTranslator.FromHtml("#95A5A6")
        btnChiudi.ForeColor = Color.White
        btnChiudi.Font = New Font("Microsoft Sans Serif", 9, FontStyle.Regular)
        btnChiudi.Cursor = Cursors.Hand
        btnChiudi.FlatStyle = FlatStyle.Flat
        btnChiudi.FlatAppearance.BorderSize = 0
        AddHandler btnChiudi.Click, AddressOf BtnChiudi_Click
        parent.Controls.Add(btnChiudi)
    End Sub

    Private Sub ModificaListaOption(options As List(Of String),
                                title As String,
                                combo As ComboBox)

        Dim prompt = $"Gestione {title}" & vbCrLf & "Scrivi le opzioni separate da virgola."
        Dim input = InputBox(prompt, title, String.Join(",", options))

        If String.IsNullOrWhiteSpace(input) Then Return

        Dim newOpts = input.Split(","c).
                     Select(Function(s) s.Trim()).
                     Where(Function(s) s <> "").
                     Distinct(StringComparer.OrdinalIgnoreCase).
                     ToList()

        options.Clear()
        options.AddRange(newOpts)

        combo.Items.Clear()
        combo.Items.AddRange(options.ToArray())

        ' Alla fine salva sempre su disco tutte e tre le liste
        PatternOptionsManager.SaveOptions(necessityOptions, frequencyOptions, seasonOptions)
    End Sub

    Private Sub CreaLabelEInput(parent As Panel, testo As String, ByRef textBox As TextBox, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = testo
        lbl.Font = New Font("Microsoft Sans Serif", 9, FontStyle.Bold)
        lbl.ForeColor = ColorTranslator.FromHtml("#2C3E50")
        lbl.Location = New Point(x, y)
        lbl.Size = New Size(180, 20)
        parent.Controls.Add(lbl)

        textBox = New TextBox()
        textBox.Size = New Size(200, 25)
        textBox.Location = New Point(x, y + 25)
        textBox.Font = New Font("Microsoft Sans Serif", 9, FontStyle.Regular)
        textBox.ForeColor = ColorTranslator.FromHtml("#34495E")
        textBox.BackColor = Color.White
        textBox.BorderStyle = BorderStyle.FixedSingle
        parent.Controls.Add(textBox)
    End Sub

    Private Sub CreaLabelECombo(parent As Panel, testo As String, ByRef comboBox As ComboBox, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = testo
        lbl.Font = New Font("Microsoft Sans Serif", 9, FontStyle.Bold)
        lbl.ForeColor = ColorTranslator.FromHtml("#2C3E50")
        lbl.Location = New Point(x, y)
        'lbl.Size = New Size(200, 20)      ' Larghezza uguale alla combobox
        lbl.AutoSize = False
        lbl.TextAlign = ContentAlignment.MiddleLeft
        lbl.Size = New Size(300, 20) ' (puoi allargare a 320‒350 se ti serve, MAI meno della larghezza della frase)
        parent.Controls.Add(lbl)

        comboBox = New ComboBox()
        comboBox.Size = New Size(200, 25)
        comboBox.Location = New Point(x, y + 25)
        comboBox.Font = New Font("Microsoft Sans Serif", 9, FontStyle.Regular)
        comboBox.ForeColor = ColorTranslator.FromHtml("#34495E")
        comboBox.BackColor = Color.White
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList
        parent.Controls.Add(comboBox)
    End Sub

    Private Sub CaricaTransazioniNonClassificate()
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim query As String = "
                SELECT ID, Descrizione, Importo
                FROM Transazioni
                WHERE MacroCategoria IS NULL OR MacroCategoria = ''
                ORDER BY Data DESC"
                Using adapter As New SQLiteDataAdapter(query, conn)
                    Dim dt As New DataTable()
                    adapter.Fill(dt)
                    dgvTransazioniNonClassificate.DataSource = dt

                    If dgvTransazioniNonClassificate.Columns.Count > 0 Then
                        With dgvTransazioniNonClassificate
                            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                            .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells

                            .Columns("ID").HeaderText = "ID"
                            .Columns("ID").MinimumWidth = 30
                            .Columns("ID").FillWeight = 0.5  ' Quota minima!

                            .Columns("Descrizione").HeaderText = "Descrizione Transazione"
                            .Columns("Descrizione").MinimumWidth = 200
                            .Columns("Descrizione").FillWeight = 9.5  ' Prende quasi tutto lo spazio
                            .Columns("Descrizione").DefaultCellStyle.WrapMode = DataGridViewTriState.True
                        End With
                    End If

                    ' Stile header
                    dgvTransazioniNonClassificate.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#2C3E50")
                    dgvTransazioniNonClassificate.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
                    dgvTransazioniNonClassificate.ColumnHeadersDefaultCellStyle.Font = New Font("Microsoft Sans Serif", 9, FontStyle.Bold)
                    dgvTransazioniNonClassificate.EnableHeadersVisualStyles = False
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore caricamento transazioni: " & ex.Message)
        End Try
    End Sub

    Private Function TrovaPatternLocalmente(parolaChiave As String, elenco As List(Of String), Optional maxSuggerimenti As Integer = 5) As List(Of String)
        If String.IsNullOrWhiteSpace(parolaChiave) OrElse elenco Is Nothing OrElse elenco.Count = 0 Then
            Return New List(Of String)
        End If
        Dim key = parolaChiave.Trim().ToLowerInvariant()
        ' Punteggio semplice: match diretto > contiene > somiglianza basica su prefissi
        Dim scored = elenco.Select(Function(p)
                                       Dim s = p?.Trim()
                                       If String.IsNullOrEmpty(s) Then Return (P:=p, Score:=-1)
                                       Dim t = s.ToLowerInvariant()
                                       Dim score As Integer = 0
                                       If t = key Then score = 100
                                       If t.Contains(key) Then score = Math.Max(score, 80)
                                       If key.Contains(t) Then score = Math.Max(score, 70)
                                       If t.StartsWith(key) Then score = Math.Max(score, 60)
                                       Return (P:=s, Score:=score)
                                   End Function).
                        Where(Function(x) x.Score >= 0).
                        OrderByDescending(Function(x) x.Score).
                        ThenBy(Function(x) x.P).
                        Take(maxSuggerimenti).
                        Select(Function(x) x.P).
                        ToList()
        Return scored
    End Function
    Private Sub CaricaCombinazioniPattern()
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()

                ' Seleziona combinazioni uniche a 2 livelli
                Dim sql As String = "
                SELECT DISTINCT 
                    MacroCategoria || ' > ' || Categoria AS DisplayText,
                    MacroCategoria,
                    Categoria,
                    Necessita,
                    Frequenza,
                    Stagionalita
                FROM Pattern
                WHERE MacroCategoria <> '' 
                  AND Categoria <> ''
                ORDER BY MacroCategoria, Categoria;"

                Using cmd As New SQLiteCommand(sql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        ' Svuota e inizializza
                        cmbCombinazioni.Items.Clear()
                        cmbCombinazioni.Items.Add("-- Seleziona combinazione esistente --")

                        ' Aggiungi i valori letti
                        While reader.Read()
                            Dim item As New ComboItem With {
                            .Text = reader("DisplayText").ToString(),
                            .MacroCategoria = reader("MacroCategoria").ToString(),
                            .Categoria = reader("Categoria").ToString(),
                            .Necessita = reader("Necessita").ToString(),
                            .Frequenza = reader("Frequenza").ToString(),
                            .Stagionalita = reader("Stagionalita").ToString()
                        }
                            cmbCombinazioni.Items.Add(item)
                        End While

                        cmbCombinazioni.SelectedIndex = 0
                        AdattaLarghezzaComboCombinazioni()
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore caricamento combinazioni esistenti: " & ex.Message,
                        "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub AdattaLarghezzaComboCombinazioni()
        If cmbCombinazioni.Items.Count = 0 Then
            cmbCombinazioni.Width = 200
            cmbCombinazioni.DropDownWidth = 200
            Exit Sub
        End If

        Dim maxWidth As Integer = 200
        Dim graf As Graphics = cmbCombinazioni.CreateGraphics()
        Dim font As Font = cmbCombinazioni.Font

        For Each itm In cmbCombinazioni.Items
            Dim textSize As SizeF = graf.MeasureString(itm.ToString(), font)
            maxWidth = Math.Max(maxWidth, CInt(textSize.Width) + 0)  ' +0px margine extra
        Next

        cmbCombinazioni.Width = maxWidth
        cmbCombinazioni.DropDownWidth = maxWidth   ' *** QUESTA è la chiave! ***
        graf.Dispose()
    End Sub

    Private Sub PopolaComboFrequenza()
        cmbFrequenza.Items.Clear()
        cmbFrequenza.Items.AddRange(frequencyOptions.ToArray())
    End Sub

    Private Sub dgvTransazioniNonClassificate_SelectionChanged(sender As Object, e As EventArgs) Handles dgvTransazioniNonClassificate.SelectionChanged
        If dgvTransazioniNonClassificate.SelectedRows.Count > 0 Then
            transazioneSelezionataID = Convert.ToInt32(dgvTransazioniNonClassificate.SelectedRows(0).Cells("ID").Value)
        End If
    End Sub

    Private Sub GestisciNecessita_Click(sender As Object, e As EventArgs)
        ModificaListaOption(necessityOptions, "Necessità", cmbNecessita)
    End Sub

    Private Sub GestisciStagionalita_Click(sender As Object, e As EventArgs)
        ModificaListaOption(seasonOptions, "Stagionalità", cmbStagionalita)
    End Sub

    Private Sub cmbCombinazioni_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbCombinazioni.SelectedIndexChanged
        If cmbCombinazioni.SelectedIndex > 0 Then
            Dim selectedItem As ComboItem = CType(cmbCombinazioni.SelectedItem, ComboItem)
            txtMacroCategoria.Text = selectedItem.MacroCategoria
            txtCategoria.Text = selectedItem.Categoria
            ' RIMUOVI txtSottoCategoria.Text = selectedItem.SottoCategoria

            cmbNecessita.SelectedItem = selectedItem.Necessita

            ' Imposta frequenza se esiste
            For i As Integer = 0 To cmbFrequenza.Items.Count - 1
                If cmbFrequenza.Items(i).ToString() = selectedItem.Frequenza Then
                    cmbFrequenza.SelectedIndex = i
                    Exit For
                End If
            Next

            cmbStagionalita.SelectedItem = selectedItem.Stagionalita
        Else
            ' Reset campi se si deseleziona
            If cmbCombinazioni.SelectedIndex = 0 Then
                txtMacroCategoria.Clear()
                txtCategoria.Clear()
                ' RIMUOVI txtSottoCategoria.Clear()
                cmbNecessita.SelectedIndex = -1
                cmbFrequenza.SelectedIndex = -1
                cmbStagionalita.SelectedIndex = -1
            End If
        End If
    End Sub

    Private Sub GestisciFrequenza_Click(sender As Object, e As EventArgs)
        ModificaListaOption(frequencyOptions, "Frequenza", cmbFrequenza)
    End Sub

    Private Async Sub BtnAIAssistita_Click(sender As Object, e As EventArgs)
        ' 1) Controllo chiave OpenAI
        If ApiValidator.ControllaSeMancaChiaveApi("OpenAI") Then
            ApiValidator.ChiediAperturaImpostazioni("OpenAI (ChatGPT)")
            Return
        End If

        ' 2) Selezione riga
        If dgvTransazioniNonClassificate Is Nothing OrElse dgvTransazioniNonClassificate.SelectedRows.Count = 0 Then
            MessageBox.Show("Seleziona prima una transazione non classificata.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Dim row = dgvTransazioniNonClassificate.SelectedRows(0)

        ' 3) Recupera descrizione e importo
        Dim descr = row.Cells("Descrizione").Value?.ToString()
        If String.IsNullOrWhiteSpace(descr) Then
            MessageBox.Show("Descrizione mancante.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        Dim importo As Decimal = 0D
        If dgvTransazioniNonClassificate.Columns.Contains("Importo") Then
            Dim s = row.Cells("Importo").Value?.ToString()
            If Not String.IsNullOrWhiteSpace(s) Then
                If Not Decimal.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.GetCultureInfo("it-IT"), importo) Then
                    Decimal.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, importo)
                End If
            End If
        End If

        ' 4) UI stato iniziale
        progBarAI.Visible = True
        btnAIAssistita.Enabled = False
        lblAIStatus.Text = "Analisi AI in corso..."
        lblAIStatus.ForeColor = Color.RoyalBlue

        Try
            ' 5) Flusso unico: AI → Google → fallback minimo
            Debug.WriteLine($"DEBUG FORM: Chiamando AnalizzaTransazione con: '{descr}', importo: {importo}")
            Dim suggerimento = Await gptClassificatore.AnalizzaTransazione(descr, importo)

            ' 6) Report token e costo
            Dim report = gptClassificatore.GetResocontoToken()
            Debug.WriteLine($"DEBUG FORM: Report token: {report}")
            MessageBox.Show(report, "Resoconto Token e Costo", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' 7) Verifica risultato
            Debug.WriteLine($"DEBUG FORM: Suggerimento ricevuto - IsValid: {suggerimento?.IsValid}, NomeSocieta: '{suggerimento?.NomeSocieta}', ParolaChiave: '{suggerimento?.ParolaChiave}'")
            If suggerimento Is Nothing OrElse Not suggerimento.IsValid Then
                MessageBox.Show($"Impossibile ottenere suggerimento. IsValid: {suggerimento?.IsValid}, Motivazione: {suggerimento?.Motivazione}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 8) Richiesta conferma all'utente
            Dim msg = $"Nome Società: {suggerimento.NomeSocieta}" & vbCrLf &
                  $"Attività: {suggerimento.DescrizioneAttivita}" & vbCrLf &
                  $"Parola Chiave: {suggerimento.ParolaChiave}" & vbCrLf &
                  $"MacroCategoria: {suggerimento.MacroCategoria}" & vbCrLf &
                  $"Categoria: {suggerimento.Categoria}" & vbCrLf &
                  $"Necessità: {suggerimento.Necessita}" & vbCrLf &
                  $"Frequenza: {suggerimento.Frequenza}" & vbCrLf &
                  $"Stagionalità: {suggerimento.Stagionalita}" & vbCrLf &
                  $"Peso: {suggerimento.Peso}" & vbCrLf &
                  $"Confidenza: {suggerimento.Confidenza}%" & vbCrLf & vbCrLf &
                  "Accettare la classificazione proposta?"
            Dim res = MessageBox.Show(msg, "Conferma classificazione AI", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)

            Select Case res
                Case DialogResult.Yes
                    ' Applica valori UI
                    txtParola.Text = suggerimento.ParolaChiave
                    txtMacroCategoria.Text = suggerimento.MacroCategoria
                    txtCategoria.Text = suggerimento.Categoria
                    If Not cmbNecessita.Items.Contains(suggerimento.Necessita) Then cmbNecessita.Items.Add(suggerimento.Necessita)
                    cmbNecessita.SelectedItem = suggerimento.Necessita
                    If Not cmbFrequenza.Items.Contains(suggerimento.Frequenza) Then cmbFrequenza.Items.Add(suggerimento.Frequenza)
                    cmbFrequenza.SelectedItem = suggerimento.Frequenza
                    If Not cmbStagionalita.Items.Contains(suggerimento.Stagionalita) Then cmbStagionalita.Items.Add(suggerimento.Stagionalita)
                    cmbStagionalita.SelectedItem = suggerimento.Stagionalita
                    txtPeso.Text = suggerimento.Peso.ToString()
                    lblAIStatus.Text = "Classificazione applicata."
                    lblAIStatus.ForeColor = Color.DarkGreen

                Case DialogResult.No
                    lblAIStatus.Text = "Scelta alternativa."
                    lblAIStatus.ForeColor = Color.DarkGoldenrod

                Case Else
                    lblAIStatus.Text = "Operazione annullata."
                    lblAIStatus.ForeColor = Color.Gray
            End Select

        Catch ex As Exception
            MessageBox.Show($"Errore durante l'analisi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lblAIStatus.Text = "Errore AI"
            lblAIStatus.ForeColor = Color.Red

        Finally
            ' 9) Ripristina UI
            progBarAI.Visible = False
            btnAIAssistita.Enabled = True
        End Try
    End Sub



    Private Sub SelezionaCombinazionePiùVicina(macro As String, categoria As String)
        If cmbCombinazioni Is Nothing OrElse cmbCombinazioni.Items.Count = 0 Then Return
        Dim target = $"{macro} > {categoria}".Trim()

        ' prova match esatto su ComboItem.Text
        For i As Integer = 0 To cmbCombinazioni.Items.Count - 1
            If TypeOf cmbCombinazioni.Items(i) Is FormRaffinaPattern.ComboItem Then
                Dim it = DirectCast(cmbCombinazioni.Items(i), FormRaffinaPattern.ComboItem)
                If String.Equals(it.Text, target, StringComparison.OrdinalIgnoreCase) Then
                    cmbCombinazioni.SelectedIndex = i
                    Return
                End If
            End If
        Next

        ' fallback: prova match solo MacroCategoria
        For i As Integer = 0 To cmbCombinazioni.Items.Count - 1
            If TypeOf cmbCombinazioni.Items(i) Is FormRaffinaPattern.ComboItem Then
                Dim it = DirectCast(cmbCombinazioni.Items(i), FormRaffinaPattern.ComboItem)
                If String.Equals(it.MacroCategoria, macro, StringComparison.OrdinalIgnoreCase) Then
                    cmbCombinazioni.SelectedIndex = i
                    Return
                End If
            End If
        Next

        ' nessuna corrispondenza: lascia selezione corrente
    End Sub

    Private Async Sub BtnTrovaSimiliAI_Click(sender As Object, e As EventArgs)
        Try
            Dim parolaBase As String = txtParola.Text.Trim()
            If String.IsNullOrWhiteSpace(parolaBase) Then
                ' Se manca txtParola, prova a dedurre dalla transazione selezionata
                If dgvTransazioniNonClassificate.SelectedRows.Count > 0 Then
                    parolaBase = dgvTransazioniNonClassificate.SelectedRows(0).Cells("Descrizione").Value?.ToString()
                End If
            End If
            If String.IsNullOrWhiteSpace(parolaBase) Then
                MessageBox.Show("Inserisci una Parola oppure seleziona una transazione per proporre un termine.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' UI busy
            progBarAI.Visible = True
            btnAIAssistita.Enabled = False
            btnTrovaSimiliAI.Enabled = False
            btnCreaPatternAI.Enabled = False
            lblAIStatus.Text = "Analisi semantica AI in corso..."
            lblAIStatus.ForeColor = Color.RoyalBlue

            ' Lista pattern esistenti
            Dim tutti As List(Of String) = GetTutteLeParolePattern()
            If tutti.Count = 0 Then
                progBarAI.Visible = False
                btnAIAssistita.Enabled = True
                btnTrovaSimiliAI.Enabled = True
                btnCreaPatternAI.Enabled = True
                lblAIStatus.Text = "Nessun pattern presente in archivio."
                lblAIStatus.ForeColor = Color.DarkOrange
                Return
            End If

            ' Prepara contesto completo della transazione
            Dim descrizioneCompleta As String = ""
            Dim importoTransazione As Decimal = 0
            Dim dataTransazione As Date = Nothing
            
            If dgvTransazioniNonClassificate.SelectedRows.Count > 0 Then
                Dim row = dgvTransazioniNonClassificate.SelectedRows(0)
                descrizioneCompleta = If(row.Cells("Descrizione").Value?.ToString(), "")
                
                If dgvTransazioniNonClassificate.Columns.Contains("Importo") Then
                    Decimal.TryParse(row.Cells("Importo").Value?.ToString(), importoTransazione)
                End If
                
                ' Prova a ottenere la data se disponibile (potrebbe essere in una colonna nascosta o nel database)
                If dgvTransazioniNonClassificate.Columns.Contains("Data") Then
                    DateTime.TryParse(row.Cells("Data").Value?.ToString(), dataTransazione)
                Else
                    ' Se non c'è colonna Data visibile, usa data corrente come approssimazione
                    dataTransazione = Date.Now
                End If
            End If

            ' Chiamata AI super-avanzata con contesto completo
            Dim risultato = Await gptClassificatore.TrovaPatternSimiliAvanzatoConContesto(
                parolaBase, tutti, descrizioneCompleta, importoTransazione, dataTransazione)

            ' Ripristina UI
            progBarAI.Visible = False
            btnAIAssistita.Enabled = True
            btnTrovaSimiliAI.Enabled = True
            btnCreaPatternAI.Enabled = True

            If risultato Is Nothing OrElse risultato.PatternSuggeriti.Count = 0 Then
                lblAIStatus.Text = "Nessun pattern simile trovato dall'AI."
                lblAIStatus.ForeColor = Color.DarkOrange
                Return
            End If

            ' Mostra risultato con interfaccia avanzata
            MostraRisultatoPatternAvanzato(risultato)

        Catch ex As Exception
            progBarAI.Visible = False
            btnAIAssistita.Enabled = True
            btnTrovaSimiliAI.Enabled = True
            btnCreaPatternAI.Enabled = True
            lblAIStatus.Text = "Errore: " & ex.Message
            lblAIStatus.ForeColor = Color.Red
        End Try
    End Sub

    Private Async Sub BtnCreaPatternAI_Click(sender As Object, e As EventArgs)
        Try
            ' Recupera descrizione/importo dalla selezione (se disponibile)
            Dim descr As String = ""
            Dim importo As Decimal = 0D
            If dgvTransazioniNonClassificate.SelectedRows.Count > 0 Then
                descr = dgvTransazioniNonClassificate.SelectedRows(0).Cells("Descrizione").Value?.ToString()
                If dgvTransazioniNonClassificate.Columns.Contains("Importo") Then
                    Dim impStr = dgvTransazioniNonClassificate.SelectedRows(0).Cells("Importo").Value?.ToString()
                    If Not String.IsNullOrWhiteSpace(impStr) Then
                        If Not Decimal.TryParse(impStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.GetCultureInfo("it-IT"), importo) Then
                            Decimal.TryParse(impStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, importo)
                        End If
                    End If
                End If
            End If

            If String.IsNullOrWhiteSpace(descr) Then
                ' In assenza di selezione, usa comunque txtParola per contesto minimo
                descr = txtParola.Text.Trim()
            End If
            If String.IsNullOrWhiteSpace(descr) Then
                MessageBox.Show("Seleziona una transazione o inserisci una Parola per dare contesto all'AI.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' UI busy
            progBarAI.Visible = True
            btnAIAssistita.Enabled = False
            btnTrovaSimiliAI.Enabled = False
            btnCreaPatternAI.Enabled = False
            lblAIStatus.Text = "Creazione pattern con AI..."
            lblAIStatus.ForeColor = Color.RoyalBlue

            Dim macroEsistenti = gptClassificatore.GetMacroCategorieEsistenti()
            Dim sugger = Await gptClassificatore.CreaPatternPersonalizzato(descr, importo, macroEsistenti)

            ' Ripristina UI
            progBarAI.Visible = False
            btnAIAssistita.Enabled = True
            btnTrovaSimiliAI.Enabled = True
            btnCreaPatternAI.Enabled = True

            If sugger Is Nothing OrElse Not sugger.IsValid Then
                lblAIStatus.Text = "Nessuna proposta AI disponibile."
                lblAIStatus.ForeColor = Color.DarkOrange
                Return
            End If

            Dim testo = "Proposta AI:" & vbCrLf &
                    $"Parola: {sugger.ParolaChiave}" & vbCrLf &
                    $"MacroCategoria: {sugger.MacroCategoria}" & vbCrLf &
                    $"Categoria: {sugger.Categoria}" & vbCrLf &
                    $"Confidenza: {sugger.Confidenza}%" & vbCrLf & vbCrLf &
                    "Applicare ai campi del pattern?"
            Dim res = MessageBox.Show(testo, "Crea pattern AI", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If res = DialogResult.Yes Then
                txtParola.Text = sugger.ParolaChiave
                txtMacroCategoria.Text = sugger.MacroCategoria
                txtCategoria.Text = sugger.Categoria
                lblAIStatus.Text = "Pattern proposto applicato ai campi."
                lblAIStatus.ForeColor = Color.DarkGreen
            Else
                lblAIStatus.Text = "Operazione annullata dall'utente."
                lblAIStatus.ForeColor = Color.Gray
            End If

        Catch ex As Exception
            progBarAI.Visible = False
            btnAIAssistita.Enabled = True
            btnTrovaSimiliAI.Enabled = True
            btnCreaPatternAI.Enabled = True
            lblAIStatus.Text = "Errore: " & ex.Message
            lblAIStatus.ForeColor = Color.Red
        End Try
    End Sub

    Private Sub btnAggiungiPattern_Click(sender As Object, e As EventArgs) Handles btnAggiungiPattern.Click
        If transazioneSelezionataID = -1 Then
            MessageBox.Show("Seleziona una transazione dalla lista.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If String.IsNullOrWhiteSpace(txtParola.Text) Then
            MessageBox.Show("Inserisci la parola del pattern.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtParola.Focus()
            Return
        End If

        ' Validazione peso
        Dim peso As Integer = 50
        If Not Integer.TryParse(txtPeso.Text, peso) OrElse peso < 0 OrElse peso > 100 Then
            MessageBox.Show("Il peso deve essere un numero intero tra 0 e 100.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPeso.Focus()
            Return
        End If

        Try
            ' Salva i valori prima di pulire i campi
            Dim parolaPattern = txtParola.Text.Trim()
            Dim macroCategoria = txtMacroCategoria.Text.Trim()
            Dim categoria = txtCategoria.Text.Trim()

            ' 1) Salva il pattern nel database
            SalvaPattern()

            ' 2) Notifica che i pattern sono cambiati
            RaiseEvent PatternsChanged()

            ' 3) Log e pulizia
            ScriviLogPatternManuale()
            
            ' 3) Conta quante transazioni verranno classificate con questo pattern
            Dim numeroTransazioni = ContaTransazioniPerPattern(parolaPattern)
            
            ' 4) Pulisci i campi dopo aver contato
            PulisciCampiPattern()
            
            ' 5) Chiedi conferma per classificare tutte le transazioni che matchano
            Dim conferma = MessageBox.Show(
            "Pattern aggiunto con successo!" & vbCrLf & vbCrLf &
            $"Vuoi classificare ORA tutte le transazioni che contengono '{parolaPattern}'?" & vbCrLf &
            $"Verranno classificate {numeroTransazioni} transazione/i.",
            "Classifica transazioni",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

            If conferma = DialogResult.Yes Then
                ' Usa il classificatore per applicare il nuovo pattern a tutte le transazioni
                Dim transazioniClassificate = ClassificaTransazioniConNuovoPattern()
                
                ' Ricarica la lista delle transazioni non classificate
                CaricaTransazioniNonClassificate()
                
                If transazioniClassificate > 0 Then
                    MessageBox.Show($"Successo! Classificate {transazioniClassificate} transazioni." & vbCrLf &
                                  "La tabella è stata aggiornata.", 
                                  "Classificazione completata", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    
                    ' Notifica il FormFinanza che le transazioni sono state classificate
                    RaiseEvent TransactionsClassified(transazioniClassificate)
                Else
                    MessageBox.Show("Nessuna transazione è stata classificata.", 
                                  "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            Else
                MessageBox.Show("Pattern aggiunto. Per classificare le transazioni puoi usare il pulsante 'Classifica Categorie' nel FormFinanza.", 
                              "Pattern salvato", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ' Ricarica la tabella comunque per aggiornare la vista
                CaricaTransazioniNonClassificate()
            End If

            ' Reset della selezione
            transazioneSelezionataID = -1
        Catch ex As Exception
            MessageBox.Show("Errore durante il salvataggio: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SalvaPattern()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim query As String = "INSERT INTO Pattern (Parola, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita, Fonte, Peso) " &
                          "VALUES (@parola, @macroCat, @cat, @necessita, @frequenza, @stagionalita, @fonte, @peso)"
            Using cmd As New SQLiteCommand(query, conn)
                cmd.Parameters.AddWithValue("@parola", txtParola.Text.Trim())
                cmd.Parameters.AddWithValue("@macroCat", txtMacroCategoria.Text.Trim())
                cmd.Parameters.AddWithValue("@cat", txtCategoria.Text.Trim())
                cmd.Parameters.AddWithValue("@necessita", If(cmbNecessita.SelectedItem?.ToString(), ""))
                cmd.Parameters.AddWithValue("@frequenza", If(cmbFrequenza.SelectedItem?.ToString(), ""))
                cmd.Parameters.AddWithValue("@stagionalita", If(cmbStagionalita.SelectedItem?.ToString(), ""))
                cmd.Parameters.AddWithValue("@fonte", "Applicazione")
                Dim peso As Integer = 50
                Integer.TryParse(txtPeso.Text, peso)
                cmd.Parameters.AddWithValue("@peso", peso)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Sub ScriviLogPatternManuale()
        Try
            Dim logPath As String = "pattern_manuali.txt"
            Using sw As New System.IO.StreamWriter(logPath, True)
                Dim linea As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ID Transazione: {transazioneSelezionataID} - Parola assegnata manualmente: {txtParola.Text.Trim()}"
                sw.WriteLine(linea)
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore scrittura log: " & ex.Message)
        End Try
    End Sub

    Private Sub PulisciCampiPattern()
        txtParola.Clear()
        txtMacroCategoria.Clear()
        txtCategoria.Clear()
        ' RIMUOVI txtSottoCategoria.Clear()
        cmbNecessita.SelectedIndex = -1
        cmbFrequenza.SelectedIndex = -1
        cmbStagionalita.SelectedIndex = -1
        txtPeso.Text = "50"
        cmbCombinazioni.SelectedIndex = 0
        transazioneSelezionataID = -1
    End Sub

    Private Sub BtnChiudi_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    ' Classe helper per ComboBox
    Private Class ComboItem
        Public Property Text As String
        Public Property MacroCategoria As String
        Public Property Categoria As String
        ' RIMUOVI Public Property SottoCategoria As String
        Public Property Necessita As String
        Public Property Frequenza As String
        Public Property Stagionalita As String

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    ' Dichiarazione controlli
    Private WithEvents dgvTransazioniNonClassificate As DataGridView
    Private txtParola As TextBox
    Private txtMacroCategoria As TextBox
    Private txtCategoria As TextBox
    'Private txtSottoCategoria As TextBox
    Private WithEvents cmbNecessita As ComboBox
    Private WithEvents btnGestisciNecessita As Button
    Private WithEvents btnGestisciFrequenza As Button
    Private WithEvents cmbFrequenza As ComboBox
    Private WithEvents cmbStagionalita As ComboBox
    Private WithEvents btnGestisciStagionalita As Button
    Private txtPeso As TextBox
    Private WithEvents cmbCombinazioni As ComboBox
    Private WithEvents btnAggiungiPattern As Button

    ' Funzione per contare le transazioni che matcheranno un pattern
    Private Function ContaTransazioniPerPattern(parolaPattern As String) As Integer
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim query As String = "
                SELECT COUNT(*) FROM Transazioni 
                WHERE (MacroCategoria IS NULL OR MacroCategoria = '') 
                AND UPPER(Descrizione) LIKE @pattern"
                Using cmd As New SQLiteCommand(query, conn)
                    cmd.Parameters.AddWithValue("@pattern", $"%{parolaPattern.ToUpper()}%")
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Errore nel conteggio transazioni: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return 0
        End Try
    End Function

    ' Funzione per classificare tutte le transazioni usando il nuovo pattern
    Private Function ClassificaTransazioniConNuovoPattern() As Integer
        Try
            ' Usa il classificatore migliorato per applicare tutti i pattern esistenti
            Return ClassificatoreTransazioniMigliorato.ClassificaTutteLeTransazioniMigliorate()
        Catch ex As Exception
            MessageBox.Show($"Errore durante la classificazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return 0
        End Try
    End Function

    ' Mostra il risultato della ricerca pattern con interfaccia avanzata
    Private Sub MostraRisultatoPatternAvanzato(risultato As GptClassificatoreTransazioni.RisultatoPatternSimili)
        ' Costruisci messaggio dettagliato
        Dim sb As New Text.StringBuilder()
        
        ' Analisi AI
        If Not String.IsNullOrWhiteSpace(risultato.Analisi) Then
            sb.AppendLine("🧠 ANALISI AI:")
            sb.AppendLine(risultato.Analisi)
            sb.AppendLine()
        End If
        
        ' Pattern suggeriti
        sb.AppendLine("🔍 PATTERN SIMILI TROVATI:")
        sb.AppendLine()
        
        For i = 0 To Math.Min(risultato.PatternSuggeriti.Count - 1, 4) ' Max 5 pattern
            Dim p = risultato.PatternSuggeriti(i)
            sb.AppendLine($"{i + 1}) 📝 {p.Parola}")
            sb.AppendLine($"   📊 Confidenza: {p.Confidenza}%")
            sb.AppendLine($"   🎯 Categoria: {p.MacroCategoria}/{p.Categoria}")
            sb.AppendLine($"   📈 Transazioni potenziali: {p.TransazioniPotenziali}")
            sb.AppendLine($"   💡 Motivo: {p.Motivazione}")
            sb.AppendLine()
        Next
        
        sb.AppendLine("Scegli un'opzione:")
        sb.AppendLine("• SÌ = Applica il primo suggerimento")
        sb.AppendLine("• NO = Seleziona manualmente")
        sb.AppendLine("• ANNULLA = Torna indietro")
        
        ' Mostra dialog
        Dim res = MessageBox.Show(sb.ToString(), "🤖 Pattern Simili - Analisi AI Avanzata", 
                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information)
        
        Select Case res
            Case DialogResult.Yes
                ' Applica primo suggerimento
                ApplicaPatternSuggerito(risultato.PatternSuggeriti(0))
                lblAIStatus.Text = "✅ Pattern AI applicato automaticamente"
                lblAIStatus.ForeColor = Color.DarkGreen
                
            Case DialogResult.No
                ' Selezione manuale con InputBox migliorato
                Dim scelta = InputBox($"Digita il numero del pattern da applicare (1-{risultato.PatternSuggeriti.Count}):", 
                                    "Seleziona Pattern", "1")
                Dim idx As Integer
                If Integer.TryParse(scelta, idx) AndAlso idx >= 1 AndAlso idx <= risultato.PatternSuggeriti.Count Then
                    ApplicaPatternSuggerito(risultato.PatternSuggeriti(idx - 1))
                    lblAIStatus.Text = $"✅ Pattern #{idx} applicato manualmente"
                    lblAIStatus.ForeColor = Color.DarkGreen
                Else
                    lblAIStatus.Text = "❌ Selezione non valida"
                    lblAIStatus.ForeColor = Color.DarkOrange
                End If
                
            Case Else
                lblAIStatus.Text = "⏸️ Operazione annullata"
                lblAIStatus.ForeColor = Color.Gray
        End Select
    End Sub

    ' Applica un pattern suggerito ai campi del form
    Private Sub ApplicaPatternSuggerito(suggerimento As GptClassificatoreTransazioni.SuggerimentoPatternAvanzato)
        txtParola.Text = suggerimento.Parola
        
        If Not String.IsNullOrWhiteSpace(suggerimento.MacroCategoria) Then
            txtMacroCategoria.Text = suggerimento.MacroCategoria
        End If
        
        If Not String.IsNullOrWhiteSpace(suggerimento.Categoria) Then
            txtCategoria.Text = suggerimento.Categoria
        End If
        
        ' Prova a selezionare la combinazione corrispondente
        SelezionaCombinazionePiùVicina(suggerimento.MacroCategoria, suggerimento.Categoria)
    End Sub
End Class
