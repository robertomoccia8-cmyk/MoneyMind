Imports System.IO
Imports GestioneFinanzePersonali.Logging
Imports GestioneFinanzePersonali.Extensions

Public Class FormGestioneBackup
    Private _backupService As BackupService
    Private _logger As ILogger
    
    Public Sub New()
        InitializeComponent()
        _logger = LoggerFactory.Instance.Logger
        _backupService = New BackupService(_logger)
        
        LoadBackups()
    End Sub
    
    Private Sub LoadBackups()
        Try
            lvBackups.Items.Clear()
            
            Dim backups = _backupService.GetAvailableBackups()
            
            For Each backup In backups
                Dim item As New ListViewItem(backup.Name)
                item.SubItems.Add(backup.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss"))
                item.SubItems.Add(backup.SizeFormatted)
                item.SubItems.Add(backup.Reason)
                item.Tag = backup
                
                lvBackups.Items.Add(item)
            Next
            
            If backups.Count = 0 Then
                Dim noBackupItem As New ListViewItem("Nessun backup disponibile")
                noBackupItem.SubItems.Add("")
                noBackupItem.SubItems.Add("")
                noBackupItem.SubItems.Add("")
                lvBackups.Items.Add(noBackupItem)
            End If
            
            UpdateButtons()
            
        Catch ex As Exception
            _logger.LogError("Errore durante il caricamento dei backup", ex)
            MessageBox.Show("Errore durante il caricamento dei backup.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub UpdateButtons()
        Dim hasSelection = lvBackups.SelectedItems.Count > 0 AndAlso 
                          lvBackups.SelectedItems(0).Tag IsNot Nothing
        
        btnRestore.Enabled = hasSelection
        btnDelete.Enabled = hasSelection
    End Sub
    
    Private Sub btnCreateBackup_Click(sender As Object, e As EventArgs) Handles btnCreateBackup.Click
        Try
            Dim result = MessageBox.Show("Creare un backup manuale del database?", 
                                       "Conferma Backup", 
                                       MessageBoxButtons.YesNo, 
                                       MessageBoxIcon.Question)
            
            If result = DialogResult.Yes Then
                Dim backupPath = _backupService.CreateBackup("manual")
                
                If Not String.IsNullOrEmpty(backupPath) Then
                    MessageBox.Show("Backup creato con successo!", 
                                  "Backup Completato", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information)
                    LoadBackups()
                Else
                    MessageBox.Show("Errore durante la creazione del backup.", 
                                  "Errore", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Error)
                End If
            End If
            
        Catch ex As Exception
            _logger.LogError("Errore durante la creazione del backup", ex)
            MessageBox.Show("Errore durante la creazione del backup.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub btnRestore_Click(sender As Object, e As EventArgs) Handles btnRestore.Click
        Try
            If lvBackups.SelectedItems.Count = 0 OrElse 
               lvBackups.SelectedItems(0).Tag Is Nothing Then
                Return
            End If
            
            Dim backup = CType(lvBackups.SelectedItems(0).Tag, BackupInfo)
            
            Dim result = MessageBox.Show(
                $"ATTENZIONE: Il ripristino sostituirà completamente il database corrente!" & vbCrLf & vbCrLf & 
                $"Backup selezionato: {backup.Name}" & vbCrLf & 
                $"Data: {backup.CreatedDate:dd/MM/yyyy HH:mm:ss}" & vbCrLf & vbCrLf & 
                "Continuare con il ripristino?",
                "Conferma Ripristino",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)
            
            If result = DialogResult.Yes Then
                If _backupService.RestoreBackup(backup.FilePath) Then
                    MessageBox.Show("Backup ripristinato con successo!" & vbCrLf & 
                                  "È consigliabile riavviare l'applicazione.", 
                                  "Ripristino Completato", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information)
                Else
                    MessageBox.Show("Errore durante il ripristino del backup.", 
                                  "Errore", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Error)
                End If
            End If
            
        Catch ex As Exception
            _logger.LogError("Errore durante il ripristino del backup", ex)
            MessageBox.Show("Errore durante il ripristino del backup.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Try
            If lvBackups.SelectedItems.Count = 0 OrElse 
               lvBackups.SelectedItems(0).Tag Is Nothing Then
                Return
            End If
            
            Dim backup = CType(lvBackups.SelectedItems(0).Tag, BackupInfo)
            
            Dim result = MessageBox.Show($"Eliminare definitivamente il backup '{backup.Name}'?", 
                                       "Conferma Eliminazione", 
                                       MessageBoxButtons.YesNo, 
                                       MessageBoxIcon.Question)
            
            If result = DialogResult.Yes Then
                File.Delete(backup.FilePath)
                _logger.LogInfo($"Backup eliminato: {backup.FilePath}")
                
                MessageBox.Show("Backup eliminato con successo!", 
                              "Eliminazione Completata", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Information)
                
                LoadBackups()
            End If
            
        Catch ex As Exception
            _logger.LogError("Errore durante l'eliminazione del backup", ex)
            MessageBox.Show("Errore durante l'eliminazione del backup.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub btnOpenBackupFolder_Click(sender As Object, e As EventArgs) Handles btnOpenBackupFolder.Click
        Try
            If Not Directory.Exists(ApplicationInfo.BackupDirectory) Then
                Directory.CreateDirectory(ApplicationInfo.BackupDirectory)
            End If
            
            Process.Start("explorer.exe", ApplicationInfo.BackupDirectory)
            
        Catch ex As Exception
            _logger.LogError("Errore apertura cartella backup", ex)
            MessageBox.Show("Impossibile aprire la cartella dei backup.", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
    
    Private Sub lvBackups_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvBackups.SelectedIndexChanged
        UpdateButtons()
    End Sub
    
    Private Sub lvBackups_DoubleClick(sender As Object, e As EventArgs) Handles lvBackups.DoubleClick
        If btnRestore.Enabled Then
            btnRestore_Click(sender, e)
        End If
    End Sub
End Class