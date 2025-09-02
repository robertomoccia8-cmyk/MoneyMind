Imports System.Security.Cryptography
Imports System.Text
Imports System.Data.SQLite
Imports MoneyMind.Security
Imports MoneyMind.Logging

Public Class GestoreApiKeys
    Private Shared ReadOnly _secureConfig As ISecureConfigurationManager
    Private Shared ReadOnly _logger As ILogger
    
    Private Const OPENAI_KEY = "OpenAI_ApiKey"
    Private Const GOOGLE_KEY = "Google_ApiKey"
    Private Const CLAUDE_KEY = "Claude_ApiKey"
    
    Shared Sub New()
        _logger = LoggerFactory.Instance.Logger
        _secureConfig = New SecureConfigurationManager(_logger)
        _logger.LogInformation("GestoreApiKeys inizializzato con configurazione sicura")
    End Sub

    Public Shared Property OpenAiKey As String
        Get
            Return GetApiKey(OPENAI_KEY)
        End Get
        Set(value As String)
            SetApiKey(OPENAI_KEY, value)
        End Set
    End Property

    Public Shared Property GoogleApiKey As String
        Get
            Return GetApiKey(GOOGLE_KEY)
        End Get
        Set(value As String)
            SetApiKey(GOOGLE_KEY, value)
        End Set
    End Property
    
    Public Shared Property ClaudeApiKey As String
        Get
            Return GetApiKey(CLAUDE_KEY)
        End Get
        Set(value As String)
            SetApiKey(CLAUDE_KEY, value)
        End Set
    End Property

    Public Shared Sub InizializzaTabella()
        ' Metodo mantenuto per compatibilità legacy, ma ora usa il nuovo sistema
        Try
            _secureConfig.InitializeEncryption()
            _logger.LogInformation("Sistema ApiKeys inizializzato")
        Catch ex As Exception
            _logger.LogError(ex, "Errore nell'inizializzazione del sistema ApiKeys")
            Throw
        End Try
    End Sub

    Public Shared Sub SalvaChiaveApi(nome As String, chiave As String)
        ' Metodo legacy - usa il nuovo sistema
        SetApiKey(nome, chiave)
    End Sub

    Public Shared Function CaricaChiaveApi(nome As String) As String
        ' Metodo legacy - usa il nuovo sistema
        Return GetApiKey(nome)
    End Function

    Public Shared Function VerificaChiaveEsiste(nome As String) As Boolean
        Return HasApiKey(nome)
    End Function
    
    Private Shared Function GetApiKey(keyName As String) As String
        Try
            Dim key = _secureConfig.GetSecureValue(keyName)
            If String.IsNullOrEmpty(key) Then
                _logger.LogWarning("API Key non configurata: {KeyName}", keyName)
                Return String.Empty
            End If
            
            ' Non loggare mai la chiave completa per sicurezza
            _logger.LogDebug("API Key recuperata per: {KeyName} (lunghezza: {Length})", keyName, key.Length)
            Return key
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel recupero API Key: {KeyName}", keyName)
            Return String.Empty
        End Try
    End Function
    
    Private Shared Sub SetApiKey(keyName As String, value As String)
        Try
            If String.IsNullOrWhiteSpace(value) Then
                _secureConfig.DeleteSecureValue(keyName)
                _logger.LogInformation("API Key eliminata: {KeyName}", keyName)
            Else
                ' Validazione base della chiave
                ValidateApiKey(keyName, value)
                
                _secureConfig.SetSecureValue(keyName, value)
                _logger.LogInformation("API Key salvata: {KeyName} (lunghezza: {Length})", keyName, value.Length)
            End If
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel salvataggio API Key: {KeyName}", keyName)
            Throw
        End Try
    End Sub
    
    Private Shared Sub ValidateApiKey(keyName As String, value As String)
        ' Validazioni specifiche per tipo di API
        Select Case keyName
            Case OPENAI_KEY
                If Not value.StartsWith("sk-") OrElse value.Length < 20 Then
                    Throw New ArgumentException("Formato OpenAI API Key non valido")
                End If
            Case GOOGLE_KEY
                If value.Length < 20 Then
                    Throw New ArgumentException("Formato Google API Key non valido")
                End If
            Case CLAUDE_KEY
                If value.Length < 20 Then
                    Throw New ArgumentException("Formato Claude API Key non valido")
                End If
        End Select
    End Sub
    
    Private Shared Function HasApiKey(keyName As String) As Boolean
        Try
            Return _secureConfig.HasValue(keyName)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nella verifica presenza API Key: {KeyName}", keyName)
            Return False
        End Try
    End Function
    
    Public Shared Function IsOpenAiConfigured() As Boolean
        Return HasApiKey(OPENAI_KEY)
    End Function
    
    Public Shared Function IsGoogleConfigured() As Boolean
        Return HasApiKey(GOOGLE_KEY)
    End Function
    
    Public Shared Function IsClaudeConfigured() As Boolean
        Return HasApiKey(CLAUDE_KEY)
    End Function
    
    Public Shared Sub ClearAllKeys()
        Try
            _secureConfig.DeleteSecureValue(OPENAI_KEY)
            _secureConfig.DeleteSecureValue(GOOGLE_KEY)
            _secureConfig.DeleteSecureValue(CLAUDE_KEY)
            _logger.LogWarning("Tutte le API Keys sono state eliminate")
        Catch ex As Exception
            _logger.LogError(ex, "Errore nell'eliminazione delle API Keys")
            Throw
        End Try
    End Sub
End Class
