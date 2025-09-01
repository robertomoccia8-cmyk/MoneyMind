Imports System.Configuration
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions

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

        If String.IsNullOrEmpty(_apiKey) Then
            ' Non solleva eccezione qui, la gestisce al momento dell'uso
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

    Public Async Function TrovaPatternSimili(parolaChiave As String, patternEsistenti As List(Of String)) As Task(Of List(Of String))
        Try
            Dim prompt = CreaPromptTrovaPattern(parolaChiave, patternEsistenti)
            Dim risposta = Await ChiamaGptApi(prompt)
            Return ParsePatternSuggeriti(risposta)
        Catch ex As Exception
            Return New List(Of String)
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

    Private Function CreaPromptTrovaPattern(parolaChiave As String, patternEsistenti As List(Of String)) As String
        Dim patternsStr = String.Join(", ", patternEsistenti.Take(20))
        Return $"Trova i 5 pattern più simili alla parola chiave:

PAROLA CHIAVE: ""{parolaChiave}""
PATTERN ESISTENTI: {patternsStr}

Rispondi con JSON:
{{
    ""PatternSuggeriti"": [""p1"",""p2"",""p3"",""p4"",""p5""]
}}"
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

        ' 2) Se AI fallisce, fallback Google
        If Not EsitoValidoCompleto(suggerimento) Then
            If ApiValidator.ControllaSeMancaChiaveApi("GooglePlaces") Then
                ApiValidator.ChiediAperturaImpostazioni("Google Places")
            End If
            Dim daWeb = Await RicercaWebPerDescrizioneAsync(descrizione)
            If daWeb IsNot Nothing AndAlso daWeb.IsValid Then
                suggerimento = daWeb
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
        If s Is Nothing OrElse Not s.IsValid Then Return False
        If String.IsNullOrWhiteSpace(s.ParolaChiave) Then Return False
        If String.IsNullOrWhiteSpace(s.NomeSocieta) AndAlso String.IsNullOrWhiteSpace(s.SommarioSocieta) Then Return False
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
        sb.AppendLine("Regole:")
        sb.AppendLine("- Se nella descrizione compare un nome di società, impostalo COME NomeSocieta E SEMPRE come ParolaChiave.")
        sb.AppendLine("- Aggiungi 'SommarioSocieta': un breve testo su cosa fa la società.")
        sb.AppendLine("- MacroCategoria e Categoria devono riflettere il settore di attività della società.")
        sb.AppendLine("- IMPORTANTE -> Se non c’è nome società, ParolaChiave deve essere ESATTAMENTE una parola PRESENTE nella descrizione.")
        sb.AppendLine("- Necessita, Frequenza e Stagionalita: scegli ESCLUSIVAMENTE da quelli elencati.")
        sb.AppendLine("- Peso sempre 75.")
        sb.AppendLine("- Non lasciare mai vuoti NomeSocieta, ParolaChiave o DescrizioneAttivita.")
        sb.AppendLine("- Confidenza 0–100; Motivazione breve.")
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
        s.SommarioSocieta = If(doc.TryGetProperty("SommarioSocietà", Nothing), doc.GetProperty("SommarioSocietà").GetString(), "")
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
                End If

                ' Estrai e pulisci content
                Dim contentStr = root.GetProperty("choices")(0).GetProperty("message").GetProperty("content").GetString()
                Dim jsonPulito = PulisciJsonResponse(contentStr)
                Dim s = ParseSuggerimentoFromJson(jsonPulito)

                ' Validazione soft
                s.IsValid = EsitoValido(descrizione, s)
                Return s
            End Using
        Catch
            Return New SuggerimentoClassificazione With {.IsValid = False}
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

        ' Imposta defaults se mancanti
        If String.IsNullOrWhiteSpace(s.MacroCategoria) Then s.MacroCategoria = "Altro"
        If String.IsNullOrWhiteSpace(s.Categoria) Then s.Categoria = "Altro"
        If String.IsNullOrWhiteSpace(s.Necessita) Then s.Necessita = "Non essenziale"
        If String.IsNullOrWhiteSpace(s.Frequenza) Then s.Frequenza = "Occasionale"
        If String.IsNullOrWhiteSpace(s.Stagionalita) Then s.Stagionalita = "Annuale"
        s.Peso = 75

        Return True
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

        ' 2) Fallback token-by-token: escludi sigle di paese e città
        Dim stopWords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "POS", "EUROZONA", "PAGAMENTO", "OPERAZIONE",
        "ITA", "EUR", "SA", "PAESTUM", "AGROPOLI"
    }

        Dim tokens = descr.ToUpperInvariant() _
        .Split({" "c}, StringSplitOptions.RemoveEmptyEntries) _
        .Select(Function(t) t.Trim("'"c, """"c, "."c, ","c)) _
        .Where(Function(t) t.Length > 2) _
        .Where(Function(t) Not stopWords.Contains(t)) _
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
