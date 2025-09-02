Imports System.IO
Imports OfficeOpenXml

Public Class FormAnteprimaFile
    Inherits Form

    Private percorsoFile As String
    Private delimiter As String
    Private rigaIntestazioneScelta As Integer = 1

    Private pnlControlli As Panel
    Private pnlCenter As Panel
    Private dgvAnteprima As DataGridView
    Private numRigaIntestazione As NumericUpDown

    Public ReadOnly Property RigaIntestazione As Integer
        Get
            Return rigaIntestazioneScelta
        End Get
    End Property

    Public Sub New(percorso As String, analisi As ImportatoreUniversale.RisultatoAnalisi)
        MyBase.New()
        Me.percorsoFile = percorso
        Me.Text = $"Anteprima - {Path.GetFileName(percorso)}"
        Me.Size = New Size(1000, 600)
        Me.StartPosition = FormStartPosition.CenterScreen

        ' 1. Crea prima il pannello centrale
        CreaPannelloCenter()
        ' 2. Poi crea il pannello controlli (dock top)
        CreaPannelloControlli()

        Dim ext = Path.GetExtension(percorso).ToLower()
        If ext = ".csv" OrElse ext = ".txt" Then
            delimiter = analisi.DelimitatoreRilevato
            CaricaAnteprimaCsv(analisi)
        ElseIf ext <> ".pdf" Then
            CaricaAnteprimaExcel()
        End If
    End Sub

    '
    ' PANNELLO CENTRALE 
    '
    Private Sub CreaPannelloCenter()
        pnlCenter = New Panel() With {.Dock = DockStyle.Fill}
        dgvAnteprima = New DataGridView() With {
        .Dock = DockStyle.Fill,
        .ReadOnly = True,
        .AllowUserToAddRows = False,
        .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    }
        pnlCenter.Controls.Add(dgvAnteprima)
        Me.Controls.Add(pnlCenter)
    End Sub

    '
    ' PANNELLO CONTROLLI IN ALTO
    '
    Private Sub CreaPannelloControlli()
        pnlControlli = New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 80,
            .BackColor = Color.FromArgb(240, 248, 255),
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblInfo As New Label() With {
            .Text = "Seleziona la riga che contiene le intestazioni delle colonne:",
            .Location = New Point(20, 15),
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .AutoSize = True
        }
        pnlControlli.Controls.Add(lblInfo)

        Dim lblRiga As New Label() With {
            .Text = "Riga intestazioni:",
            .Location = New Point(20, 45),
            .Font = New Font("Segoe UI", 9),
            .AutoSize = True
        }
        pnlControlli.Controls.Add(lblRiga)

        numRigaIntestazione = New NumericUpDown() With {
            .Name = "numRigaIntestazione",
            .Location = New Point(140, 42),
            .Size = New Size(80, 25),
            .Minimum = 1,
            .Maximum = 1000,
            .Value = 1
        }
        AddHandler numRigaIntestazione.ValueChanged, AddressOf NumRigaIntestazione_ValueChanged
        pnlControlli.Controls.Add(numRigaIntestazione)

        Dim btnConferma As New Button() With {
            .Text = "✓ Conferma e Continua",
            .Location = New Point(800, 20),
            .Size = New Size(180, 35),
            .BackColor = Color.FromArgb(40, 180, 100),
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .FlatStyle = FlatStyle.Flat,
            .DialogResult = DialogResult.OK
        }
        pnlControlli.Controls.Add(btnConferma)

        Me.Controls.Add(pnlControlli)
    End Sub



    Private Sub CaricaAnteprimaExcel()
        Using pkg As New ExcelPackage(New FileInfo(percorsoFile))
            Dim ws = pkg.Workbook.Worksheets.FirstOrDefault()
            If ws Is Nothing Then Return

            Dim startRow = ws.Dimension.Start.Row
            Dim endRow = Math.Min(ws.Dimension.End.Row, startRow + 25)
            Dim startCol = ws.Dimension.Start.Column
            Dim endCol = ws.Dimension.End.Column

            dgvAnteprima.Columns.Clear()
            For col = startCol To endCol
                dgvAnteprima.Columns.Add($"C{col}", ws.Cells(startRow, col).Text)
            Next

            For row = startRow To endRow
                Dim vals = Enumerable.Range(startCol, endCol - startCol + 1).
                           Select(Function(c) ws.Cells(row, c).Text).ToArray()
                Dim idx = dgvAnteprima.Rows.Add(vals)
                dgvAnteprima.Rows(idx).HeaderCell.Value = row.ToString()
            Next
        End Using
    End Sub

    Private Sub CaricaAnteprimaCsv(analisi As ImportatoreUniversale.RisultatoAnalisi)
        dgvAnteprima.Columns.Clear()
        Dim headers = analisi.ColonneRilevate.OrderBy(Function(c) c.Indice).Select(Function(c) c.Nome).ToArray()
        For Each h In headers
            dgvAnteprima.Columns.Add(h, h)
        Next

        For Each rowArr In analisi.DatiAnteprima
            dgvAnteprima.Rows.Add(rowArr)
        Next

        numRigaIntestazione.Maximum = dgvAnteprima.Rows.Count
    End Sub

    Private Sub NumRigaIntestazione_ValueChanged(sender As Object, e As EventArgs)
        rigaIntestazioneScelta = CInt(numRigaIntestazione.Value)
        EvidenziaRigaIntestazione()
    End Sub

    Private Sub EvidenziaRigaIntestazione()
        For Each r As DataGridViewRow In dgvAnteprima.Rows
            r.DefaultCellStyle.BackColor = Color.White
        Next

        Dim idx = rigaIntestazioneScelta - 1
        If idx >= 0 AndAlso idx < dgvAnteprima.Rows.Count Then
            dgvAnteprima.Rows(idx).DefaultCellStyle.BackColor = Color.Yellow
            dgvAnteprima.FirstDisplayedScrollingRowIndex = Math.Max(0, idx - 3)
        End If
    End Sub
End Class
