Imports System.Data.SQLite

Public Class FormNuovoSet

    Private Sub FormNuovoSet_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        txtMacroCategoria.Focus()
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If ValidaCampi() Then
            If CreaSet() Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If
        End If
    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Function ValidaCampi() As Boolean
        If String.IsNullOrWhiteSpace(txtMacroCategoria.Text) Then
            MessageBox.Show("Inserisci la Macro Categoria!", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtMacroCategoria.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtCategoria.Text) Then
            MessageBox.Show("Inserisci la Categoria!", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtCategoria.Focus()
            Return False
        End If

        ' RIMUOVI validazione SottoCategoria

        If String.IsNullOrWhiteSpace(txtParole.Text) Then
            MessageBox.Show("Inserisci almeno una parola per il pattern!", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtParole.Focus()
            Return False
        End If

        Return True
    End Function

    Private Function CreaSet() As Boolean
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    ' Verifica se il set esiste già
                    Dim sqlCheck As String = "SELECT COUNT(*) FROM Pattern WHERE MacroCategoria = @macro AND Categoria = @cat"
                    Using cmdCheck As New SQLiteCommand(sqlCheck, conn, trans)
                        cmdCheck.Parameters.AddWithValue("@macro", txtMacroCategoria.Text.Trim())
                        cmdCheck.Parameters.AddWithValue("@cat", txtCategoria.Text.Trim())

                        If Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0 Then
                            If MessageBox.Show("Il set esiste già. Vuoi aggiungere le nuove parole a quello esistente?",
                                             "Set Esistente", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                                Return False
                            End If
                        End If
                    End Using

                    ' Inserisci le parole
                    Dim parole = txtParole.Text.Split(","c).Select(Function(p) p.Trim()).Where(Function(p) Not String.IsNullOrEmpty(p))
                    Dim sqlInsert As String = "INSERT OR IGNORE INTO Pattern (Parola, MacroCategoria, Categoria, Peso, Fonte) VALUES (@parola, @macro, @cat, @peso, @fonte)"

                    For Each parola In parole
                        Using cmd As New SQLiteCommand(sqlInsert, conn, trans)
                            cmd.Parameters.AddWithValue("@parola", parola.ToLower())
                            cmd.Parameters.AddWithValue("@macro", txtMacroCategoria.Text.Trim())
                            cmd.Parameters.AddWithValue("@cat", txtCategoria.Text.Trim())
                            cmd.Parameters.AddWithValue("@peso", 5) ' Peso medio di default
                            cmd.Parameters.AddWithValue("@fonte", "Manuale")
                            cmd.ExecuteNonQuery()
                        End Using
                    Next

                    trans.Commit()
                    MessageBox.Show("Set creato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return True
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Errore durante la creazione del set: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub txtParole_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtParole.KeyPress
        ' Allow enter for new line
        If e.KeyChar = Chr(13) Then
            e.Handled = False
        End If
    End Sub

End Class