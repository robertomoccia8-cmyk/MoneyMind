Imports System.Data.SQLite
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports ExcelDataReader
Imports iTextSharp.text.pdf
Imports iTextSharp.text.pdf.parser
Imports OfficeOpenXml
Imports System.Linq


Public Class ImportatoreUniversale
    Public Structure ColonnaMappata
        Public Property Indice As Integer
        Public Property Nome As String
        Public Property Tipo As TipoColonna
        Public Property Confidenza As Double ' 0-1 per indicare quanto siamo sicuri
    End Structure

    Public Enum TipoColonna
        Data
        Importo
        Descrizione
        Sconosciuto
    End Enum

    Public Structure RisultatoAnalisi
        Public Property DelimitatoreRilevato As String
        Public Property ColonneRilevate As List(Of ColonnaMappata)
        Public Property DatiAnteprima As List(Of String())
        Public Property NumeroRighe As Integer
        Public Property RiconoscimentoAutomatico As Boolean
        Public Property HeaderRow As Integer
        Public Property Encoding As Encoding
        Public Property Errori As List(Of String)
        Public Property Avvisi As List(Of String)
    End Structure

    Public Shared Function AnalizzaFile(percorsoFile As String) As RisultatoAnalisi
        If Not File.Exists(percorsoFile) Then
            Throw New FileNotFoundException($"File non trovato: {percorsoFile}")
        End If

        Dim fileInfo As New FileInfo(percorsoFile)
        If fileInfo.Length = 0 Then
            Throw New Exception("Il file è vuoto")
        End If

        If fileInfo.Length > 100 * 1024 * 1024 Then ' >100MB
            Throw New Exception("File troppo grande (max 100MB)")
        End If

        Dim estensione = System.IO.Path.GetExtension(percorsoFile).ToLower()

        Try
            Select Case estensione
                Case ".csv", ".txt"
                    Return AnalizzaCsv(percorsoFile)
                Case ".xlsx", ".xls"
                    Return AnalizzaExcel(percorsoFile)
                Case ".pdf"
                    Return AnalizzaPdf(percorsoFile)
                Case Else
                    Throw New NotSupportedException($"Formato file non supportato: {estensione}")
            End Select
        Catch ex As Exception
            Throw New Exception($"Errore durante l'analisi del file: {ex.Message}", ex)
        End Try
    End Function

    Public Shared Function AnalizzaCsv(percorsoFile As String) As RisultatoAnalisi
        Dim risultato As New RisultatoAnalisi() With {
            .Errori = New List(Of String),
            .Avvisi = New List(Of String)
        }

        Try
            ' Rileva encoding
            risultato.Encoding = RilevaEncoding(percorsoFile)

            ' Leggi tutte le righe con encoding corretto
            Dim righe = File.ReadAllLines(percorsoFile, risultato.Encoding)
            If righe.Length = 0 Then
                Throw New Exception("Il file CSV/TXT è vuoto")
            End If

            ' Rimuovi righe completamente vuote all'inizio
            righe = righe.SkipWhile(Function(r) String.IsNullOrWhiteSpace(r)).ToArray()
            If righe.Length = 0 Then
                Throw New Exception("Il file non contiene dati validi")
            End If

            ' Rileva delimitatore con maggiore accuratezza
            risultato.DelimitatoreRilevato = RilevaDelimitatoreAvanzato(righe.Take(Math.Min(10, righe.Length)).ToArray())

            ' Trova la riga di intestazione
            Dim rigaIntestazione = TrovaRigaIntestazione(righe, risultato.DelimitatoreRilevato)
            risultato.HeaderRow = rigaIntestazione + 1

            ' Estrai intestazioni
            Dim intestazioni = SplitCsvRiga(righe(rigaIntestazione), risultato.DelimitatoreRilevato)
            Dim campioneDati As String()() = righe.
            Skip(rigaIntestazione + 1).
            Take(5).
            Select(Function(r) SplitCsvRiga(r, risultato.DelimitatoreRilevato)).
            ToArray()
            risultato.ColonneRilevate = AnalizzaIntestazioniAvanzate(intestazioni, campioneDati, risultato.DelimitatoreRilevato)

            ' Prepara anteprima (righe dati significative)
            risultato.DatiAnteprima = New List(Of String())
            Dim righeDatiCaricate = 0
            Dim maxAnteprima = 10

            For i = rigaIntestazione + 1 To righe.Length - 1
                If righeDatiCaricate >= maxAnteprima Then Exit For

                Dim campi = SplitCsvRiga(righe(i), risultato.DelimitatoreRilevato)
                If ValidaRigaDati(campi) Then
                    risultato.DatiAnteprima.Add(PulisciCampiAnteprima(campi))
                    righeDatiCaricate += 1
                End If
            Next

            risultato.NumeroRighe = ContaRigheDatiValide(righe.Skip(rigaIntestazione + 1).ToArray(), risultato.DelimitatoreRilevato)
            risultato.RiconoscimentoAutomatico = VerificaRiconoscimentoCompleto(risultato.ColonneRilevate)

            ' Aggiungi avvisi se necessario
            If risultato.DelimitatoreRilevato = "," Then
                risultato.Avvisi.Add("Rilevato delimitatore virgola: attenzione ai numeri decimali")
            End If

        Catch ex As Exception
            risultato.Errori.Add($"Errore CSV: {ex.Message}")
            Throw
        End Try

        Return risultato
    End Function

    Public Shared Function AnalizzaExcel(percorsoFile As String, Optional rigaIntestazione As Integer = 1) As RisultatoAnalisi
        Dim risultato As New RisultatoAnalisi() With {
            .Errori = New List(Of String),
            .Avvisi = New List(Of String),
            .HeaderRow = rigaIntestazione
        }

        Try
            Dim estensione = System.IO.Path.GetExtension(percorsoFile).ToLower()

            If estensione = ".xls" Then
                Return AnalizzaExcelLegacy(percorsoFile, rigaIntestazione)
            Else
                ' EPPlus per .xlsx
                Using package As New OfficeOpenXml.ExcelPackage(New IO.FileInfo(percorsoFile))
                    Dim worksheet = package.Workbook.Worksheets.FirstOrDefault()
                    If worksheet Is Nothing Then
                        Throw New Exception("Il file Excel non contiene fogli di lavoro")
                    End If

                    If worksheet.Dimension Is Nothing Then
                        Throw New Exception("Il foglio Excel è vuoto")
                    End If

                    Dim startRow As Integer = rigaIntestazione
                    Dim endRow As Integer = worksheet.Dimension.End.Row
                    Dim startCol As Integer = worksheet.Dimension.Start.Column
                    Dim endCol As Integer = worksheet.Dimension.End.Column

                    ' Validazioni
                    If startRow > endRow Then
                        Throw New Exception($"Riga intestazione {startRow} oltre i dati disponibili ({endRow})")
                    End If

                    ' Estrai intestazioni
                    Dim intestazioni As New List(Of String)
                    For col = startCol To endCol
                        Dim valore As String = ""
                        Try
                            If worksheet.Cells(startRow, col).Value IsNot Nothing Then
                                valore = worksheet.Cells(startRow, col).Value.ToString().Trim()
                            End If
                            If String.IsNullOrWhiteSpace(valore) Then
                                valore = $"Colonna{col}"
                            End If
                        Catch ex As Exception
                            valore = $"Colonna{col}"
                            risultato.Avvisi.Add($"Errore lettura cella ({startRow},{col}): {ex.Message}")
                        End Try
                        intestazioni.Add(valore)
                    Next

                    ' Analizza campione dati per migliorare riconoscimento
                    Dim campioneDati As New List(Of String())
                    For row = startRow + 1 To Math.Min(startRow + 5, endRow)
                        Dim rigaDati As New List(Of String)
                        For col = startCol To endCol
                            rigaDati.Add(EstraiValoreCellaExcel(worksheet, row, col))
                        Next
                        If ValidaRigaDati(rigaDati.ToArray()) Then
                            campioneDati.Add(rigaDati.ToArray())
                        End If
                    Next

                    risultato.ColonneRilevate = AnalizzaIntestazioniAvanzate(intestazioni.ToArray(), campioneDati.ToArray())
                    risultato.DatiAnteprima = New List(Of String())

                    ' Carica anteprima
                    Dim righeDatiCaricate = 0
                    Dim maxRigheAnteprima = 10

                    For row = startRow + 1 To endRow
                        If righeDatiCaricate >= maxRigheAnteprima Then Exit For

                        Dim rigaHaDati = False
                        For col = startCol To endCol
                            If worksheet.Cells(row, col).Value IsNot Nothing AndAlso
                               Not String.IsNullOrWhiteSpace(worksheet.Cells(row, col).Value.ToString()) Then
                                rigaHaDati = True
                                Exit For
                            End If
                        Next

                        If rigaHaDati Then
                            Dim rigaDati As New List(Of String)
                            For col = startCol To endCol
                                rigaDati.Add(EstraiValoreCellaExcel(worksheet, row, col))
                            Next

                            If ValidaRigaDati(rigaDati.ToArray()) Then
                                risultato.DatiAnteprima.Add(rigaDati.ToArray())
                                righeDatiCaricate += 1
                            End If
                        End If
                    Next

                    risultato.NumeroRighe = CountRigheDatiEffettive(worksheet, startRow + 1, endRow, startCol, endCol)
                    risultato.RiconoscimentoAutomatico = VerificaRiconoscimentoCompleto(risultato.ColonneRilevate)
                End Using
            End If

        Catch ex As Exception
            risultato.Errori.Add($"Errore Excel: {ex.Message}")
            Throw
        End Try

        Return risultato
    End Function

    Public Shared Function AnalizzaExcelLegacy(percorsoFile As String, Optional rigaIntestazione As Integer = 1) As RisultatoAnalisi
        Dim risultato As New RisultatoAnalisi() With {
        .Errori = New List(Of String),
        .Avvisi = New List(Of String),
        .HeaderRow = rigaIntestazione
    }

        Try
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

            Using stream = File.Open(percorsoFile, FileMode.Open, FileAccess.Read)
                Using reader = ExcelReaderFactory.CreateReader(stream)
                    Dim dataSet = reader.AsDataSet()
                    If dataSet.Tables.Count = 0 OrElse dataSet.Tables(0).Rows.Count = 0 Then
                        Throw New Exception("Il file Excel è vuoto")
                    End If

                    Dim dataTable = dataSet.Tables(0)
                    Dim rowIndexHeader As Integer = Math.Max(0, rigaIntestazione - 1)

                    If rowIndexHeader >= dataTable.Rows.Count Then
                        Throw New Exception($"Riga intestazione {rigaIntestazione} oltre i dati disponibili")
                    End If

                    ' Estrai intestazioni
                    Dim intestazioni As New List(Of String)
                    For i = 0 To dataTable.Columns.Count - 1
                        Dim valore As String = ""
                        Try
                            If dataTable.Rows(rowIndexHeader)(i) IsNot Nothing AndAlso
                           dataTable.Rows(rowIndexHeader)(i) IsNot DBNull.Value Then
                                valore = dataTable.Rows(rowIndexHeader)(i).ToString().Trim()
                            End If
                            If String.IsNullOrWhiteSpace(valore) Then
                                valore = $"Colonna{i + 1}"
                            End If
                        Catch ex As Exception
                            valore = $"Colonna{i + 1}"
                            risultato.Avvisi.Add($"Errore lettura intestazione colonna {i + 1}: {ex.Message}")
                        End Try
                        intestazioni.Add(valore)
                    Next

                    ' Analizza campione dati
                    Dim campioneDati As New List(Of String())
                    For row = rowIndexHeader + 1 To Math.Min(rowIndexHeader + 5, dataTable.Rows.Count - 1)
                        Dim rigaDati As New List(Of String)
                        For col = 0 To dataTable.Columns.Count - 1
                            rigaDati.Add(EstraiValoreCellaDataTable(dataTable, row, col))
                        Next
                        If ValidaRigaDati(rigaDati.ToArray()) Then
                            campioneDati.Add(rigaDati.ToArray())
                        End If
                    Next

                    risultato.ColonneRilevate = AnalizzaIntestazioniAvanzate(intestazioni.ToArray(), campioneDati.ToArray())
                    risultato.DatiAnteprima = New List(Of String())

                    ' Carica anteprima
                    Dim righeDatiCaricate = 0
                    Dim maxRigheAnteprima = 10

                    For row = rowIndexHeader + 1 To dataTable.Rows.Count - 1
                        If righeDatiCaricate >= maxRigheAnteprima Then Exit For

                        Dim rigaHaDati = False
                        For col = 0 To dataTable.Columns.Count - 1
                            If dataTable.Rows(row)(col) IsNot Nothing AndAlso
                           dataTable.Rows(row)(col) IsNot DBNull.Value AndAlso
                           Not String.IsNullOrWhiteSpace(dataTable.Rows(row)(col).ToString()) Then
                                rigaHaDati = True
                                Exit For
                            End If
                        Next

                        If rigaHaDati Then
                            Dim rigaDati As New List(Of String)
                            For col = 0 To dataTable.Columns.Count - 1
                                rigaDati.Add(EstraiValoreCellaDataTable(dataTable, row, col))
                            Next

                            If ValidaRigaDati(rigaDati.ToArray()) Then
                                risultato.DatiAnteprima.Add(rigaDati.ToArray())
                                righeDatiCaricate += 1
                            End If
                        End If
                    Next

                    risultato.NumeroRighe = CountRigheDatiEffettiveDataTable(dataTable, rowIndexHeader + 1)
                    risultato.RiconoscimentoAutomatico = VerificaRiconoscimentoCompleto(risultato.ColonneRilevate)
                End Using
            End Using

        Catch ex As ExcelDataReader.Exceptions.HeaderException
            ' Gestione specifica per file Excel corrotti o non validi
            Dim messaggio = "Il file Excel selezionato non può essere letto." & vbCrLf & vbCrLf &
                       "Possibili cause:" & vbCrLf &
                       "• Il file è corrotto o danneggiato" & vbCrLf &
                       "• Il file non è realmente un file Excel (.xls)" & vbCrLf &
                       "• Il file è protetto da password" & vbCrLf &
                       "• Il formato del file è obsoleto o non supportato" & vbCrLf & vbCrLf &
                       "Suggerimenti:" & vbCrLf &
                       "• Prova ad aprire il file in Excel e salvarlo nuovamente" & vbCrLf &
                       "• Verifica che l'estensione del file corrisponda al formato reale" & vbCrLf &
                       "• Se il file è in formato .xlsx, prova a convertirlo in .xls" & vbCrLf &
                       "• Controlla che il file non sia protetto o crittografato"

            MessageBox.Show(messaggio, "File Excel Non Leggibile", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            risultato.Errori.Add("File Excel corrotto o formato non valido")
            Throw New Exception("File Excel non leggibile: il file potrebbe essere corrotto, protetto o in un formato non supportato.")

        Catch ex As UnauthorizedAccessException
            MessageBox.Show($"Impossibile accedere al file:{vbCrLf}{vbCrLf}" &
                       $"• Il file potrebbe essere aperto in un altro programma{vbCrLf}" &
                       $"• Non si dispone dei permessi per leggere il file{vbCrLf}" &
                       $"• Il file è protetto da scrittura{vbCrLf}{vbCrLf}" &
                       $"Chiudi il file se è aperto in Excel e riprova.",
                       "Accesso Negato", MessageBoxButtons.OK, MessageBoxIcon.Stop)

            risultato.Errori.Add("Accesso negato al file")
            Throw

        Catch ex As IOException
            MessageBox.Show($"Errore di lettura del file:{vbCrLf}{vbCrLf}" &
                       $"• Il file è in uso da un altro programma{vbCrLf}" &
                       $"• Problemi di connessione di rete (se il file è in rete){vbCrLf}" &
                       $"• Spazio insufficiente sul disco{vbCrLf}{vbCrLf}" &
                       $"Dettagli tecnici: {ex.Message}",
                       "Errore I/O", MessageBoxButtons.OK, MessageBoxIcon.Error)

            risultato.Errori.Add($"Errore I/O: {ex.Message}")
            Throw

        Catch ex As Exception
            risultato.Errori.Add($"Errore Excel Legacy: {ex.Message}")
            Throw
        End Try

        Return risultato
    End Function


    Public Shared Function AnalizzaPdf(percorsoFile As String) As RisultatoAnalisi
        Dim risultato As New RisultatoAnalisi() With {
            .Errori = New List(Of String),
            .Avvisi = New List(Of String)
        }

        Try
            Dim testoCompleto As New StringBuilder()
            Using reader As New PdfReader(percorsoFile)
                For i = 1 To reader.NumberOfPages
                    Try
                        Dim strategy As New SimpleTextExtractionStrategy()
                        Dim testoPagina = PdfTextExtractor.GetTextFromPage(reader, i, strategy)
                        testoCompleto.AppendLine(testoPagina)
                    Catch ex As Exception
                        risultato.Avvisi.Add($"Errore lettura pagina {i}: {ex.Message}")
                    End Try
                Next
            End Using

            If testoCompleto.Length = 0 Then
                Throw New Exception("Impossibile estrarre testo dal PDF")
            End If

            ' Estrai righe tabellari
            Dim righe = EstraiRigheTabellareDaPdf(testoCompleto.ToString())
            If righe.Count = 0 Then
                Throw New Exception("Nessuna tabella rilevata nel PDF")
            End If

            risultato.DelimitatoreRilevato = vbTab
            risultato.HeaderRow = 1

            If righe.Count > 0 Then
                risultato.ColonneRilevate = AnalizzaIntestazioniAvanzate(righe(0), righe.Skip(1).Take(5).ToArray())
                risultato.DatiAnteprima = righe.Skip(1).Take(10).ToList()
                risultato.NumeroRighe = righe.Count - 1
            End If

            risultato.RiconoscimentoAutomatico = VerificaRiconoscimentoCompleto(risultato.ColonneRilevate)
            risultato.Avvisi.Add("Dati PDF: verificare accuratezza dell'estrazione")

        Catch ex As Exception
            risultato.Errori.Add($"Errore PDF: {ex.Message}")
            Throw
        End Try

        Return risultato
    End Function

    ' FUNZIONE MIGLIORATA PER PARSING IMPORTI CON TUTTI I CASI
    Public Shared Function ParseImportoItaliano(importoStr As String) As Decimal
        Try
            If String.IsNullOrWhiteSpace(importoStr) Then
                Throw New FormatException("Importo vuoto")
            End If

            ' 1. Pulizia iniziale MOLTO aggressiva
            Dim s = importoStr.Trim().TrimStart(ChrW(&HFEFF))
            ' RIMUOVI SUBITO caratteri di delimitazione CSV che possono essere rimasti
            s = s.TrimEnd(";"c, ","c, "|"c, vbTab(0), " "c)
            ' Rimuovi simboli di valuta comuni
            s = s.Replace("€", "").Replace("EUR", "").Replace("$", "").Replace("USD", "").Trim()
            ' Rimuovi spazi interni
            s = Regex.Replace(s, "\s+", "")

            ' 2. Conserva il segno meno se presente
            Dim isNegative As Boolean = s.StartsWith("-")
            If isNegative Then
                s = s.Substring(1) ' Rimuovi il segno meno temporaneamente
            End If

            ' 3. Estrai solo numeri, punti e virgole (senza il meno)
            Dim match = Regex.Match(s, "[\d\.,]+")
            If Not match.Success OrElse match.Value.Length = 0 Then
                Throw New FormatException($"Nessun numero trovato in '{importoStr}' (pulito: '{s}')")
            End If
            s = match.Value

            ' 4. Gestione intelligente di punti e virgole
            If s.Contains(",") AndAlso s.Contains(".") Then
                Dim ultimaVirgola = s.LastIndexOf(",")
                Dim ultimoPunto = s.LastIndexOf(".")
                If ultimaVirgola > ultimoPunto Then
                    ' Formato: 1.234,56
                    s = s.Substring(0, ultimaVirgola).Replace(".", "") + "," + s.Substring(ultimaVirgola + 1)
                Else
                    ' Formato: 1,234.56
                    s = s.Substring(0, ultimoPunto).Replace(",", "") + "," + s.Substring(ultimoPunto + 1)
                End If
            ElseIf s.Contains(".") AndAlso Not s.Contains(",") Then
                ' Solo punti: determina se è decimale o migliaia
                Dim ultimoPunto = s.LastIndexOf(".")
                Dim cifreDopoUltimo = s.Length - ultimoPunto - 1
                If cifreDopoUltimo = 2 Then
                    ' 1234.56 -> 1234,56
                    s = s.Replace(".", ",")
                Else
                    ' 1.234 -> 1234 (migliaia)
                    s = s.Replace(".", "")
                End If
            End If
            ' Per il caso "1.136,50" - questo è già formato italiano corretto
            ' Non fare nulla, s rimane "1136,50" dopo la rimozione del punto delle migliaia

            ' 5. Parse finale con supporto segno meno
            Dim valore As Decimal
            If Decimal.TryParse(s, NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"), valore) Then
                Return If(isNegative, -valore, valore)
            Else
                Throw New FormatException($"Impossibile convertire '{s}' in decimale (da '{importoStr}')")
            End If

        Catch ex As Exception When Not TypeOf ex Is FormatException
            Throw New FormatException($"Errore parsing importo '{importoStr}': {ex.Message}")
        End Try
    End Function

    ' FUNZIONI HELPER AVANZATE

    Private Shared Function RilevaEncoding(percorsoFile As String) As Encoding
        Try
            Using fs As New FileStream(percorsoFile, FileMode.Open, FileAccess.Read)
                Dim bom(3) As Byte
                fs.Read(bom, 0, 4)

                ' Controlla BOM
                If bom(0) = &HEF AndAlso bom(1) = &HBB AndAlso bom(2) = &HBF Then
                    Return Encoding.UTF8
                ElseIf bom(0) = &HFF AndAlso bom(1) = &HFE Then
                    Return Encoding.Unicode ' UTF-16 LE
                ElseIf bom(0) = &HFE AndAlso bom(1) = &HFF Then
                    Return Encoding.BigEndianUnicode ' UTF-16 BE
                End If
            End Using

            ' Prova a leggere come UTF-8
            Try
                File.ReadAllText(percorsoFile, Encoding.UTF8)
                Return Encoding.UTF8
            Catch
                Return Encoding.Default
            End Try
        Catch
            Return Encoding.UTF8
        End Try
    End Function

    Private Shared Function RilevaDelimitatoreAvanzato(righe As String()) As String
        Dim delimitatori = {";", ",", vbTab, "|", ":"}
        Dim punteggi As New Dictionary(Of String, Double)

        For Each delim In delimitatori
            Dim consistenza = CalcolaConsistenzaDelimitatore(righe, delim)
            Dim frequenza = righe.Sum(Function(r) ContaDelimitatoriInRiga(r, delim))
            Dim punteggio = (consistenza / 100.0) * 0.7 + (Math.Min(frequenza, 100) / 100.0) * 0.3
            punteggi(delim) = punteggio
        Next

        Return punteggi.OrderByDescending(Function(p) p.Value).First().Key
    End Function

    Private Shared Function TrovaRigaIntestazione(righe As String(), delimitatore As String) As Integer
        For i = 0 To Math.Min(5, righe.Length - 1)
            Dim campi = SplitCsvRiga(righe(i), delimitatore)
            If campi.Length >= 3 AndAlso campi.Any(Function(c) RiconosciTipoColonna(c) <> TipoColonna.Sconosciuto) Then
                Return i
            End If
        Next
        Return 0 ' Default prima riga
    End Function

    Private Shared Function AnalizzaIntestazioniAvanzate(intestazioni As String(), Optional campioneDati As String()() = Nothing, Optional delimitatore As String = "") As List(Of ColonnaMappata)
        Dim colonneRilevate As New List(Of ColonnaMappata)

        For i = 0 To intestazioni.Length - 1
            Dim tipo = RiconosciTipoColonna(intestazioni(i))
            Dim confidenza = 0.5

            ' Migliora riconoscimento con dati di esempio
            If campioneDati IsNot Nothing AndAlso campioneDati.Length > 0 Then
                Dim campione As New List(Of String)
                For Each riga In campioneDati
                    If i < riga.Length AndAlso Not String.IsNullOrWhiteSpace(riga(i)) Then
                        campione.Add(riga(i))
                    End If
                Next

                If campione.Count > 0 Then
                    Dim tipoRilevato = RiconosciTipoDaDati(campione)
                    If tipoRilevato <> TipoColonna.Sconosciuto Then
                        If tipo = TipoColonna.Sconosciuto OrElse tipoRilevato = tipo Then
                            tipo = tipoRilevato
                            confidenza = 0.8
                        End If
                    End If
                End If
            End If

            Dim colonna As New ColonnaMappata With {
                .Indice = i,
                .Nome = intestazioni(i),
                .Tipo = tipo,
                .Confidenza = confidenza
            }
            colonneRilevate.Add(colonna)
        Next

        Return colonneRilevate
    End Function

    Private Shared Function RiconosciTipoDaDati(campione As List(Of String)) As TipoColonna
        Dim conteggioDate = 0
        Dim conteggioNumeri = 0
        Dim conteggioTesto = 0

        For Each valore In campione.Take(Math.Min(5, campione.Count))
            If IsData(valore) Then
                conteggioDate += 1
            ElseIf IsImporto(valore) Then
                conteggioNumeri += 1
            Else
                conteggioTesto += 1
            End If
        Next

        Dim totale = conteggioDate + conteggioNumeri + conteggioTesto
        If totale = 0 Then Return TipoColonna.Sconosciuto

        Dim percentualeDate = conteggioDate / totale
        Dim percentualeNumeri = conteggioNumeri / totale

        If percentualeDate > 0.6 Then Return TipoColonna.Data
        If percentualeNumeri > 0.6 Then Return TipoColonna.Importo
        If conteggioTesto > 0 Then Return TipoColonna.Descrizione

        Return TipoColonna.Sconosciuto
    End Function

    Private Shared Function IsData(valore As String) As Boolean
        If String.IsNullOrWhiteSpace(valore) Then Return False

        Dim formatiData = {
            "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy",
            "dd/MM/yy", "dd-MM-yy", "yy-MM-dd", "MM/dd/yy",
            "d/M/yyyy", "d-M-yyyy", "yyyy-M-d", "M/d/yyyy"
        }

        For Each formato In formatiData
            If DateTime.TryParseExact(valore.Trim(), formato, CultureInfo.InvariantCulture, DateTimeStyles.None, Nothing) Then
                Return True
            End If
        Next

        Return DateTime.TryParse(valore.Trim(), Nothing)
    End Function

    Private Shared Function IsImporto(valore As String) As Boolean
        Try
            ParseImportoItaliano(valore)
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Shared Function ValidaRigaDati(campi As String()) As Boolean
        If campi Is Nothing OrElse campi.Length = 0 Then Return False
        Return campi.Any(Function(c) Not String.IsNullOrWhiteSpace(c))
    End Function

    Private Shared Function PulisciCampiAnteprima(campi As String()) As String()
        Return campi.Select(Function(c)
                                If String.IsNullOrWhiteSpace(c) Then
                                    Return ""
                                Else
                                    ' Pulizia aggressiva per CSV
                                    Dim pulito = c.Trim()
                                    ' Rimuovi caratteri finali che possono essere residui del parsing CSV
                                    pulito = pulito.TrimEnd(";"c, ","c, "|"c, vbTab(0))
                                    Return pulito.Trim()
                                End If
                            End Function).ToArray()
    End Function



    Private Shared Function ContaRigheDatiValide(righe As String(), delimitatore As String) As Integer
        Return righe.Count(Function(r) ValidaRigaDati(SplitCsvRiga(r, delimitatore)))
    End Function

    Private Shared Function EstraiValoreCellaExcel(worksheet As OfficeOpenXml.ExcelWorksheet, row As Integer, col As Integer) As String
        Try
            If worksheet.Cells(row, col).Value Is Nothing Then Return ""

            Dim valoreCella = worksheet.Cells(row, col).Value

            If TypeOf valoreCella Is DateTime Then
                Return DirectCast(valoreCella, DateTime).ToString("dd/MM/yyyy")
            ElseIf TypeOf valoreCella Is Double AndAlso worksheet.Cells(row, col).Style.Numberformat.Format.Contains("d") Then
                Try
                    Dim dataExcel = DateTime.FromOADate(CDbl(valoreCella))
                    Return dataExcel.ToString("dd/MM/yyyy")
                Catch
                    Return valoreCella.ToString()
                End Try
            ElseIf TypeOf valoreCella Is Double OrElse TypeOf valoreCella Is Decimal Then
                Return Convert.ToDecimal(valoreCella).ToString("F2", CultureInfo.GetCultureInfo("it-IT"))
            Else
                Return valoreCella.ToString().Trim()
            End If
        Catch
            Return ""
        End Try
    End Function

    Private Shared Function EstraiValoreCellaDataTable(dataTable As Data.DataTable, row As Integer, col As Integer) As String
        Try
            If dataTable.Rows(row)(col) Is Nothing OrElse dataTable.Rows(row)(col) Is DBNull.Value Then
                Return ""
            End If

            Dim valoreCella = dataTable.Rows(row)(col)

            If TypeOf valoreCella Is DateTime Then
                Return DirectCast(valoreCella, DateTime).ToString("dd/MM/yyyy")
            ElseIf IsNumeric(valoreCella) AndAlso Not TypeOf valoreCella Is DateTime Then
                Try
                    Return Convert.ToDecimal(valoreCella).ToString("F2", CultureInfo.GetCultureInfo("it-IT"))
                Catch
                    Return valoreCella.ToString().Trim()
                End Try
            Else
                Return valoreCella.ToString().Trim()
            End If
        Catch
            Return ""
        End Try
    End Function

    ' RESTO DELLE FUNZIONI (invariate ma con piccole migliorie)

    Private Shared Function ContaDelimitatoriInRiga(riga As String, delimitatore As String) As Integer
        If String.IsNullOrWhiteSpace(riga) Then Return 0

        Dim dentroVirgolette = False
        Dim conteggio = 0
        Dim i = 0

        While i <= riga.Length - delimitatore.Length
            If i < riga.Length AndAlso riga(i) = """"c Then
                dentroVirgolette = Not dentroVirgolette
            ElseIf Not dentroVirgolette AndAlso riga.Substring(i, delimitatore.Length) = delimitatore Then
                conteggio += 1
                i += delimitatore.Length - 1
            End If
            i += 1
        End While

        Return conteggio
    End Function

    Private Shared Function CalcolaConsistenzaDelimitatore(righe As String(), delimitatore As String) As Integer
        If righe.Length = 0 Then Return 0

        Dim conteggi As New List(Of Integer)
        For Each riga In righe
            If Not String.IsNullOrWhiteSpace(riga) Then
                conteggi.Add(ContaDelimitatoriInRiga(riga, delimitatore))
            End If
        Next

        If conteggi.Count = 0 Then Return 0

        ' Calcola la consistenza basata sulla moda (valore più frequente)
        Dim gruppi = conteggi.GroupBy(Function(c) c).OrderByDescending(Function(g) g.Count())
        Dim modaConteggio = gruppi.First().Key
        Dim righeConsistenti = conteggi.Where(Function(c) c = modaConteggio).Count()

        Return CInt((righeConsistenti / conteggi.Count) * 100)
    End Function

    Private Shared Function SplitCsvRiga(riga As String, delimitatore As String) As String()
        If String.IsNullOrWhiteSpace(riga) Then Return New String() {}

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
            ElseIf Not dentroVirgolette AndAlso i + delimitatore.Length <= riga.Length AndAlso
               riga.Substring(i, delimitatore.Length) = delimitatore Then
                campi.Add(campoCorrente.ToString().Trim())
                campoCorrente.Clear()
                i += delimitatore.Length - 1
            Else
                campoCorrente.Append(carattere)
            End If

            i += 1
        End While

        ' Aggiungi l'ultimo campo (anche se vuoto)
        campi.Add(campoCorrente.ToString().Trim())

        Return campi.ToArray()
    End Function


    Private Shared Function RiconosciTipoColonna(nomeColonna As String) As TipoColonna
        If String.IsNullOrWhiteSpace(nomeColonna) Then Return TipoColonna.Sconosciuto

        Dim nome = nomeColonna.ToLower().Trim()

        ' Pattern per Data (più completi)
        Dim patternData = {
            "data", "date", "datetime", "timestamp", "valuta", "operazione", "giorno",
            "day", "fecha", "datum", "temps", "ora", "time", "periodo"
        }
        If patternData.Any(Function(p) nome.Contains(p)) Then Return TipoColonna.Data

        ' Pattern per Importo (più completi)
        Dim patternImporto = {
            "importo", "amount", "valore", "prezzo", "price", "euro", "eur", "€",
            "entrata", "uscita", "credito", "debito", "saldo", "balance", "costo",
            "cost", "totale", "total", "somma", "sum", "denaro", "money", "cifra"
        }
        If patternImporto.Any(Function(p) nome.Contains(p)) Then Return TipoColonna.Importo

        ' Pattern per Descrizione (più completi)
        Dim patternDescrizione = {
            "descrizione", "description", "dettagli", "details", "causale", "beneficiario",
            "oggetto", "note", "memo", "commento", "comment", "testo", "text", "nome",
            "name", "titolo", "title", "label", "etichetta", "categoria", "category"
        }
        If patternDescrizione.Any(Function(p) nome.Contains(p)) Then Return TipoColonna.Descrizione

        Return TipoColonna.Sconosciuto
    End Function

    Private Shared Function VerificaRiconoscimentoCompleto(colonne As List(Of ColonnaMappata)) As Boolean
        Dim haData = colonne.Any(Function(c) c.Tipo = TipoColonna.Data AndAlso c.Confidenza > 0.6)
        Dim haImporto = colonne.Any(Function(c) c.Tipo = TipoColonna.Importo AndAlso c.Confidenza > 0.6)
        Dim haDescrizione = colonne.Any(Function(c) c.Tipo = TipoColonna.Descrizione AndAlso c.Confidenza > 0.6)

        Return haData AndAlso haImporto AndAlso haDescrizione
    End Function

    Private Shared Function CountRigheDatiEffettive(worksheet As OfficeOpenXml.ExcelWorksheet,
                                               startRow As Integer, endRow As Integer,
                                               startCol As Integer, endCol As Integer) As Integer
        Dim conta = 0
        For row = startRow To endRow
            Dim rigaHaDati = False
            For col = startCol To endCol
                If worksheet.Cells(row, col).Value IsNot Nothing AndAlso
                   Not String.IsNullOrWhiteSpace(worksheet.Cells(row, col).Value.ToString()) Then
                    rigaHaDati = True
                    Exit For
                End If
            Next
            If rigaHaDati Then conta += 1
        Next
        Return conta
    End Function

    Private Shared Function CountRigheDatiEffettiveDataTable(dataTable As Data.DataTable, startRow As Integer) As Integer
        Dim conta = 0
        For row = startRow To dataTable.Rows.Count - 1
            Dim rigaHaDati = False
            For col = 0 To dataTable.Columns.Count - 1
                If dataTable.Rows(row)(col) IsNot Nothing AndAlso
                   dataTable.Rows(row)(col) IsNot DBNull.Value AndAlso
                   Not String.IsNullOrWhiteSpace(dataTable.Rows(row)(col).ToString()) Then
                    rigaHaDati = True
                    Exit For
                End If
            Next
            If rigaHaDati Then conta += 1
        Next
        Return conta
    End Function

    Private Shared Function EstraiRigheTabellareDaPdf(testo As String) As List(Of String())
        Dim righe As New List(Of String())
        Dim righeTesto = testo.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)

        For Each riga In righeTesto
            If RigaSembraTabellare(riga) Then
                Dim campi = Regex.Split(riga.Trim(), "\s{2,}|\t").
                           Where(Function(s) Not String.IsNullOrWhiteSpace(s)).
                           Select(Function(s) s.Trim()).ToArray()
                If campi.Length >= 2 Then
                    righe.Add(campi)
                End If
            End If
        Next

        Return righe
    End Function

    Private Shared Function RigaSembraTabellare(riga As String) As Boolean
        If String.IsNullOrWhiteSpace(riga) Then Return False

        Dim contieneMoltiSpazi = Regex.IsMatch(riga, "\s{2,}")
        Dim contieneNumeri = Regex.IsMatch(riga, "\d")
        Dim contieneVirgoleDecimali = Regex.IsMatch(riga, "\d+[.,]\d+")
        Dim contieneDate = Regex.IsMatch(riga, "\d{1,2}[/\-]\d{1,2}[/\-]\d{2,4}")

        Return contieneMoltiSpazi AndAlso (contieneNumeri OrElse contieneVirgoleDecimali OrElse contieneDate)
    End Function

End Class
