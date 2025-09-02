Imports System.Data
Imports System.Data.SQLite
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports ExcelDataReader
Imports GestioneFinanzePersonali.ImportatoreUniversale
Imports GestioneFinanzePersonali.Models
Imports GestioneFinanzePersonali.Logging
Imports OfficeOpenXml
Public Class FormFinanza
    Private _isFilterActive As Boolean = False
    Private _filterStartDate As DateTime
    Private _filterEndDate As DateTime
    Private _filterMacroCategory As String = ""
    Private _filterCategory As String = ""
    Private _filterSearchTerm As String = ""
    Private _isDetailedWordFilterActive As Boolean = False
    Private _detailedFilterMacroCategory As String = ""
    Private _detailedFilterCategory As String = ""
    
    ' Legacy property mappings for backward compatibility
    Private Property FiltroAttivo As Boolean
        Get
            Return _isFilterActive
        End Get
        Set(value As Boolean)
            _isFilterActive = value
        End Set
    End Property
    
    Private Property FiltroMacro As String
        Get
            Return _filterMacroCategory
        End Get
        Set(value As String)
            _filterMacroCategory = value
        End Set
    End Property
    
    Private Property FiltroCat As String
        Get
            Return _filterCategory
        End Get
        Set(value As String)
            _filterCategory = value
        End Set
    End Property
    
    Private Property filtroParola As String
        Get
            Return _filterSearchTerm
        End Get
        Set(value As String)
            _filterSearchTerm = value
        End Set
    End Property
    
    Private Property FiltroParolaDettagliatoAttivo As Boolean
        Get
            Return _isDetailedWordFilterActive
        End Get
        Set(value As Boolean)
            _isDetailedWordFilterActive = value
        End Set
    End Property
    
    Private Property FiltroParolaDettagliataMacro As String
        Get
            Return _detailedFilterMacroCategory
        End Get
        Set(value As String)
            _detailedFilterMacroCategory = value
        End Set
    End Property
    
    Private Property FiltroParolaDettagliataCategoria As String
        Get
            Return _detailedFilterCategory
        End Get
        Set(value As String)
            _detailedFilterCategory = value
        End Set
    End Property
    
    Private Property FiltroInizio As DateTime
        Get
            Return _filterStartDate
        End Get
        Set(value As DateTime)
            _filterStartDate = value
        End Set
    End Property
    
    Private Property FiltroFine As DateTime
        Get
            Return _filterEndDate
        End Get
        Set(value As DateTime)
            _filterEndDate = value
        End Set
    End Property

    Private WithEvents _monthDateTimePicker As New DateTimePicker With {
        .Format = DateTimePickerFormat.Custom,
        .CustomFormat = "MM/yyyy",
        .ShowUpDown = True,
        .Value = DateTime.Now
    }

    Public Sub SetWordFilter(searchTerm As String)
        _filterSearchTerm = searchTerm
        CaricaTransazioni()
    End Sub

    Public Sub SetDetailedWordFilter(searchTerm As String, macroCategory As String, category As String)
        _isDetailedWordFilterActive = True
        _filterSearchTerm = searchTerm
        _detailedFilterMacroCategory = macroCategory
        _detailedFilterCategory = category

        ' Reset other filters
        _isFilterActive = False

        CaricaTransazioni()
    End Sub

    ' Legacy method names for backward compatibility
    Public Sub ImpostaFiltroParola(searchTerm As String)
        SetWordFilter(searchTerm)
    End Sub
    
    Public Sub ImpostaFiltroParolaDettagliato(searchTerm As String, macroCategory As String, category As String)
        SetDetailedWordFilter(searchTerm, macroCategory, category)
    End Sub

    Private Sub FormFinanza_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ExcelPackage.License.SetNonCommercialPersonal("Gestione Finanze Personali")
        
        ' Sottoscrivi agli eventi dei pattern per aggiornare automaticamente la tabella
        AddHandler FormRaffinaPattern.PatternsChanged, AddressOf OnPatternsChanged
        AddHandler FormRaffinaPattern.TransactionsClassified, AddressOf OnTransactionsClassified
        AddHandler FormGestionePattern.PatternsChanged, AddressOf OnPatternsChanged
        
        ' Controlla aggiornamenti in background all'avvio
        CheckForUpdatesAsync()
        
        Try
            DatabaseManager.InitializeDatabase()
            DatabaseManager.RimuoviSottoCategoriaSchema() ' Rimuovi se esiste
            DatabaseManager.MigrazioneConfigurazioneStipendi()
            DatabaseManager.ValidaConfigurazioneStipendi()

            ' Non creare lblInfoMensile ma usa quello del Designer
            lblInfoMensile.AutoSize = False
            lblInfoMensile.Dock = DockStyle.Fill
            lblInfoMensile.TextAlign = ContentAlignment.MiddleLeft
            lblInfoMensile.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            lblInfoMensile.Padding = New Padding(10, 0, 0, 0)
            lblInfoMensile.BackColor = Color.Transparent

            ' Non cancellare i controlli del pannello
            ' panelInfoMensile.Controls.Clear()

            CaricaTransazioni()
            AggiornaInfoMensile()

        Catch ex As Exception
            MessageBox.Show("Errore durante l'avvio del form: " & ex.Message,
                  "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub InitializeComponents()
        ' Imposta proprietà form
        Me.Text = "Gestione Finanze Personali - Scheda Finanza"
        Me.Size = New Size(1200, 800)
        Me.StartPosition = FormStartPosition.CenterScreen

        ' Panel principale
        Dim panelMain As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(10)
        }

        ' Panel inserimento in alto
        Dim panelInserimento As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 100,
            .BackColor = Color.FromArgb(240, 248, 255),
            .BorderStyle = BorderStyle.FixedSingle
        }

        ' Controlli inserimento
        Dim lblData As New Label With {
            .Text = "Data:",
            .Location = New Point(10, 15),
            .Size = New Size(50, 20),
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        }


        Me.Controls.Add(panelMain)

        ' Eventi
        AddHandler btnAggiungi.Click, AddressOf BtnAggiungi_Click
        AddHandler btnClassifica.Click, AddressOf BtnClassifica_Click
        AddHandler btnImportaExcel.Click, AddressOf BtnImportaFile_Click
        AddHandler btnEliminaRiga.Click, AddressOf BtnEliminaRiga_Click
        AddHandler btnGestionePattern.Click, AddressOf BtnGestionePattern_Click
        AddHandler txtImporto.KeyPress, AddressOf TxtImporto_KeyPress
    End Sub

    Public Sub New(Optional dataInizio As Date = Nothing, Optional dataFine As Date = Nothing, Optional macro As String = "", Optional categoria As String = "")
        InitializeComponent()
        If dataInizio <> Nothing AndAlso dataFine <> Nothing Then
            FiltroAttivo = True
            FiltroInizio = dataInizio
            FiltroFine = dataFine
            FiltroMacro = macro
            FiltroCat = categoria
        End If
    End Sub

    ' Aggiungi questa funzione in FormFinanza.vb
    ' Calcola il totale Entrata o Uscita in base al “segno” dell’importo e al mese/anno corrente
    Private Function GetTotale(tipologia As String) As Decimal
        ' Ricava il periodo gestionale corrente
        Dim dataInizio As Date, dataFine As Date
        GetPeriodoGestionale(dataInizio, dataFine)
        Dim totale As Decimal = 0
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String
            If tipologia = "Entrata" Then
                sql = "SELECT SUM(Importo) FROM Transazioni " &
                  "WHERE Importo >= 0 AND Data >= @dataInizio AND Data <= @dataFine"
            Else
                sql = "SELECT SUM(Importo) FROM Transazioni " &
                  "WHERE Importo < 0 AND Data >= @dataInizio AND Data <= @dataFine"
            End If
            Using cmd As New SQLite.SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@dataInizio", dataInizio.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@dataFine", dataFine.ToString("yyyy-MM-dd"))
                Dim result = cmd.ExecuteScalar()
                If result IsNot DBNull.Value Then totale = Math.Abs(Convert.ToDecimal(result))
            End Using
        End Using
        Return totale
    End Function



    Private Sub AggiornaInfoMensile()
        ' Determina il mese corrente basato sui periodi stipendiali
        Dim oggi = Date.Today
        Dim annoCorrente = oggi.Year
        Dim meseStipendiale = 1

        ' Trova il mese stipendiale che contiene la data odierna
        For mese As Integer = 1 To 12
            Dim periodo = GestoreStipendi.CalcolaPeriodoStipendiale(annoCorrente, mese)
            If oggi >= periodo.DataInizio AndAlso oggi <= periodo.DataFine Then
                meseStipendiale = mese
                Exit For
            End If
        Next

        ' Calcola il periodo del mese stipendiale corrente
        Dim periodoCorrente = GestoreStipendi.CalcolaPeriodoStipendiale(annoCorrente, meseStipendiale)
        Dim dataInizio = periodoCorrente.DataInizio
        Dim dataFine = periodoCorrente.DataFine

        ' Calcola entrate e uscite
        Dim entrate As Decimal = 0, uscite As Decimal = 0

        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String = "SELECT Importo FROM Transazioni WHERE Data >= @inizio AND Data <= @fine"
            Using cmd As New SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@inizio", dataInizio.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@fine", dataFine.ToString("yyyy-MM-dd"))
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim importo As Decimal = Convert.ToDecimal(reader("Importo"))
                        If importo > 0 Then entrate += importo
                        If importo < 0 Then uscite += Math.Abs(importo)
                    End While
                End Using
            End Using
        End Using

        ' Il mese di riferimento dalla data di fine (regola fondamentale)
        Dim nomeMese = dataFine.ToString("MMMM yyyy", CultureInfo.CurrentCulture)
        Dim risparmio = entrate - uscite

        ' Aggiorna la label esistente con tutte le informazioni
        lblInfoMensile.Text = $"Periodo: {dataInizio:dd/MM/yyyy} - {dataFine:dd/MM/yyyy} ({nomeMese})" & vbCrLf &
                         $"Entrate: {entrate:C2} | Uscite: {uscite:C2} | Risparmio: {risparmio:C2}"

        ' Cambia colore in base al risparmio
        If risparmio >= 0 Then
            lblInfoMensile.ForeColor = Color.DarkGreen
        Else
            lblInfoMensile.ForeColor = Color.DarkRed
        End If
    End Sub

    Private Sub ApriConfigurazioneStipendi()
        Using frmConfig As New FormImpostazioniGenerali()
            If frmConfig.ShowDialog() = DialogResult.OK Then
                ' Configurazione salvata, aggiorna le visualizzazioni
                GestoreStipendi.InvalidaCache()
                AggiornaInfoMensile()  ' Ricarica i dati con la nuova configurazione
                CaricaTransazioni()    ' Ricarica la griglia se necessario
                MessageBox.Show("Configurazione applicata. I periodi verranno ricalcolati.", "Configurazione Aggiornata", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Using
    End Sub

    ' Oppure aggiungi un bottone nell'interfaccia:
    Private Sub btnConfiguraStipendi_Click(sender As Object, e As EventArgs) Handles btnConfiguraStipendi.Click
        ApriConfigurazioneStipendi()
    End Sub

    Private Sub GetPeriodoGestionale(ByRef dataInizio As Date, ByRef dataFine As Date)
        Dim oggi As Date = Now.Date
        Dim anno As Integer = oggi.Year
        Dim mese As Integer = oggi.Month

        If oggi.Day < 23 Then
            Dim mesePrecedente As Integer = If(mese = 1, 12, mese - 1)
            Dim annoPrecedente As Integer = If(mese = 1, anno - 1, anno)
            dataInizio = New Date(annoPrecedente, mesePrecedente, 23)
            dataFine = New Date(anno, mese, 22)
        Else
            Dim meseSuccessivo As Integer = If(mese = 12, 1, mese + 1)
            Dim annoSuccessivo As Integer = If(mese = 12, anno + 1, anno)
            dataInizio = New Date(anno, mese, 23)
            dataFine = New Date(annoSuccessivo, meseSuccessivo, 22)
        End If
    End Sub

    Private Function GetTotaleIntervallo(tipologia As String, dataInizio As Date, dataFine As Date) As Decimal
        Dim totale As Decimal = 0
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String
            If tipologia = "Entrata" Then
                sql = "SELECT SUM(Importo) FROM Transazioni " &
                  "WHERE Importo >= 0 AND Data >= @dataInizio AND Data <= @dataFine"
            Else ' Uscita
                sql = "SELECT SUM(Importo) FROM Transazioni " &
                  "WHERE Importo < 0 AND Data >= @dataInizio AND Data <= @dataFine"
            End If
            Using cmd As New SQLite.SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@dataInizio", dataInizio.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@dataFine", dataFine.ToString("yyyy-MM-dd"))
                Dim result = cmd.ExecuteScalar()
                If result IsNot DBNull.Value Then totale = Math.Abs(Convert.ToDecimal(result))
            End Using
        End Using
        Return totale
    End Function


    ' Evento per aggiungere transazione
    Private Sub BtnAggiungi_Click(sender As Object, e As EventArgs) Handles btnAggiungi.Click
        Try
            ' Validazione
            If String.IsNullOrWhiteSpace(txtDescrizione.Text) Then
                MessageBox.Show("Inserisci una descrizione per la transazione.",
                              "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtDescrizione.Focus()
                Return
            End If

            Dim importo As Decimal
            If Not Decimal.TryParse(txtImporto.Text, importo) OrElse importo = 0 Then
                MessageBox.Show("Inserisci un importo valido diverso da zero.",
                              "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtImporto.Focus()
                Return
            End If

            ' Crea nuova transazione
            Dim nuovaTransazione As New Transazione(txtDescrizione.Text.Trim(), importo, dtpData.Value)

            ' Salva nel database
            If SalvaTransazione(nuovaTransazione) Then
                MessageBox.Show("Transazione aggiunta con successo!",
                              "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)

                ' Pulisci campi
                txtDescrizione.Clear()
                txtImporto.Clear()
                dtpData.Value = Date.Today
                txtDescrizione.Focus()


                CaricaTransazioni() ' Ricarica la griglia
                AggiornaInfoMensile() ' Aggiorna i valori di entrata - uscite - risparmio
            End If

        Catch ex As Exception
            MessageBox.Show("Errore durante l'aggiunta della transazione: " & ex.Message,
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub dgvTransazioni_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvTransazioni.CellFormatting
        If dgvTransazioni.Columns(e.ColumnIndex).Name = "Importo" AndAlso e.Value IsNot Nothing Then
            Dim importoValue As Decimal
            If Decimal.TryParse(e.Value.ToString(), importoValue) Then
                If importoValue > 0 Then
                    e.CellStyle.BackColor = Color.LightGreen
                    e.CellStyle.ForeColor = Color.Black
                ElseIf importoValue < 0 Then
                    e.CellStyle.BackColor = Color.LightCoral
                    e.CellStyle.ForeColor = Color.Black
                Else
                    e.CellStyle.BackColor = Color.White
                    e.CellStyle.ForeColor = Color.Black
                End If
            End If
        End If
    End Sub

    Public Sub PulisciFileLog()
        Dim logPath As String = "verifica_match_parola_con_descrizione.txt"
        Try
            System.IO.File.WriteAllText(logPath, String.Empty) ' svuota il file
        Catch ex As Exception
            MessageBox.Show("Errore durante la pulizia del file log: " & ex.Message)
        End Try
    End Sub

    ' Evento per classificare le categorie
    Private Sub BtnClassifica_Click(sender As Object, e As EventArgs) Handles btnClassifica.Click

        Dim result As DialogResult

        result = MessageBox.Show(
    "SI: Classifica solo le transazioni non classificate o nuove" & vbCrLf &
    "NO: Ri-classifica TUTTE le transazioni da capo. Verranno ripulite le categorie precedenti.",
    "Classifica Categorie",
    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1)


        Select Case result
            Case DialogResult.Yes ' Solo non classificate
                Call PulisciFileLog()
                Call ClassificaSoloNonClassificate()
                Call CaricaTransazioni()  ' AGGIORNA LA TABELLA SUBITO DOPO!

            Case DialogResult.No ' Tutte le transazioni
                Dim conferma As DialogResult = MessageBox.Show("Confermi di voler riclassificare TUTTE le transazioni?", "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If conferma = DialogResult.Yes Then
                    Call PulisciFileLog()
                    Call RiclassificaTutte()
                    Call CaricaTransazioni()  ' AGGIORNA LA TABELLA SUBITO DOPO!
                End If
            Case DialogResult.Cancel
                ' Nessuna azione
        End Select
    End Sub

    Private Sub ClassificaSoloNonClassificate()
        Me.Cursor = Cursors.WaitCursor
        btnClassifica.Enabled = False
        btnClassifica.Text = "Classificando..."
        Try
            Dim transazioniClassificate As Integer = ClassificatoreTransazioniMigliorato.ClassificaTutteLeTransazioniMigliorate()
            MessageBox.Show($"Classificazione completata! {transazioniClassificate} transazioni sono state classificate.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Errore durante la classificazione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
            btnClassifica.Enabled = True
            btnClassifica.Text = "Classifica Categorie"
        End Try
    End Sub

    Private Sub RiclassificaTutte()
        Me.Cursor = Cursors.WaitCursor
        btnClassifica.Enabled = False
        btnClassifica.Text = "Riclassificando tutte..."
        Try
            Using connection As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
                connection.Open()
                Using command As New SQLite.SQLiteCommand("UPDATE Transazioni SET MacroCategoria = '', Categoria = '', Necessita = '', Frequenza = '', Stagionalita = ''", connection)
                    command.ExecuteNonQuery()
                End Using
            End Using

            Dim transazioniClassificate As Integer = ClassificatoreTransazioniMigliorato.ClassificaTutteLeTransazioniMigliorate()
            MessageBox.Show($"Riclassificazione completata! {transazioniClassificate} transazioni sono state classificate.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Errore durante la riclassificazione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
            btnClassifica.Enabled = True
            btnClassifica.Text = "Classifica Categorie"
        End Try
    End Sub

    Private Sub BtnGestionePattern_Click(sender As Object, e As EventArgs) Handles btnGestionePattern.Click
        Using dlg As New FormGestionePattern()
            dlg.ShowDialog()
        End Using
    End Sub

    ' Validazione input importo
    Private Sub TxtImporto_KeyPress(sender As Object, e As KeyPressEventArgs)
        ' Consenti solo numeri, virgola, punto e backspace
        If Not Char.IsDigit(e.KeyChar) AndAlso e.KeyChar <> "."c AndAlso
           e.KeyChar <> ","c AndAlso e.KeyChar <> "-"c AndAlso
           Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If

        ' Consenti solo una virgola/punto decimale
        If (e.KeyChar = "."c OrElse e.KeyChar = ","c) AndAlso
           (sender.Text.Contains(".") OrElse sender.Text.Contains(",")) Then
            e.Handled = True
        End If
    End Sub

    ' Salva transazione nel database
    Private Function SalvaTransazione(transazione As Transazione) As Boolean
        Try
            Using connection As New SQLiteConnection(DatabaseManager.GetConnectionString)
                connection.Open()

                Dim query As String = "
                INSERT INTO Transazioni (Data, Importo, Descrizione, Causale, MacroCategoria, 
                                       Categoria, Necessita, Frequenza, Stagionalita)
                VALUES (@data, @importo, @desc, @causale, @macro, @cat, @nec, @freq, @stag)"

                Using command As New SQLiteCommand(query, connection)
                    command.Parameters.AddWithValue("@data", transazione.Data.ToString("yyyy-MM-dd"))
                    command.Parameters.AddWithValue("@importo", transazione.Importo)
                    command.Parameters.AddWithValue("@desc", transazione.Descrizione)
                    command.Parameters.AddWithValue("@causale", transazione.Causale)
                    command.Parameters.AddWithValue("@macro", transazione.MacroCategoria)
                    command.Parameters.AddWithValue("@cat", transazione.Categoria)
                    command.Parameters.AddWithValue("@nec", transazione.Necessita)
                    command.Parameters.AddWithValue("@freq", transazione.Frequenza)
                    command.Parameters.AddWithValue("@stag", transazione.Stagionalita)

                    Return command.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore nel salvataggio: " & ex.Message, "Errore",
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' Carica transazioni nella griglia
    Private Sub CaricaTransazioni()
        Try
            Using connection As New SQLiteConnection(DatabaseManager.GetConnectionString)
                connection.Open()
                Dim query As String = "SELECT ID, Data, Importo, Descrizione, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita FROM Transazioni"
                Dim whereConditions As New List(Of String)
                Dim cmd As New SQLiteCommand()
                cmd.Connection = connection

                If FiltroAttivo Then
                    whereConditions.Add("Data >= @dataInizio")
                    whereConditions.Add("Data <= @dataFine")
                    cmd.Parameters.AddWithValue("@dataInizio", FiltroInizio.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@dataFine", FiltroFine.ToString("yyyy-MM-dd"))

                    If Not String.IsNullOrWhiteSpace(FiltroMacro) Then
                        whereConditions.Add("MacroCategoria = @macro")
                        cmd.Parameters.AddWithValue("@macro", FiltroMacro)
                    End If
                    If Not String.IsNullOrWhiteSpace(FiltroCat) Then
                        whereConditions.Add("Categoria = @cat")
                        cmd.Parameters.AddWithValue("@cat", FiltroCat)
                    End If
                    ' RIMUOVI filtro SottoCategoria
                End If

                ' Filtro per parola
                If Not String.IsNullOrWhiteSpace(filtroParola) Then
                    whereConditions.Add("LOWER(Descrizione) LIKE @parola")
                    cmd.Parameters.AddWithValue("@parola", "%" & filtroParola.ToLower() & "%")
                End If

                If whereConditions.Count > 0 Then
                    query &= " WHERE " & String.Join(" AND ", whereConditions)
                End If

                query &= " ORDER BY Data DESC, ID DESC"
                cmd.CommandText = query

                Using adapter As New SQLiteDataAdapter(cmd)
                    Dim dataTable As New DataTable()
                    adapter.Fill(dataTable)
                    dgvTransazioni.DataSource = dataTable
                End Using
            End Using

            AggiornaInfoFiltro()
            AggiornaInfoMensile() ' Aggiorna i valori di entrata - uscite - risparmio

        Catch ex As Exception
            MessageBox.Show("Errore nel caricamento dei dati: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function ContaTransazioniFiltroParola() As Integer
        Try
            Using connection As New SQLiteConnection(DatabaseManager.GetConnectionString)
                connection.Open()
                Dim sql As String = "SELECT COUNT(*) FROM Transazioni WHERE LOWER(Descrizione) LIKE @parola"

                Using cmd As New SQLiteCommand(sql, connection)
                    cmd.Parameters.AddWithValue("@parola", "%" & filtroParola.ToLower() & "%")
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch
            Return 0
        End Try
    End Function

    Private Sub AggiornaInfoFiltro()
        If FiltroParolaDettagliatoAttivo Then
            ' Filtro parola dettagliato dalla Dashboard Set Categorie
            Dim totaleTransazioni As Integer = ContaTransazioniFiltroParola()
            lblFiltroParolaDettagliato.Text = $"Filtro PAROLA PATTERN attivo: '{filtroParola}' | Set: {FiltroParolaDettagliataMacro} > {FiltroParolaDettagliataCategoria} | Transazioni trovate: {totaleTransazioni}"
            lblFiltroParolaDettagliato.Visible = True
            btnChiudiDettagli.Visible = True

            ' Nascondi altri label
            lblFiltroAttivo.Visible = False
            lblInfoMensile.Visible = False

        ElseIf FiltroAttivo OrElse Not String.IsNullOrWhiteSpace(filtroParola) Then
            ' Filtro normale per categoria o parola singola
            Dim totaleEntrate As Decimal = CalcolaTotaleFiltrato(True)
            Dim totaleUscite As Decimal = CalcolaTotaleFiltrato(False)
            Dim saldo As Decimal = totaleEntrate - totaleUscite

            lblFiltroAttivo.Text = $"Filtro attivo su: {FiltroMacro} / {FiltroCat} / Parola: {filtroParola}" &
                          $" - Periodo {FiltroInizio:dd.MM.yyyy} → {FiltroFine:dd.MM.yyyy}" & vbCrLf &
                          $"Entrate: {totaleEntrate:N2}€, Uscite: {totaleUscite:N2}€, Risparmio: {saldo:N2}€"
            lblFiltroAttivo.Visible = True
            btnChiudiDettagli.Visible = True

            ' Nascondi altri label
            lblFiltroParolaDettagliato.Visible = False
            lblInfoMensile.Visible = False
        Else
            ' Nessun filtro attivo
            lblFiltroAttivo.Visible = False
            lblFiltroParolaDettagliato.Visible = False
            btnChiudiDettagli.Visible = False
            lblInfoMensile.Visible = True
        End If
    End Sub

    Private Function CalcolaTotaleFiltrato(isEntrata As Boolean) As Decimal
        Dim totale As Decimal = 0
        Using connection As New SQLiteConnection(DatabaseManager.GetConnectionString)
            connection.Open()
            Dim sql As String = "SELECT SUM(Importo) FROM Transazioni WHERE Importo " &
                         If(isEntrata, ">= 0", "< 0") &
                         " AND Data >= @dataInizio AND Data <= @dataFine"
            If Not String.IsNullOrWhiteSpace(FiltroMacro) Then sql &= " AND MacroCategoria = @macro"
            If Not String.IsNullOrWhiteSpace(FiltroCat) Then sql &= " AND Categoria = @cat"
            ' RIMUOVI filtro SottoCategoria

            Using cmd As New SQLiteCommand(sql, connection)
                cmd.Parameters.AddWithValue("@dataInizio", FiltroInizio.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@dataFine", FiltroFine.ToString("yyyy-MM-dd"))
                If Not String.IsNullOrWhiteSpace(FiltroMacro) Then cmd.Parameters.AddWithValue("@macro", FiltroMacro)
                If Not String.IsNullOrWhiteSpace(FiltroCat) Then cmd.Parameters.AddWithValue("@cat", FiltroCat)
                ' RIMUOVI parametro SottoCategoria

                Dim result = cmd.ExecuteScalar()
                If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                    totale = Math.Abs(Convert.ToDecimal(result))
                End If
            End Using
        End Using
        Return totale
    End Function


    ' INIZIO NUOVO IMPORTATORE UNIVERSALE
    Private Sub BtnImportaFile_Click(sender As Object, e As EventArgs) Handles btnImportaExcel.Click
        Using ofd As New OpenFileDialog() With {
        .Title = "Seleziona file da importare",
        .Filter = "Tutti i formati|*.xlsx;*.xls;*.csv;*.txt;*.pdf|" &
                  "Excel (*.xlsx;*.xls)|*.xlsx;*.xls|" &
                  "CSV/TXT (*.csv;*.txt)|*.csv;*.txt|" &
                  "PDF (*.pdf)|*.pdf"
    }
            If ofd.ShowDialog() <> DialogResult.OK Then Return

            Dim percorso = ofd.FileName
            Dim ext = System.IO.Path.GetExtension(percorso).ToLower()

            ' 1) Configurazione (carica o crea)
            Dim config As ConfigurazioneImportazione = Nothing
            Dim mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer) = Nothing
            Dim headerRow As Integer = 1

            ' Seleziona configurazione solo per file non-PDF
            If ext <> ".pdf" Then
                Using frmConfig As New FormSceltaConfigurazione()
                    If frmConfig.ShowDialog() = DialogResult.OK Then
                        If Not frmConfig.CreaNuovaConfigurazione Then
                            config = frmConfig.ConfigurazioneSelezionata
                            ' Se ho una config, uso i suoi valori
                            If config IsNot Nothing Then
                                headerRow = config.RigaIntestazione
                                mapping = config.MappingColonne
                            End If
                        End If
                    Else
                        Return ' Utente ha annullato
                    End If
                End Using
            End If

            ' 2) Analisi preliminare con headerRow corretto
            Dim analisi As ImportatoreUniversale.RisultatoAnalisi
            Try
                Select Case ext
                    Case ".csv", ".txt"
                        analisi = ImportatoreUniversale.AnalizzaCsv(percorso)
                    Case ".xls"
                        analisi = ImportatoreUniversale.AnalizzaExcelLegacy(percorso, headerRow)
                    Case ".xlsx"
                        analisi = ImportatoreUniversale.AnalizzaExcel(percorso, headerRow)
                    Case ".pdf"
                        analisi = ImportatoreUniversale.AnalizzaPdf(percorso)
                    Case Else
                        MessageBox.Show($"Formato non supportato: {ext}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                End Select
            Catch ex As Exception
                MessageBox.Show($"Errore durante l'analisi del file: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End Try

            ' 3) Se NON ho config, chiedo riga intestazione (solo per Excel/CSV)
            If config Is Nothing AndAlso ext <> ".pdf" Then
                Using frmA As New FormAnteprimaFile(percorso, analisi)
                    If frmA.ShowDialog() <> DialogResult.OK Then Return
                    headerRow = frmA.RigaIntestazione
                End Using

                ' Rianalizza Excel con headerRow scelto dall'utente
                If ext = ".xls" Then
                    analisi = ImportatoreUniversale.AnalizzaExcelLegacy(percorso, headerRow)
                ElseIf ext = ".xlsx" Then
                    analisi = ImportatoreUniversale.AnalizzaExcel(percorso, headerRow)
                End If
            End If

            ' 4) Mapping colonne (usa config o chiedi manualmente)
            If mapping Is Nothing Then
                Using frmMap As New FormMappingColonne(analisi)
                    If frmMap.ShowDialog() <> DialogResult.OK Then Return
                    mapping = frmMap.MappingColonne
                End Using
            End If

            ' 5) Salva nuova configurazione se necessario
            If config Is Nothing AndAlso ext <> ".pdf" Then
                Dim defaultName = If(ext.Contains("xls"), "Config Excel", "Config CSV")
                Dim nome = InputBox("Nome configurazione:", "Salva Configurazione", defaultName)
                If Not String.IsNullOrWhiteSpace(nome) Then
                    Dim nuovaConfig = New ConfigurazioneImportazione With {
                    .Nome = nome,
                    .RigaIntestazione = headerRow,
                    .MappingColonne = mapping
                }
                    GestoreConfigurazioni.SalvaConfigurazione(nuovaConfig)
                    MessageBox.Show("Configurazione salvata.", "Configurazione", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If

            ' 6) Validazione mapping
            If Not mapping.ContainsKey(ImportatoreUniversale.TipoColonna.Data) OrElse
           Not mapping.ContainsKey(ImportatoreUniversale.TipoColonna.Importo) OrElse
           Not mapping.ContainsKey(ImportatoreUniversale.TipoColonna.Descrizione) Then
                MessageBox.Show("Mapping incompleto: mancano Data, Importo o Descrizione.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' 7) Anteprima e importazione finale
            Try
                If MostraAnteprimaImportazione(percorso, analisi, mapping) Then
                    Dim count = EseguiImportazioneFinale(percorso, analisi, mapping)
                    If count > 0 Then
                        MessageBox.Show($"Importate {count} transazioni.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        CaricaTransazioni()
                        AggiornaInfoMensile()
                    Else
                        MessageBox.Show("Nessuna transazione importata.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    End If
                End If
            Catch ex As Exception
                MessageBox.Show($"Errore durante l'importazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub




    ' Nuova funzione per creare mapping automatico
    Private Function CreaMapppingAutomatico(analisi As ImportatoreUniversale.RisultatoAnalisi) As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)
        Dim mapping As New Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)

        For Each colonna In analisi.ColonneRilevate
            If Not mapping.ContainsKey(colonna.Tipo) AndAlso colonna.Tipo <> ImportatoreUniversale.TipoColonna.Sconosciuto Then
                mapping(colonna.Tipo) = colonna.Indice
            End If
        Next

        Return mapping
    End Function

    ' Nuova funzione per mostrare anteprima
    Private Function MostraAnteprimaImportazione(percorsoFile As String,
                                             analisi As ImportatoreUniversale.RisultatoAnalisi,
                                             mappingColonne As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)) As Boolean

        Dim risultati As List(Of RigaImportConEsito)
        Dim estensione = System.IO.Path.GetExtension(percorsoFile).ToLower()

        Select Case estensione
            Case ".csv", ".txt"
                risultati = ElaboraCsvConEsiti(percorsoFile, analisi, mappingColonne, 20)
            Case ".xlsx", ".xls"
                ' NOTA: ElaboraExcelLegacyConEsiti e ElaboraExcelConEsiti NON vogliono 'analisi' come parametro,
                ' ma solo mappingColonne e maxRighe.
                If estensione = ".xls" Then
                    risultati = ElaboraExcelLegacyConEsiti(percorsoFile, analisi, mappingColonne, 20)
                Else
                    risultati = ElaboraExcelConEsiti(percorsoFile, analisi, mappingColonne, 20)
                End If
            Case ".pdf"
                risultati = ElaboraPdfConEsiti(percorsoFile, analisi, mappingColonne, 20)
            Case Else
                MessageBox.Show("Formato file non supportato per l'anteprima.", "Errore importazione", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
        End Select

        If risultati Is Nothing OrElse risultati.Count = 0 Then
            MessageBox.Show("Nessuna riga valida trovata nel file.", "Anteprima", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        ' =====================================================
        ' CREA FORM PRINCIPALE
        ' =====================================================
        Dim formAnteprima As New Form()
        formAnteprima.Text = $"Anteprima Importazione - {System.IO.Path.GetFileName(percorsoFile)}"
        formAnteprima.Size = New Size(1200, 700)
        formAnteprima.StartPosition = FormStartPosition.CenterScreen
        formAnteprima.BackColor = Color.White

        ' =====================================================
        ' PANEL 1: TOP (INFO RIGHE)
        ' =====================================================
        Dim panelTop As New Panel()
        panelTop.Dock = DockStyle.Top
        panelTop.Height = 90
        panelTop.BackColor = Color.FromArgb(235, 245, 255)
        panelTop.BorderStyle = BorderStyle.FixedSingle

        Dim lblInfo As New Label()
        lblInfo.Text = $"File: {System.IO.Path.GetFileName(percorsoFile)}" & vbCrLf &
               $"Righe totali: {analisi.NumeroRighe} | Anteprima: {risultati.Count}" & vbCrLf &
               $"✓ OK: {risultati.Where(Function(r) String.IsNullOrWhiteSpace(r.Errore)).Count} | ✗ Errori: {risultati.Where(Function(r) Not String.IsNullOrWhiteSpace(r.Errore)).Count}"
        lblInfo.Font = New Font("Segoe UI", 11, FontStyle.Regular)
        lblInfo.ForeColor = Color.FromArgb(50, 50, 50)
        lblInfo.Location = New Point(20, 15)
        lblInfo.Size = New Size(1150, 60)
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        panelTop.Controls.Add(lblInfo)

        ' =====================================================
        ' PANEL 2: BOTTOM (PULSANTI)
        ' =====================================================
        Dim panelBottom As New Panel()
        panelBottom.Dock = DockStyle.Bottom
        panelBottom.Height = 70
        panelBottom.BackColor = Color.FromArgb(240, 240, 240)
        panelBottom.BorderStyle = BorderStyle.FixedSingle

        Dim btnConferma As New Button()
        btnConferma.Text = "✓ Conferma Importazione"
        btnConferma.DialogResult = DialogResult.OK
        btnConferma.Size = New Size(250, 40)
        btnConferma.Location = New Point(520, 15)
        btnConferma.BackColor = Color.FromArgb(40, 180, 100)
        btnConferma.ForeColor = Color.White
        btnConferma.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        btnConferma.FlatStyle = FlatStyle.Flat
        btnConferma.FlatAppearance.BorderSize = 0
        btnConferma.Cursor = Cursors.Hand
        panelBottom.Controls.Add(btnConferma)


        Dim btnAnnulla As New Button()
        btnAnnulla.Text = "✗ Annulla"
        btnAnnulla.DialogResult = DialogResult.Cancel
        btnAnnulla.Size = New Size(120, 40)
        btnAnnulla.Location = New Point(780, 15)
        btnAnnulla.BackColor = Color.FromArgb(220, 60, 60)
        btnAnnulla.ForeColor = Color.White
        btnAnnulla.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        btnAnnulla.FlatStyle = FlatStyle.Flat
        btnAnnulla.FlatAppearance.BorderSize = 0
        btnAnnulla.Cursor = Cursors.Hand
        panelBottom.Controls.Add(btnAnnulla)

        ' =====================================================
        ' PANEL 3: FILL (CONTENITORE DATAGRIDVIEW)
        ' =====================================================
        Dim panelCenter As New Panel()
        panelCenter.Dock = DockStyle.Fill
        panelCenter.BackColor = Color.White
        panelCenter.Padding = New Padding(10)

        ' =====================================================
        ' DATAGRIDVIEW (DENTRO PANEL CENTER)
        ' =====================================================
        Dim dgvAnteprima As New DataGridView()
        dgvAnteprima.Dock = DockStyle.Fill
        dgvAnteprima.ReadOnly = True
        dgvAnteprima.AllowUserToAddRows = False
        dgvAnteprima.AllowUserToDeleteRows = False
        dgvAnteprima.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvAnteprima.MultiSelect = True
        dgvAnteprima.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        ' *** IMPOSTAZIONI PER SCROLLBAR ORIZZONTALE ***
        dgvAnteprima.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None  ' Cambiato da Fill
        dgvAnteprima.ScrollBars = ScrollBars.Both  ' Scrollbar sia verticale che orizzontale
        dgvAnteprima.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells)  ' Ridimensiona al contenuto

        dgvAnteprima.Font = New Font("Segoe UI", 10)
        dgvAnteprima.RowHeadersVisible = False
        dgvAnteprima.BackgroundColor = Color.White
        dgvAnteprima.GridColor = Color.LightGray
        dgvAnteprima.EnableHeadersVisualStyles = False
        dgvAnteprima.BorderStyle = BorderStyle.Fixed3D

        ' Stile header
        dgvAnteprima.ColumnHeadersDefaultCellStyle = New DataGridViewCellStyle With {
        .BackColor = Color.FromArgb(50, 120, 200),
        .ForeColor = Color.White,
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .Alignment = DataGridViewContentAlignment.MiddleCenter
    }
        dgvAnteprima.ColumnHeadersHeight = 35

        ' Aggiungi colonne
        dgvAnteprima.Columns.Add("Riga", "Riga File")
        dgvAnteprima.Columns.Add("Data", "Data")
        dgvAnteprima.Columns.Add("Importo", "Importo")
        dgvAnteprima.Columns.Add("Descrizione", "Descrizione")
        dgvAnteprima.Columns.Add("Errore", "Stato/Errore")

        ' Imposta larghezze
        dgvAnteprima.Columns("Riga").Width = 80
        dgvAnteprima.Columns("Data").Width = 100
        dgvAnteprima.Columns("Importo").Width = 120
        dgvAnteprima.Columns("Descrizione").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dgvAnteprima.Columns("Errore").Width = 250

        ' Popola i dati
        For Each riga In risultati
            Dim indiceRiga = dgvAnteprima.Rows.Add(
            riga.IndiceRiga,
            If(riga.Transazione IsNot Nothing, riga.Transazione.Data.ToShortDateString(), "N/A"),
            If(riga.Transazione IsNot Nothing, riga.Transazione.Importo.ToString("N2") & " €", "N/A"),
            If(riga.Transazione IsNot Nothing, riga.Transazione.Descrizione, "N/A"),
            If(String.IsNullOrWhiteSpace(riga.Errore), "✓ OK", "✗ " & riga.Errore)
        )

            ' Colora la riga
            If String.IsNullOrWhiteSpace(riga.Errore) Then
                dgvAnteprima.Rows(indiceRiga).DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230) ' Verde chiaro
                dgvAnteprima.Rows(indiceRiga).DefaultCellStyle.ForeColor = Color.FromArgb(20, 100, 20)
            Else
                dgvAnteprima.Rows(indiceRiga).DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230) ' Rosso chiaro
                dgvAnteprima.Rows(indiceRiga).DefaultCellStyle.ForeColor = Color.FromArgb(150, 20, 20)
            End If
        Next

        ' Aggiungi DataGridView al panel center
        panelCenter.Controls.Add(dgvAnteprima)

        ' =====================================================
        ' AGGIUNGI I PANEL AL FORM NELL'ORDINE CORRETTO
        ' =====================================================
        formAnteprima.Controls.Add(panelCenter)  ' Fill per ultimo
        formAnteprima.Controls.Add(panelTop)     ' Top per secondo  
        formAnteprima.Controls.Add(panelBottom)  ' Bottom per primo
        ' =====================================================
        ' MOSTRA IL FORM E RESTITUISCI RISULTATO
        ' =====================================================
        Return formAnteprima.ShowDialog() = DialogResult.OK

    End Function

    ' Nuova funzione per eseguire importazione finale
    ' Funzione di importazione finale che processa solo righe valide
    Private Function EseguiImportazioneFinale(percorsoFile As String,
                                        analisi As ImportatoreUniversale.RisultatoAnalisi,
                                        mappingColonne As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)) As Integer

        Dim risultati As List(Of RigaImportConEsito)
        Dim estensione = System.IO.Path.GetExtension(percorsoFile).ToLower()

        Select Case estensione
            Case ".csv", ".txt"
                risultati = ElaboraCsvConEsiti(percorsoFile, analisi, mappingColonne, 0)
            Case ".xlsx", ".xls"
                risultati = ElaboraExcelConEsiti(percorsoFile, analisi, mappingColonne, 0)
            Case ".pdf"
                risultati = ElaboraPdfConEsiti(percorsoFile, analisi, mappingColonne, 0)
            Case Else
                Throw New NotSupportedException($"Formato file non supportato: {estensione}")
        End Select

        ' Filtra solo le righe valide (senza errori)
        Dim righeValide = risultati.Where(Function(r) String.IsNullOrWhiteSpace(r.Errore) AndAlso r.Transazione IsNot Nothing).ToList()

        If righeValide.Count = 0 Then
            MessageBox.Show("Nessuna transazione valida da importare!", "Importazione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return 0
        End If

        ' Log di pre-importazione
        Dim logInfo = $"=== IMPORTAZIONE {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===" & vbCrLf &
                  $"File: {System.IO.Path.GetFileName(percorsoFile)}" & vbCrLf &
                  $"Righe totali analizzate: {risultati.Count}" & vbCrLf &
                  $"Righe valide da importare: {righeValide.Count}" & vbCrLf &
                  $"Righe scartate (con errori): {risultati.Count - righeValide.Count}" & vbCrLf

        ' Mostra conferma finale con statistiche
        Dim conferma = MessageBox.Show(
        $"Pronto per importare {righeValide.Count} transazioni valide." & vbCrLf &
        $"Righe scartate: {risultati.Count - righeValide.Count}" & vbCrLf & vbCrLf &
        "Procedere con l'importazione?",
        "Conferma Importazione",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question)

        If conferma <> DialogResult.Yes Then
            Return 0
        End If

        Dim importate As Integer = 0
        Dim erroriImportazione As New List(Of String)

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    Dim sql = "INSERT INTO Transazioni (Data, Importo, Descrizione, Causale, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita) " &
                         "VALUES (@data, @importo, @desc, @causale, @macro, @cat, @nec, @freq, @stag)"

                    Using cmd As New SQLiteCommand(sql, conn, trans)
                        For Each rigaValida In righeValide
                            Try
                                cmd.Parameters.Clear()
                                cmd.Parameters.AddWithValue("@data", rigaValida.Transazione.Data.ToString("yyyy-MM-dd"))
                                cmd.Parameters.AddWithValue("@importo", rigaValida.Transazione.Importo)
                                cmd.Parameters.AddWithValue("@desc", rigaValida.Transazione.Descrizione)
                                cmd.Parameters.AddWithValue("@causale", "Importazione automatica")
                                cmd.Parameters.AddWithValue("@macro", "") ' Sarà popolato dalla classificazione
                                cmd.Parameters.AddWithValue("@cat", "")
                                cmd.Parameters.AddWithValue("@nec", "")
                                cmd.Parameters.AddWithValue("@freq", "")
                                cmd.Parameters.AddWithValue("@stag", "")

                                If cmd.ExecuteNonQuery() > 0 Then
                                    importate += 1
                                End If

                            Catch ex As Exception
                                erroriImportazione.Add($"Riga {rigaValida.IndiceRiga}: {ex.Message}")
                            End Try
                        Next
                    End Using

                    trans.Commit()
                End Using
            End Using

            ' Log finale
            logInfo &= $"Transazioni importate con successo: {importate}" & vbCrLf
            If erroriImportazione.Count > 0 Then
                logInfo &= $"Errori durante l'importazione: {erroriImportazione.Count}" & vbCrLf
                For Each errore In erroriImportazione.Take(5) ' Prime 5 per brevità
                    logInfo &= $"  - {errore}" & vbCrLf
                Next
            End If

            ' Scrivi log su file
            SalvaLogImportazione(logInfo)

            ' Se sono state importate transazioni, proponi classificazione automatica       
            If importate > 0 Then
                Dim classificare = MessageBox.Show(
                $"Importazione completata: {importate} transazioni." & vbCrLf & vbCrLf &
                "Vuoi classificare automaticamente le nuove transazioni?",
                "Classificazione Automatica",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

                If classificare = DialogResult.Yes Then
                    ' Usa il sistema di classificazione esistente
                    Try
                        Me.Cursor = Cursors.WaitCursor
                        Dim transazioniClassificate = ClassificatoreTransazioniMigliorato.ClassificaTutteLeTransazioniMigliorate()
                        MessageBox.Show($"Classificate {transazioniClassificate} transazioni.", "Classificazione", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Catch ex As Exception
                        MessageBox.Show($"Errore durante la classificazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Finally
                        Me.Cursor = Cursors.Default
                    End Try
                End If
            End If

        Catch ex As Exception
            MessageBox.Show($"Errore grave durante l'importazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return 0
        End Try

        Return importate
    End Function

    ' Nuova funzione per gestire file .xls
    Private Function ElaboraExcelLegacyConEsiti(percorsoFile As String,
                                           analisi As ImportatoreUniversale.RisultatoAnalisi,
                                           mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                                           maxRighe As Integer) As List(Of RigaImportConEsito)

        Dim risultati As New List(Of RigaImportConEsito)
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

        Using stream = File.Open(percorsoFile, FileMode.Open, FileAccess.Read)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                Dim dt = reader.AsDataSet().Tables(0)
                Dim hdr = analisi.HeaderRow - 1  ' zero-based

                Dim contate = 0
                For r = hdr + 1 To dt.Rows.Count - 1
                    If maxRighe > 0 AndAlso contate >= maxRighe Then Exit For

                    ' Verifica dati
                    Dim hasData = False
                    For c = 0 To dt.Columns.Count - 1
                        Dim v = dt.Rows(r)(c)
                        If v IsNot DBNull.Value AndAlso Not String.IsNullOrWhiteSpace(v.ToString()) Then
                            hasData = True : Exit For
                        End If
                    Next
                    If Not hasData Then Continue For

                    Dim campi As New List(Of String)
                    For c = 0 To dt.Columns.Count - 1
                        Dim v = dt.Rows(r)(c)
                        Dim s = If(TypeOf v Is DateTime, DirectCast(v, DateTime).ToString("dd/MM/yyyy"), v.ToString())
                        campi.Add(s)
                    Next

                    Dim err As String = ""
                    Dim tr = CreaTransazioneDaCampiConErrori(campi.ToArray(), mapping, err)
                    risultati.Add(New RigaImportConEsito With {
                    .Transazione = tr,
                    .Errore = err,
                    .IndiceRiga = r + 1,
                    .Campi = campi.ToArray()
                })

                    contate += 1
                Next
            End Using
        End Using

        Return risultati
    End Function

    ' Funzione per salvare log di importazione
    Private Sub SalvaLogImportazione(logInfo As String)
        Try
            Dim cartaellaLog = System.IO.Path.Combine(Application.StartupPath, "Logs")
            If Not Directory.Exists(cartaellaLog) Then
                Directory.CreateDirectory(cartaellaLog)
            End If

            Dim nomeFileLog = $"importazione_{DateTime.Now:yyyyMMdd}.log"
            Dim percorsoLog = System.IO.Path.Combine(cartaellaLog, nomeFileLog)

            File.AppendAllText(percorsoLog, logInfo & vbCrLf & vbCrLf, Encoding.UTF8)


        Catch ex As Exception
            ' Log fallito, ma non bloccare il processo
            Console.WriteLine($"Impossibile scrivere log: {ex.Message}")
        End Try
    End Sub

    ' Funzione helper per elaborare transazioni da file
    Private Function ElaboraTransazioniDaFile(percorsoFile As String,
                                         analisi As ImportatoreUniversale.RisultatoAnalisi,
                                         mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                                         soloAnteprima As Boolean) As List(Of TransazioneDaImportare)

        Dim transazioni As New List(Of TransazioneDaImportare)
        Dim estensione = Path.GetExtension(percorsoFile).ToLower()

        Select Case estensione
            Case ".csv", ".txt"
                transazioni = ElaboraCsv(percorsoFile, analisi, mapping, soloAnteprima)
            Case ".xlsx", ".xls"
                transazioni = ElaboraExcel(percorsoFile, mapping, soloAnteprima)
            Case ".pdf"
                transazioni = ElaboraPdf(percorsoFile, analisi, mapping, soloAnteprima)
        End Select

        Return transazioni
    End Function

    ' Funzione per elaborare CSV
    Private Function ElaboraCsv(percorsoFile As String,
                           analisi As ImportatoreUniversale.RisultatoAnalisi,
                           mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                           soloAnteprima As Boolean) As List(Of TransazioneDaImportare)

        Dim transazioni As New List(Of TransazioneDaImportare)
        Dim righe = File.ReadAllLines(percorsoFile, Encoding.UTF8)
        Dim righeElaborare = If(soloAnteprima, Math.Min(20, righe.Length - 1), righe.Length - 1)

        ' Salta la prima riga (intestazioni)
        For i = 1 To righeElaborare
            Try
                Dim campi = SplitCsvRiga(righe(i), analisi.DelimitatoreRilevato)
                Dim transazione = CreaTransazioneDaCampi(campi, mapping)
                If transazione IsNot Nothing Then
                    transazioni.Add(transazione)
                End If
            Catch ex As Exception
                ' Ignora righe con errori
            End Try
        Next

        Return transazioni
    End Function

    ' Funzione per elaborare Excel
    Private Function ElaboraExcel(percorsoFile As String,
                             mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                             soloAnteprima As Boolean) As List(Of TransazioneDaImportare)

        Dim transazioni As New List(Of TransazioneDaImportare)

        Using package As New ExcelPackage(New FileInfo(percorsoFile))
            Dim worksheet = package.Workbook.Worksheets.FirstOrDefault()
            If worksheet Is Nothing Then Return transazioni

            Dim startRow = worksheet.Dimension.Start.Row + 1 ' Salta intestazioni
            Dim endRow = worksheet.Dimension.End.Row
            Dim startCol = worksheet.Dimension.Start.Column
            Dim endCol = worksheet.Dimension.End.Column

            Dim righeElaborare = If(soloAnteprima, Math.Min(startRow + 20, endRow), endRow)

            For row = startRow To righeElaborare
                Try
                    Dim campi As New List(Of String)
                    For col = startCol To endCol
                        Dim valore As String
                        If worksheet.Cells(row, col).Value IsNot Nothing Then
                            valore = worksheet.Cells(row, col).Value.ToString()
                        Else
                            valore = ""
                        End If
                        campi.Add(valore)
                    Next

                    Dim transazione = CreaTransazioneDaCampi(campi.ToArray(), mapping)
                    If transazione IsNot Nothing Then
                        transazioni.Add(transazione)
                    End If
                Catch ex As Exception
                    ' Ignora righe con errori
                End Try
            Next
        End Using

        Return transazioni
    End Function

    ' Funzione per elaborare PDF
    Private Function ElaboraPdf(percorsoFile As String,
                           analisi As ImportatoreUniversale.RisultatoAnalisi,
                           mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                           soloAnteprima As Boolean) As List(Of TransazioneDaImportare)

        Dim transazioni As New List(Of TransazioneDaImportare)
        Dim righeElaborare = If(soloAnteprima, Math.Min(20, analisi.DatiAnteprima.Count - 1), analisi.DatiAnteprima.Count - 1)

        ' Salta la prima riga (intestazioni)
        For i = 1 To righeElaborare
            Try
                Dim transazione = CreaTransazioneDaCampi(analisi.DatiAnteprima(i), mapping)
                If transazione IsNot Nothing Then
                    transazioni.Add(transazione)
                End If
            Catch ex As Exception
                ' Ignora righe con errori
            End Try
        Next

        Return transazioni
    End Function

    ' Funzione helper per creare transazione dai campi
    Private Function CreaTransazioneDaCampi(campi As String(),
                                   mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)) As TransazioneDaImportare
        Try
            Dim indiceData = mapping(ImportatoreUniversale.TipoColonna.Data)
            Dim indiceImporto = mapping(ImportatoreUniversale.TipoColonna.Importo)
            Dim indiceDescrizione = mapping(ImportatoreUniversale.TipoColonna.Descrizione)

            ' Verifica che gli indici siano validi
            If indiceData >= campi.Length Or indiceImporto >= campi.Length Or indiceDescrizione >= campi.Length Then
                Return Nothing
            End If

            ' Parsing data
            Dim data As Date
            If Not Date.TryParse(campi(indiceData), data) Then
                Return Nothing
            End If

            ' Parsing importo - USA SOLO ParseImportoItaliano
            Dim importo As Decimal = ImportatoreUniversale.ParseImportoItaliano(campi(indiceImporto))

            ' Descrizione
            Dim descrizione = campi(indiceDescrizione).Trim()
            If String.IsNullOrEmpty(descrizione) Then
                Return Nothing
            End If

            Return New TransazioneDaImportare With {
            .Data = data,
            .Importo = importo,
            .Descrizione = descrizione
        }

        Catch ex As Exception
            Return Nothing
        End Try
    End Function


    ' Versione che restituisce sia la transazione che una descrizione dell'errore (vuota se OK)
    Private Function CreaTransazioneDaCampiConErrori(campi As String(),
                           mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                           ByRef errore As String) As TransazioneDaImportare
        errore = ""
        Try
            Dim indiceData = mapping(ImportatoreUniversale.TipoColonna.Data)
            Dim indiceImporto = mapping(ImportatoreUniversale.TipoColonna.Importo)
            Dim indiceDescrizione = mapping(ImportatoreUniversale.TipoColonna.Descrizione)

            If indiceData >= campi.Length Or indiceImporto >= campi.Length Or indiceDescrizione >= campi.Length Then
                errore = "Column index out of range"
                Return Nothing
            End If

            ' Validazione
            errore = ValidaRigaImport(campi(indiceData), campi(indiceImporto), campi(indiceDescrizione))
            If errore <> "" Then Return Nothing

            ' Parsing (se la validazione è OK)
            Dim data As Date = Date.Parse(campi(indiceData))
            Dim importo As Decimal = ImportatoreUniversale.ParseImportoItaliano(campi(indiceImporto))
            Dim descrizione = campi(indiceDescrizione).Trim()

            Return New TransazioneDaImportare With {
            .Data = data,
            .Importo = importo,
            .Descrizione = descrizione
        }

        Catch ex As Exception
            errore = $"Eccezione: {ex.Message}"
            Return Nothing
        End Try
    End Function


    ' Aggiungi questa struttura per l'anteprima avanzata:
    Public Class RigaImportConEsito
        Public Property Transazione As TransazioneDaImportare
        Public Property Errore As String
        Public Property IndiceRiga As Integer
        Public Property Campi As String()
    End Class

    Private Function ElaboraCsvConEsiti(percorsoFile As String,
                                    analisi As ImportatoreUniversale.RisultatoAnalisi,
                                    mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                                    maxRighe As Integer) As List(Of RigaImportConEsito)

        Dim risultati As New List(Of RigaImportConEsito)
        Dim righe = File.ReadAllLines(percorsoFile, Encoding.UTF8)

        ' Se maxRighe = 0, elabora tutte le righe, altrimenti limita
        Dim righeDaElaborare = If(maxRighe = 0, righe.Length - 1, Math.Min(maxRighe, righe.Length - 1))

        For i = 1 To righeDaElaborare ' Salta intestazioni (riga 0)
            Try
                Dim campi = SplitCsvRiga(righe(i), analisi.DelimitatoreRilevato)
                Dim errore As String = ""
                Dim transaz = CreaTransazioneDaCampiConErrori(campi, mapping, errore)
                risultati.Add(New RigaImportConEsito With {
                .Transazione = transaz,
                .Errore = errore,
                .IndiceRiga = i + 1,
                .Campi = campi
            })
            Catch ex As Exception
                risultati.Add(New RigaImportConEsito With {
                .Transazione = Nothing,
                .Errore = $"Errore parsing riga: {ex.Message}",
                .IndiceRiga = i + 1,
                .Campi = {}
            })
            End Try
        Next

        Return risultati
    End Function

    Private Function ElaboraExcelConEsiti(percorsoFile As String,
                                      analisi As ImportatoreUniversale.RisultatoAnalisi,
                                      mappingColonne As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                                      maxRighe As Integer) As List(Of RigaImportConEsito)

        Dim risultati As New List(Of RigaImportConEsito)

        Using package As New ExcelPackage(New IO.FileInfo(percorsoFile))
            Dim ws = package.Workbook.Worksheets.FirstOrDefault()
            If ws Is Nothing Then Return risultati

            Dim headerRow = analisi.HeaderRow
            Dim startRow = headerRow + 1
            Dim endRow = ws.Dimension.End.Row
            Dim startCol = ws.Dimension.Start.Column
            Dim endCol = ws.Dimension.End.Column

            Dim contate = 0
            For row = startRow To endRow
                If maxRighe > 0 AndAlso contate >= maxRighe Then Exit For

                Dim hasData = False
                For col = startCol To endCol
                    Dim v = ws.Cells(row, col).Value
                    If v IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(v.ToString()) Then
                        hasData = True : Exit For
                    End If
                Next
                If Not hasData Then Continue For

                Dim campi As New List(Of String)
                For col = startCol To endCol
                    Dim v = ws.Cells(row, col).Value
                    Dim s As String
                    If v IsNot Nothing Then
                        s = v.ToString()
                    Else
                        s = ""
                    End If
                    campi.Add(s)
                Next

                Dim err As String = ""
                Dim tr = CreaTransazioneDaCampiConErrori(campi.ToArray(), mappingColonne, err)
                risultati.Add(New RigaImportConEsito With {
                    .Transazione = tr,
                    .Errore = err,
                    .IndiceRiga = row,
                    .Campi = campi.ToArray()
                })

                contate += 1
            Next
        End Using

        Return risultati
    End Function

    Private Function ElaboraPdfConEsiti(percorsoFile As String,
                                    analisi As ImportatoreUniversale.RisultatoAnalisi,
                                    mapping As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer),
                                    maxRighe As Integer) As List(Of RigaImportConEsito)

        Dim risultati As New List(Of RigaImportConEsito)

        ' Se maxRighe = 0, analizza tutte le righe di analisi.DatiAnteprima (escluse intestazioni)
        Dim totalRigheDisponibili = analisi.DatiAnteprima.Count - 1
        Dim righeDaAnalizzare As Integer =
        If(maxRighe = 0, totalRigheDisponibili, Math.Min(maxRighe, totalRigheDisponibili))

        For i = 1 To righeDaAnalizzare
            Dim campi = analisi.DatiAnteprima(i)
            Dim errore As String = ""
            Dim transaz = CreaTransazioneDaCampiConErrori(campi, mapping, errore)

            risultati.Add(New RigaImportConEsito With {
            .Transazione = transaz,
            .Errore = errore,
            .IndiceRiga = i + 1,  ' +1 per contare la prima riga intestazioni
            .Campi = campi
        })
        Next

        Return risultati
    End Function

    ' Funzione che restituisce un report di eventuali problemi per ogni campo
    Private Function ValidaRigaImport(dataStr As String, importoStr As String, descrizioneStr As String) As String
        Dim msg As String = ""

        ' Data obbligatoria, formato valido
        Dim dataTmp As Date
        If String.IsNullOrWhiteSpace(dataStr) Then
            msg &= "Data mancante; "
        ElseIf Not Date.TryParse(dataStr, dataTmp) Then
            msg &= $"Data non valida: {dataStr}; "
        End If

        ' Importo obbligatorio, numerico - USA ParseImportoItaliano
        If String.IsNullOrWhiteSpace(importoStr) Then
            msg &= "Importo mancante; "
        Else
            Try
                Dim importoTmp As Decimal = ImportatoreUniversale.ParseImportoItaliano(importoStr)
            Catch ex As Exception
                msg &= $"Importo non valido: {importoStr} - {ex.Message}; "
            End Try
        End If

        ' Descrizione obbligatoria
        If String.IsNullOrWhiteSpace(descrizioneStr) Then
            msg &= "Descrizione mancante; "
        End If

        Return msg.Trim()
    End Function

    ' Helper per split CSV (copia dalla classe ImportatoreUniversale)
    Private Function SplitCsvRiga(riga As String, delimitatore As String) As String()
        Dim campi As New List(Of String)
        Dim campoCorrente As New StringBuilder()
        Dim dentroVirgolette = False
        Dim i = 0

        While i < riga.Length
            Dim carattere = riga(i)

            If carattere = """"c Then
                If dentroVirgolette AndAlso i + 1 < riga.Length AndAlso riga(i + 1) = """"c Then
                    campoCorrente.Append(""""c)
                    i += 1
                Else
                    dentroVirgolette = Not dentroVirgolette
                End If
            ElseIf Not dentroVirgolette AndAlso riga.Substring(i, Math.Min(delimitatore.Length, riga.Length - i)) = delimitatore Then
                campi.Add(campoCorrente.ToString().Trim())
                campoCorrente.Clear()
                i += delimitatore.Length - 1
            Else
                campoCorrente.Append(carattere)
            End If

            i += 1
        End While

        campi.Add(campoCorrente.ToString().Trim())
        Return campi.ToArray()
    End Function

    ' Classe helper per le transazioni importate
    Public Class TransazioneDaImportare
        Public Property Data As Date
        Public Property Importo As Decimal
        Public Property Descrizione As String
    End Class

    Private Function ChiediRigaIntestazione() As Integer
        Dim rigaDefault As Integer = 1
        Dim input As String = InputBox("Inserisci il numero di riga della riga intestazioni nel file Excel:" & vbCrLf &
                                  "(default 1 - prima riga)", "Riga Intestazione", rigaDefault.ToString())
        Dim riga As Integer
        If Integer.TryParse(input, riga) AndAlso riga > 0 Then
            Return riga
        Else
            Return rigaDefault
        End If
    End Function

    ' Evento per eliminare riga selezionata
    Private Sub BtnEliminaRiga_Click(sender As Object, e As EventArgs) Handles btnEliminaRiga.Click
        If dgvTransazioni.SelectedRows.Count = 0 Then
            MessageBox.Show("Seleziona almeno una riga da eliminare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Messaggio di conferma dinamico
        Dim messaggio As String
        If dgvTransazioni.SelectedRows.Count = 1 Then
            Dim id As Integer = CInt(dgvTransazioni.SelectedRows(0).Cells("ID").Value)
            messaggio = $"Eliminare la transazione ID {id}?"
        Else
            messaggio = $"Eliminare le {dgvTransazioni.SelectedRows.Count} transazioni selezionate?"
        End If

        If MessageBox.Show(messaggio, "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Try
            ' Raccogli gli ID delle transazioni selezionate
            Dim idsToDelete As New List(Of Integer)
            For Each row As DataGridViewRow In dgvTransazioni.SelectedRows
                idsToDelete.Add(CInt(row.Cells("ID").Value))
            Next

            ' Elimina dal database con transazione per sicurezza
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()
                    Try
                        For Each id In idsToDelete
                            Using cmd As New SQLiteCommand("DELETE FROM Transazioni WHERE ID = @id", conn, trans)
                                cmd.Parameters.AddWithValue("@id", id)
                                cmd.ExecuteNonQuery()
                            End Using
                        Next
                        trans.Commit()

                        ' Messaggio di successo
                        Dim messaggioSuccesso As String
                        If idsToDelete.Count = 1 Then
                            messaggioSuccesso = "Transazione eliminata."
                        Else
                            messaggioSuccesso = $"{idsToDelete.Count} transazioni eliminate."
                        End If
                        MessageBox.Show(messaggioSuccesso, "Eliminazione", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    Catch ex As Exception
                        trans.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            ' Ricarica i dati e aggiorna le statistiche
            CaricaTransazioni()
            AggiornaInfoMensile() ' Aggiorna i valori di entrata - uscite - risparmio

        Catch ex As Exception
            MessageBox.Show($"Errore durante l'eliminazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    ' Elimina transazione dal database
    Private Function EliminaTransazione(idTransazione As Integer) As Boolean
        Try
            Using connection As New SQLiteConnection(DatabaseManager.GetConnectionString())
                connection.Open()

                Dim query As String = "DELETE FROM Transazioni WHERE ID = @id"

                Using command As New SQLiteCommand(query, connection)
                    command.Parameters.AddWithValue("@id", idTransazione)
                    Return command.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Errore nel database durante l'eliminazione: {ex.Message}",
                      "Errore Database", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function


    Private Sub btnAnalisi_Click(sender As Object, e As EventArgs) Handles btnAnalisi.Click
        Using f As New FormAnalisi()
            f.ShowDialog()
        End Using
    End Sub

    ''' <summary>
    ''' Controlla aggiornamenti disponibili in background
    ''' </summary>
    Private Async Sub CheckForUpdatesAsync()
        Try
            ' Esegui il controllo in background per non bloccare l'UI
            Await Task.Run(Async Function()
                               Dim logger = LoggerFactory.Instance.Logger
                               Using updaterService As New UpdaterService(logger)
                                   Dim updateInfo = Await updaterService.CheckForUpdatesAsync()
                                   
                                   If updateInfo IsNot Nothing Then
                                       ' Torna al thread UI per mostrare la notifica
                                       Me.Invoke(Sub() ShowUpdateNotification(updateInfo))
                                   End If
                               End Using
                           End Function)
            
        Catch ex As Exception
            ' Log dell'errore ma non mostrare messaggi all'utente (controllo silenzioso)
            Dim logger = LoggerFactory.Instance.Logger
            logger.LogWarning("Controllo aggiornamenti fallito (normale se offline)", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Mostra la notifica di aggiornamento disponibile
    ''' </summary>
    Private Sub ShowUpdateNotification(updateInfo As VersionInfo)
        Try
            Dim result = MessageBox.Show(
                $"È disponibile una nuova versione dell'applicazione!" & vbCrLf & vbCrLf &
                $"Versione corrente: {ApplicationInfo.CurrentVersion}" & vbCrLf &
                $"Nuova versione: {updateInfo.Version}" & vbCrLf &
                $"Data rilascio: {updateInfo.ReleaseDate:dd/MM/yyyy}" & vbCrLf & vbCrLf &
                "Visualizzare i dettagli e aggiornare ora?",
                "Aggiornamento Disponibile",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information)
            
            If result = DialogResult.Yes Then
                ShowUpdateDialog(updateInfo)
            End If
            
        Catch ex As Exception
            Dim logger = LoggerFactory.Instance.Logger
            logger.LogError("Errore durante la visualizzazione della notifica di aggiornamento", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Mostra il dialog dettagliato degli aggiornamenti
    ''' </summary>
    Private Sub ShowUpdateDialog(updateInfo As VersionInfo)
        Try
            Using updateForm As New FormAggiornamenti(updateInfo)
                updateForm.ShowDialog(Me)
            End Using
        Catch ex As Exception
            Dim logger = LoggerFactory.Instance.Logger
            logger.LogError("Errore durante l'apertura del form aggiornamenti", ex)
            MessageBox.Show("Errore durante l'apertura del pannello aggiornamenti.",
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Controllo manuale aggiornamenti dal bottone
    ''' </summary>
    Private Async Sub btnAggiornamenti_Click(sender As Object, e As EventArgs) Handles btnAggiornamenti.Click
        Try
            btnAggiornamenti.Enabled = False
            btnAggiornamenti.Text = "Controllo..."
            
            Dim logger = LoggerFactory.Instance.Logger
            Using updaterService As New UpdaterService(logger)
                Dim updateInfo = Await updaterService.CheckForUpdatesAsync()
                
                If updateInfo IsNot Nothing Then
                    ShowUpdateDialog(updateInfo)
                Else
                    MessageBox.Show("L'applicazione è già aggiornata alla versione più recente.", 
                                  "Nessun Aggiornamento", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information)
                End If
            End Using
            
        Catch ex As Exception
            Dim logger = LoggerFactory.Instance.Logger
            logger.LogError("Errore durante il controllo manuale aggiornamenti", ex)
            MessageBox.Show("Errore durante il controllo degli aggiornamenti. Verificare la connessione internet.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            btnAggiornamenti.Enabled = True
            btnAggiornamenti.Text = "Aggiornamenti"
        End Try
    End Sub

    Private Sub btnChiudiDettagli_Click(sender As Object, e As EventArgs) Handles btnChiudiDettagli.Click
        ' Reset tutti i filtri
        FiltroAttivo = False
        FiltroParolaDettagliatoAttivo = False
        filtroParola = ""
        FiltroParolaDettagliataMacro = ""
        FiltroParolaDettagliataCategoria = ""

        ' Ricarica senza filtri
        CaricaTransazioni()
        AggiornaInfoMensile()

        ' Chiude il form per tornare indietro
        Me.Close()
    End Sub

    ' ===== GESTORI EVENTI PER AGGIORNAMENTO AUTOMATICO TABELLA =====
    
    ' Chiamato quando i pattern vengono modificati (aggiunti/eliminati)
    Private Sub OnPatternsChanged()
        ' Aggiorna la tabella quando i pattern cambiano
        If InvokeRequired Then
            BeginInvoke(New Action(AddressOf OnPatternsChanged))
            Return
        End If
        
        CaricaTransazioni()
        AggiornaInfoMensile()
    End Sub
    
    ' Chiamato quando le transazioni vengono classificate automaticamente
    Private Sub OnTransactionsClassified(count As Integer)
        ' Aggiorna la tabella quando le transazioni sono state classificate
        If InvokeRequired Then
            BeginInvoke(New Action(Of Integer)(AddressOf OnTransactionsClassified), count)
            Return
        End If
        
        CaricaTransazioni()
        AggiornaInfoMensile()
    End Sub
    
    ' Pulisce gli eventi quando il form viene chiuso per evitare memory leak
    Private Sub FormFinanza_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            RemoveHandler FormRaffinaPattern.PatternsChanged, AddressOf OnPatternsChanged
            RemoveHandler FormRaffinaPattern.TransactionsClassified, AddressOf OnTransactionsClassified
            RemoveHandler FormGestionePattern.PatternsChanged, AddressOf OnPatternsChanged
        Catch
            ' Ignora errori durante la rimozione degli eventi
        End Try
    End Sub

End Class
