Imports System.IO
Imports System.IO.Compression
Imports GestioneFinanzePersonali.Logging
Imports GestioneFinanzePersonali.Extensions
Imports Newtonsoft.Json

''' <summary>
''' Servizio per gestire i backup del database prima degli aggiornamenti
''' </summary>
Public Class BackupService
    Private ReadOnly _logger As ILogger
    
    Public Sub New(logger As ILogger)
        _logger = logger
    End Sub
    
    ''' <summary>
    ''' Crea un backup del database prima dell'aggiornamento
    ''' </summary>
    Public Function CreateBackup(Optional reason As String = "update") As String
        Try
            _logger.LogInfo($"Creazione backup del database - Motivo: {reason}")
            
            ' Assicurati che la directory backup esista
            Dim backupDirectory = ApplicationInfo.BackupDirectory
            If Not Directory.Exists(backupDirectory) Then
                Directory.CreateDirectory(backupDirectory)
                _logger.LogInfo($"Creata directory backup: {backupDirectory}")
            End If
            
            ' Trova il percorso del database
            Dim databasePath = FindDatabasePath()
            If String.IsNullOrEmpty(databasePath) OrElse Not File.Exists(databasePath) Then
                _logger.LogWarning("Database non trovato per il backup")
                Return String.Empty
            End If
            
            ' Crea il nome del backup con timestamp
            Dim timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            Dim backupFileName = $"backup_{timestamp}_{reason}.zip"
            Dim backupPath = Path.Combine(backupDirectory, backupFileName)
            
            ' Crea il backup compresso
            Using fileStream As New FileStream(backupPath, FileMode.Create)
                Using archive As New ZipArchive(fileStream, ZipArchiveMode.Create)
                    ' Aggiungi il database principale
                    Dim entry = archive.CreateEntry(ApplicationInfo.DatabaseName)
                    Using entryStream = entry.Open()
                        Using fileStreamIn As New FileStream(databasePath, FileMode.Open, FileAccess.Read)
                            fileStreamIn.CopyTo(entryStream)
                        End Using
                    End Using
                
                ' Aggiungi eventuali file di configurazione
                Dim configFiles = FindConfigurationFiles()
                    For Each configFile In configFiles
                        If File.Exists(configFile) Then
                            Dim fileName = Path.GetFileName(configFile)
                            Dim configEntry = archive.CreateEntry(fileName)
                            Using configEntryStream = configEntry.Open()
                                Using configFileStream As New FileStream(configFile, FileMode.Open, FileAccess.Read)
                                    configFileStream.CopyTo(configEntryStream)
                                End Using
                            End Using
                            _logger.LogInfo($"Aggiunto al backup: {fileName}")
                        End If
                    Next
                    
                    ' Aggiungi metadati del backup
                    CreateBackupMetadata(archive, reason)
                End Using
            End Using
            
            _logger.LogInfo($"Backup creato con successo: {backupPath}")
            
            ' Pulisci i vecchi backup
            CleanupOldBackups()
            
            Return backupPath
            
        Catch ex As Exception
            _logger.LogError("Errore durante la creazione del backup", ex)
            Return String.Empty
        End Try
    End Function
    
    ''' <summary>
    ''' Ripristina un backup del database
    ''' </summary>
    Public Function RestoreBackup(backupPath As String) As Boolean
        Try
            _logger.LogInfo($"Ripristino backup: {backupPath}")
            
            If Not File.Exists(backupPath) Then
                _logger.LogError("File di backup non trovato")
                Return False
            End If
            
            Dim databasePath = FindDatabasePath()
            If String.IsNullOrEmpty(databasePath) Then
                _logger.LogError("Percorso database non determinato")
                Return False
            End If
            
            ' Crea backup di sicurezza prima del ripristino
            If File.Exists(databasePath) Then
                Dim emergencyBackup = Path.Combine(ApplicationInfo.BackupDirectory, 
                                                 $"emergency_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db")
                File.Copy(databasePath, emergencyBackup, True)
                _logger.LogInfo($"Backup di emergenza creato: {emergencyBackup}")
            End If
            
            ' Estrai il backup
            Using fileStream As New FileStream(backupPath, FileMode.Open)
                Using archive As New ZipArchive(fileStream, ZipArchiveMode.Read)
                    Dim dbEntry = archive.GetEntry(ApplicationInfo.DatabaseName)
                    If dbEntry IsNot Nothing Then
                        Using entryStream = dbEntry.Open()
                            Using outputStream As New FileStream(databasePath, FileMode.Create)
                                entryStream.CopyTo(outputStream)
                            End Using
                        End Using
                        _logger.LogInfo("Database ripristinato con successo")
                        Return True
                    Else
                        _logger.LogError("Database non trovato nel backup")
                        Return False
                    End If
                End Using
            End Using
            
        Catch ex As Exception
            _logger.LogError("Errore durante il ripristino del backup", ex)
            Return False
        End Try
    End Function
    
    ''' <summary>
    ''' Ottiene la lista dei backup disponibili
    ''' </summary>
    Public Function GetAvailableBackups() As List(Of BackupInfo)
        Dim backups As New List(Of BackupInfo)()
        
        Try
            If Not Directory.Exists(ApplicationInfo.BackupDirectory) Then
                Return backups
            End If
            
            Dim backupFiles = Directory.GetFiles(ApplicationInfo.BackupDirectory, "backup_*.zip")
            
            For Each backupFile In backupFiles
                Try
                    Dim fileInfo As New FileInfo(backupFile)
                    Dim fileName = Path.GetFileNameWithoutExtension(fileInfo.Name)
                    
                    ' Parsing del nome file: backup_20250901_143022_update.zip
                    Dim parts = fileName.Split("_"c)
                    If parts.Length >= 3 Then
                        Dim datePart = parts(1)
                        Dim timePart = parts(2)
                        Dim reason = If(parts.Length > 3, parts(3), "manual")
                        
                        If DateTime.TryParseExact($"{datePart}_{timePart}", "yyyyMMdd_HHmmss", 
                                                Nothing, Globalization.DateTimeStyles.None, Nothing) Then
                            
                            Dim backupInfo As New BackupInfo() With {
                                .FilePath = backupFile,
                                .CreatedDate = fileInfo.CreationTime,
                                .Size = fileInfo.Length,
                                .Reason = reason,
                                .Name = $"Backup {fileInfo.CreationTime:dd/MM/yyyy HH:mm} ({reason})"
                            }
                            
                            backups.Add(backupInfo)
                        End If
                    End If
                    
                Catch ex As Exception
                    _logger.LogWarning($"Errore parsing backup {backupFile}: {ex.Message}")
                End Try
            Next
            
            ' Ordina per data decrescente (più recenti prima)
            backups = backups.OrderByDescending(Function(b) b.CreatedDate).ToList()
            
        Catch ex As Exception
            _logger.LogError("Errore durante il recupero dei backup", ex)
        End Try
        
        Return backups
    End Function
    
    ''' <summary>
    ''' Trova il percorso del database SQLite
    ''' </summary>
    Private Function FindDatabasePath() As String
        ' Controlla diverse posizioni possibili
        Dim possiblePaths() As String = {
            Path.Combine(Application.StartupPath, ApplicationInfo.DatabaseName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        ApplicationInfo.ApplicationName, ApplicationInfo.DatabaseName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                        ApplicationInfo.ApplicationName, ApplicationInfo.DatabaseName),
            ApplicationInfo.DatabaseName ' Current directory
        }
        
        For Each path In possiblePaths
            If File.Exists(path) Then
                _logger.LogInfo($"Database trovato: {path}")
                Return path
            End If
        Next
        
        ' Se non trovato, restituisci il primo percorso come default
        Return possiblePaths(0)
    End Function
    
    ''' <summary>
    ''' Trova i file di configurazione da includere nel backup
    ''' </summary>
    Private Function FindConfigurationFiles() As List(Of String)
        Dim configFiles As New List(Of String)()
        
        ' Aggiungi eventuali file di configurazione importanti
        Dim possibleConfigs() As String = {
            Path.Combine(Application.StartupPath, "App.config"),
            Path.Combine(Application.StartupPath, "appsettings.json"),
            Path.Combine(Application.StartupPath, "config.xml")
        }
        
        For Each configPath In possibleConfigs
            If File.Exists(configPath) Then
                configFiles.Add(configPath)
            End If
        Next
        
        Return configFiles
    End Function
    
    ''' <summary>
    ''' Crea i metadati del backup
    ''' </summary>
    Private Sub CreateBackupMetadata(archive As ZipArchive, reason As String)
        Dim metadata As New With {
            .Version = ApplicationInfo.CurrentVersion,
            .CreatedDate = DateTime.Now,
            .Reason = reason,
            .MachineName = Environment.MachineName,
            .UserName = Environment.UserName
        }
        
        Dim metadataJson = JsonConvert.SerializeObject(metadata, Formatting.Indented)
        Dim metadataEntry = archive.CreateEntry("backup_metadata.json")
        
        Using stream = metadataEntry.Open()
            Using writer As New StreamWriter(stream)
                writer.Write(metadataJson)
            End Using
        End Using
    End Sub
    
    ''' <summary>
    ''' Rimuove i backup più vecchi mantenendo solo gli ultimi N
    ''' </summary>
    Private Sub CleanupOldBackups()
        Try
            Dim backups = GetAvailableBackups()
            
            If backups.Count > ApplicationInfo.MaxBackups Then
                Dim backupsToDelete = backups.Skip(ApplicationInfo.MaxBackups).ToList()
                
                For Each backup In backupsToDelete
                    Try
                        File.Delete(backup.FilePath)
                        _logger.LogInfo($"Rimosso vecchio backup: {backup.Name}")
                    Catch ex As Exception
                        _logger.LogWarning($"Impossibile rimuovere backup {backup.FilePath}: {ex.Message}")
                    End Try
                Next
            End If
            
        Catch ex As Exception
            _logger.LogWarning("Errore durante la pulizia dei vecchi backup", ex)
        End Try
    End Sub
End Class

''' <summary>
''' Informazioni su un backup
''' </summary>
Public Class BackupInfo
    Public Property FilePath As String
    Public Property Name As String
    Public Property CreatedDate As DateTime
    Public Property Size As Long
    Public Property Reason As String
    
    Public ReadOnly Property SizeFormatted As String
        Get
            If Size < 1024 Then
                Return $"{Size} B"
            ElseIf Size < 1024 * 1024 Then
                Return $"{Size / 1024:F1} KB"
            Else
                Return $"{Size / (1024 * 1024):F1} MB"
            End If
        End Get
    End Property
End Class