Imports System.IO
Imports System.Text.Json

Public Module PatternOptionsManager

    Private ReadOnly configPath As String =
        Path.Combine(Application.StartupPath, "App_Data", "pattern_options.json")

    Public Class OptionsSet
        Public Property Necessita   As List(Of String)
        Public Property Frequenza   As List(Of String)
        Public Property Stagionalita As List(Of String)
    End Class

    Public Sub LoadOptions(ByRef necessityOptions As List(Of String),
                           ByRef frequencyOptions As List(Of String),
                           ByRef seasonOptions    As List(Of String))
        Try
            If Not Directory.Exists(Path.GetDirectoryName(configPath)) Then
                Directory.CreateDirectory(Path.GetDirectoryName(configPath))
            End If

            If File.Exists(configPath) Then
                Dim json = File.ReadAllText(configPath)
                Dim opts = JsonSerializer.Deserialize(Of OptionsSet)(json)
                necessityOptions = opts.Necessita
                frequencyOptions = opts.Frequenza
                seasonOptions    = opts.Stagionalita
            End If
        Catch
            ' Se fallisce, mantieni le opzioni di default
        End Try
    End Sub

    Public Sub SaveOptions(necessityOptions As List(Of String),
                           frequencyOptions As List(Of String),
                           seasonOptions    As List(Of String))
        Try
            Dim opts As New OptionsSet With {
                .Necessita   = necessityOptions,
                .Frequenza   = frequencyOptions,
                .Stagionalita = seasonOptions
            }
            Dim json = JsonSerializer.Serialize(opts, New JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(configPath, json)
        Catch ex As Exception
            MessageBox.Show("Errore salvataggio opzioni: " & ex.Message)
        End Try
    End Sub

End Module
