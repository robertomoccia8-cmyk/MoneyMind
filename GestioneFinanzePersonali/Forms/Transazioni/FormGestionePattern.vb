Imports System.Data.SQLite
Imports System.IO

Public Class FormGestionePattern
    Inherits Form

    Private dgvPattern As DataGridView
    Private btnSalva As Button
    Private btnImportaCsv As Button
    Private btnElimina As Button
    Private btnRaffina As Button
    Private btnDashboard As Button

    Public Sub New()
        ' Costruttore
        InitializeComponent()
        AddHandler Me.Load, AddressOf FormGestionePattern_Load
    End Sub

    Private Sub InitializeComponent()
        ' Proprietà form
        Me.Text = "Gestione Pattern"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(1400, 800) ' Larghezza più ampia
        Me.BackColor = Color.White

        ' Pannello centrale per DataGridView (fill)
        Dim pnlCenter As New Panel With {
        .Dock = DockStyle.Fill,
        .Padding = New Padding(10)
    }
        Me.Controls.Add(pnlCenter)

        ' DataGridView riempie tutta l’area centrale
        dgvPattern = New DataGridView With {
        .Dock = DockStyle.Fill,
        .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        .MultiSelect = True,
        .AllowUserToAddRows = False,
        .AllowUserToDeleteRows = False,
        .ReadOnly = False,
        .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        .BackgroundColor = Color.White
    }
        pnlCenter.Controls.Add(dgvPattern)

        ' Pannello superiore con pulsanti
        Dim pnlTop As New Panel With {
        .Dock = DockStyle.Top,
        .Height = 60,
        .BackColor = Color.FromArgb(44, 62, 80),
        .Padding = New Padding(10)
    }
        Me.Controls.Add(pnlTop)

        ' Label titolo
        Dim lblTitolo As New Label With {
        .Text = "Gestione Pattern",
        .ForeColor = Color.White,
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .AutoSize = True,
        .Location = New Point(10, 15)
    }
        pnlTop.Controls.Add(lblTitolo)

        ' Pulsanti sulla stessa riga, a destra
        Dim buttonsLeft As Integer = Me.Width - 10
        Const spacing As Integer = 10

        ' Pulsante Dashboard
        btnDashboard = New Button With {
        .Text = "Gestione Set Pattern",
        .BackColor = Color.FromArgb(41, 128, 185),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Size = New Size(180, 40)
    }
        AddHandler btnDashboard.Click, Sub()
                                           Using f As New FormGestioneSetCategorie()
                                               f.ShowDialog()
                                           End Using
                                       End Sub
        pnlTop.Controls.Add(btnDashboard)

        ' Pulsante Raffina
        btnRaffina = New Button With {
        .Text = "Raffina Pattern",
        .BackColor = Color.FromArgb(41, 128, 185),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Size = New Size(140, 40)
    }
        AddHandler btnRaffina.Click, Sub()
                                         Using f As New FormRaffinaPattern()
                                             f.ShowDialog()
                                         End Using
                                         CaricaPattern()
                                     End Sub
        pnlTop.Controls.Add(btnRaffina)

        ' Pulsante Elimina
        btnElimina = New Button With {
        .Text = "Elimina Pattern",
        .BackColor = Color.FromArgb(41, 128, 185),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Size = New Size(160, 40)
    }
        AddHandler btnElimina.Click, AddressOf btnEliminaPattern_Click
        pnlTop.Controls.Add(btnElimina)

        ' Pulsante Importa CSV
        btnImportaCsv = New Button With {
        .Text = "Importa CSV",
        .BackColor = Color.FromArgb(41, 128, 185),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Size = New Size(140, 40)
    }
        AddHandler btnImportaCsv.Click, AddressOf btnImportaCsv_Click
        pnlTop.Controls.Add(btnImportaCsv)

        ' Pulsante Salva
        btnSalva = New Button With {
        .Text = "Salva Modifiche",
        .BackColor = Color.FromArgb(41, 128, 185),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Size = New Size(160, 40)
    }
        AddHandler btnSalva.Click, AddressOf btnSalva_Click
        pnlTop.Controls.Add(btnSalva)

        ' Ordine pulsanti da destra verso sinistra
        Dim rightPos As Integer = pnlTop.Width - 10
        Dim buttons As Button() = {btnSalva, btnImportaCsv, btnElimina, btnRaffina, btnDashboard}
        For Each btn As Button In buttons
            btn.Location = New Point(rightPos - btn.Width, 10)
            rightPos -= btn.Width + spacing
        Next
    End Sub

    Private Sub FormGestionePattern_Load(sender As Object, e As EventArgs)
        CaricaPattern()
        ' AggiornaPatternDaTransazioni() -> chiama questa funzione solo se hai bisogno di ripristinare le colonne Necessità Frequenza e Stagionalità cancellate per errore
    End Sub

    Private Sub CaricaPattern()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String = "
            SELECT
              ID,
              Parola,
              MacroCategoria,
              Categoria,
              COALESCE(Necessita,'')   AS Necessita,
              COALESCE(Frequenza,'')   AS Frequenza,
              COALESCE(Stagionalita,'') AS Stagionalita,
              COALESCE(Fonte,'')       AS Fonte,
              COALESCE(Peso,5)         AS Peso
            FROM Pattern
            ORDER BY MacroCategoria, Categoria, Parola;"
            Using da As New SQLiteDataAdapter(sql, conn)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvPattern.DataSource = dt
            End Using
        End Using

        For Each col As DataGridViewColumn In dgvPattern.Columns
            col.SortMode = DataGridViewColumnSortMode.Automatic
        Next
        If dgvPattern.Columns.Contains("ID") Then
            dgvPattern.Columns("ID").ReadOnly = True
        End If
    End Sub

    ''' Aggiorna retroattivamente i valori Necessita/Frequenza/Stagionalita in Pattern dal primo utilizzo in Transazioni. '''
    Private Sub AggiornaPatternDaTransazioni()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String = "
            UPDATE Pattern
            SET
              Necessita    = COALESCE((
                SELECT t.Necessita
                FROM Transazioni t
                WHERE LOWER(t.Descrizione) LIKE '%' || LOWER(Pattern.Parola) || '%'
                LIMIT 1
              ), ''),
              Frequenza    = COALESCE((
                SELECT t.Frequenza
                FROM Transazioni t
                WHERE LOWER(t.Descrizione) LIKE '%' || LOWER(Pattern.Parola) || '%'
                LIMIT 1
              ), ''),
              Stagionalita = COALESCE((
                SELECT t.Stagionalita
                FROM Transazioni t
                WHERE LOWER(t.Descrizione) LIKE '%' || LOWER(Pattern.Parola) || '%'
                LIMIT 1
              ), '');"
            Using cmd As New SQLiteCommand(sql, conn)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        CaricaPattern()
        MessageBox.Show("Pattern aggiornati dai dati di Transazioni.", "Aggiornamento", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
    Private Sub btnSalva_Click(sender As Object, e As EventArgs)
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            For Each row As DataGridViewRow In dgvPattern.Rows
                If row.IsNewRow Then Continue For
                Dim idVal = row.Cells("ID").Value
                Dim isUpdate As Boolean = (idVal IsNot Nothing AndAlso IsNumeric(idVal))

                Dim sql As String
                If isUpdate Then
                    sql = "
                    UPDATE Pattern SET
                      Parola        = @p,
                      MacroCategoria= @m,
                      Categoria     = @c,
                      Necessita     = @n,
                      Frequenza     = @f,
                      Stagionalita  = @s,
                      Fonte         = @o,
                      Peso          = @w
                    WHERE ID=@id;"
                Else
                    sql = "
                    INSERT INTO Pattern
                      (Parola,MacroCategoria,Categoria,
                       Necessita,Frequenza,Stagionalita,
                       Fonte,Peso)
                    VALUES
                      (@p,@m,@c,@n,@f,@s,@o,@w);"
                End If

                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@p", row.Cells("Parola").Value?.ToString().Trim())
                    cmd.Parameters.AddWithValue("@m", row.Cells("MacroCategoria").Value?.ToString().Trim())
                    cmd.Parameters.AddWithValue("@c", row.Cells("Categoria").Value?.ToString().Trim())
                    cmd.Parameters.AddWithValue("@n", row.Cells("Necessita").Value?.ToString().Trim())
                    cmd.Parameters.AddWithValue("@f", row.Cells("Frequenza").Value?.ToString().Trim())
                    cmd.Parameters.AddWithValue("@s", row.Cells("Stagionalita").Value?.ToString().Trim())
                    cmd.Parameters.AddWithValue("@o", row.Cells("Fonte").Value?.ToString().Trim())
                    Dim peso As Integer = 5
                    Integer.TryParse(row.Cells("Peso").Value?.ToString(), peso)
                    cmd.Parameters.AddWithValue("@w", peso)
                    If isUpdate Then
                        cmd.Parameters.AddWithValue("@id", CInt(idVal))
                    End If
                    cmd.ExecuteNonQuery()
                End Using
            Next
        End Using

        CaricaPattern()
        MessageBox.Show("Pattern salvati correttamente.", "Salva", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ' 3) Importa pattern da CSV
    Private Sub btnImportaCsv_Click(sender As Object, e As EventArgs)
        Using ofd As New OpenFileDialog() With {
        .Title = "Seleziona file CSV",
        .Filter = "File CSV (*.csv)|*.csv"
    }
            If ofd.ShowDialog() <> DialogResult.OK Then Return

            Dim lines = IO.File.ReadAllLines(ofd.FileName, System.Text.Encoding.UTF8)
            If lines.Length < 2 Then
                MessageBox.Show("File CSV vuoto o non valido.", "Importazione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim delim As Char = If(lines(0).Contains(";"c), ";"c, ","c)
            Dim headers = lines(0).Split(delim)
            Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            For i = 0 To headers.Length - 1
                idx(headers(i).Trim()) = i
            Next

            Dim countImported As Integer = 0
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                For rowIndex = 1 To lines.Length - 1
                    Dim cols = lines(rowIndex).Split(delim)
                    If cols.Length <> headers.Length Then Continue For

                    Dim idxParola = If(idx.ContainsKey("Parola"), idx("Parola"), -1)
                    Dim parola = ""
                    If idxParola >= 0 AndAlso idxParola < cols.Length Then
                        parola = cols(idxParola).Trim()
                    End If
                    If String.IsNullOrEmpty(parola) Then Continue For

                    Dim macroCat = If(idx.ContainsKey("MacroCategoria"), cols(idx("MacroCategoria")).Trim(), "")
                    Dim categoria = If(idx.ContainsKey("Categoria"), cols(idx("Categoria")).Trim(), "")
                    Dim necessita = If(idx.ContainsKey("Necessita"), cols(idx("Necessita")).Trim(), "")
                    Dim frequenza = If(idx.ContainsKey("Frequenza"), cols(idx("Frequenza")).Trim(), "")
                    Dim stagionalita = If(idx.ContainsKey("Stagionalita"), cols(idx("Stagionalita")).Trim(), "")
                    Dim fonte = If(idx.ContainsKey("Fonte"), cols(idx("Fonte")).Trim(), "Applicazione")
                    Dim peso As Integer = 5
                    If idx.ContainsKey("Peso") Then Integer.TryParse(cols(idx("Peso")).Trim(), peso)

                    Dim sql As String = "
                    INSERT OR IGNORE INTO Pattern
                      (Parola,MacroCategoria,Categoria,
                       Necessita,Frequenza,Stagionalita,Fonte,Peso)
                    VALUES
                      (@p,@m,@c,@n,@f,@s,@o,@w);"
                    Using cmd As New SQLiteCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@p", parola)
                        cmd.Parameters.AddWithValue("@m", macroCat)
                        cmd.Parameters.AddWithValue("@c", categoria)
                        cmd.Parameters.AddWithValue("@n", necessita)
                        cmd.Parameters.AddWithValue("@f", frequenza)
                        cmd.Parameters.AddWithValue("@s", stagionalita)
                        cmd.Parameters.AddWithValue("@o", fonte)
                        cmd.Parameters.AddWithValue("@w", peso)
                        countImported += cmd.ExecuteNonQuery()
                    End Using
                Next
            End Using

            MessageBox.Show($"Importazione completata. {countImported} record importati.", "Import CSV", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CaricaPattern()
        End Using
    End Sub


    ' Metodo per eliminare uno o più pattern selezionati
    Private Sub btnEliminaPattern_Click(sender As Object, e As EventArgs)
        Dim toDeleteIds As New List(Of Integer)
        For Each row As DataGridViewRow In dgvPattern.SelectedRows
            toDeleteIds.Add(Convert.ToInt32(row.Cells("ID").Value))
        Next
        If toDeleteIds.Count = 0 Then
            MessageBox.Show("Seleziona almeno un pattern da eliminare.", "Elimina", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If MessageBox.Show($"Confermi eliminazione di {toDeleteIds.Count} pattern?", "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd As New SQLiteCommand("DELETE FROM Pattern WHERE ID = @id", conn)
                Dim param = cmd.Parameters.Add("@id", DbType.Int32)
                For Each idVal In toDeleteIds
                    param.Value = idVal
                    cmd.ExecuteNonQuery()
                Next
            End Using
        End Using

        MessageBox.Show($"{toDeleteIds.Count} pattern eliminati.", "Elimina", MessageBoxButtons.OK, MessageBoxIcon.Information)
        CaricaPattern()
    End Sub


End Class
