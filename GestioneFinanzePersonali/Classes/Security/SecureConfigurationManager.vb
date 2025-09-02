Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports Newtonsoft.Json
Imports GestioneFinanzePersonali.Logging

Namespace Security
    Public Class SecureConfigurationManager
    Implements ISecureConfigurationManager
    
    Private ReadOnly _configPath As String
    Private ReadOnly _logger As ILogger
    Private _encryptionKey As Byte()
    Private _encryptedConfig As Dictionary(Of String, String)
    
    Public Sub New(logger As ILogger, Optional configPath As String = Nothing)
        _logger = logger
        _configPath = If(configPath, Path.Combine(Application.StartupPath, "config.enc"))
        _encryptedConfig = New Dictionary(Of String, String)()
        InitializeEncryption()
        LoadConfiguration()
    End Sub
    
    Public Sub InitializeEncryption() Implements ISecureConfigurationManager.InitializeEncryption
        Try
            ' Genera una chiave basata sull'identità della macchina per maggiore sicurezza
            Dim machineKey = Environment.MachineName + Environment.UserName + Environment.ProcessorCount.ToString()
            
            Using sha256 As SHA256 = SHA256.Create()
                _encryptionKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineKey))
            End Using
            
            _logger.LogInformation("Sistema di crittografia inizializzato")
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nell'inizializzazione del sistema di crittografia")
            Throw
        End Try
    End Sub
    
    Public Function GetSecureValue(key As String) As String Implements ISecureConfigurationManager.GetSecureValue
        Try
            If String.IsNullOrWhiteSpace(key) Then
                Throw New ArgumentException("La chiave non può essere vuota", NameOf(key))
            End If
            
            If Not _encryptedConfig.ContainsKey(key) Then
                _logger.LogWarning("Chiave di configurazione non trovata: {Key}", key)
                Return Nothing
            End If
            
            Dim encryptedValue = _encryptedConfig(key)
            Return DecryptString(encryptedValue)
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel recupero del valore sicuro per la chiave: {Key}", key)
            Return Nothing
        End Try
    End Function
    
    Public Sub SetSecureValue(key As String, value As String) Implements ISecureConfigurationManager.SetSecureValue
        Try
            If String.IsNullOrWhiteSpace(key) Then
                Throw New ArgumentException("La chiave non può essere vuota", NameOf(key))
            End If
            
            If String.IsNullOrEmpty(value) Then
                DeleteSecureValue(key)
                Return
            End If
            
            Dim encryptedValue = EncryptString(value)
            _encryptedConfig(key) = encryptedValue
            
            SaveConfiguration()
            _logger.LogInformation("Valore sicuro salvato per la chiave: {Key}", key)
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel salvataggio del valore sicuro per la chiave: {Key}", key)
            Throw
        End Try
    End Sub
    
    Public Sub DeleteSecureValue(key As String) Implements ISecureConfigurationManager.DeleteSecureValue
        Try
            If String.IsNullOrWhiteSpace(key) Then
                Return
            End If
            
            If _encryptedConfig.ContainsKey(key) Then
                _encryptedConfig.Remove(key)
                SaveConfiguration()
                _logger.LogInformation("Valore sicuro eliminato per la chiave: {Key}", key)
            End If
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nell'eliminazione del valore sicuro per la chiave: {Key}", key)
            Throw
        End Try
    End Sub
    
    Public Function HasValue(key As String) As Boolean Implements ISecureConfigurationManager.HasValue
        Return Not String.IsNullOrWhiteSpace(key) AndAlso _encryptedConfig.ContainsKey(key)
    End Function
    
    Private Function EncryptString(plainText As String) As String
        If String.IsNullOrEmpty(plainText) Then
            Return String.Empty
        End If
        
        Using aes As Aes = Aes.Create()
            aes.Key = _encryptionKey
            aes.GenerateIV()
            
            Using encryptor = aes.CreateEncryptor()
                Using msEncrypt As New MemoryStream()
                    ' Salva l'IV all'inizio
                    msEncrypt.Write(aes.IV, 0, aes.IV.Length)
                    
                    Using csEncrypt As New CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)
                        Using swEncrypt As New StreamWriter(csEncrypt)
                            swEncrypt.Write(plainText)
                        End Using
                    End Using
                    
                    Return Convert.ToBase64String(msEncrypt.ToArray())
                End Using
            End Using
        End Using
    End Function
    
    Private Function DecryptString(cipherText As String) As String
        If String.IsNullOrEmpty(cipherText) Then
            Return String.Empty
        End If
        
        Dim fullCipher = Convert.FromBase64String(cipherText)
        
        Using aes As Aes = Aes.Create()
            aes.Key = _encryptionKey
            
            ' Estrai l'IV dai primi 16 bytes
            Dim iv(15) As Byte
            Array.Copy(fullCipher, iv, iv.Length)
            aes.IV = iv
            
            Using decryptor = aes.CreateDecryptor()
                Using msDecrypt As New MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length)
                    Using csDecrypt As New CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)
                        Using srDecrypt As New StreamReader(csDecrypt)
                            Return srDecrypt.ReadToEnd()
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Function
    
    Private Sub LoadConfiguration()
        Try
            If Not File.Exists(_configPath) Then
                _logger.LogInformation("File di configurazione non trovato, verrà creato al primo salvataggio")
                Return
            End If
            
            Dim encryptedContent = File.ReadAllText(_configPath, Encoding.UTF8)
            If String.IsNullOrWhiteSpace(encryptedContent) Then
                Return
            End If
            
            _encryptedConfig = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(encryptedContent)
            _logger.LogInformation("Configurazione caricata con successo")
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel caricamento della configurazione")
            _encryptedConfig = New Dictionary(Of String, String)()
        End Try
    End Sub
    
    Private Sub SaveConfiguration()
        Try
            Dim jsonContent = JsonConvert.SerializeObject(_encryptedConfig, Formatting.Indented)
            
            ' Assicurati che la directory esista
            Dim directoryPath = Path.GetDirectoryName(_configPath)
            If Not String.IsNullOrEmpty(directoryPath) AndAlso Not Directory.Exists(directoryPath) Then
                Directory.CreateDirectory(directoryPath)
            End If
            
            ' Salvataggio atomico
            Dim tempPath = _configPath + ".tmp"
            File.WriteAllText(tempPath, jsonContent, Encoding.UTF8)
            
            If File.Exists(_configPath) Then
                File.Replace(tempPath, _configPath, Nothing)
            Else
                File.Move(tempPath, _configPath)
            End If
            
            _logger.LogInformation("Configurazione salvata con successo")
            
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel salvataggio della configurazione")
            Throw
        End Try
    End Sub
    End Class
End Namespace