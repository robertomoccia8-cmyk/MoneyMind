Imports System.IO

Namespace Logging
    Public Class LoggerFactory
    Private Shared _instance As LoggerFactory
    Private Shared ReadOnly _lockObject As New Object()
    Private _logger As ILogger
    
    Private Sub New()
        InitializeLogger()
    End Sub
    
    Public Shared ReadOnly Property Instance As LoggerFactory
        Get
            If _instance Is Nothing Then
                SyncLock _lockObject
                    If _instance Is Nothing Then
                        _instance = New LoggerFactory()
                    End If
                End SyncLock
            End If
            Return _instance
        End Get
    End Property
    
    Public ReadOnly Property Logger As ILogger
        Get
            Return _logger
        End Get
    End Property
    
    Private Sub InitializeLogger()
        Try
            Dim logDirectory = Path.Combine(Application.StartupPath, "Logs")
            Dim logPath = Path.Combine(logDirectory, "GestioneFinanze.log")
            
            _logger = New FileLogger(logPath, FileLogger.LogLevel.Information)
            _logger.LogInformation("Logger inizializzato correttamente")
            
        Catch ex As Exception
            ' Fallback a console logging se il file logging fallisce
            Debug.WriteLine($"Impossibile inizializzare il file logger: {ex.Message}")
            _logger = New ConsoleLogger()
        End Try
    End Sub
    
    Public Sub ConfigureLogger(logger As ILogger)
        _logger = logger
        _logger.LogInformation("Logger configurato manualmente")
    End Sub
End Class

Public Class ConsoleLogger
    Implements ILogger
    
    Public Sub LogDebug(message As String, ParamArray args() As Object) Implements ILogger.LogDebug
        WriteToConsole("DEBUG", message, args)
    End Sub
    
    Public Sub LogInformation(message As String, ParamArray args() As Object) Implements ILogger.LogInformation
        WriteToConsole("INFO", message, args)
    End Sub
    
    Public Sub LogWarning(message As String, ParamArray args() As Object) Implements ILogger.LogWarning
        WriteToConsole("WARN", message, args)
    End Sub
    
    Public Sub LogError(exception As Exception, message As String, ParamArray args() As Object) Implements ILogger.LogError
        Dim formattedMessage = If(args IsNot Nothing AndAlso args.Length > 0, String.Format(message, args), message)
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {formattedMessage}")
        If exception IsNot Nothing Then
            Debug.WriteLine($"Exception: {exception}")
        End If
    End Sub
    
    Public Sub LogError(message As String, ParamArray args() As Object) Implements ILogger.LogError
        WriteToConsole("ERROR", message, args)
    End Sub
    
    Public Sub LogCritical(exception As Exception, message As String, ParamArray args() As Object) Implements ILogger.LogCritical
        Dim formattedMessage = If(args IsNot Nothing AndAlso args.Length > 0, String.Format(message, args), message)
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [CRITICAL] {formattedMessage}")
        If exception IsNot Nothing Then
            Debug.WriteLine($"Exception: {exception}")
        End If
    End Sub
    
    Private Sub WriteToConsole(level As String, message As String, args() As Object)
        Dim formattedMessage = If(args IsNot Nothing AndAlso args.Length > 0, String.Format(message, args), message)
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {formattedMessage}")
    End Sub
    End Class
End Namespace