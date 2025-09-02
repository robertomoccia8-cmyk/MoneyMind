Imports System.Configuration
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Data.SQLite
Imports MoneyMind.Models

Public Class GptClassificatoreTransazioni
    Implements IDisposable

    Public Class SuggerimentoClassificazione
        Public Property NomeSocieta As String
        Public Property SommarioSocieta As String
        Public Property DescrizioneAttivita As String
        Public Property ParolaChiave As String
        Public Property MacroCategoria As String
        Public Property Categoria As String
        Public Property NuovaCategoria As String
        Public Property Necessita As String        ' ← NUOVO
        Public Property Frequenza As String        ' ← NUOVO
        Public Property Stagionalita As String     ' ← NUOVO
        Public Property Peso As Integer            ' ← NUOVO (sempre 75)
        Public Property Confidenza As Integer
        Public Property Motivazione As String
        Public Property IsValid As Boolean
        Public Property PatternSuggeriti As List(Of String)

        Public Sub New()
            IsValid = False
            Confidenza = 0
            PatternSuggeriti = New List(Of String)
        End Sub
    End Class

    Private lastPromptTokens As Integer
    Private lastCompletionTokens As Integer
    Private lastTotalTokens As Integer

    Private ReadOnly _httpClient As HttpClient
    Private ReadOnly _apiKey As String

    Public Sub New()
        _httpClient = New HttpClient()

        ' Carica la chiave dal database invece che dal config
        _apiKey = GestoreApiKeys.CaricaChiaveApi("OpenAI")
        Debug.WriteLine($"DEBUG: Chiave OpenAI caricata: {If(String.IsNullOrEmpty(_apiKey), "VUOTA", $"{_apiKey.Substring(0, Math.Min(10, _apiKey.Length))}...")}")

        If String.IsNullOrEmpty(_apiKey) Then
            Debug.WriteLine("DEBUG: ERRORE - Chiave OpenAI è vuota!")
            Return
        End If

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}")
    End Sub

    Public Class ApiValidator
        Public Shared Function ControllaSeMancaChiaveApi(tipoApi As String) As Boolean
            Dim chiave As String = GestoreApiKeys.CaricaChiaveApi(tipoApi)
            Return String.IsNullOrWhiteSpace(chiave)
        End Function

        Public Shared Function ChiediAperturaImpostazioni(nomeApi As String) As Boolean
            Dim risultato = MessageBox.Show(
            $"La chiave API per {nomeApi} non è configurata o non è valida." & vbCrLf & vbCrLf &
            "Questa funzione richiede una chiave API attiva per funzionare." & vbCrLf & vbCrLf &
            "Vuoi aprire le impostazioni per configurarla ora?",
            "Chiave API Mancante",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

            If risultato = DialogResult.Yes Then
                Using formImpostazioni As New FormImpostazioniGenerali()
                    formImpostazioni.ShowDialog()
                End Using
                Return True
            End If

            Return False
        End Function
    End Class

    ' Versione originale mantenuta per compatibilità
    Public Async Function TrovaPatternSimili(parolaChiave As String, patternEsistenti As List(Of String)) As Task(Of List(Of String))
        Try
            Dim risultatoAvanzato = Await TrovaPatternSimiliAvanzato(parolaChiave, patternEsistenti)
            Return risultatoAvanzato.PatternSuggeriti.Select(Function(p) p.Parola).ToList()
        Catch ex As Exception
            Return New List(Of String)
        End Try
    End Function

    ' Nuova versione avanzata
    Public Async Function TrovaPatternSimiliAvanzato(parolaChiave As String, patternEsistenti As List(Of String)) As Task(Of RisultatoPatternSimili)
        Try
            Dim prompt = CreaPromptTrovaPattern(parolaChiave, patternEsistenti)
            Dim risposta = Await ChiamaGptApi(prompt)
            Return ParsePatternSuggeritiaAvanzato(risposta)
        Catch ex As Exception
            Return New RisultatoPatternSimili With {
                .Analisi = "Errore nell'analisi AI: " & ex.Message
            }
        End Try
    End Function

    ' Versione super-avanzata con contesto completo della transazione
    Public Async Function TrovaPatternSimiliAvanzatoConContesto(parolaChiave As String, patternEsistenti As List(Of String),
                                                               descrizioneCompleta As String, importo As Decimal, 
                                                               dataTransazione As Date) As Task(Of RisultatoPatternSimili)
        Try
            Dim prompt = CreaPromptTrovaPatternConContesto(parolaChiave, patternEsistenti, 
                                                          descrizioneCompleta, importo, dataTransazione)
            Dim risposta = Await ChiamaGptApi(prompt)
            Return ParsePatternSuggeritiaAvanzato(risposta)
        Catch ex As Exception
            Return New RisultatoPatternSimili With {
                .Analisi = "Errore nell'analisi AI contestuale: " & ex.Message
            }
        End Try
    End Function

    Public Async Function CreaPatternPersonalizzato(descrizione As String, importo As Decimal, macroCategorie As List(Of String)) As Task(Of SuggerimentoClassificazione)
        Try
            Dim prompt = CreaPromptCreaPattern(descrizione, importo, macroCategorie)
            Dim risposta = Await ChiamaGptApi(prompt)
            Return ParseRisppostaCreaPattern(risposta)
        Catch ex As Exception
            Return New SuggerimentoClassificazione With {
                .IsValid = False,
                .Motivazione = $"Errore: {ex.Message}"
            }
        End Try
    End Function

    ' Nuova versione che accetta contesto transazione completo
    Public Function CreaPromptTrovaPatternConContesto(parolaChiave As String, patternEsistenti As List(Of String), 
                                                     Optional descrizioneCompleta As String = "", 
                                                     Optional importo As Decimal = 0, 
                                                     Optional dataTransazione As Date = Nothing) As String
        ' Carica informazioni aggiuntive per ogni pattern
        Dim patternDettagliati = GetPatternConDettagli(patternEsistenti.Take(30).ToList())
        
        ' Analisi contesto temporale
        Dim contestoTemporale = AnalizzaContestoTemporale(dataTransazione)
        
        ' Analisi fascia importo
        Dim contestoImporto = AnalizzaFasciaImporto(importo)
        
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("🧠 ANALISI SEMANTICA SUPER-AVANZATA - Pattern Simili Intelligenti")
        sb.AppendLine()
        sb.AppendLine("📊 CONTESTO TRANSAZIONE:")
        sb.AppendLine($"• Termine chiave: ""{parolaChiave}""")
        If Not String.IsNullOrEmpty(descrizioneCompleta) Then
            sb.AppendLine($"• Descrizione completa: ""{descrizioneCompleta}""")
        End If
        If importo <> 0 Then
            sb.AppendLine($"• Importo: {importo:C2} ({contestoImporto})")
        End If
        If dataTransazione <> Nothing Then
            sb.AppendLine($"• Periodo: {dataTransazione:MMMM yyyy} ({contestoTemporale})")
        End If
        sb.AppendLine()
        
        sb.AppendLine("📋 PATTERN ESISTENTI ORDINATI PER SUCCESSO:")
        For Each p In patternDettagliati.Take(20)
            sb.AppendLine($"• ""{p.Parola}"" → {p.MacroCategoria}/{p.Categoria}")
            sb.AppendLine($"  Statistiche: Peso {p.Peso}/100, {p.UsoCount} transazioni, Frequenza {p.Frequenza}")
        Next
        sb.AppendLine()
        
        sb.AppendLine("🎯 COMPITI AVANZATI:")
        sb.AppendLine("1. ANALISI SEMANTICA: Identifica settore, tipo business, categoria merceologica")
        sb.AppendLine("2. MATCHING INTELLIGENTE: Non solo similarità ortografica, ma significato e contesto")
        sb.AppendLine("3. PESO STORICO: Favorisci pattern con alto successo (peso × utilizzo)")
        sb.AppendLine("4. CONTESTO IMPORTO: Considera la fascia di spesa per suggerimenti più precisi")
        sb.AppendLine("5. STAGIONALITÀ: Se applicabile, considera periodo dell'anno")
        sb.AppendLine()
        
        sb.AppendLine("📤 FORMATO RISPOSTA JSON:")
        sb.AppendLine("{")
        sb.AppendLine("  ""Analisi"": ""[Analisi dettagliata: settore identificato, contesto, ragionamento]"",")
        sb.AppendLine("  ""PatternSuggeriti"": [")
        sb.AppendLine("    {""Parola"": ""pattern1"", ""Confidenza"": 95, ""Motivazione"": ""Perché è il migliore""},")
        sb.AppendLine("    {""Parola"": ""pattern2"", ""Confidenza"": 88, ""Motivazione"": ""Alternativa valida""},")
        sb.AppendLine("    {""Parola"": ""pattern3"", ""Confidenza"": 75, ""Motivazione"": ""Opzione di backup""}")
        sb.AppendLine("  ]")
        sb.AppendLine("}")
        
        Return sb.ToString()
    End Function

    ' Versione semplificata per compatibilità
    Private Function CreaPromptTrovaPattern(parolaChiave As String, patternEsistenti As List(Of String)) As String
        Return CreaPromptTrovaPatternConContesto(parolaChiave, patternEsistenti)
    End Function

    ' Classe per pattern dettagliati
    Public Class PatternDettagliato
        Public Property Parola As String
        Public Property MacroCategoria As String
        Public Property Categoria As String
        Public Property Peso As Integer
        Public Property Frequenza As String
        Public Property UsoCount As Integer
    End Class

    ' Classe per suggerimenti avanzati con AI
    Public Class SuggerimentoPatternAvanzato
        Public Property Parola As String
        Public Property Confidenza As Integer
        Public Property Motivazione As String
        Public Property MacroCategoria As String
        Public Property Categoria As String
        Public Property TransazioniPotenziali As Integer
    End Class

    ' Risultato completo della ricerca pattern simili
    Public Class RisultatoPatternSimili
        Public Property Analisi As String
        Public Property PatternSuggeriti As List(Of SuggerimentoPatternAvanzato)
        
        Public Sub New()
            PatternSuggeriti = New List(Of SuggerimentoPatternAvanzato)
        End Sub
    End Class

    ' Ottiene pattern con tutti i dettagli
    Private Function GetPatternConDettagli(parolePattern As List(Of String)) As List(Of PatternDettagliato)
        Dim result As New List(Of PatternDettagliato)
        
        Try
            Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                
                For Each parola In parolePattern
                    Dim sql As String = "
                    SELECT Parola, MacroCategoria, Categoria, 
                           COALESCE(Peso, 50) as Peso, 
                           COALESCE(Frequenza, 'Occasionale') as Frequenza,
                           (SELECT COUNT(*) FROM Transazioni WHERE UPPER(Descrizione) LIKE '%' || UPPER(@parola) || '%') as UsoCount
                    FROM Pattern 
                    WHERE Parola = @parola 
                    LIMIT 1"
                    
                    Using cmd As New SQLite.SQLiteCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@parola", parola)
                        Using reader As SQLite.SQLiteDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                result.Add(New PatternDettagliato With {
                                    .Parola = reader.GetString(0),
                                    .MacroCategoria = reader.GetString(1),
                                    .Categoria = reader.GetString(2),
                                    .Peso = reader.GetInt32(3),
                                    .Frequenza = reader.GetString(4),
                                    .UsoCount = reader.GetInt32(5)
                                })
                            End If
                        End Using
                    End Using
                Next
            End Using
        Catch ex As Exception
            Debug.WriteLine($"Errore GetPatternConDettagli: {ex.Message}")
        End Try
        
        Return result.OrderByDescending(Function(p) p.Peso * p.UsoCount).ToList()
    End Function

    ' Analizza il contesto temporale per suggerimenti stagionali
    Private Function AnalizzaContestoTemporale(dataTransazione As Date) As String
        If dataTransazione = Nothing Then Return "N/A"
        
        Dim mese = dataTransazione.Month
        Select Case mese
            Case 12, 1, 2
                Return "Periodo invernale - possibili spese riscaldamento, regali, vacanze"
            Case 3, 4, 5
                Return "Periodo primaverile - possibili spese giardinaggio, pulizie, abbigliamento"
            Case 6, 7, 8
                Return "Periodo estivo - possibili spese vacanze, condizionamento, mare"
            Case 9, 10, 11
                Return "Periodo autunnale - possibili spese rientro, scuola, riscaldamento"
            Case Else
                Return "Periodo generico"
        End Select
    End Function

    ' Analizza la fascia di importo per contestualizzare i suggerimenti
    Private Function AnalizzaFasciaImporto(importo As Decimal) As String
        Dim abs = Math.Abs(importo)
        Select Case abs
            Case 0 To 10
                Return "Spesa molto piccola - snack, caffè, piccoli acquisti"
            Case 10 To 50
                Return "Spesa piccola - pranzi, trasporti, piccole necessità"
            Case 50 To 150
                Return "Spesa media - ristoranti, abbigliamento, spesa settimanale"
            Case 150 To 500
                Return "Spesa significativa - elettronica, riparazioni, grandi acquisti"
            Case 500 To 2000
                Return "Spesa importante - elettrodomestici, vacanze, rate"
            Case Is > 2000
                Return "Spesa molto importante - auto, immobili, investimenti"
            Case Else
                Return "Fascia non determinata"
        End Select
    End Function

    Private Function CreaPromptCreaPattern(descrizione As String, importo As Decimal, macroCategorie As List(Of String)) As String
        Dim macroStr = String.Join(", ", macroCategorie)
        Return $"Crea un pattern ottimale per questa transazione:

DESCRIZIONE: ""{descrizione}""
IMPORTO: {importo:C2}
MACROCATEGORIE: {macroStr}

Rispondi ESATTAMENTE in JSON:
{{
    ""ParolaChiave"": ""[chiave]"",
    ""MacroCategoria"": ""[categoria macro]"",
    ""Categoria"": ""[categoria]"",
    ""Confidenza"": [0-100],
    ""Motivazione"": ""[perché]""
}}"
    End Function

    ' Nuovo metodo pubblico per analisi dinamica
    Public Async Function AnalizzaConPrompt(prompt As String) As Task(Of String)
        Return Await ChiamaGptApi(prompt)
    End Function

    Public Async Function ChiamaGptApi(prompt As String) As Task(Of String)
        Dim requestBody = New Dictionary(Of String, Object) From {
        {"model", "gpt-4o-mini"},
        {"messages", New Object() {New Dictionary(Of String, String) From {
            {"role", "user"},
            {"content", prompt}
        }}},
        {"temperature", 0.3},
        {"max_tokens", 500}
    }
        Dim jsonRequest = JsonSerializer.Serialize(requestBody)
        Using content = New StringContent(jsonRequest, Encoding.UTF8, "application/json")
            Dim response = Await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content)
            response.EnsureSuccessStatusCode()
            Dim jsonResponse = Await response.Content.ReadAsStringAsync()
            Dim root = JsonDocument.Parse(jsonResponse).RootElement
            ' Estrai usage
            If root.TryGetProperty("usage", Nothing) Then
                Dim usage = root.GetProperty("usage")
                lastPromptTokens = usage.GetProperty("prompt_tokens").GetInt32()
                lastCompletionTokens = usage.GetProperty("completion_tokens").GetInt32()
                lastTotalTokens = usage.GetProperty("total_tokens").GetInt32()
            End If
            ' Ritorna solo il content per parsing
            Return jsonResponse
        End Using
    End Function

    Public Function GetResocontoToken() As String
        ' Prezzi GPT-4o-mini
        Const costInputPerThousand As Double = 0.00015  ' $ per 1000 input token
        Const costOutputPerThousand As Double = 0.0003  ' $ per 1000 output token

        Dim costInput = lastPromptTokens / 1000.0 * costInputPerThousand
        Dim costOutput = lastCompletionTokens / 1000.0 * costOutputPerThousand
        Dim costTotal = costInput + costOutput

        Return $"Token input: {lastPromptTokens} ({costInput:C6})" & vbCrLf &
           $"Token output: {lastCompletionTokens} ({costOutput:C6})" & vbCrLf &
           $"Token totali: {lastTotalTokens} ({costTotal:C6})"
    End Function

    ' Analisi principale con fallback web
    Public Async Function AnalizzaTransazione(descrizione As String, importo As Decimal) As Task(Of SuggerimentoClassificazione)
        ' 1) Chiamata AI
        Dim suggerimento = Await AnalizzaConIA(descrizione, importo)
        Debug.WriteLine($"DEBUG: Dopo AnalizzaConIA - IsValid: {suggerimento?.IsValid}, NomeSocieta: '{suggerimento?.NomeSocieta}', ParolaChiave: '{suggerimento?.ParolaChiave}'")

        ' 2) Se AI fallisce, fallback Google
        If Not EsitoValidoCompleto(suggerimento) Then
            Debug.WriteLine("DEBUG: EsitoValidoCompleto è False, provando fallback Google")
            If ApiValidator.ControllaSeMancaChiaveApi("GooglePlaces") Then
                ApiValidator.ChiediAperturaImpostazioni("Google Places")
            End If
            Dim daWeb = Await RicercaWebPerDescrizioneAsync(descrizione)
            If daWeb IsNot Nothing AndAlso daWeb.IsValid Then
                suggerimento = daWeb
                Debug.WriteLine("DEBUG: Fallback Google riuscito")
            Else
                Debug.WriteLine("DEBUG: Fallback Google fallito")
            End If
        End If

        ' 3) Fallback parola chiave: sempre estrai da descrizione se vuota o se è un termine di categoria
        If String.IsNullOrWhiteSpace(suggerimento.ParolaChiave) _
      OrElse {"ALTRO", "RISTORAZIONE", "USCITE", "ENTRATE"}.Contains(suggerimento.ParolaChiave.ToUpperInvariant()) Then

            Dim estratta = EstraiParolaChiaveDaDescrizione(descrizione)
            Debug.WriteLine($"EstraiParolaChiaveDaDescrizione('{descrizione}') → '{estratta}'")
            suggerimento.ParolaChiave = estratta
        End If

        ' 4) Se ancora non valido, fallback completo
        If Not EsitoValidoCompleto(suggerimento) Then
            Return CostruisciFallbackMinimo(descrizione, suggerimento)
        End If

        ' 5) Restituisci
        Return suggerimento
    End Function

    ' Controllo di validità "completo" per esigere almeno nome o keyword
    Private Function EsitoValidoCompleto(s As SuggerimentoClassificazione) As Boolean
        If s Is Nothing Then
            Debug.WriteLine("DEBUG: EsitoValidoCompleto - suggerimento è Nothing")
            Return False
        End If
        If Not s.IsValid Then
            Debug.WriteLine("DEBUG: EsitoValidoCompleto - IsValid è False")
            Return False
        End If
        If String.IsNullOrWhiteSpace(s.ParolaChiave) Then
            Debug.WriteLine("DEBUG: EsitoValidoCompleto - ParolaChiave è vuota")
            Return False
        End If
        ' Rimosso il controllo rigido su NomeSocieta/SommarioSocieta - basta avere ParolaChiave
        Debug.WriteLine("DEBUG: EsitoValidoCompleto - VALIDO")
        Return True
    End Function

    '========================
    ' Generazione del Prompt
    '========================
    Public Function CreaPromptAnalisiDinamico(descr As String, importo As Decimal) As String
        ' Serializza le liste esistenti
        Dim macrosJson = JsonSerializer.Serialize(GetMacroCategorieEsistenti())
        Dim catsJson = JsonSerializer.Serialize(GetCategorieEsistenti())
        Dim necessitaJson = JsonSerializer.Serialize(GetNecessitaEsistenti())
        Dim frequenzaJson = JsonSerializer.Serialize(GetFrequenzaEsistenti())
        Dim stagJson = JsonSerializer.Serialize(GetStagionalitaEsistenti())

        ' Esempi storici
        Dim storici = GetPatternStorici(descr)
        Dim esempi As String = String.Join(vbCrLf,
        storici.Select(Function(x) $"- ""{x.Descrizione}"" → Parola:{x.Parola}, Macro:{x.MacroCategoria}, Categoria:{x.Categoria}"))

        Dim sb As New Text.StringBuilder()
        sb.AppendLine("Analizza SOLO in JSON:")
        sb.AppendLine($"TRANSAZIONE: ""{descr}""")
        sb.AppendLine($"IMPORTO: {importo:C2}")
        sb.AppendLine()
        sb.AppendLine($"MacroCategorieEsistenti: {macrosJson}")
        sb.AppendLine($"CategorieEsistenti: {catsJson}")
        sb.AppendLine($"NecessitaEsistenti: {necessitaJson}")
        sb.AppendLine($"FrequenzaEsistenti: {frequenzaJson}")
        sb.AppendLine($"StagionalitaEsistenti: {stagJson}")
        sb.AppendLine()
        sb.AppendLine("Esempi storici (descrizione → ParolaChiave estrapolata → pattern estratto):")
        sb.AppendLine(esempi)
        sb.AppendLine()
        sb.AppendLine("REGOLE OBBLIGATORIE:")
        sb.AppendLine("1. NOME SOCIETÀ: Se presente un nome di azienda nella descrizione, impostalo ESATTAMENTE come NomeSocieta E come ParolaChiave.")
        sb.AppendLine("2. SOMMARIO SOCIETÀ: Sempre fornire una breve descrizione dell'attività dell'azienda.")
        sb.AppendLine("3. DESCRIZIONE ATTIVITÀ: Mai lasciare vuoto, descrivere sempre l'attività.")
        sb.AppendLine("4. PAROLA CHIAVE: Se non c'è società, deve essere una parola SPECIFICA dalla descrizione (non generica).")
        sb.AppendLine("5. MACRO/CATEGORIE: USA SOLO quelle negli elenchi sopra. Se non trovi corrispondenza esatta, scegli la più simile.")
        sb.AppendLine("6. VINCOLI ASSOLUTI: MacroCategoria e Categoria DEVONO essere dalla lista fornita, mai inventare.")
        sb.AppendLine("7. ALTRI CAMPI: Necessita/Frequenza/Stagionalita solo da elenchi forniti.")
        sb.AppendLine("8. Peso sempre 75, Confidenza 0-100.")
        sb.AppendLine()
        sb.AppendLine("Formato JSON di risposta:")
        sb.AppendLine("{")
        sb.AppendLine("  ""NomeSocieta"": """",")
        sb.AppendLine("  ""SommarioSocieta"": """",")
        sb.AppendLine("  ""DescrizioneAttivita"": """",")
        sb.AppendLine("  ""ParolaChiave"": """",")
        sb.AppendLine("  ""MacroCategoria"": """",")
        sb.AppendLine("  ""Categoria"": """",")
        sb.AppendLine("  ""NuovaCategoria"": """",")
        sb.AppendLine("  ""Necessita"": """",")
        sb.AppendLine("  ""Frequenza"": """",")
        sb.AppendLine("  ""Stagionalita"": """",")
        sb.AppendLine("  ""Peso"": 75,")
        sb.AppendLine("  ""Confidenza"": 0,")
        sb.AppendLine("  ""Motivazione"": """"")
        sb.Append("}")
        Return sb.ToString()
    End Function

    ' MacroCategorie esistenti (DISTINCT) per guidare la creazione AI
    Public Function GetMacroCategorieEsistenti() As List(Of String)
        Dim list As New List(Of String)
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd = New SQLite.SQLiteCommand(
            "SELECT DISTINCT MacroCategoria FROM Pattern WHERE MacroCategoria<>'' ORDER BY MacroCategoria", conn)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        list.Add(r.GetString(0))
                    End While
                End Using
            End Using
        End Using
        Return list
    End Function

    ' Restituisce tutte le Categorie esistenti
    Private Function GetCategorieEsistenti() As List(Of String)
        Dim list As New List(Of String)
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd = New SQLite.SQLiteCommand(
            "SELECT DISTINCT Categoria FROM Pattern WHERE Categoria<>'' ORDER BY Categoria", conn)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        list.Add(r.GetString(0))
                    End While
                End Using
            End Using
        End Using
        Return list
    End Function

    ' Restituisce gli ultimi N pattern associati a descrizioni simili
    Private Function GetPatternStorici(descr As String, Optional n As Integer = 5) _
    As List(Of (Descrizione As String, Parola As String, MacroCategoria As String, Categoria As String))

        Dim result As New List(Of (String, String, String, String))
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()

            Dim sql As String =
            "SELECT Parola AS Descrizione, Parola, MacroCategoria, Categoria
             FROM Pattern
             WHERE Parola LIKE @p
             ORDER BY ROWID DESC
             LIMIT @n;"

            Using cmd As New SQLite.SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@p", "%" & descr & "%")
                cmd.Parameters.AddWithValue("@n", n)

                Using reader As SQLite.SQLiteDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        result.Add((
                        reader.GetString(0),  ' Descrizione mostrata (Parola)
                        reader.GetString(1),  ' Parola
                        reader.GetString(2),  ' MacroCategoria
                        reader.GetString(3))) ' Categoria
                    End While
                End Using
            End Using
        End Using

        Return result
    End Function

    '==============================
    ' Parsing e Validazione del JSON
    '==============================
    Private Function ParseRispostaAnalisiDinamico(json As String, descr As String) As SuggerimentoClassificazione
        Dim s As New SuggerimentoClassificazione()
        Dim doc = JsonDocument.Parse(json).RootElement

        s.NomeSocieta = If(doc.TryGetProperty("NomeSocieta", Nothing), doc.GetProperty("NomeSocieta").GetString(), "")
        s.SommarioSocieta = If(doc.TryGetProperty("SommarioSocieta", Nothing), doc.GetProperty("SommarioSocieta").GetString(), "")
        s.DescrizioneAttivita = If(doc.TryGetProperty("DescrizioneAttivita", Nothing), doc.GetProperty("DescrizioneAttivita").GetString(), "")
        s.ParolaChiave = If(doc.TryGetProperty("ParolaChiave", Nothing), doc.GetProperty("ParolaChiave").GetString(), "")
        s.MacroCategoria = If(doc.TryGetProperty("MacroCategoria", Nothing), doc.GetProperty("MacroCategoria").GetString(), "")
        s.Categoria = If(doc.TryGetProperty("Categoria", Nothing), doc.GetProperty("Categoria").GetString(), "")
        s.NuovaCategoria = If(doc.TryGetProperty("NuovaCategoria", Nothing), doc.GetProperty("NuovaCategoria").GetString(), "")
        s.Necessita = If(doc.TryGetProperty("Necessita", Nothing), doc.GetProperty("Necessita").GetString(), "")
        s.Frequenza = If(doc.TryGetProperty("Frequenza", Nothing), doc.GetProperty("Frequenza").GetString(), "")
        s.Stagionalita = If(doc.TryGetProperty("Stagionalita", Nothing), doc.GetProperty("Stagionalita").GetString(), "")

        If doc.TryGetProperty("Peso", Nothing) AndAlso doc.GetProperty("Peso").ValueKind = JsonValueKind.Number Then
            s.Peso = doc.GetProperty("Peso").GetInt32()
        Else
            s.Peso = 0
        End If

        If doc.TryGetProperty("Confidenza", Nothing) AndAlso doc.GetProperty("Confidenza").ValueKind = JsonValueKind.Number Then
            s.Confidenza = doc.GetProperty("Confidenza").GetInt32()
        Else
            s.Confidenza = 0
        End If

        s.Motivazione = If(doc.TryGetProperty("Motivazione", Nothing), doc.GetProperty("Motivazione").GetString(), "")

        ' Validazione secondo le regole
        s.IsValid = True

        ' 1) Se NomeSocieta compare nella descr, ParolaChiave DEVE = NomeSocieta
        If Not String.IsNullOrWhiteSpace(s.NomeSocieta) Then
            If s.ParolaChiave <> s.NomeSocieta Then s.IsValid = False
        Else
            ' altrimenti ParolaChiave deve essere parola presente
            If String.IsNullOrWhiteSpace(s.ParolaChiave) OrElse Not descr.Split(" "c).Contains(s.ParolaChiave) Then s.IsValid = False
        End If

        If String.IsNullOrWhiteSpace(s.MacroCategoria) Then s.IsValid = False
        If String.IsNullOrWhiteSpace(s.Categoria) Then s.IsValid = False
        If String.IsNullOrWhiteSpace(s.Necessita) OrElse String.IsNullOrWhiteSpace(s.Frequenza) OrElse String.IsNullOrWhiteSpace(s.Stagionalita) Then s.IsValid = False
        If s.Peso <> 75 Then s.IsValid = False

        Return s
    End Function

    Public Function GetNecessitaEsistenti() As List(Of String)
        Dim list As New List(Of String)
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd = New SQLite.SQLiteCommand(
            "SELECT DISTINCT Necessita FROM Pattern WHERE Necessita<>'' ORDER BY Necessita", conn)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        list.Add(r.GetString(0))
                    End While
                End Using
            End Using
        End Using
        Return list
    End Function

    Public Function GetFrequenzaEsistenti() As List(Of String)
        Dim list As New List(Of String)
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd = New SQLite.SQLiteCommand(
            "SELECT DISTINCT Frequenza FROM Pattern WHERE Frequenza<>'' ORDER BY Frequenza", conn)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        list.Add(r.GetString(0))
                    End While
                End Using
            End Using
        End Using
        Return list
    End Function


    Public Function GetStagionalitaEsistenti() As List(Of String)
        Dim list As New List(Of String)
        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd = New SQLite.SQLiteCommand(
            "SELECT DISTINCT Stagionalita FROM Pattern WHERE Stagionalita<>'' ORDER BY Stagionalita", conn)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        list.Add(r.GetString(0))
                    End While
                End Using
            End Using
        End Using
        Return list
    End Function

    ' Prompt snello + chiamata modello economico
    Private Async Function AnalizzaConIA(descrizione As String, importo As Decimal) As Task(Of SuggerimentoClassificazione)
        Try
            ' Usa il prompt dinamico
            Dim prompt = CreaPromptAnalisiDinamico(descrizione, importo)
            Dim requestBody = New Dictionary(Of String, Object) From {
            {"model", "gpt-4o-mini"},
            {"messages", New Object() {New Dictionary(Of String, String) From {
                {"role", "user"}, {"content", prompt}
            }}},
            {"temperature", 0.3},
            {"max_tokens", 500}
        }
            Dim jsonRequest = JsonSerializer.Serialize(requestBody)
            Using content = New StringContent(jsonRequest, Encoding.UTF8, "application/json")
                Dim response = Await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content)
                response.EnsureSuccessStatusCode()
                Dim jsonResponse = Await response.Content.ReadAsStringAsync()
                Dim root = JsonDocument.Parse(jsonResponse).RootElement

                ' Estrai usage token
                If root.TryGetProperty("usage", Nothing) Then
                    Dim usage = root.GetProperty("usage")
                    lastPromptTokens = usage.GetProperty("prompt_tokens").GetInt32()
                    lastCompletionTokens = usage.GetProperty("completion_tokens").GetInt32()
                    lastTotalTokens = usage.GetProperty("total_tokens").GetInt32()
                    Debug.WriteLine($"DEBUG: Token tracciati - Input:{lastPromptTokens}, Output:{lastCompletionTokens}, Totale:{lastTotalTokens}")
                Else
                    Debug.WriteLine("DEBUG: ERRORE - Nessun campo usage nella risposta API")
                End If

                ' Estrai e pulisci content
                Dim contentStr = root.GetProperty("choices")(0).GetProperty("message").GetProperty("content").GetString()
                Debug.WriteLine($"DEBUG: Risposta AI raw: {contentStr}")
                Dim jsonPulito = PulisciJsonResponse(contentStr)
                Debug.WriteLine($"DEBUG: JSON pulito: {jsonPulito}")
                Dim s = ParseSuggerimentoFromJson(jsonPulito)

                ' Validazione soft
                s.IsValid = EsitoValido(descrizione, s)
                Debug.WriteLine($"DEBUG: Dopo EsitoValido - IsValid: {s.IsValid}")
                Return s
            End Using
        Catch ex As Exception
            ' Log dell'errore per debug
            Debug.WriteLine($"Errore in AnalizzaConIA: {ex.Message}")
            Return New SuggerimentoClassificazione With {
                .IsValid = False,
                .Motivazione = $"Errore analisi AI: {ex.Message}"
            }
        End Try
    End Function

    ' Utility di parsing JSON risposta IA
    Private Function ParseSuggerimentoFromJson(json As String) As SuggerimentoClassificazione
        Dim s As New SuggerimentoClassificazione()
        Dim root = JsonDocument.Parse(json).RootElement
        s.NomeSocieta = GetJsonProperty(root, "NomeSocieta")
        s.SommarioSocieta = GetJsonProperty(root, "SommarioSocieta")
        s.DescrizioneAttivita = GetJsonProperty(root, "DescrizioneAttivita")
        s.ParolaChiave = GetJsonProperty(root, "ParolaChiave")
        s.MacroCategoria = GetJsonProperty(root, "MacroCategoria")
        s.Categoria = GetJsonProperty(root, "Categoria")
        s.NuovaCategoria = GetJsonProperty(root, "NuovaCategoria")
        s.Necessita = GetJsonProperty(root, "Necessita")
        s.Frequenza = GetJsonProperty(root, "Frequenza")
        s.Stagionalita = GetJsonProperty(root, "Stagionalita")
        s.Peso = If(GetJsonIntProperty(root, "Peso") = 0, 75, GetJsonIntProperty(root, "Peso"))
        s.Confidenza = GetJsonIntProperty(root, "Confidenza")
        s.Motivazione = GetJsonProperty(root, "Motivazione")
        Return s
    End Function

    ' Validazione elastica e coerente con le regole
    Private Function EsitoValido(descr As String, s As SuggerimentoClassificazione) As Boolean
        If s Is Nothing Then Return False

        ' Validazione ParolaChiave più flessibile
        Dim keyU = If(s.ParolaChiave, "").ToUpperInvariant().Trim()
        If String.IsNullOrWhiteSpace(keyU) Then Return False

        ' Controlla se la parola chiave è contenuta nella descrizione (case-insensitive)
        Dim descrU = If(descr, "").ToUpperInvariant()
        Dim found = False

        ' Prova match diretto
        If descrU.Contains(keyU) Then found = True

        ' Prova senza articoli iniziali (L', IL, LA, etc.)
        If Not found AndAlso keyU.Length > 2 Then
            Dim keyWithoutArticle = keyU.Replace("L'", "").Replace("IL ", "").Replace("LA ", "").Trim()
            If descrU.Contains(keyWithoutArticle) Then found = True
        End If

        If Not found Then Return False

        ' Imposta defaults se mancanti, ma prova prima a mappare sui valori esistenti
        If String.IsNullOrWhiteSpace(s.MacroCategoria) Then 
            s.MacroCategoria = TrovaCategoriaSimilare(GetMacroCategorieEsistenti(), "Altro")
        End If
        If String.IsNullOrWhiteSpace(s.Categoria) Then 
            s.Categoria = TrovaCategoriaSimilare(GetCategorieEsistenti(), "Altro")
        End If
        If String.IsNullOrWhiteSpace(s.Necessita) Then 
            s.Necessita = TrovaCategoriaSimilare(GetNecessitaEsistenti(), "Base")
        End If
        If String.IsNullOrWhiteSpace(s.Frequenza) Then 
            s.Frequenza = TrovaCategoriaSimilare(GetFrequenzaEsistenti(), "Occasionale")
        End If
        If String.IsNullOrWhiteSpace(s.Stagionalita) Then 
            s.Stagionalita = TrovaCategoriaSimilare(GetStagionalitaEsistenti(), "Annuale")
        End If
        s.Peso = 75

        ' Validazione finale: assicurati che le categorie siano valide
        ValidaECorreggiCategorie(s)

        Return True
    End Function

    ' Valida e corregge le categorie per assicurarsi che siano dalle liste esistenti
    Private Sub ValidaECorreggiCategorie(s As SuggerimentoClassificazione)
        Dim macroEsistenti = GetMacroCategorieEsistenti()
        Dim catEsistenti = GetCategorieEsistenti()
        Dim necEsistenti = GetNecessitaEsistenti()
        Dim freqEsistenti = GetFrequenzaEsistenti()
        Dim stagEsistenti = GetStagionalitaEsistenti()

        ' Verifica MacroCategoria
        If Not macroEsistenti.Any(Function(x) String.Equals(x, s.MacroCategoria, StringComparison.OrdinalIgnoreCase)) Then
            s.MacroCategoria = TrovaCategoriaSimilare(macroEsistenti, "Altro")
        End If

        ' Verifica Categoria
        If Not catEsistenti.Any(Function(x) String.Equals(x, s.Categoria, StringComparison.OrdinalIgnoreCase)) Then
            s.Categoria = TrovaCategoriaSimilare(catEsistenti, "Altro")
        End If

        ' Verifica Necessita
        If Not necEsistenti.Any(Function(x) String.Equals(x, s.Necessita, StringComparison.OrdinalIgnoreCase)) Then
            s.Necessita = TrovaCategoriaSimilare(necEsistenti, "Base")
        End If

        ' Verifica Frequenza
        If Not freqEsistenti.Any(Function(x) String.Equals(x, s.Frequenza, StringComparison.OrdinalIgnoreCase)) Then
            s.Frequenza = TrovaCategoriaSimilare(freqEsistenti, "Occasionale")
        End If

        ' Verifica Stagionalita
        If Not stagEsistenti.Any(Function(x) String.Equals(x, s.Stagionalita, StringComparison.OrdinalIgnoreCase)) Then
            s.Stagionalita = TrovaCategoriaSimilare(stagEsistenti, "Annuale")
        End If
    End Sub

    ' Funzione helper per trovare categoria simile o fallback
    Private Function TrovaCategoriaSimilare(listaEsistenti As List(Of String), fallback As String) As String
        If listaEsistenti IsNot Nothing AndAlso listaEsistenti.Count > 0 Then
            ' Cerca "Altro" se esiste
            Dim altro = listaEsistenti.FirstOrDefault(Function(x) x.ToLowerInvariant().Contains("altro"))
            If altro IsNot Nothing Then Return altro
            
            ' Cerca fallback se esiste  
            Dim fb = listaEsistenti.FirstOrDefault(Function(x) x.ToLowerInvariant().Contains(fallback.ToLowerInvariant()))
            If fb IsNot Nothing Then Return fb
            
            ' Altrimenti prendi il primo disponibile
            Return listaEsistenti.First()
        End If
        Return fallback
    End Function

    ' Fallback minimo quando IA e web non aiutano
    Public Function CostruisciFallbackMinimo(descr As String, s As SuggerimentoClassificazione) As SuggerimentoClassificazione
        If s Is Nothing Then s = New SuggerimentoClassificazione()
        s.NomeSocieta = If(String.IsNullOrWhiteSpace(s.NomeSocieta), "", s.NomeSocieta)
        s.ParolaChiave = If(String.IsNullOrWhiteSpace(s.ParolaChiave), EstraiParolaChiaveDaDescrizione(descr), s.ParolaChiave)
        s.MacroCategoria = If(String.IsNullOrWhiteSpace(s.MacroCategoria), "Altro", s.MacroCategoria)
        s.Categoria = If(String.IsNullOrWhiteSpace(s.Categoria), "Altro", s.Categoria)
        s.Necessita = If(String.IsNullOrWhiteSpace(s.Necessita), "Non essenziale", s.Necessita)
        s.Frequenza = If(String.IsNullOrWhiteSpace(s.Frequenza), "Occasionale", s.Frequenza)
        s.Stagionalita = If(String.IsNullOrWhiteSpace(s.Stagionalita), "Annuale", s.Stagionalita)
        s.Peso = 75
        s.Confidenza = Math.Max(s.Confidenza, 60)
        s.IsValid = True
        s.Motivazione = "Fallback minimo applicato."
        Return s
    End Function

    ' Usa questo per ricavare la keyword dal nome società (fallback web/places)
    Private Function EstrarreParolaChiaveDaNome(nomeSocieta As String) As String
        ' Funzione semplice che usa il nome completo o parte di esso come ParolaChiave
        Return If(String.IsNullOrWhiteSpace(nomeSocieta), "", nomeSocieta.ToUpperInvariant())
    End Function

    ' Usa questo per ricavare la keyword quando non hai un nome società certo
    Private Function EstraiParolaChiaveDaDescrizione(descr As String) As String
        If String.IsNullOrWhiteSpace(descr) Then
            Return ""
        End If

        ' 1) Regex raffinato: preferisce nomi con articolo oppure almeno 3 parole
        '    Cattura gruppi di almeno due parole (maiuscole) con un articolo iniziale facoltativo,
        '    ignorando finale ITA, EUR, etc.
        Dim pattern As String = "\b(L'?[A-ZÀ-ÖØ-Þ]{2,}(?:\s+(?:DEL|DELLA|DEI|DELE|DI)?\s*[A-ZÀ-ÖØ-Þ]{2,})(?:\s+[A-ZÀ-ÖØ-Þ]{2,})*)\b"
        Dim m As Match = Regex.Match(descr.Trim(), pattern)
        If m.Success Then
            Return m.Groups(1).Value.Trim()
        End If

        ' 2) Fallback token-by-token: escludi sigle di paese, città, date e codici
        Dim stopWords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "POS", "EUROZONA", "PAGAMENTO", "OPERAZIONE", "DEL", "CARTA",
        "ITA", "EUR", "SA", "PAESTUM", "AGROPOLI", "MARINO", "NOLA"
    }
        
        ' Pattern per riconoscere date (DD.MM.YY)
        Dim datePattern As String = "\d{1,2}\.\d{1,2}\.\d{2,4}"

        Dim tokens = descr.ToUpperInvariant() _
        .Split({" "c}, StringSplitOptions.RemoveEmptyEntries) _
        .Select(Function(t) t.Trim("'"c, """"c, "."c, ","c, "*"c)) _
        .Where(Function(t) t.Length > 2) _
        .Where(Function(t) Not stopWords.Contains(t)) _
        .Where(Function(t) Not Regex.IsMatch(t, datePattern)) _
        .Where(Function(t) Not Regex.IsMatch(t, "\*\d+")) _
        .Where(Function(t) Not Regex.IsMatch(t, "\d{1,2}:\d{2}")) _
        .ToList()

        If Not tokens.Any() Then
            tokens = descr.ToUpperInvariant() _
            .Split({" "c}, StringSplitOptions.RemoveEmptyEntries) _
            .Select(Function(t) t.Trim("'"c, """"c, "."c, ","c)) _
            .Where(Function(t) t.Length > 2) _
            .ToList()
        End If

        ' Ordina per lunghezza e posizione, poi prendi i primi 2 token
        Dim scelti = tokens _
        .Select(Function(t, i) New With {.Token = t, .Index = i}) _
        .OrderByDescending(Function(x) x.Token.Length) _
        .ThenByDescending(Function(x) x.Index) _
        .Take(2) _
        .Select(Function(x) x.Token) _
        .ToList()

        Return String.Join(" ", scelti)
    End Function


    Private Function RemoviApostrofi(s As String) As String
        If s Is Nothing Then Return ""
        Return s.Replace("’", "'")
    End Function

    Private Function EstraiSubstringPresente(descrUpper As String, nomeUpper As String) As String
        ' Torna il pezzo di nome che è contenuto nella descrizione (gestione apostrofi)
        Dim n = RemoviApostrofi(nomeUpper)
        Dim d = RemoviApostrofi(descrUpper)
        If d.Contains(n) Then Return n
        ' prova senza parti finali
        Dim parts = n.Split(" "c)
        For i = parts.Length To 1 Step -1
            Dim candidate = String.Join(" ", parts.Take(i))
            If d.Contains(candidate) Then Return candidate
        Next
        Return n
    End Function

    ' FALLBACK WEB: Google Places Text Search v1 (richiede API key)
    Public Async Function RicercaWebPerDescrizioneAsync(descrizione As String) As Task(Of SuggerimentoClassificazione)
        Try
            Dim apiKey = GestoreApiKeys.CaricaChiaveApi("GooglePlaces")
            If String.IsNullOrWhiteSpace(apiKey) Then Return Nothing

            ' Usa l'endpoint v1 places:searchText per maggiore controllo campi
            ' Documentazione: Places API Text Search (New)
            ' Campi richiesti per risparmiare quota (FieldMask)
            Using req As New HttpRequestMessage(HttpMethod.Post, "https://places.googleapis.com/v1/places:searchText")
                Dim body = New With {
                .textQuery = descrizione
            }
                req.Content = New StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                req.Headers.Add("X-Goog-Api-Key", apiKey)
                req.Headers.Add("X-Goog-FieldMask", "places.displayName,places.formattedAddress,places.types")

                Using resp = Await _httpClient.SendAsync(req)
                    resp.EnsureSuccessStatusCode()
                    Dim payload = Await resp.Content.ReadAsStringAsync()
                    Dim doc = JsonDocument.Parse(payload).RootElement
                    If doc.TryGetProperty("places", Nothing) AndAlso doc.GetProperty("places").GetArrayLength() > 0 Then
                        Dim p = doc.GetProperty("places")(0)
                        Dim nome = p.GetProperty("displayName").GetProperty("text").GetString()
                        Dim addr = If(p.TryGetProperty("formattedAddress", Nothing), p.GetProperty("formattedAddress").GetString(), "")
                        Dim tipi As New List(Of String)
                        If p.TryGetProperty("types", Nothing) Then
                            For Each t In p.GetProperty("types").EnumerateArray()
                                tipi.Add(t.GetString())
                            Next
                        End If

                        Dim macro = MapMacroCategoriaGoogle(tipi)
                        Dim cat = MapCategoriaGoogle(tipi)

                        Return New SuggerimentoClassificazione With {
                        .NomeSocieta = nome,
                        .SommarioSocieta = $"Attività a {addr}. Tipi: {String.Join(", ", tipi)}",
                        .ParolaChiave = EstrarreParolaChiaveDaNome(nome),
                        .MacroCategoria = If(String.IsNullOrWhiteSpace(macro), "Altro", macro),
                        .Categoria = If(String.IsNullOrWhiteSpace(cat), "Altro", cat),
                        .Necessita = "Non essenziale",
                        .Frequenza = "Occasionale",
                        .Stagionalita = "Annuale",
                        .Peso = 75,
                        .Confidenza = 85,
                        .IsValid = True,
                        .Motivazione = "Derivato da Google Places Text Search"
                    }
                    End If
                End Using
            End Using
        Catch
            ' Ignora e torna Nothing per non bloccare il flusso
        End Try
        Return Nothing
    End Function

    ' Funzioni di mapping personalizzate da tipi Google Places a categorie tue:
    ' Mapping tipi Google → categorie interne (minimo necessario)
    Private Function MapMacroCategoriaGoogle(tipi As List(Of String)) As String
        Dim u = tipi.Select(Function(t) t.ToLowerInvariant()).ToList()
        If u.Contains("cafe") OrElse u.Contains("bar") OrElse u.Contains("bakery") Then Return "Bar e caffetterie"
        If u.Contains("restaurant") Then Return "Ristorazione"
        If u.Contains("clothing_store") OrElse u.Contains("store") Then Return "Commercio"
        Return "Altro"
    End Function

    Private Function MapCategoriaGoogle(tipi As List(Of String)) As String
        Dim u = tipi.Select(Function(t) t.ToLowerInvariant()).ToList()
        If u.Contains("cafe") OrElse u.Contains("bakery") Then Return "Colazioni e dolci"
        If u.Contains("bar") Then Return "Bar"
        If u.Contains("restaurant") Then Return "Ristoranti"
        If u.Contains("clothing_store") Then Return "Abbigliamento"
        Return "Altro"
    End Function

    Private Function ParseRispostaAnalisi(rispostaGpt As String) As SuggerimentoClassificazione
        Try
            Dim jsonPulito = PulisciJsonResponse(rispostaGpt)
            Dim root = JsonDocument.Parse(jsonPulito).RootElement
            Return New SuggerimentoClassificazione With {
                .NomeSocieta = GetJsonProperty(root, "NomeSocieta"),
                .DescrizioneAttivita = GetJsonProperty(root, "DescrizioneAttivita"),
                .ParolaChiave = GetJsonProperty(root, "ParolaChiave"),
                .MacroCategoria = GetJsonProperty(root, "MacroCategoria"),
                .Categoria = GetJsonProperty(root, "Categoria"),
                .Confidenza = GetJsonIntProperty(root, "Confidenza"),
                .Motivazione = GetJsonProperty(root, "Motivazione"),
                .IsValid = True
            }
        Catch ex As Exception
            Return New SuggerimentoClassificazione With {
                .IsValid = False,
                .Motivazione = $"Errore parsing risposta GPT: {ex.Message}"
            }
        End Try
    End Function

    ' Versione originale mantenuta
    Private Function ParsePatternSuggeriti(rispostaGpt As String) As List(Of String)
        Try
            Dim jsonPulito = PulisciJsonResponse(rispostaGpt)
            Dim root = JsonDocument.Parse(jsonPulito).RootElement
            Dim list As New List(Of String)
            If root.TryGetProperty("PatternSuggeriti", Nothing) Then
                For Each el In root.GetProperty("PatternSuggeriti").EnumerateArray()
                    list.Add(el.GetString())
                Next
            End If
            Return list
        Catch
            Return New List(Of String)
        End Try
    End Function

    ' Nuova funzione di parsing avanzata
    Private Function ParsePatternSuggeritiaAvanzato(rispostaGpt As String) As RisultatoPatternSimili
        Dim risultato As New RisultatoPatternSimili()
        
        Try
            Dim jsonPulito = PulisciJsonResponse(rispostaGpt)
            Debug.WriteLine($"JSON Parse Avanzato: {jsonPulito}")
            Dim root = JsonDocument.Parse(jsonPulito).RootElement
            
            ' Estrai analisi
            If root.TryGetProperty("Analisi", Nothing) Then
                risultato.Analisi = root.GetProperty("Analisi").GetString()
            End If
            
            ' Estrai pattern suggeriti
            If root.TryGetProperty("PatternSuggeriti", Nothing) Then
                For Each el In root.GetProperty("PatternSuggeriti").EnumerateArray()
                    Dim suggerimento As New SuggerimentoPatternAvanzato()
                    
                    If el.TryGetProperty("Parola", Nothing) Then
                        suggerimento.Parola = el.GetProperty("Parola").GetString()
                    End If
                    
                    If el.TryGetProperty("Confidenza", Nothing) Then
                        suggerimento.Confidenza = el.GetProperty("Confidenza").GetInt32()
                    End If
                    
                    If el.TryGetProperty("Motivazione", Nothing) Then
                        suggerimento.Motivazione = el.GetProperty("Motivazione").GetString()
                    End If
                    
                    ' Arricchisci con dati dal database
                    If Not String.IsNullOrWhiteSpace(suggerimento.Parola) Then
                        Dim dettagli = CaricaDettagliPattern(suggerimento.Parola)
                        suggerimento.MacroCategoria = dettagli.Macro
                        suggerimento.Categoria = dettagli.Cat
                        suggerimento.TransazioniPotenziali = ContaTransazioniPotenziali(suggerimento.Parola)
                    End If
                    
                    risultato.PatternSuggeriti.Add(suggerimento)
                Next
            End If
            
        Catch ex As Exception
            Debug.WriteLine($"Errore parsing avanzato: {ex.Message}")
            risultato.Analisi = $"Errore nel parsing: {ex.Message}"
        End Try
        
        Return risultato
    End Function

    ' Carica dettagli di un pattern dal database
    Private Function CaricaDettagliPattern(parola As String) As (Macro As String, Cat As String)
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "SELECT MacroCategoria, Categoria FROM Pattern WHERE Parola = @p LIMIT 1"
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@p", parola)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return (reader.GetString(0), reader.GetString(1))
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Debug.WriteLine($"Errore CaricaDettagliPattern: {ex.Message}")
        End Try
        Return ("", "")
    End Function

    ' Conta quante transazioni potrebbero essere classificate con questo pattern
    Private Function ContaTransazioniPotenziali(parola As String) As Integer
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim query = "SELECT COUNT(*) FROM Transazioni WHERE (MacroCategoria IS NULL OR MacroCategoria = '') AND UPPER(Descrizione) LIKE @pattern"
                Using cmd As New SQLiteCommand(query, conn)
                    cmd.Parameters.AddWithValue("@pattern", $"%{parola.ToUpper()}%")
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch
            Return 0
        End Try
    End Function

    Private Function ParseRisppostaCreaPattern(rispostaGpt As String) As SuggerimentoClassificazione
        Try
            Dim jsonPulito = PulisciJsonResponse(rispostaGpt)
            Dim root = JsonDocument.Parse(jsonPulito).RootElement
            Return New SuggerimentoClassificazione With {
                .ParolaChiave = GetJsonProperty(root, "ParolaChiave"),
                .MacroCategoria = GetJsonProperty(root, "MacroCategoria"),
                .Categoria = GetJsonProperty(root, "Categoria"),
                .Confidenza = GetJsonIntProperty(root, "Confidenza"),
                .Motivazione = GetJsonProperty(root, "Motivazione"),
                .IsValid = True
            }
        Catch ex As Exception
            Return New SuggerimentoClassificazione With {
                .IsValid = False,
                .Motivazione = $"Errore parsing risposta GPT: {ex.Message}"
            }
        End Try
    End Function

    Private Function PulisciJsonResponse(risposta As String) As String
        Dim pulito As String = risposta.Trim()

        ' Rimuove l'apertura del blocco Markdown (```json o ```)
        If pulito.StartsWith("```json") Then
            pulito = pulito.Substring(7)
        ElseIf pulito.StartsWith("```") Then
            pulito = pulito.Substring(3)
        End If

        ' Rimuove la chiusura del blocco Markdown ```
        If pulito.EndsWith("```") Then
            pulito = pulito.Substring(0, pulito.Length - 3)
        End If

        Return pulito.Trim()
    End Function


    Private Function GetJsonProperty(root As JsonElement, name As String) As String
        Try
            If root.TryGetProperty(name, Nothing) Then
                Return root.GetProperty(name).GetString()
            End If
        Catch
        End Try
        Return ""
    End Function

    Private Function GetJsonIntProperty(root As JsonElement, name As String) As Integer
        Try
            If root.TryGetProperty(name, Nothing) Then
                Return root.GetProperty(name).GetInt32()
            End If
        Catch
        End Try
        Return 0
    End Function

    ' IDisposable pattern
    Private disposedValue As Boolean

    Protected Overridable Overloads Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                _httpClient?.Dispose()
            End If
            disposedValue = True
        End If
    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
