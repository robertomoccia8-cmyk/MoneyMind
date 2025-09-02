Imports System.Data.SQLite
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Strings
Imports MoneyMind.Models

Public Class ClassificatoreTransazioniMigliorato

    ' Soglia minima per considerare un match valido
    Private Shared ReadOnly SogliaMatch As Double = 0.5

    ' Funzione di normalizzazione del testo usando funzioni VB.NET native
    Public Shared Function NormalizzaTesto(testo As String) As String
        If String.IsNullOrEmpty(testo) Then Return ""
        Dim normalizzato As String = LCase(Trim(testo))
        normalizzato = Regex.Replace(normalizzato, "[^\w\s]", " ")
        normalizzato = Regex.Replace(normalizzato, "\s+", " ").Trim()
        normalizzato = EspandiAbbreviazioni(normalizzato)
        Return normalizzato
    End Function

    ' Espansione abbreviazioni usando funzioni VB.NET native
    Private Shared Function EspandiAbbreviazioni(testo As String) As String
        Dim risultato As String = testo
        ' ... (stesse Replace di prima, senza modifiche) ...
        Return risultato
    End Function

    ' Calcola similarità tra parole usando InStr nativo
    Public Shared Function CalcolaSimilaritaParole(pattern As String, testo As String) As Double
        Dim parolePattern() As String = Split(NormalizzaTesto(pattern), " ")
        Dim testoNorm As String = NormalizzaTesto(testo)
        Dim paroleMatchate As Integer = 0
        For Each parola As String In parolePattern
            If Len(parola) > 2 AndAlso InStr(testoNorm, parola) > 0 Then
                paroleMatchate += 1
            End If
        Next
        If parolePattern.Length = 0 Then Return 0.0
        Return paroleMatchate / parolePattern.Length
    End Function

    ' Calcola punteggio di match combinato
    Public Shared Function CalcolaPunteggioMatch(descrizioneTransazione As String,
                                                pattern As String, peso As Integer) As Double
        Dim testoNorm As String = NormalizzaTesto(descrizioneTransazione)
        Dim patternNorm As String = NormalizzaTesto(pattern)
        Dim punteggio As Double = 0.0
        If InStr(testoNorm, patternNorm) > 0 Then punteggio += 0.6
        Dim similParole As Double = CalcolaSimilaritaParole(pattern, descrizioneTransazione)
        punteggio += 0.4 * similParole
        Dim moltiplicatorePeso As Double = Math.Min(peso / 5.0, 2.0)
        Return punteggio * moltiplicatorePeso
    End Function

    Public Shared Sub LogParolaMatch(IDTransazione As Integer, parolaMatch As String)
        Try
            Dim logPath As String = "verifica_match_parola_con_descrizione.txt"
            Using sw As New System.IO.StreamWriter(logPath, True)
                Dim parolaToWrite As String = If(String.IsNullOrWhiteSpace(parolaMatch), "NO MATCH", parolaMatch)
                sw.WriteLine($"ID: {IDTransazione} | Parola Pattern usata: {parolaToWrite}")
            End Using
        Catch
            ' Ignora errori di log
        End Try
    End Sub

    ' Classifica una singola transazione
    Public Shared Function ClassificaTransazioneMigliorata(transazione As Transazione) As Transazione
        Try
            Using connection As New SQLiteConnection(DatabaseManager.GetConnectionString())
                connection.Open()
                Dim query As String = "
                    SELECT Parola, MacroCategoria, Categoria,
                           Necessita, Frequenza, Stagionalita,
                           COALESCE(Peso, 5) as Peso
                    FROM Pattern
                    ORDER BY Peso DESC"
                Using cmd As New SQLiteCommand(query, connection)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        Dim migliorePattern As String = ""
                        Dim migliorePunteggio As Double = 0.0
                        Dim migliorMatch As Dictionary(Of String, String) = Nothing
                        While reader.Read()
                            Dim p As String = reader("Parola").ToString()
                            Dim peso As Integer = Convert.ToInt32(reader("Peso"))
                            Dim punteggio As Double = CalcolaPunteggioMatch(transazione.Descrizione, p, peso)
                            If punteggio > migliorePunteggio AndAlso punteggio >= SogliaMatch Then
                                migliorePunteggio = punteggio
                                migliorePattern = p
                                migliorMatch = New Dictionary(Of String, String) From {
                                    {"MacroCategoria", reader("MacroCategoria").ToString()},
                                    {"Categoria", reader("Categoria").ToString()},
                                    {"Necessita", reader("Necessita").ToString()},
                                    {"Frequenza", reader("Frequenza").ToString()},
                                    {"Stagionalita", reader("Stagionalita").ToString()}
                                }
                            End If
                        End While

                        If migliorMatch IsNot Nothing Then
                            transazione.MacroCategoria = migliorMatch("MacroCategoria")
                            transazione.Categoria = migliorMatch("Categoria")
                            transazione.Necessita = migliorMatch("Necessita")
                            transazione.Frequenza = migliorMatch("Frequenza")
                            transazione.Stagionalita = migliorMatch("Stagionalita")
                        End If

                        LogParolaMatch(transazione.ID, migliorePattern)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore durante classificazione migliorata: " & ex.Message)
        End Try
        Return transazione
    End Function

    ' Classifica tutte le transazioni non classificate
    Public Shared Function ClassificaTutteLeTransazioniMigliorate() As Integer
        Dim countClassificate As Integer = 0
        Try
            Using connection As New SQLiteConnection(DatabaseManager.GetConnectionString())
                connection.Open()
                ' Seleziona transazioni non classificate
                Dim selectQuery As String = "SELECT * FROM Transazioni WHERE MacroCategoria = '' OR MacroCategoria IS NULL"
                Dim transList As New List(Of Transazione)
                Using cmd As New SQLiteCommand(selectQuery, connection)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            transList.Add(New Transazione With {
                                .ID = Convert.ToInt32(reader("ID")),
                                .Data = Convert.ToDateTime(reader("Data")),
                                .Importo = Convert.ToDecimal(reader("Importo")),
                                .Descrizione = reader("Descrizione").ToString(),
                                .Causale = reader("Causale").ToString(),
                                .MacroCategoria = reader("MacroCategoria").ToString(),
                                .Categoria = reader("Categoria").ToString(),
                                .Necessita = reader("Necessita").ToString(),
                                .Frequenza = reader("Frequenza").ToString(),
                                .Stagionalita = reader("Stagionalita").ToString()
                            })
                        End While
                    End Using
                End Using

                For Each tr In transList
                    Dim trClass As Transazione = ClassificaTransazioneMigliorata(tr)
                    If Not String.IsNullOrEmpty(trClass.MacroCategoria) Then
                        ' Aggiorna senza DataModifica
                        Dim updateQuery As String = "
                            UPDATE Transazioni SET
                              MacroCategoria = @macro,
                              Categoria = @cat,
                              Causale = @causale,
                              Necessita = @nec,
                              Frequenza = @freq,
                              Stagionalita = @stag
                            WHERE ID = @id"
                        Using upd As New SQLiteCommand(updateQuery, connection)
                            upd.Parameters.AddWithValue("@macro", trClass.MacroCategoria)
                            upd.Parameters.AddWithValue("@cat", trClass.Categoria)
                            upd.Parameters.AddWithValue("@causale", trClass.Causale)
                            upd.Parameters.AddWithValue("@nec", trClass.Necessita)
                            upd.Parameters.AddWithValue("@freq", trClass.Frequenza)
                            upd.Parameters.AddWithValue("@stag", trClass.Stagionalita)
                            upd.Parameters.AddWithValue("@id", trClass.ID)
                            If upd.ExecuteNonQuery() > 0 Then countClassificate += 1
                        End Using
                    End If
                Next
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore durante la classificazione automatica migliorata: " & ex.Message,
                            "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        Return countClassificate
    End Function

End Class