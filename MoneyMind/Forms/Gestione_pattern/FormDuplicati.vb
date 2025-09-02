Imports System.Data.SQLite

Public Class FormDuplicati

    Private duplicati As List(Of (Set1 As FormGestioneSetCategorie.SetCategoria, Set2 As FormGestioneSetCategorie.SetCategoria, Similitudine As Double))

    Public Sub New(duplicatiTrovati As List(Of (Set1 As FormGestioneSetCategorie.SetCategoria, Set2 As FormGestioneSetCategorie.SetCategoria, Similitudine As Double)))
        InitializeComponent()
        duplicati = duplicatiTrovati
    End Sub

    Private Sub FormDuplicati_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ImpostaDgv()
        CaricaDuplicati()
    End Sub

    Private Sub ImpostaDgv()
        dgvDuplicati.Columns.AddRange({
            New DataGridViewCheckBoxColumn With {.Name = "Seleziona", .HeaderText = "✓", .Width = 50},
            New DataGridViewTextBoxColumn With {.Name = "Set1", .HeaderText = "Primo Set"},
            New DataGridViewTextBoxColumn With {.Name = "Set2", .HeaderText = "Secondo Set"},
            New DataGridViewTextBoxColumn With {.Name = "Similitudine", .HeaderText = "% Simil.", .Width = 80},
            New DataGridViewTextBoxColumn With {.Name = "Pattern1", .HeaderText = "Pattern Set 1", .Width = 60},
            New DataGridViewTextBoxColumn With {.Name = "Pattern2", .HeaderText = "Pattern Set 2", .Width = 60},
            New DataGridViewTextBoxColumn With {.Name = "Trans1", .HeaderText = "Trans. 1", .Width = 60},
            New DataGridViewTextBoxColumn With {.Name = "Trans2", .HeaderText = "Trans. 2", .Width = 60}
        })
    End Sub

    Private Sub CaricaDuplicati()
        dgvDuplicati.Rows.Clear()

        For Each dup In duplicati.OrderByDescending(Function(d) d.Similitudine)
            dgvDuplicati.Rows.Add(
                False, ' Checkbox non selezionato di default
                dup.Set1.NomeCompleto,
                dup.Set2.NomeCompleto,
                $"{dup.Similitudine:P1}",
                dup.Set1.NumeroPattern,
                dup.Set2.NumeroPattern,
                dup.Set1.NumeroTransazioni,
                dup.Set2.NumeroTransazioni
            )
        Next
    End Sub

    Private Sub btnUnisciSelezionati_Click(sender As Object, e As EventArgs) Handles btnUnisciSelezionati.Click
        Dim selezionati As New List(Of Integer)

        For i = 0 To dgvDuplicati.Rows.Count - 1
            Dim isSelected = Convert.ToBoolean(dgvDuplicati.Rows(i).Cells("Seleziona").Value)
            If isSelected Then
                selezionati.Add(i)
            End If
        Next

        If selezionati.Count = 0 Then
            MessageBox.Show("Seleziona almeno una coppia di duplicati da unire!", "Selezione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        For Each index In selezionati
            Dim dup = duplicati(index)
            UnisciDueSet(dup.Set1, dup.Set2)
        Next

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub UnisciDueSet(set1 As FormGestioneSetCategorie.SetCategoria, set2 As FormGestioneSetCategorie.SetCategoria)
        ' Mostra dialog per scegliere quale set mantenere
        Using frmScelta As New FormSceltaSet(set1, set2)
            If frmScelta.ShowDialog() = DialogResult.OK Then
                Dim setDestinazione = frmScelta.SetSelezionato
                Dim setDaEliminare = If(setDestinazione.ChiaveSet = set1.ChiaveSet, set2, set1)

                Try
                    Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                        conn.Open()
                        Using trans = conn.BeginTransaction()

                            ' Sposta pattern da set da eliminare a set destinazione
                            Dim sqlPattern As String = "UPDATE Pattern SET MacroCategoria = @nuovaMacro, Categoria = @nuovaCat WHERE MacroCategoria = @vecchiaMacro AND Categoria = @vecchiaCat"
                            Using cmd As New SQLiteCommand(sqlPattern, conn, trans)
                                cmd.Parameters.AddWithValue("@nuovaMacro", setDestinazione.MacroCategoria)
                                cmd.Parameters.AddWithValue("@nuovaCat", setDestinazione.Categoria)
                                cmd.Parameters.AddWithValue("@vecchiaMacro", setDaEliminare.MacroCategoria)
                                cmd.Parameters.AddWithValue("@vecchiaCat", setDaEliminare.Categoria)
                                cmd.ExecuteNonQuery()
                            End Using

                            ' Sposta transazioni
                            Dim sqlTrans As String = "UPDATE Transazioni SET MacroCategoria = @nuovaMacro, Categoria = @nuovaCat WHERE MacroCategoria = @vecchiaMacro AND Categoria = @vecchiaCat"
                            Using cmd As New SQLiteCommand(sqlTrans, conn, trans)
                                cmd.Parameters.AddWithValue("@nuovaMacro", setDestinazione.MacroCategoria)
                                cmd.Parameters.AddWithValue("@nuovaCat", setDestinazione.Categoria)
                                cmd.Parameters.AddWithValue("@vecchiaMacro", setDaEliminare.MacroCategoria)
                                cmd.Parameters.AddWithValue("@vecchiaCat", setDaEliminare.Categoria)
                                cmd.ExecuteNonQuery()
                            End Using

                            trans.Commit()
                        End Using
                    End Using

                Catch ex As Exception
                    MessageBox.Show("Errore durante l'unione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub btnIgnora_Click(sender As Object, e As EventArgs) Handles btnIgnora.Click
        ' Rimuovi le righe selezionate dalla lista (ignora i duplicati)
        For i = dgvDuplicati.Rows.Count - 1 To 0 Step -1
            Dim isSelected = Convert.ToBoolean(dgvDuplicati.Rows(i).Cells("Seleziona").Value)
            If isSelected Then
                dgvDuplicati.Rows.RemoveAt(i)
                duplicati.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub btnChiudi_Click(sender As Object, e As EventArgs) Handles btnChiudi.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
