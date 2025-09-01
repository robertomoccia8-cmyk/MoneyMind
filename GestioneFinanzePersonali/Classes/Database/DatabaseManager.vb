Imports System.Data.SQLite
Imports System.IO

Public Class DatabaseManager
    Private Shared ConnectionString As String = ""

    ' Inizializza il database e le tabelle
    Public Shared Sub InitializeDatabase()
        Try
            Dim applicationPath As String = Application.StartupPath
            Dim dataFolder As String = Path.Combine(applicationPath, "Data")
            Dim databasePath As String = Path.Combine(dataFolder, "finanze.db")

            If Not Directory.Exists(dataFolder) Then
                Directory.CreateDirectory(dataFolder)
            End If

            ConnectionString = "Data Source=" & databasePath & ";Version=3;"

            Using connection As New SQLiteConnection(ConnectionString)
                connection.Open()

                ' Tabella Transazioni
                Dim createTransQuery As String = "
                CREATE TABLE IF NOT EXISTS Transazioni (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Data DATE NOT NULL,
                    Importo DECIMAL(10,2) NOT NULL,
                    Descrizione TEXT NOT NULL,
                    Causale TEXT DEFAULT '',
                    MacroCategoria TEXT DEFAULT '',
                    Categoria TEXT DEFAULT '',
                    Necessita TEXT DEFAULT '',
                    Frequenza TEXT DEFAULT '',
                    Stagionalita TEXT DEFAULT '',
                    DataInserimento DATETIME DEFAULT CURRENT_TIMESTAMP,
                    DataModifica DATETIME DEFAULT CURRENT_TIMESTAMP
                );"

                Using cmd As New SQLiteCommand(createTransQuery, connection)
                    cmd.ExecuteNonQuery()
                End Using

                ' Tabella Pattern semplificata
                Dim createPatternQuery As String = "
                CREATE TABLE IF NOT EXISTS Pattern (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Parola TEXT NOT NULL UNIQUE,
                    MacroCategoria TEXT NOT NULL DEFAULT '',
                    Categoria TEXT NOT NULL DEFAULT '',
                    Necessita TEXT NOT NULL DEFAULT '',
                    Frequenza TEXT NOT NULL DEFAULT '',
                    Stagionalita TEXT NOT NULL DEFAULT ''
                );"

                Using cmd As New SQLiteCommand(createPatternQuery, connection)
                    cmd.ExecuteNonQuery()
                End Using

                ' Aggiorna struttura Pattern: aggiunge colonne Fonte e Peso se mancanti
                AggiornaStrutturaPattern(connection)

                ' Esempi iniziali (solo se vuota)
                InserisciPatternEsempio(connection)
            End Using

            ' *** MIGRAZIONE AUTOMATICA - Chiamata dopo l'inizializzazione base ***
            MigrazioneConfigurazioneStipendi()

        Catch ex As Exception
            MessageBox.Show("Errore inizializzazione database: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' *** MIGRAZIONE AUTOMATICA - Controlla e crea le tabelle stipendi se non esistono ***
    Public Shared Sub MigrazioneConfigurazioneStipendi()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Controlla se le tabelle stipendi esistono
                Dim sqlCheck As String = "SELECT name FROM sqlite_master WHERE type='table' AND name='ConfigurazioneStipendi'"
                Using cmd As New SQLiteCommand(sqlCheck, conn)
                    Dim exists = cmd.ExecuteScalar()

                    If exists Is Nothing Then
                        ' Prima volta - crea tabelle e configurazione di default
                        CreaTabellConfigurazioneStipendi()

                        ' Messaggio informativo per utenti esistenti
                        MessageBox.Show("Benvenuto nel nuovo sistema di configurazione stipendi!" & vbCrLf &
                                       "È stata creata una configurazione predefinita:" & vbCrLf &
                                       "- Giorno stipendio: 23" & vbCrLf &
                                       "- Weekend: anticipa al venerdì" & vbCrLf &
                                       "- Dicembre: giorno 15 (tredicesima)" & vbCrLf & vbCrLf &
                                       "Puoi personalizzarla dal menu Impostazioni.",
                                       "Aggiornamento Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore durante la migrazione del sistema stipendi: " & ex.Message, "Errore Migrazione", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Crea le tabelle per la configurazione stipendi
    Public Shared Sub CreaTabellConfigurazioneStipendi()
        Using conn As New SQLiteConnection(GetConnectionString())
            conn.Open()

            ' Tabella configurazione principale
            Dim sqlConfig As String = "
            CREATE TABLE IF NOT EXISTS ConfigurazioneStipendi (
                Id INTEGER PRIMARY KEY,
                GiornoDefault INTEGER NOT NULL DEFAULT 23,
                RegoleWeekend TEXT NOT NULL DEFAULT 'ANTICIPA',
                DataCreazione DATETIME DEFAULT CURRENT_TIMESTAMP,
                DataModifica DATETIME DEFAULT CURRENT_TIMESTAMP
            )"

            ' Tabella eccezioni mensili
            Dim sqlEccezioni As String = "
            CREATE TABLE IF NOT EXISTS EccezioniStipendi (
                Id INTEGER PRIMARY KEY,
                Mese INTEGER NOT NULL,
                GiornoSpeciale INTEGER NOT NULL,
                Descrizione TEXT,
                Attivo INTEGER DEFAULT 1,
                UNIQUE(Mese)
            )"

            Using cmd As New SQLiteCommand(sqlConfig, conn)
                cmd.ExecuteNonQuery()
            End Using

            Using cmd As New SQLiteCommand(sqlEccezioni, conn)
                cmd.ExecuteNonQuery()
            End Using

            ' Inserisci configurazione predefinita se non esiste
            Dim sqlCheck As String = "SELECT COUNT(*) FROM ConfigurazioneStipendi"
            Using cmd As New SQLiteCommand(sqlCheck, conn)
                If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then
                    Dim sqlInsert As String = "INSERT INTO ConfigurazioneStipendi (GiornoDefault, RegoleWeekend) VALUES (23, 'ANTICIPA')"
                    Using cmdInsert As New SQLiteCommand(sqlInsert, conn)
                        cmdInsert.ExecuteNonQuery()
                    End Using

                    ' Aggiungi eccezione dicembre (tredicesima)
                    Dim sqlEccDic As String = "INSERT INTO EccezioniStipendi (Mese, GiornoSpeciale, Descrizione) VALUES (12, 15, 'Tredicesima - Anticipo Dicembre')"
                    Using cmdEcc As New SQLiteCommand(sqlEccDic, conn)
                        cmdEcc.ExecuteNonQuery()
                    End Using
                End If
            End Using
        End Using
    End Sub

    ' Test di validazione configurazione stipendi
    Public Shared Sub ValidaConfigurazioneStipendi()
        Try
            Dim config = GestoreStipendi.CaricaConfigurazione()
            Dim annoTest = Date.Today.Year

            For mese As Integer = 1 To 12
                Dim payDate = GestoreStipendi.CalcolaPayDate(annoTest, mese)
                Dim periodo = GestoreStipendi.CalcolaPeriodoStipendiale(annoTest, mese)

                ' Verifica che non ci siano sovrapposizioni
                If mese < 12 Then
                    Dim periodoSuccessivo = GestoreStipendi.CalcolaPeriodoStipendiale(annoTest, mese + 1)
                    If periodo.DataFine >= periodoSuccessivo.DataInizio Then
                        Throw New Exception($"Sovrapposizione rilevata tra {mese} e {mese + 1}")
                    End If
                End If
            Next

            Debug.WriteLine("Configurazione stipendi validata con successo")

        Catch ex As Exception
            MessageBox.Show($"Errore nella configurazione stipendi: {ex.Message}", "Errore Configurazione", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '
    'Rimuove la colonna SottoCategoria dallo schema del database ***
    '
    Public Shared Sub RimuoviSottoCategoriaSchema()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    ' 1. Crea nuove tabelle senza SottoCategoria
                    Dim sqlNuoveTabelle As String = "
                    CREATE TABLE IF NOT EXISTS Pattern_Temp (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Parola TEXT NOT NULL,
                        MacroCategoria TEXT NOT NULL,
                        Categoria TEXT NOT NULL,
                        Necessita TEXT,
                        Frequenza TEXT,
                        Stagionalita TEXT,
                        Fonte TEXT DEFAULT 'Manuale',
                        Peso INTEGER DEFAULT 5
                    );
                    
                    CREATE TABLE IF NOT EXISTS Transazioni_Temp (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Data DATE NOT NULL,
                        Importo DECIMAL(10,2) NOT NULL,
                        Descrizione TEXT NOT NULL,
                        Causale TEXT,
                        MacroCategoria TEXT,
                        Categoria TEXT,
                        Necessita TEXT,
                        Frequenza TEXT,
                        Stagionalita TEXT
                    );"

                    Using cmd As New SQLiteCommand(sqlNuoveTabelle, conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' 2. Copia dati dalle tabelle originali (senza SottoCategoria)
                    Dim sqlCopiaPattern As String = "
                    INSERT INTO Pattern_Temp (ID, Parola, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita, Fonte, Peso)
                    SELECT ID, Parola, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita, Fonte, Peso 
                    FROM Pattern"

                    Using cmd As New SQLiteCommand(sqlCopiaPattern, conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    Dim sqlCopiaTransazioni As String = "
                    INSERT INTO Transazioni_Temp (ID, Data, Importo, Descrizione, Causale, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita)
                    SELECT ID, Data, Importo, Descrizione, Causale, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita 
                    FROM Transazioni"

                    Using cmd As New SQLiteCommand(sqlCopiaTransazioni, conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' 3. Elimina tabelle originali
                    Using cmd As New SQLiteCommand("DROP TABLE Pattern", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using
                    Using cmd As New SQLiteCommand("DROP TABLE Transazioni", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' 4. Rinomina tabelle temporanee
                    Using cmd As New SQLiteCommand("ALTER TABLE Pattern_Temp RENAME TO Pattern", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using
                    Using cmd As New SQLiteCommand("ALTER TABLE Transazioni_Temp RENAME TO Transazioni", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                End Using
            End Using

            'MessageBox.Show("Schema database aggiornato: SottoCategoria rimossa con successo!", "Migrazione Completata", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Errore durante la migrazione database: {ex.Message}", "Errore Migrazione", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Throw
        End Try
    End Sub

    ' Verifica e aggiunge colonne mancanti nella tabella Pattern
    Private Shared Sub AggiornaStrutturaPattern(connection As SQLiteConnection)
        Dim cols As New List(Of String)
        Using cmd As New SQLiteCommand("PRAGMA table_info(Pattern)", connection)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    cols.Add(reader("name").ToString().ToLower())
                End While
            End Using
        End Using

        If Not cols.Contains("fonte") Then
            Using cmd As New SQLiteCommand("ALTER TABLE Pattern ADD COLUMN Fonte TEXT DEFAULT ''", connection)
                cmd.ExecuteNonQuery()
            End Using
        End If

        If Not cols.Contains("peso") Then
            Using cmd As New SQLiteCommand("ALTER TABLE Pattern ADD COLUMN Peso INTEGER DEFAULT 5", connection)
                cmd.ExecuteNonQuery()
            End Using
        End If
    End Sub

    ' Inserisce pattern di esempio se la tabella è vuota
    Private Shared Sub InserisciPatternEsempio(connection As SQLiteConnection)
        Using countCmd As New SQLiteCommand("SELECT COUNT(*) FROM Pattern", connection)
            Dim cnt As Integer = CInt(countCmd.ExecuteScalar())
            If cnt > 0 Then Return
        End Using

        Dim patternsBase As New List(Of (String, String, String, String, String, String, Integer)) From {
            ("Supermercato", "Alimentari", "Spesa quotidiana", "SUPERMERCATO", "Essenziale", "Ricorrente", 10),
            ("Farmacia", "Salute", "Medicinali", "FARMACIA", "Essenziale", "Occasionale", 10),
            ("Benzina", "Trasporti", "Carburante", "BENZINA", "Essenziale", "Ricorrente", 10),
            ("Amazon", "Shopping", "E-commerce", "AMAZON", "Utile", "Occasionale", 9)
        }

        For Each p In patternsBase
            Dim insertQuery As String = "
            INSERT OR IGNORE INTO Pattern
              (MacroCategoria, Categoria, Parola, Necessita, Frequenza, Stagionalita, Peso, Fonte)
            VALUES
              (@macro, @cat, @parola, @nec, @freq, @stag, @peso, '');"

            Using cmd As New SQLiteCommand(insertQuery, connection)
                cmd.Parameters.AddWithValue("@macro", p.Item1)
                cmd.Parameters.AddWithValue("@cat", p.Item2)
                'cmd.Parameters.AddWithValue("@sotto", p.Item3)
                cmd.Parameters.AddWithValue("@parola", p.Item4)
                cmd.Parameters.AddWithValue("@nec", p.Item5)
                cmd.Parameters.AddWithValue("@freq", p.Item6)
                cmd.Parameters.AddWithValue("@stag", p.Item5) ' usa Necessita per Stagionalita
                cmd.Parameters.AddWithValue("@peso", p.Item7)
                cmd.ExecuteNonQuery()
            End Using
        Next
    End Sub

    ' Proprietà per ottenere la connection string
    Public Shared ReadOnly Property GetConnectionString As String
        Get
            Return ConnectionString
        End Get
    End Property

End Class
