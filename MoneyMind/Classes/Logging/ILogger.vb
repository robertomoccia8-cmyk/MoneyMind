Namespace Logging
    Public Interface ILogger
    Sub LogDebug(message As String, ParamArray args() As Object)
    Sub LogInformation(message As String, ParamArray args() As Object)
    Sub LogWarning(message As String, ParamArray args() As Object)
    Sub LogError(exception As Exception, message As String, ParamArray args() As Object)
    Sub LogError(message As String, ParamArray args() As Object)
    Sub LogCritical(exception As Exception, message As String, ParamArray args() As Object)
    End Interface
End Namespace