Imports Newtonsoft.Json

''' <summary>
''' Rappresenta le informazioni di una versione dell'applicazione
''' </summary>
Public Class VersionInfo
    <JsonProperty("version")>
    Public Property Version As String
    
    <JsonProperty("releaseDate")>
    Public Property ReleaseDate As DateTime
    
    <JsonProperty("changes")>
    Public Property Changes As List(Of String)
    
    <JsonProperty("downloadUrl")>
    Public Property DownloadUrl As String
    
    <JsonProperty("mandatory")>
    Public Property Mandatory As Boolean = False
    
    <JsonProperty("minimumVersion")>
    Public Property MinimumVersion As String
    
    Public Sub New()
        Changes = New List(Of String)()
    End Sub
    
    ''' <summary>
    ''' Converte la stringa versione in oggetto Version per confronti
    ''' </summary>
    Public Function GetVersion() As Version
        Return New Version(Version)
    End Function
    
    ''' <summary>
    ''' Controlla se questa versione è più recente di quella specificata
    ''' </summary>
    Public Function IsNewerThan(currentVersion As String) As Boolean
        Try
            Dim current As New Version(currentVersion)
            Dim thisVersion As New Version(Version)
            Return thisVersion > current
        Catch
            Return False
        End Try
    End Function
End Class

''' <summary>
''' Risposta dell'API GitHub per le release
''' </summary>
Public Class GitHubRelease
    <JsonProperty("tag_name")>
    Public Property TagName As String
    
    <JsonProperty("name")>
    Public Property Name As String
    
    <JsonProperty("published_at")>
    Public Property PublishedAt As DateTime
    
    <JsonProperty("body")>
    Public Property Body As String
    
    <JsonProperty("assets")>
    Public Property Assets As List(Of GitHubAsset)
    
    <JsonProperty("prerelease")>
    Public Property PreRelease As Boolean
    
    Public Sub New()
        Assets = New List(Of GitHubAsset)()
    End Sub
End Class

''' <summary>
''' Asset di una release GitHub (file scaricabili)
''' </summary>
Public Class GitHubAsset
    <JsonProperty("name")>
    Public Property Name As String
    
    <JsonProperty("browser_download_url")>
    Public Property BrowserDownloadUrl As String
    
    <JsonProperty("size")>
    Public Property Size As Long
End Class