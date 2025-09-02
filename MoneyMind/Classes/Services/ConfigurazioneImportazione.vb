Imports System.IO
Imports System.Text

Public Class ConfigurazioneImportazione
    Public Property Nome As String
    Public Property RigaIntestazione As Integer
    Public Property MappingColonne As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)
    Public Property DataCreazione As DateTime
    Public Property UltimoUtilizzo As DateTime

    Public Sub New()
        MappingColonne = New Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)
        DataCreazione = DateTime.Now
        UltimoUtilizzo = DateTime.Now
    End Sub
End Class

Public Class GestoreConfigurazioni
    Private Shared ReadOnly CARTELLA_CONFIG As String = Path.Combine(Application.StartupPath, "ConfigurazioniImportazione")

    Shared Sub New()
        If Not Directory.Exists(CARTELLA_CONFIG) Then
            Directory.CreateDirectory(CARTELLA_CONFIG)
        End If
    End Sub

    Public Shared Sub SalvaConfigurazione(config As ConfigurazioneImportazione)
        Try
            Dim nomeFile = $"{SanitizeFileName(config.Nome)}.json"
            Dim percorso = Path.Combine(CARTELLA_CONFIG, nomeFile)

            Dim json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented)
            File.WriteAllText(percorso, json, Encoding.UTF8)
        Catch ex As Exception
            MessageBox.Show($"Errore nel salvataggio configurazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Shared Function CaricaConfigurazione(nome As String) As ConfigurazioneImportazione
        Try
            Dim nomeFile = $"{SanitizeFileName(nome)}.json"
            Dim percorso = Path.Combine(CARTELLA_CONFIG, nomeFile)

            If File.Exists(percorso) Then
                Dim json = File.ReadAllText(percorso, Encoding.UTF8)
                Dim config = Newtonsoft.Json.JsonConvert.DeserializeObject(Of ConfigurazioneImportazione)(json)
                config.UltimoUtilizzo = DateTime.Now
                SalvaConfigurazione(config) ' Aggiorna ultimo utilizzo
                Return config
            End If
        Catch ex As Exception
            MessageBox.Show($"Errore nel caricamento configurazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        Return Nothing
    End Function

    Public Shared Function GetConfigurazioni() As List(Of String)
        Try
            Return Directory.GetFiles(CARTELLA_CONFIG, "*.json").
                   Select(Function(f) Path.GetFileNameWithoutExtension(f)).
                   OrderBy(Function(n) n).ToList()
        Catch
            Return New List(Of String)
        End Try
    End Function

    Public Shared Sub EliminaConfigurazione(nome As String)
        Try
            Dim nomeFile = $"{SanitizeFileName(nome)}.json"
            Dim percorso = Path.Combine(CARTELLA_CONFIG, nomeFile)
            If File.Exists(percorso) Then
                File.Delete(percorso)
            End If
        Catch ex As Exception
            MessageBox.Show($"Errore nell'eliminazione configurazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Function SanitizeFileName(nome As String) As String
        Dim caratteriNonValidi = Path.GetInvalidFileNameChars()
        For Each c In caratteriNonValidi
            nome = nome.Replace(c, "_")
        Next
        Return nome
    End Function
End Class
