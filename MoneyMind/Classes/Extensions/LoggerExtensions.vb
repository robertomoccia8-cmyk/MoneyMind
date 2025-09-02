Imports System.Runtime.CompilerServices
Imports MoneyMind.Logging

Namespace Extensions
    Public Module LoggerExtensions
        ''' <summary>
        ''' Metodo di convenienza per LogInformation
        ''' </summary>
        <Extension>
        Public Sub LogInfo(logger As ILogger, message As String, ParamArray args() As Object)
            logger.LogInformation(message, args)
        End Sub
        
        ''' <summary>
        ''' Metodo di convenienza per LogError con eccezione
        ''' </summary>
        <Extension>
        Public Sub LogError(logger As ILogger, message As String, exception As Exception)
            logger.LogError(exception, message)
        End Sub
    End Module
End Namespace