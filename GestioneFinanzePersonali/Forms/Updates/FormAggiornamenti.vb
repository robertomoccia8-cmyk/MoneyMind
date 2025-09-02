Imports System.Threading.Tasks
Imports GestioneFinanzePersonali.Logging
Imports GestioneFinanzePersonali.Extensions

Public Class FormAggiornamenti
    Private _updaterService As UpdaterService
    Private _backupService As BackupService
    Private _versionInfo As VersionInfo
    Private _logger As ILogger
    
    Public Sub New(versionInfo As VersionInfo)
        InitializeComponent()
        _versionInfo = versionInfo
        _logger = LoggerFactory.Instance.Logger
        _updaterService = New UpdaterService(_logger)
        _backupService = New BackupService(_logger)
        
        InitializeForm()
    End Sub
    
    Private Sub InitializeForm()
        Try
            ' Imposta le informazioni della versione
            lblCurrentVersion.Text = $"Versione corrente: {ApplicationInfo.CurrentVersion}"
            lblNewVersion.Text = $"Nuova versione: {_versionInfo.Version}"
            lblReleaseDate.Text = $"Data rilascio: {_versionInfo.ReleaseDate:dd/MM/yyyy}"
            
            ' Carica il changelog
            LoadChangelog()
            
            ' Imposta UI iniziale
            progressDownload.Visible = False
            lblProgress.Visible = False
            btnUpdate.Enabled = True
            btnCancel.Enabled = True
            
        Catch ex As Exception
            _logger.LogError("Errore durante l'inizializzazione del form aggiornamenti", ex)
            MessageBox.Show("Errore durante il caricamento delle informazioni di aggiornamento.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub LoadChangelog()
        Try
            lstChangelog.Items.Clear()
            
            If _versionInfo.Changes IsNot Nothing AndAlso _versionInfo.Changes.Count > 0 Then
                For Each change In _versionInfo.Changes
                    lstChangelog.Items.Add($"• {change}")
                Next
            Else
                lstChangelog.Items.Add("Nessun dettaglio disponibile per questa versione.")
            End If
            
        Catch ex As Exception
            _logger.LogError("Errore durante il caricamento del changelog", ex)
            lstChangelog.Items.Add("Errore nel caricamento dei dettagli.")
        End Try
    End Sub
    
    Private Async Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        Try
            ' Disabilita i controlli durante l'aggiornamento
            btnUpdate.Enabled = False
            btnCancel.Enabled = False
            progressDownload.Visible = True
            lblProgress.Visible = True
            
            ' Step 1: Crea backup del database
            lblProgress.Text = "Creazione backup del database..."
            Application.DoEvents()
            
            Dim backupPath = _backupService.CreateBackup("pre-update")
            If String.IsNullOrEmpty(backupPath) Then
                Throw New Exception("Impossibile creare il backup del database")
            End If
            
            _logger.LogInfo($"Backup creato: {backupPath}")
            
            ' Step 2: Scarica l'aggiornamento
            lblProgress.Text = "Download dell'aggiornamento in corso..."
            progressDownload.Value = 0
            Application.DoEvents()
            
            Dim installerPath = Await _updaterService.DownloadUpdateAsync(_versionInfo, 
                Sub(progress)
                    If progressDownload.InvokeRequired Then
                        progressDownload.Invoke(Sub() progressDownload.Value = progress)
                        lblProgress.Invoke(Sub() lblProgress.Text = $"Download: {progress}%")
                    Else
                        progressDownload.Value = progress
                        lblProgress.Text = $"Download: {progress}%"
                    End If
                End Sub)
            
            If String.IsNullOrEmpty(installerPath) Then
                Throw New Exception("Errore durante il download dell'aggiornamento")
            End If
            
            ' Step 3: Conferma installazione
            lblProgress.Text = "Download completato!"
            progressDownload.Value = 100
            Application.DoEvents()
            
            Dim result = MessageBox.Show(
                "Download completato!" & vbCrLf & vbCrLf & 
                "L'applicazione verrà chiusa e l'installer sarà avviato." & vbCrLf & 
                "Continuare con l'installazione?",
                "Conferma Installazione",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
            
            If result = DialogResult.Yes Then
                ' Step 4: Avvia installazione
                If _updaterService.InstallUpdate(installerPath) Then
                    ' Chiudi l'applicazione per permettere l'aggiornamento
                    Application.Exit()
                Else
                    Throw New Exception("Impossibile avviare l'installer")
                End If
            Else
                ' Ripristina UI se l'utente annulla
                ResetUI()
            End If
            
        Catch ex As Exception
            _logger.LogError("Errore durante l'aggiornamento", ex)
            
            MessageBox.Show($"Errore durante l'aggiornamento: {ex.Message}" & vbCrLf & vbCrLf & 
                          "L'applicazione rimarrà nella versione corrente.", 
                          "Errore Aggiornamento", MessageBoxButtons.OK, MessageBoxIcon.Error)
            
            ResetUI()
        End Try
    End Sub
    
    Private Sub ResetUI()
        btnUpdate.Enabled = True
        btnCancel.Enabled = True
        progressDownload.Visible = False
        lblProgress.Visible = False
    End Sub
    
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
    
    Private Sub btnShowBackups_Click(sender As Object, e As EventArgs) Handles btnShowBackups.Click
        Try
            Dim formBackups As New FormGestioneBackup()
            formBackups.ShowDialog()
        Catch ex As Exception
            _logger.LogError("Errore apertura form backup", ex)
            MessageBox.Show("Errore nell'apertura della gestione backup.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub FormAggiornamenti_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            _updaterService?.Dispose()
        Catch ex As Exception
            _logger.LogWarning("Errore durante la chiusura del form aggiornamenti", ex)
        End Try
    End Sub
End Class