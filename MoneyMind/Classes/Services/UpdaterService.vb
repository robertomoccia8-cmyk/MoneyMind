Imports System.IO
Imports System.Net.Http
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports System.IO.Compression
Imports MoneyMind.Logging
Imports MoneyMind.Extensions

''' <summary>
''' Servizio per gestire gli aggiornamenti dell'applicazione
''' </summary>
Public Class UpdaterService
    Implements IDisposable
    Private ReadOnly _httpClient As HttpClient
    Private ReadOnly _logger As ILogger
    
    Public Sub New(logger As ILogger)
        _logger = logger
        _httpClient = New HttpClient()
        _httpClient.Timeout = TimeSpan.FromSeconds(ApplicationInfo.HttpTimeoutSeconds)
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"{ApplicationInfo.ApplicationName}/{ApplicationInfo.CurrentVersion}")
    End Sub
    
    ''' <summary>
    ''' Controlla se sono disponibili aggiornamenti
    ''' </summary>
    Public Async Function CheckForUpdatesAsync() As Task(Of VersionInfo)
        Try
            _logger.LogInformation("Controllo aggiornamenti disponibili...")
            
            Dim response = Await _httpClient.GetStringAsync($"{ApplicationInfo.GitHubApiUrl}/latest")
            Dim release = JsonConvert.DeserializeObject(Of GitHubRelease)(response)
            
            If release Is Nothing OrElse String.IsNullOrEmpty(release.TagName) Then
                _logger.LogWarning("Nessuna release trovata su GitHub")
                Return Nothing
            End If
            
            ' Rimuovi il prefisso 'v' se presente (es. v1.2.0 -> 1.2.0)
            Dim versionString = release.TagName.TrimStart("v"c)
            
            Dim versionInfo As New VersionInfo() With {
                .Version = versionString,
                .ReleaseDate = release.PublishedAt,
                .Changes = ParseChangelog(release.Body),
                .DownloadUrl = GetInstallerUrl(release)
            }
            
            If versionInfo.IsNewerThan(ApplicationInfo.CurrentVersion) Then
                _logger.LogInformation($"Nuovo aggiornamento disponibile: {versionInfo.Version}")
                Return versionInfo
            Else
                _logger.LogInformation("Applicazione già aggiornata")
                Return Nothing
            End If
            
        Catch ex As Exception
            _logger.LogError("Errore durante il controllo aggiornamenti", ex)
            Return Nothing
        End Try
    End Function
    
    ''' <summary>
    ''' Scarica l'installer dell'aggiornamento
    ''' </summary>
    Public Async Function DownloadUpdateAsync(versionInfo As VersionInfo, 
                                            progressCallback As Action(Of Integer)) As Task(Of String)
        Try
            _logger.LogInfo($"Scaricamento aggiornamento {versionInfo.Version}...")
            
            Dim tempPath = Path.Combine(Path.GetTempPath(), ApplicationInfo.InstallerFileName)
            
            Using response = Await _httpClient.GetAsync(versionInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead)
                response.EnsureSuccessStatusCode()
                
                Dim totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault()
                Using contentStream = Await response.Content.ReadAsStreamAsync()
                    Using fileStream As New FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None)
                        Const bufferSize As Integer = 8192
                        Dim buffer(bufferSize - 1) As Byte
                        Dim totalRead As Long = 0
                        
                        While True
                            Dim bytesRead = Await contentStream.ReadAsync(buffer, 0, buffer.Length)
                            If bytesRead = 0 Then Exit While
                            
                            Await fileStream.WriteAsync(buffer, 0, bytesRead)
                            totalRead += bytesRead
                            
                            If totalBytes > 0 Then
                                Dim progress = CInt((totalRead * 100) / totalBytes)
                                progressCallback?.Invoke(progress)
                            End If
                        End While
                    End Using
                End Using
            End Using
            
            _logger.LogInfo($"Aggiornamento scaricato: {tempPath}")
            Return tempPath
            
        Catch ex As Exception
            _logger.LogError("Errore durante il download dell'aggiornamento", ex)
            Throw
        End Try
    End Function
    
    ''' <summary>
    ''' Installa l'aggiornamento scaricato
    ''' </summary>
    Public Function InstallUpdate(installerPath As String) As Boolean
        Try
            _logger.LogInfo($"Avvio installazione: {installerPath}")
            
            If Not File.Exists(installerPath) Then
                _logger.LogError("File installer non trovato")
                Return False
            End If
            
            ' Avvia il processo di installazione MSI
            Dim startInfo As New ProcessStartInfo() With {
                .FileName = "msiexec",
                .Arguments = $"/i ""{installerPath}"" /quiet /norestart",
                .UseShellExecute = True,
                .Verb = "runas" ' Richiedi privilegi amministratore
            }
            
            Dim process As Process = Process.Start(startInfo)
            If process IsNot Nothing Then
                _logger.LogInfo("Installazione avviata con successo")
                process.Dispose()
                Return True
            End If
            
            Return False
            
        Catch ex As Exception
            _logger.LogError("Errore durante l'installazione", ex)
            Return False
        End Try
    End Function
    
    ''' <summary>
    ''' Estrae il changelog dal corpo della release GitHub
    ''' </summary>
    Private Function ParseChangelog(releaseBody As String) As List(Of String)
        Dim changes As New List(Of String)()
        
        If String.IsNullOrWhiteSpace(releaseBody) Then
            Return changes
        End If
        
        ' Dividi per righe e filtra quelle che iniziano con -, *, o emoji
        Dim lines = releaseBody.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        
        For Each line In lines
            Dim trimmed = line.Trim()
            If trimmed.StartsWith("-") OrElse 
               trimmed.StartsWith("*") OrElse 
               trimmed.StartsWith("•") OrElse
               (trimmed.Length > 0 AndAlso Char.IsSymbol(trimmed(0))) Then
                
                ' Rimuovi il prefisso markdown
                If trimmed.StartsWith("-") OrElse trimmed.StartsWith("*") Then
                    trimmed = trimmed.Substring(1).Trim()
                End If
                
                If Not String.IsNullOrWhiteSpace(trimmed) Then
                    changes.Add(trimmed)
                End If
            End If
        Next
        
        ' Se non troviamo nessun cambiamento formattato, aggiungi il primo paragrafo
        If changes.Count = 0 AndAlso Not String.IsNullOrWhiteSpace(releaseBody) Then
            Dim firstParagraph = releaseBody.Split({vbCrLf & vbCrLf, vbLf & vbLf}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
            If Not String.IsNullOrWhiteSpace(firstParagraph) Then
                changes.Add(firstParagraph.Trim())
            End If
        End If
        
        Return changes
    End Function
    
    ''' <summary>
    ''' Trova l'URL dell'installer nella release GitHub
    ''' </summary>
    Private Function GetInstallerUrl(release As GitHubRelease) As String
        ' Cerca il file MSI negli assets
        Dim installerAsset = release.Assets.FirstOrDefault(Function(a) a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
        
        If installerAsset IsNot Nothing Then
            Return installerAsset.BrowserDownloadUrl
        End If
        
        ' Se non troviamo MSI, cerca un ZIP
        Dim zipAsset = release.Assets.FirstOrDefault(Function(a) a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        
        If zipAsset IsNot Nothing Then
            Return zipAsset.BrowserDownloadUrl
        End If
        
        ' Fallback: restituisci URL vuoto (mostrerà errore)
        Return String.Empty
    End Function
    
    ''' <summary>
    ''' Rilascia le risorse
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        _httpClient?.Dispose()
    End Sub
End Class