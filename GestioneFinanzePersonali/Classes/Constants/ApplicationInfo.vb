Imports System.IO

''' <summary>
''' Costanti dell'applicazione per versioning e aggiornamenti
''' </summary>
Public Class ApplicationInfo
    ''' <summary>
    ''' Versione corrente dell'applicazione (Semantic Versioning)
    ''' </summary>
    Public Shared ReadOnly Property CurrentVersion As String = "1.0.0"
    
    ''' <summary>
    ''' Nome dell'applicazione
    ''' </summary>
    Public Shared ReadOnly Property ApplicationName As String = "MoneyMind"

    ''' <summary>
    ''' Repository GitHub per gli aggiornamenti
    ''' IMPORTANTE: Sostituire con il tuo username/repository
    ''' </summary>
    Public Shared ReadOnly Property GitHubRepository As String = "robertomoccia8-cmyk/MoneyMind"

    ''' <summary>
    ''' URL base delle API GitHub
    ''' </summary>
    Public Shared ReadOnly Property GitHubApiUrl As String = 
        $"https://api.github.com/repos/{GitHubRepository}/releases"
    
    ''' <summary>
    ''' Nome del file installer per gli aggiornamenti
    ''' </summary>
    Public Shared ReadOnly Property InstallerFileName As String = "MoneyMind-Setup.msi"
    
    ''' <summary>
    ''' Directory per i backup del database
    ''' </summary>
    Public Shared ReadOnly Property BackupDirectory As String = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                     ApplicationName, "Backups")
    
    ''' <summary>
    ''' Nome del database SQLite
    ''' </summary>
    Public Shared ReadOnly Property DatabaseName As String = "finanze.db"
    
    ''' <summary>
    ''' Numero massimo di backup da mantenere
    ''' </summary>
    Public Shared ReadOnly Property MaxBackups As Integer = 3
    
    ''' <summary>
    ''' Timeout per le richieste HTTP (in secondi)
    ''' </summary>
    Public Shared ReadOnly Property HttpTimeoutSeconds As Integer = 30
End Class