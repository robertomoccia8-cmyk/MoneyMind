Imports System.Data.SQLite

Public Class GestoreStipendi

    Public Enum RegoleWeekend
        ANTICIPA = 0    ' Sposta al venerdì precedente
        POSTICIPA = 1   ' Sposta al lunedì successivo
        IGNORA = 2      ' Non considerare weekend
    End Enum

    Public Class ConfigurazioneStipendio
        Public Property GiornoDefault As Integer
        Public Property RegoleWeekend As RegoleWeekend
        Public Property EccezioniMensili As Dictionary(Of Integer, Integer)

        Public Sub New()
            EccezioniMensili = New Dictionary(Of Integer, Integer)
        End Sub
    End Class

    Private Shared _configurazioneCorrente As ConfigurazioneStipendio = Nothing

    ''' <summary>
    ''' Carica la configurazione stipendi dal database
    ''' </summary>
    Public Shared Function CaricaConfigurazione() As ConfigurazioneStipendio
        If _configurazioneCorrente IsNot Nothing Then
            Return _configurazioneCorrente
        End If
        _configurazioneCorrente = New ConfigurazioneStipendio()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString)
            conn.Open()
            ' Carica configurazione principale
            Const sqlConfig = "SELECT GiornoDefault, RegoleWeekend FROM ConfigurazioneStipendi ORDER BY Id DESC LIMIT 1"
            Using cmd = New SQLiteCommand(sqlConfig, conn),
                  reader = cmd.ExecuteReader()
                If reader.Read() Then
                    _configurazioneCorrente.GiornoDefault = Convert.ToInt32(reader("GiornoDefault"))
                    _configurazioneCorrente.RegoleWeekend = [Enum].Parse(GetType(RegoleWeekend), reader("RegoleWeekend").ToString())
                Else
                    _configurazioneCorrente.GiornoDefault = 23
                    _configurazioneCorrente.RegoleWeekend = RegoleWeekend.ANTICIPA
                End If
            End Using
            ' Carica eccezioni mensili
            Const sqlEccezioni = "SELECT Mese, GiornoSpeciale FROM EccezioniStipendi WHERE Attivo = 1"
            Using cmd = New SQLiteCommand(sqlEccezioni, conn),
                  reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim mese = Convert.ToInt32(reader("Mese"))
                    Dim giorno = Convert.ToInt32(reader("GiornoSpeciale"))
                    _configurazioneCorrente.EccezioniMensili(mese) = giorno
                End While
            End Using
        End Using
        Return _configurazioneCorrente
    End Function

    ''' <summary>
    ''' Salva la configurazione nel database
    ''' </summary>
    Public Shared Sub SalvaConfigurazione(configurazione As ConfigurazioneStipendio)
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString)
            conn.Open()
            Using trans = conn.BeginTransaction()
                Try
                    ' Aggiorna configurazione principale
                    Const sqlUpdate = "
                        UPDATE ConfigurazioneStipendi 
                        SET GiornoDefault = @giorno, RegoleWeekend = @regole, DataModifica = CURRENT_TIMESTAMP"
                    Using cmd = New SQLiteCommand(sqlUpdate, conn)
                        cmd.Parameters.AddWithValue("@giorno", configurazione.GiornoDefault)
                        cmd.Parameters.AddWithValue("@regole", configurazione.RegoleWeekend.ToString())
                        cmd.ExecuteNonQuery()
                    End Using
                    ' Cancella eccezioni
                    Const sqlDelete = "DELETE FROM EccezioniStipendi"
                    Using cmd = New SQLiteCommand(sqlDelete, conn)
                        cmd.ExecuteNonQuery()
                    End Using
                    ' Inserisce eccezioni aggiornate
                    For Each kv In configurazione.EccezioniMensili
                        Const sqlInsert = "
                            INSERT INTO EccezioniStipendi (Mese, GiornoSpeciale, Attivo) 
                            VALUES (@mese, @giorno, 1)"
                        Using cmd = New SQLiteCommand(sqlInsert, conn)
                            cmd.Parameters.AddWithValue("@mese", kv.Key)
                            cmd.Parameters.AddWithValue("@giorno", kv.Value)
                            cmd.ExecuteNonQuery()
                        End Using
                    Next
                    trans.Commit()
                    _configurazioneCorrente = configurazione
                Catch
                    trans.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Calcola il pay-date per un dato mese/anno considerando tutte le regole
    ''' </summary>
    Public Shared Function CalcolaPayDate(anno As Integer, mese As Integer) As Date
        Dim cfg = CaricaConfigurazione()
        ' Giorno base (default o eccezione)
        Dim giornoBase = If(cfg.EccezioniMensili.ContainsKey(mese),
                           cfg.EccezioniMensili(mese),
                           cfg.GiornoDefault)
        Dim giorno = Math.Min(giornoBase, DateTime.DaysInMonth(anno, mese))
        Dim pd = New Date(anno, mese, giorno)
        ' Applica regole weekend
        Select Case cfg.RegoleWeekend
            Case RegoleWeekend.ANTICIPA
                If pd.DayOfWeek = DayOfWeek.Saturday Then pd = pd.AddDays(-1)
                If pd.DayOfWeek = DayOfWeek.Sunday Then pd = pd.AddDays(-2)
            Case RegoleWeekend.POSTICIPA
                If pd.DayOfWeek = DayOfWeek.Saturday Then pd = pd.AddDays(2)
                If pd.DayOfWeek = DayOfWeek.Sunday Then pd = pd.AddDays(1)
            Case RegoleWeekend.IGNORA
                ' nulla
        End Select
        Return pd
    End Function

    ''' <summary>
    ''' Calcola il periodo stipendiale completo per un dato mese/anno
    ''' </summary>
    Public Shared Function CalcolaPeriodoStipendiale(anno As Integer, mese As Integer) _
        As (DataInizio As Date, DataFine As Date)

        Dim mesePrec = If(mese = 1, 12, mese - 1)
        Dim annoPrec = If(mese = 1, anno - 1, anno)
        Dim payPrev = CalcolaPayDate(annoPrec, mesePrec)
        Dim payCurr = CalcolaPayDate(anno, mese)
        Return (payPrev, payCurr.AddDays(-1))
    End Function

    ''' <summary>
    ''' Invalida la cache della configurazione
    ''' </summary>
    Public Shared Sub InvalidaCache()
        _configurazioneCorrente = Nothing
    End Sub

    ''' <summary>
    ''' Ottieni anteprima pay-dates per un anno
    ''' </summary>
    Public Shared Function OttieniAnteprimaAnnuale(anno As Integer) _
        As List(Of (Mese As Integer, PayDate As Date, Periodo As String))

        Dim list As New List(Of (Integer, Date, String))
        For mese As Integer = 1 To 12
            Dim pd = CalcolaPayDate(anno, mese)
            Dim periodo = CalcolaPeriodoStipendiale(anno, mese)
            Dim txt = $"{periodo.DataInizio:dd/MM} - {periodo.DataFine:dd/MM}"
            list.Add((mese, pd, txt))
        Next
        Return list
    End Function

End Class
