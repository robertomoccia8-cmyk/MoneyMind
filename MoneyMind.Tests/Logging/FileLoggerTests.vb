Imports NUnit.Framework
Imports FluentAssertions
Imports System.IO

<TestFixture>
Public Class FileLoggerTests
    Private _tempLogPath As String
    Private _logger As FileLogger
    
    <SetUp>
    Public Sub Setup()
        _tempLogPath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log")
        _logger = New FileLogger(_tempLogPath, FileLogger.LogLevel.Debug)
    End Sub
    
    <TearDown>
    Public Sub TearDown()
        _logger = Nothing
        Try
            If File.Exists(_tempLogPath) Then
                File.Delete(_tempLogPath)
            End If
            
            ' Pulisci eventuali file di backup
            Dim directory = Path.GetDirectoryName(_tempLogPath)
            Dim fileName = Path.GetFileNameWithoutExtension(_tempLogPath)
            Dim extension = Path.GetExtension(_tempLogPath)
            Dim pattern = $"{fileName}_*{extension}"
            
            For Each backupFile In Directory.GetFiles(directory, pattern)
                File.Delete(backupFile)
            Next
        Catch
            ' Ignora errori di pulizia
        End Try
    End Sub
    
    <Test>
    Public Sub LogInformation_ShouldCreateLogFile_WhenFileDoesNotExist()
        ' Act
        _logger.LogInformation("Test message")
        
        ' Assert
        File.Exists(_tempLogPath).Should().BeTrue()
    End Sub
    
    <Test>
    Public Sub LogInformation_ShouldWriteFormattedMessage()
        ' Arrange
        Dim message = "Test information message"
        
        ' Act
        _logger.LogInformation(message)
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        logContent.Should().Contain("[INFORMATION]")
        logContent.Should().Contain(message)
        logContent.Should().Match("*[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9]:[0-9][0-9]*")
    End Sub
    
    <Test>
    Public Sub LogError_ShouldWriteExceptionDetails_WhenExceptionProvided()
        ' Arrange
        Dim message = "Error occurred"
        Dim exception = New InvalidOperationException("Test exception")
        
        ' Act
        _logger.LogError(exception, message)
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        logContent.Should().Contain("[ERROR]")
        logContent.Should().Contain(message)
        logContent.Should().Contain("Exception: InvalidOperationException")
        logContent.Should().Contain("Test exception")
        logContent.Should().Contain("StackTrace:")
    End Sub
    
    <Test>
    Public Sub LogWithParameters_ShouldFormatParametersCorrectly()
        ' Arrange
        Dim messageTemplate = "User {0} performed action {1} at {2}"
        Dim user = "TestUser"
        Dim action = "Login"
        Dim timestamp = DateTime.Now
        
        ' Act
        _logger.LogInformation(messageTemplate, user, action, timestamp)
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        logContent.Should().Contain($"User {user} performed action {action} at {timestamp}")
    End Sub
    
    <Test>
    Public Sub LogLevels_ShouldRespectMinimumLevel()
        ' Arrange
        Dim warningLogger = New FileLogger(_tempLogPath, FileLogger.LogLevel.Warning)
        
        ' Act
        warningLogger.LogDebug("Debug message")
        warningLogger.LogInformation("Info message")
        warningLogger.LogWarning("Warning message")
        warningLogger.LogError("Error message")
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        logContent.Should().NotContain("Debug message")
        logContent.Should().NotContain("Info message")
        logContent.Should().Contain("Warning message")
        logContent.Should().Contain("Error message")
    End Sub
    
    <Test>
    Public Sub MultipleLogEntries_ShouldAppendToFile()
        ' Act
        _logger.LogInformation("First message")
        _logger.LogInformation("Second message")
        _logger.LogError("Error message")
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        Dim lines = logContent.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
        lines.Should().HaveCountGreaterThan(2)
        logContent.Should().Contain("First message")
        logContent.Should().Contain("Second message")
        logContent.Should().Contain("Error message")
    End Sub
    
    <Test>
    Public Sub LogCritical_ShouldWriteWithCorrectLevel()
        ' Arrange
        Dim message = "Critical system failure"
        Dim exception = New SystemException("System is down")
        
        ' Act
        _logger.LogCritical(exception, message)
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        logContent.Should().Contain("[CRITICAL]")
        logContent.Should().Contain(message)
        logContent.Should().Contain("SystemException")
    End Sub
    
    <Test>
    Public Sub LogWarning_ShouldWriteWithCorrectLevel()
        ' Arrange
        Dim message = "This is a warning"
        
        ' Act
        _logger.LogWarning(message)
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        logContent.Should().Contain("[WARNING]")
        logContent.Should().Contain(message)
    End Sub
    
    <Test>
    Public Sub LogDebug_ShouldWriteOnlyWhenDebugLevelEnabled()
        ' Arrange
        Dim infoLogger = New FileLogger(_tempLogPath, FileLogger.LogLevel.Information)
        Dim debugMessage = "Debug details"
        
        ' Act
        infoLogger.LogDebug(debugMessage)
        
        ' Assert
        If File.Exists(_tempLogPath) Then
            Dim logContent = File.ReadAllText(_tempLogPath)
            logContent.Should().NotContain(debugMessage)
        End If
    End Sub
    
    <Test>
    Public Sub InnerException_ShouldBeLogged_WhenPresent()
        ' Arrange
        Dim innerException = New ArgumentException("Inner error")
        Dim outerException = New InvalidOperationException("Outer error", innerException)
        
        ' Act
        _logger.LogError(outerException, "Error with inner exception")
        
        ' Assert
        Dim logContent = File.ReadAllText(_tempLogPath)
        logContent.Should().Contain("InvalidOperationException")
        logContent.Should().Contain("Outer error")
        logContent.Should().Contain("Inner Exception: ArgumentException")
        logContent.Should().Contain("Inner error")
    End Sub
End Class