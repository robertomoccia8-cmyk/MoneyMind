Imports System.IO
Imports System.Text

Namespace Logging
    Public Class FileLogger
    Implements ILogger
    
    Private ReadOnly _logPath As String
    Private ReadOnly _minLogLevel As LogLevel
    Private ReadOnly _lockObject As New Object()
    
    Public Enum LogLevel
        Debug = 0
        Information = 1
        Warning = 2
        [Error] = 3
        Critical = 4
    End Enum
    
    Public Sub New(logPath As String, Optional minLogLevel As LogLevel = LogLevel.Information)
        _logPath = logPath
        _minLogLevel = minLogLevel
        
        ' Crea la directory se non esiste
        Dim directoryPath = Path.GetDirectoryName(_logPath)
        If Not String.IsNullOrEmpty(directoryPath) AndAlso Not Directory.Exists(directoryPath) Then
            Directory.CreateDirectory(directoryPath)
        End If
    End Sub
    
    Public Sub LogDebug(message As String, ParamArray args() As Object) Implements ILogger.LogDebug
        If _minLogLevel <= LogLevel.Debug Then
            WriteLog(LogLevel.Debug, Nothing, message, args)
        End If
    End Sub
    
    Public Sub LogInformation(message As String, ParamArray args() As Object) Implements ILogger.LogInformation
        If _minLogLevel <= LogLevel.Information Then
            WriteLog(LogLevel.Information, Nothing, message, args)
        End If
    End Sub
    
    Public Sub LogWarning(message As String, ParamArray args() As Object) Implements ILogger.LogWarning
        If _minLogLevel <= LogLevel.Warning Then
            WriteLog(LogLevel.Warning, Nothing, message, args)
        End If
    End Sub
    
    Public Sub LogError(exception As Exception, message As String, ParamArray args() As Object) Implements ILogger.LogError
        If _minLogLevel <= LogLevel.Error Then
            WriteLog(LogLevel.Error, exception, message, args)
        End If
    End Sub
    
    Public Sub LogError(message As String, ParamArray args() As Object) Implements ILogger.LogError
        If _minLogLevel <= LogLevel.Error Then
            WriteLog(LogLevel.Error, Nothing, message, args)
        End If
    End Sub
    
    Public Sub LogCritical(exception As Exception, message As String, ParamArray args() As Object) Implements ILogger.LogCritical
        If _minLogLevel <= LogLevel.Critical Then
            WriteLog(LogLevel.Critical, exception, message, args)
        End If
    End Sub
    
    Private Sub WriteLog(level As LogLevel, exception As Exception, message As String, args() As Object)
        Try
            Dim formattedMessage = If(args IsNot Nothing AndAlso args.Length > 0, String.Format(message, args), message)
            Dim timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            Dim logEntry As New StringBuilder()
            
            logEntry.AppendFormat("[{0}] [{1}] {2}", timestamp, level.ToString().ToUpper(), formattedMessage)
            logEntry.AppendLine()
            
            If exception IsNot Nothing Then
                logEntry.AppendLine($"Exception: {exception.GetType().Name}")
                logEntry.AppendLine($"Message: {exception.Message}")
                logEntry.AppendLine($"StackTrace: {exception.StackTrace}")
                
                Dim innerEx = exception.InnerException
                While innerEx IsNot Nothing
                    logEntry.AppendLine($"Inner Exception: {innerEx.GetType().Name}")
                    logEntry.AppendLine($"Inner Message: {innerEx.Message}")
                    innerEx = innerEx.InnerException
                End While
            End If
            
            ' Thread-safe file writing
            SyncLock _lockObject
                File.AppendAllText(_logPath, logEntry.ToString(), Encoding.UTF8)
            End SyncLock
            
            ' Rotazione log se il file diventa troppo grande (>10MB)
            RotateLogIfNeeded()
            
        Catch ex As Exception
            ' Se il logging fallisce, almeno scriviamo nel debug output
            Debug.WriteLine($"Logging failed: {ex.Message}")
        End Try
    End Sub
    
    Private Sub RotateLogIfNeeded()
        Try
            If File.Exists(_logPath) Then
                Dim fileInfo As New FileInfo(_logPath)
                If fileInfo.Length > 10 * 1024 * 1024 Then ' 10MB
                    Dim backupPath = _logPath.Replace(".log", $"_{DateTime.Now:yyyyMMdd_HHmmss}.log")
                    File.Move(_logPath, backupPath)
                    
                    ' Mantieni solo gli ultimi 5 file di backup
                    CleanOldLogFiles()
                End If
            End If
        Catch
            ' Ignora errori di rotazione per non interrompere l'applicazione
        End Try
    End Sub
    
    Private Sub CleanOldLogFiles()
        Try
            Dim directoryPath = Path.GetDirectoryName(_logPath)
            Dim fileName = Path.GetFileNameWithoutExtension(_logPath)
            Dim extension = Path.GetExtension(_logPath)
            
            Dim pattern = $"{fileName}_*{extension}"
            Dim backupFiles = Directory.GetFiles(directoryPath, pattern)
            
            If backupFiles.Length > 5 Then
                Array.Sort(backupFiles)
                For i = 0 To backupFiles.Length - 6
                    File.Delete(backupFiles(i))
                Next
            End If
        Catch
            ' Ignora errori di pulizia
        End Try
    End Sub
    End Class
End Namespace