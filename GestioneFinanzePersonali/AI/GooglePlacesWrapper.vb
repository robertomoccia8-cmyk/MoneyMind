Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading.Tasks

Public Class GooglePlacesWrapper
    Private ReadOnly _httpClient As HttpClient
    Private ReadOnly _apiKey As String

    Public Sub New(apiKey As String)
        _httpClient = New HttpClient()
        _apiKey = apiKey
    End Sub

    ' Metodo per fare ricerca testo su Google Places
    Public Async Function RicercaLuogoAsync(query As String, Optional localita As String = "") As Task(Of GooglePlaceResult)
        Dim baseUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json"
        Dim locationParam = If(String.IsNullOrWhiteSpace(localita), "", $"&location={localita}")
        Dim url = $"{baseUrl}?query={Uri.EscapeDataString(query)}{locationParam}&key={_apiKey}"

        Dim response = Await _httpClient.GetAsync(url)
        response.EnsureSuccessStatusCode()
        Dim json = Await response.Content.ReadAsStringAsync()

        Dim doc = JsonDocument.Parse(json)
        If doc.RootElement.GetProperty("status").GetString() = "OK" Then
            Dim results = doc.RootElement.GetProperty("results")
            If results.GetArrayLength() > 0 Then
                Dim first = results(0)
                Dim nome = first.GetProperty("name").GetString()
                Dim indirizzo = first.GetProperty("formatted_address").GetString()
                Dim tipi = first.GetProperty("types").EnumerateArray().Select(Function(t) t.GetString()).ToList()

                Return New GooglePlaceResult With {
                    .Nome = nome,
                    .Indirizzo = indirizzo,
                    .Tipi = tipi
                }
            End If
        End If

        Return Nothing
    End Function
End Class

Public Class GooglePlaceResult
    Public Property Nome As String
    Public Property Indirizzo As String
    Public Property Tipi As List(Of String)
End Class
