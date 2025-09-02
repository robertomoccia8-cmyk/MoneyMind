Imports System.Data.SQLite

Public Class FormUnisciSet

    Private setOrigine As FormGestioneSetCategorie.SetCategoria
    Private setList As List(Of FormGestioneSetCategorie.SetCategoria)
    Private setDestinazione As FormGestioneSetCategorie.SetCategoria
    Private setMap As New Dictionary(Of String, FormGestioneSetCategorie.SetCategoria)

    Public Sub New(tuttiiSet As List(Of FormGestioneSetCategorie.SetCategoria), setDaUnire As FormGestioneSetCategorie.SetCategoria)
        InitializeComponent()
        setList = tuttiiSet
        setOrigine = setDaUnire
    End Sub

    Private Sub FormUnisciSet_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Mostra info set origine
        lblInfoOrigine.Text = $"{setOrigine.NomeCompleto}" & vbCrLf &
                             $"Pattern: {setOrigine.NumeroPattern}, Transazioni: {setOrigine.NumeroTransazioni}" & vbCrLf &
                             $"Parole: {String.Join(", ", setOrigine.Parole.Take(8))}{If(setOrigine.Parole.Count > 8, "...", "")}"

        ' Popola combo con altri set (approccio alternativo)
        cmbDestinazione.Items.Clear()
        setMap.Clear()

        For Each sc In setList.Where(Function(s) s.ChiaveSet <> setOrigine.ChiaveSet).OrderBy(Function(s) s.NomeCompleto)
            cmbDestinazione.Items.Add(sc.NomeCompleto)
            setMap(sc.NomeCompleto) = sc
        Next

        If cmbDestinazione.Items.Count = 0 Then
            lblInfoDestinazione.Text = "Nessun altro set disponibile per l'unione."
            btnOK.Enabled = False
        End If
    End Sub

    Private Sub cmbDestinazione_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDestinazione.SelectedIndexChanged
        If cmbDestinazione.SelectedItem IsNot Nothing Then
            Dim nomeSet = cmbDestinazione.SelectedItem.ToString()
            setDestinazione = setMap(nomeSet)

            lblInfoDestinazione.Text = $"{setDestinazione.NomeCompleto}" & vbCrLf &
                                      $"Pattern: {setDestinazione.NumeroPattern}, Transazioni: {setDestinazione.NumeroTransazioni}" & vbCrLf &
                                      $"Parole: {String.Join(", ", setDestinazione.Parole.Take(8))}{If(setDestinazione.Parole.Count > 8, "...", "")}"

            btnOK.Enabled = True
        Else
            lblInfoDestinazione.Text = "Seleziona un set di destinazione..."
            btnOK.Enabled = False
        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If setDestinazione Is Nothing Then
            MessageBox.Show("Seleziona un set di destinazione!", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim conferma = MessageBox.Show(
        $"Confermi l'unione di:" & vbCrLf & vbCrLf &
        $"DA: {setOrigine.NomeCompleto} ({setOrigine.NumeroPattern} pattern, {setOrigine.NumeroTransazioni} transazioni)" & vbCrLf &
        $"A: {setDestinazione.NomeCompleto} ({setDestinazione.NumeroPattern} pattern, {setDestinazione.NumeroTransazioni} transazioni)" & vbCrLf & vbCrLf &
        "Il set di origine verrà eliminato definitivamente.",
        "Conferma Unione", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If conferma <> DialogResult.Yes Then Return

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    ' Sposta pattern da origine a destinazione - RIMUOVI SottoCategoria
                    Dim sqlPattern As String = "UPDATE Pattern SET MacroCategoria = @nuovaMacro, Categoria = @nuovaCat WHERE MacroCategoria = @vecchiaMacro AND Categoria = @vecchiaCat"
                    Using cmd As New SQLiteCommand(sqlPattern, conn, trans)
                        cmd.Parameters.AddWithValue("@nuovaMacro", setDestinazione.MacroCategoria)
                        cmd.Parameters.AddWithValue("@nuovaCat", setDestinazione.Categoria)
                        cmd.Parameters.AddWithValue("@vecchiaMacro", setOrigine.MacroCategoria)
                        cmd.Parameters.AddWithValue("@vecchiaCat", setOrigine.Categoria)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Sposta transazioni da origine a destinazione - RIMUOVI SottoCategoria
                    Dim sqlTrans As String = "UPDATE Transazioni SET MacroCategoria = @nuovaMacro, Categoria = @nuovaCat WHERE MacroCategoria = @vecchiaMacro AND Categoria = @vecchiaCat"
                    Using cmd As New SQLiteCommand(sqlTrans, conn, trans)
                        cmd.Parameters.AddWithValue("@nuovaMacro", setDestinazione.MacroCategoria)
                        cmd.Parameters.AddWithValue("@nuovaCat", setDestinazione.Categoria)
                        cmd.Parameters.AddWithValue("@vecchiaMacro", setOrigine.MacroCategoria)
                        cmd.Parameters.AddWithValue("@vecchiaCat", setOrigine.Categoria)
                        cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                End Using
            End Using

            MessageBox.Show("Set uniti con successo!", "Unione Completata", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("Errore durante l'unione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
