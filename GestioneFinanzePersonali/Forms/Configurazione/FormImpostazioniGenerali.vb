Imports System.Globalization
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Windows.Forms

Public Class FormImpostazioniGenerali ' Rinominato da FormConfigurazioneStipendi
    Private configurazione As GestoreStipendi.ConfigurazioneStipendio
    Private dgvEccezioni As DataGridView
    Private dgvAnteprima As DataGridView
    Private numGiornoDefault As NumericUpDown
    Private rbAnticipa As RadioButton
    Private rbPosticipa As RadioButton
    Private rbIgnora As RadioButton

    ' Controlli per API
    Private txtOpenAiKey As TextBox
    Private txtGooglePlacesKey As TextBox
    Private btnTestOpenAi As Button
    Private btnTestGooglePlaces As Button
    Private lblOpenAiStatus As Label
    Private lblGooglePlacesStatus As Label

    Public Sub New()
        InitializeComponent()
        GestoreApiKeys.InizializzaTabella() ' Assicura che la tabella esista
        ImpostaInterfaccia()
        CaricaConfigurazioneCorrente()
        CaricaChiaviApi()
        AggiornaAnteprima()
    End Sub

    Private Sub ImpostaInterfaccia()
        Me.Text = "Impostazioni Generali"
        Me.Size = New Size(950, 750)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        ' TabControl principale
        Dim tabControl As New System.Windows.Forms.TabControl()
        tabControl.Dock = System.Windows.Forms.DockStyle.Fill
        tabControl.Size = New System.Drawing.Size(950, 750)
        tabControl.Location = New System.Drawing.Point(10, 10)
        ' Invece di Padding, usa Margin (che è più compatibile)
        tabControl.Margin = New System.Windows.Forms.Padding(10)

        ' === TAB 1: STIPENDIO ===
        Dim tabStipendio As New TabPage("Imposta giorno Stipendio ed eccezioni")
        CreaTabStipendio(tabStipendio)
        tabControl.TabPages.Add(tabStipendio)

        ' === TAB 2: API ===
        Dim tabApi As New TabPage("Gestisci API")
        CreaTabApi(tabApi)
        tabControl.TabPages.Add(tabApi)

        Me.Controls.Add(tabControl)

        ' Bottoni in fondo
        Dim pnlBottoni As New Panel() With {
            .Size = New Size(950, 50),
            .Dock = DockStyle.Bottom
        }

        Dim btnSalva As New Button() With {
            .Text = "Salva Tutte le Impostazioni",
            .Size = New Size(180, 35),
            .Location = New Point(650, 7),
            .BackColor = Color.FromArgb(52, 152, 219),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        AddHandler btnSalva.Click, AddressOf SalvaTutteLeImpostazioni
        pnlBottoni.Controls.Add(btnSalva)

        Dim btnAnnulla As New Button() With {
            .Text = "Annulla",
            .Size = New Size(100, 35),
            .Location = New Point(840, 7),
            .DialogResult = DialogResult.Cancel
        }
        pnlBottoni.Controls.Add(btnAnnulla)

        Me.Controls.Add(pnlBottoni)
    End Sub

    Private Sub CreaTabStipendio(tab As TabPage)
        Dim pnlStipendio As New Panel() With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}

        ' === SEZIONE CONFIGURAZIONE BASE ===
        Dim grpBase As New GroupBox() With {
            .Text = "Configurazione Base",
            .Size = New Size(860, 120),
            .Location = New Point(10, 10)
        }

        Dim lblGiorno As New Label() With {
            .Text = "Giorno stipendio di default:",
            .Location = New Point(20, 30),
            .AutoSize = True
        }
        grpBase.Controls.Add(lblGiorno)

        numGiornoDefault = New NumericUpDown() With {
            .Location = New Point(200, 27),
            .Size = New Size(60, 25),
            .Minimum = 1,
            .Maximum = 31,
            .Value = 23
        }
        AddHandler numGiornoDefault.ValueChanged, AddressOf AggiornaAnteprima
        grpBase.Controls.Add(numGiornoDefault)

        Dim lblRegole As New Label() With {
            .Text = "Quando cade nel weekend:",
            .Location = New Point(20, 70),
            .AutoSize = True
        }
        grpBase.Controls.Add(lblRegole)

        rbAnticipa = New RadioButton() With {
            .Text = "Anticipa al venerdì",
            .Location = New Point(200, 68),
            .AutoSize = True,
            .Checked = True
        }
        AddHandler rbAnticipa.CheckedChanged, AddressOf AggiornaAnteprima
        grpBase.Controls.Add(rbAnticipa)

        rbPosticipa = New RadioButton() With {
            .Text = "Posticipa al lunedì",
            .Location = New Point(350, 68),
            .AutoSize = True
        }
        AddHandler rbPosticipa.CheckedChanged, AddressOf AggiornaAnteprima
        grpBase.Controls.Add(rbPosticipa)

        rbIgnora = New RadioButton() With {
            .Text = "Ignora weekend",
            .Location = New Point(500, 68),
            .AutoSize = True
        }
        AddHandler rbIgnora.CheckedChanged, AddressOf AggiornaAnteprima
        grpBase.Controls.Add(rbIgnora)

        pnlStipendio.Controls.Add(grpBase)

        ' === SEZIONE ECCEZIONI ===
        Dim grpEccezioni As New GroupBox() With {
            .Text = "Eccezioni Mensili",
            .Size = New Size(860, 200),
            .Location = New Point(10, 140)
        }

        dgvEccezioni = New DataGridView() With {
            .Size = New Size(830, 150),
            .Location = New Point(15, 25),
            .AllowUserToDeleteRows = True,
            .AutoGenerateColumns = False
        }

        dgvEccezioni.Columns.AddRange({
            New DataGridViewComboBoxColumn() With {
                .Name = "Mese", .HeaderText = "Mese", .Width = 150,
                .DataSource = Enumerable.Range(1, 12).Select(Function(m) New With {.Value = m, .Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)}).ToList(),
                .DisplayMember = "Text", .ValueMember = "Value"
            },
            New DataGridViewTextBoxColumn() With {.Name = "Giorno", .HeaderText = "Giorno Speciale", .Width = 120},
            New DataGridViewTextBoxColumn() With {.Name = "Descrizione", .HeaderText = "Descrizione", .Width = 300}
        })

        AddHandler dgvEccezioni.CellValueChanged, AddressOf AggiornaAnteprima
        grpEccezioni.Controls.Add(dgvEccezioni)

        Dim btnAggiungiEccezione As New Button() With {
            .Text = "Aggiungi Eccezione",
            .Location = New Point(680, 25),
            .Size = New Size(120, 30)
        }
        AddHandler btnAggiungiEccezione.Click, AddressOf AggiungiEccezione
        grpEccezioni.Controls.Add(btnAggiungiEccezione)

        pnlStipendio.Controls.Add(grpEccezioni)

        ' === ANTEPRIMA ===
        Dim grpAnteprima As New GroupBox() With {
            .Text = "Anteprima Anno Corrente",
            .Size = New Size(860, 220),
            .Location = New Point(10, 350)
        }

        dgvAnteprima = New DataGridView() With {
            .Size = New Size(830, 180),
            .Location = New Point(15, 25),
            .ReadOnly = True,
            .AutoGenerateColumns = False,
            .AllowUserToAddRows = False
        }

        dgvAnteprima.Columns.AddRange({
            New DataGridViewTextBoxColumn() With {.Name = "Mese", .HeaderText = "Mese", .Width = 100},
            New DataGridViewTextBoxColumn() With {.Name = "PayDate", .HeaderText = "Data Stipendio", .Width = 120},
            New DataGridViewTextBoxColumn() With {.Name = "Periodo", .HeaderText = "Periodo Gestionale", .Width = 200},
            New DataGridViewTextBoxColumn() With {.Name = "GiornoSettimana", .HeaderText = "Giorno", .Width = 100},
            New DataGridViewTextBoxColumn() With {.Name = "Note", .HeaderText = "Note", .Width = 200}
        })

        grpAnteprima.Controls.Add(dgvAnteprima)
        pnlStipendio.Controls.Add(grpAnteprima)

        tab.Controls.Add(pnlStipendio)
    End Sub

    Private Sub CreaTabApi(tab As TabPage)
        Dim pnlApi As New Panel() With {.Dock = DockStyle.Fill, .Padding = New Padding(20)}

        ' === OPENAI API ===
        Dim grpOpenAi As New GroupBox() With {
            .Text = "OpenAI API (ChatGPT)",
            .Size = New Size(860, 180),
            .Location = New Point(20, 20)
        }

        Dim lblOpenAiDesc As New Label() With {
            .Text = "Necessaria per la classificazione automatica delle transazioni tramite AI.",
            .Location = New Point(20, 25),
            .Size = New Size(800, 20),
            .ForeColor = Color.DarkBlue
        }
        grpOpenAi.Controls.Add(lblOpenAiDesc)

        Dim lblOpenAiKey As New Label() With {
            .Text = "Chiave API OpenAI:",
            .Location = New Point(20, 55),
            .AutoSize = True
        }
        grpOpenAi.Controls.Add(lblOpenAiKey)

        txtOpenAiKey = New TextBox() With {
            .Location = New Point(150, 53),
            .Size = New Size(400, 25),
            .UseSystemPasswordChar = True
        }
        grpOpenAi.Controls.Add(txtOpenAiKey)

        btnTestOpenAi = New Button() With {
            .Text = "Test Connessione",
            .Location = New Point(570, 52),
            .Size = New Size(120, 27)
        }
        AddHandler btnTestOpenAi.Click, AddressOf TestConnessioneOpenAi
        grpOpenAi.Controls.Add(btnTestOpenAi)

        lblOpenAiStatus = New Label() With {
            .Location = New Point(700, 55),
            .Size = New Size(120, 25),
            .Text = "Non testata",
            .ForeColor = Color.Gray
        }
        grpOpenAi.Controls.Add(lblOpenAiStatus)

        Dim lblOpenAiInfo As New Label() With {
            .Text = "Come ottenere la chiave:" & vbCrLf &
                   "1. Vai su https://platform.openai.com" & vbCrLf &
                   "2. Accedi al tuo account OpenAI" & vbCrLf &
                   "3. Vai su 'API Keys' nel menu" & vbCrLf &
                   "4. Clicca 'Create new secret key'" & vbCrLf &
                   "5. Copia e incolla qui la chiave generata",
            .Location = New Point(20, 85),
            .Size = New Size(800, 80),
            .ForeColor = Color.DarkGreen
        }
        grpOpenAi.Controls.Add(lblOpenAiInfo)

        pnlApi.Controls.Add(grpOpenAi)

        ' === GOOGLE PLACES API ===
        Dim grpGooglePlaces As New GroupBox() With {
            .Text = "Google Places API",
            .Size = New Size(860, 180),
            .Location = New Point(20, 220)
        }

        Dim lblGoogleDesc As New Label() With {
            .Text = "Necessaria per migliorare la classificazione ricercando informazioni sulle attività commerciali.",
            .Location = New Point(20, 25),
            .Size = New Size(800, 20),
            .ForeColor = Color.DarkBlue
        }
        grpGooglePlaces.Controls.Add(lblGoogleDesc)

        Dim lblGoogleKey As New Label() With {
            .Text = "Chiave API Google:",
            .Location = New Point(20, 55),
            .AutoSize = True
        }
        grpGooglePlaces.Controls.Add(lblGoogleKey)

        txtGooglePlacesKey = New TextBox() With {
            .Location = New Point(150, 53),
            .Size = New Size(400, 25),
            .UseSystemPasswordChar = True
        }
        grpGooglePlaces.Controls.Add(txtGooglePlacesKey)

        btnTestGooglePlaces = New Button() With {
            .Text = "Test Connessione",
            .Location = New Point(570, 52),
            .Size = New Size(120, 27)
        }
        AddHandler btnTestGooglePlaces.Click, AddressOf TestConnessioneGooglePlaces
        grpGooglePlaces.Controls.Add(btnTestGooglePlaces)

        lblGooglePlacesStatus = New Label() With {
            .Location = New Point(700, 55),
            .Size = New Size(120, 25),
            .Text = "Non testata",
            .ForeColor = Color.Gray
        }
        grpGooglePlaces.Controls.Add(lblGooglePlacesStatus)

        Dim lblGoogleInfo As New Label() With {
            .Text = "Come ottenere la chiave:" & vbCrLf &
                   "1. Vai su https://console.cloud.google.com" & vbCrLf &
                   "2. Crea un progetto o selezionane uno esistente" & vbCrLf &
                   "3. Abilita 'Places API (New)'" & vbCrLf &
                   "4. Vai su 'Credenziali' → 'Crea credenziali' → 'Chiave API'" & vbCrLf &
                   "5. Copia e incolla qui la chiave generata",
            .Location = New Point(20, 85),
            .Size = New Size(800, 80),
            .ForeColor = Color.DarkGreen
        }
        grpGooglePlaces.Controls.Add(lblGoogleInfo)

        pnlApi.Controls.Add(grpGooglePlaces)

        tab.Controls.Add(pnlApi)
    End Sub

    Private Sub CaricaChiaviApi()
        txtOpenAiKey.Text = GestoreApiKeys.CaricaChiaveApi("OpenAI")
        txtGooglePlacesKey.Text = GestoreApiKeys.CaricaChiaveApi("GooglePlaces")

        ' Aggiorna stato visuale
        If Not String.IsNullOrWhiteSpace(txtOpenAiKey.Text) Then
            lblOpenAiStatus.Text = "Configurata"
            lblOpenAiStatus.ForeColor = Color.Green
        End If

        If Not String.IsNullOrWhiteSpace(txtGooglePlacesKey.Text) Then
            lblGooglePlacesStatus.Text = "Configurata"
            lblGooglePlacesStatus.ForeColor = Color.Green
        End If
    End Sub

    Private Async Sub TestConnessioneOpenAi(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtOpenAiKey.Text) Then
            MessageBox.Show("Inserisci prima una chiave API OpenAI.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        btnTestOpenAi.Enabled = False
        lblOpenAiStatus.Text = "Testando..."
        lblOpenAiStatus.ForeColor = Color.Orange

        Try
            ' Test semplice con OpenAI
            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {txtOpenAiKey.Text}")

                Dim requestBody = New Dictionary(Of String, Object) From {
                    {"model", "gpt-4o-mini"},
                    {"messages", New Object() {New Dictionary(Of String, String) From {
                        {"role", "user"},
                        {"content", "Test"}
                    }}},
                    {"max_tokens", 5}
                }

                Dim json = JsonSerializer.Serialize(requestBody)
                Using content = New StringContent(json, Encoding.UTF8, "application/json")
                    Dim response = Await client.PostAsync("https://api.openai.com/v1/chat/completions", content)

                    If response.IsSuccessStatusCode Then
                        lblOpenAiStatus.Text = "✓ Funzionante"
                        lblOpenAiStatus.ForeColor = Color.Green
                        MessageBox.Show("Connessione OpenAI testata con successo!", "Test Riuscito", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        lblOpenAiStatus.Text = "✗ Errore"
                        lblOpenAiStatus.ForeColor = Color.Red
                        MessageBox.Show($"Errore nella connessione: {response.StatusCode}", "Test Fallito", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End Using
            End Using
        Catch ex As Exception
            lblOpenAiStatus.Text = "✗ Errore"
            lblOpenAiStatus.ForeColor = Color.Red
            MessageBox.Show($"Errore nel test: {ex.Message}", "Test Fallito", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnTestOpenAi.Enabled = True
        End Try
    End Sub

    Private Async Sub TestConnessioneGooglePlaces(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtGooglePlacesKey.Text) Then
            MessageBox.Show("Inserisci prima una chiave API Google Places.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        btnTestGooglePlaces.Enabled = False
        lblGooglePlacesStatus.Text = "Testando..."
        lblGooglePlacesStatus.ForeColor = Color.Orange

        Try
            Using client As New HttpClient()
                Using req As New HttpRequestMessage(HttpMethod.Post, "https://places.googleapis.com/v1/places:searchText")
                    Dim body = New With {.textQuery = "test"}
                    req.Content = New StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                    req.Headers.Add("X-Goog-Api-Key", txtGooglePlacesKey.Text)
                    req.Headers.Add("X-Goog-FieldMask", "places.displayName")

                    Dim resp = Await client.SendAsync(req)

                    If resp.IsSuccessStatusCode Then
                        lblGooglePlacesStatus.Text = "✓ Funzionante"
                        lblGooglePlacesStatus.ForeColor = Color.Green
                        MessageBox.Show("Connessione Google Places testata con successo!", "Test Riuscito", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        lblGooglePlacesStatus.Text = "✗ Errore"
                        lblGooglePlacesStatus.ForeColor = Color.Red
                        MessageBox.Show($"Errore nella connessione: {resp.StatusCode}", "Test Fallito", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End Using
            End Using
        Catch ex As Exception
            lblGooglePlacesStatus.Text = "✗ Errore"
            lblGooglePlacesStatus.ForeColor = Color.Red
            MessageBox.Show($"Errore nel test: {ex.Message}", "Test Fallito", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnTestGooglePlaces.Enabled = True
        End Try
    End Sub

    ' Tutti i metodi esistenti per stipendio rimangono invariati...
    Private Sub CaricaConfigurazioneCorrente()
        configurazione = GestoreStipendi.CaricaConfigurazione()
        numGiornoDefault.Value = configurazione.GiornoDefault
        Select Case configurazione.RegoleWeekend
            Case GestoreStipendi.RegoleWeekend.ANTICIPA : rbAnticipa.Checked = True
            Case GestoreStipendi.RegoleWeekend.POSTICIPA : rbPosticipa.Checked = True
            Case GestoreStipendi.RegoleWeekend.IGNORA : rbIgnora.Checked = True
        End Select
        dgvEccezioni.Rows.Clear()
        For Each eccezione In configurazione.EccezioniMensili
            Dim descrizione = If(eccezione.Key = 12, "Tredicesima", "Eccezione speciale")
            dgvEccezioni.Rows.Add(eccezione.Key, eccezione.Value, descrizione)
        Next
    End Sub

    Private Sub AggiungiEccezione(sender As Object, e As EventArgs)
        dgvEccezioni.Rows.Add(1, 15, "Nuova eccezione")
        AggiornaAnteprima()
    End Sub

    ' Metodo aggiornato per salvare tutto
    Private Sub SalvaTutteLeImpostazioni(sender As Object, e As EventArgs)
        Try
            ' Salva configurazione stipendio (codice esistente)
            Dim nuovaConfig As New GestoreStipendi.ConfigurazioneStipendio() With {
                .GiornoDefault = CInt(numGiornoDefault.Value)
            }
            If rbAnticipa.Checked Then nuovaConfig.RegoleWeekend = GestoreStipendi.RegoleWeekend.ANTICIPA
            If rbPosticipa.Checked Then nuovaConfig.RegoleWeekend = GestoreStipendi.RegoleWeekend.POSTICIPA
            If rbIgnora.Checked Then nuovaConfig.RegoleWeekend = GestoreStipendi.RegoleWeekend.IGNORA

            For Each row As DataGridViewRow In dgvEccezioni.Rows
                If Not row.IsNewRow AndAlso row.Cells("Mese").Value IsNot Nothing AndAlso row.Cells("Giorno").Value IsNot Nothing Then
                    Try
                        Dim mese As Integer = Convert.ToInt32(row.Cells("Mese").Value)
                        Dim giorno As Integer = Convert.ToInt32(row.Cells("Giorno").Value)
                        If giorno >= 1 And giorno <= 31 Then
                            nuovaConfig.EccezioniMensili(mese) = giorno
                        End If
                    Catch
                    End Try
                End If
            Next

            GestoreStipendi.SalvaConfigurazione(nuovaConfig)

            ' Salva chiavi API
            If Not String.IsNullOrWhiteSpace(txtOpenAiKey.Text) Then
                GestoreApiKeys.SalvaChiaveApi("OpenAI", txtOpenAiKey.Text)
            End If

            If Not String.IsNullOrWhiteSpace(txtGooglePlacesKey.Text) Then
                GestoreApiKeys.SalvaChiaveApi("GooglePlaces", txtGooglePlacesKey.Text)
            End If

            MessageBox.Show("Tutte le impostazioni sono state salvate con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show($"Errore nel salvataggio: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Metodi esistenti per anteprima stipendio rimangono invariati...
    Private Sub AggiornaAnteprima()
        If dgvAnteprima Is Nothing Then Return
        Dim configTemp As New GestoreStipendi.ConfigurazioneStipendio() With {
            .GiornoDefault = CInt(numGiornoDefault.Value)
        }
        If rbAnticipa.Checked Then configTemp.RegoleWeekend = GestoreStipendi.RegoleWeekend.ANTICIPA
        If rbPosticipa.Checked Then configTemp.RegoleWeekend = GestoreStipendi.RegoleWeekend.POSTICIPA
        If rbIgnora.Checked Then configTemp.RegoleWeekend = GestoreStipendi.RegoleWeekend.IGNORA
        For Each row As DataGridViewRow In dgvEccezioni.Rows
            If Not row.IsNewRow AndAlso row.Cells("Mese").Value IsNot Nothing AndAlso row.Cells("Giorno").Value IsNot Nothing Then
                Try
                    Dim mese As Integer = Convert.ToInt32(row.Cells("Mese").Value)
                    Dim giorno As Integer = Convert.ToInt32(row.Cells("Giorno").Value)
                    configTemp.EccezioniMensili(mese) = giorno
                Catch
                End Try
            End If
        Next
        Dim annoCorrente = Date.Today.Year
        dgvAnteprima.Rows.Clear()
        For mese As Integer = 1 To 12
            Try
                Dim payDate = SimulaCalcoloPayDate(annoCorrente, mese, configTemp)
                Dim periodo = SimulaCalcoloPeriodo(annoCorrente, mese, configTemp)
                Dim nomeMese = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mese)
                Dim giornoSettimana = payDate.ToString("dddd", CultureInfo.CurrentCulture)
                Dim note As String = ""
                If configTemp.EccezioniMensili.ContainsKey(mese) Then
                    note = "Eccezione attiva"
                End If
                If payDate.DayOfWeek = DayOfWeek.Saturday OrElse payDate.DayOfWeek = DayOfWeek.Sunday Then
                    note += If(String.IsNullOrEmpty(note), "", " | ") + "Weekend gestito"
                End If
                dgvAnteprima.Rows.Add(nomeMese, payDate.ToString("dd/MM/yyyy"), $"{periodo.DataInizio:dd/MM} - {periodo.DataFine:dd/MM}", giornoSettimana, note)
            Catch ex As Exception
                dgvAnteprima.Rows.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mese), "ERRORE", "ERRORE", "", ex.Message)
            End Try
        Next
    End Sub

    Private Function SimulaCalcoloPayDate(anno As Integer, mese As Integer, config As GestoreStipendi.ConfigurazioneStipendio) As Date
        Dim giornoBase As Integer = config.GiornoDefault
        If config.EccezioniMensili.ContainsKey(mese) Then
            giornoBase = config.EccezioniMensili(mese)
        End If
        Dim payDate As New Date(anno, mese, Math.Min(giornoBase, DateTime.DaysInMonth(anno, mese)))
        Select Case config.RegoleWeekend
            Case GestoreStipendi.RegoleWeekend.ANTICIPA
                If payDate.DayOfWeek = DayOfWeek.Saturday Then payDate = payDate.AddDays(-1)
                If payDate.DayOfWeek = DayOfWeek.Sunday Then payDate = payDate.AddDays(-2)
            Case GestoreStipendi.RegoleWeekend.POSTICIPA
                If payDate.DayOfWeek = DayOfWeek.Saturday Then payDate = payDate.AddDays(2)
                If payDate.DayOfWeek = DayOfWeek.Sunday Then payDate = payDate.AddDays(1)
        End Select
        Return payDate
    End Function

    Private Function SimulaCalcoloPeriodo(anno As Integer, mese As Integer, config As GestoreStipendi.ConfigurazioneStipendio) As (DataInizio As Date, DataFine As Date)
        Dim mesePrec As Integer = If(mese = 1, 12, mese - 1)
        Dim annoPrec As Integer = If(mese = 1, anno - 1, anno)
        Dim payDatePrecedente = SimulaCalcoloPayDate(annoPrec, mesePrec, config)
        Dim payDateCorrente = SimulaCalcoloPayDate(anno, mese, config)
        Return (payDatePrecedente, payDateCorrente.AddDays(-1))
    End Function
End Class
