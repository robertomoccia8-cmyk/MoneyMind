Imports System.Data.SQLite
Imports System.Globalization
Imports LiveChartsCore
Imports LiveChartsCore.SkiaSharpView.WinForms
Imports LiveChartsCore.SkiaSharpView

Public Class FormGrafici

    Private Const GIORNO_PERIODO As Integer = 23
    Private annoCorrente As Integer

    Public Sub New(Optional annoSel As Integer = 0)
        InitializeComponent()
        If annoSel > 0 Then
            annoCorrente = annoSel
        Else
            annoCorrente = Date.Today.Year
        End If
    End Sub

    Public Class VerticalLabel
        Inherits Label
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            e.Graphics.TranslateTransform(0, Me.Height)
            e.Graphics.RotateTransform(-90)
            e.Graphics.DrawString(Me.Text, Me.Font, New SolidBrush(Me.ForeColor), 0, 0)
        End Sub
    End Class

    Private Sub FormGrafici3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CaricaAnniDisponibili()
        If cmbAnnoGrafici.Items.Count > 0 Then
            If cmbAnnoGrafici.Items.Contains(annoCorrente.ToString()) Then
                cmbAnnoGrafici.SelectedItem = annoCorrente.ToString()
            Else
                cmbAnnoGrafici.SelectedIndex = 0
            End If
            PopolaTuttiIGrafici()
        End If
    End Sub

    Private Sub cmbAnnoGrafici_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAnnoGrafici.SelectedIndexChanged
        PopolaTuttiIGrafici()
    End Sub

    Private Sub btnAggiornaGrafici_Click(sender As Object, e As EventArgs) Handles btnAggiornaGrafici.Click
        PopolaTuttiIGrafici()
    End Sub

    Private Sub CaricaAnniDisponibili()
        cmbAnnoGrafici.Items.Clear()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Dim sql As String = "SELECT DISTINCT strftime('%Y', Data) AS Anno FROM Transazioni ORDER BY Anno DESC"
            Using cmd As New SQLiteCommand(sql, conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        cmbAnnoGrafici.Items.Add(reader("Anno").ToString())
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub PopolaTuttiIGrafici()
        If cmbAnnoGrafici.SelectedItem Is Nothing Then Exit Sub
        Dim anno As Integer = Integer.Parse(cmbAnnoGrafici.SelectedItem.ToString())
        PopolaGraficoEntrate(anno)
        PopolaGraficoUscite(anno)
        PopolaGraficoRisparmio(anno)
    End Sub

    Private Sub PopolaGraficoEntrate(anno As Integer)
        Dim entrateMese As New List(Of Double)
        Dim nomiMesi As New List(Of String)
        For mese = 1 To 12
            Dim inizio As Date = CalcolaPeriodoInizio(New DateTime(anno, mese, 15))
            Dim fine As Date = CalcolaPeriodoFine(inizio).AddDays(-1)
            Dim entrate As Double = 0
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "SELECT Importo FROM Transazioni WHERE Data >= @inizio AND Data <= @fine"
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@inizio", inizio.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@fine", fine.ToString("yyyy-MM-dd"))
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim imp As Double = Convert.ToDouble(reader("Importo"))
                            If imp > 0 Then entrate += imp
                        End While
                    End Using
                End Using
            End Using
            entrateMese.Add(entrate)
            nomiMesi.Add(fine.ToString("MMM", CultureInfo.CurrentCulture))
        Next

        cartesianChartEntrate.Series = New ISeries() {
            New LiveChartsCore.SkiaSharpView.ColumnSeries(Of Double) With {
                .Name = "Entrate",
                .Values = entrateMese
            }
        }
        cartesianChartEntrate.XAxes = New List(Of Axis) From {
            New Axis With {.Labels = nomiMesi}
        }
    End Sub

    Private Sub PopolaGraficoUscite(anno As Integer)
        Dim usciteMese As New List(Of Double)
        Dim nomiMesi As New List(Of String)
        For mese = 1 To 12
            Dim inizio As Date = CalcolaPeriodoInizio(New DateTime(anno, mese, 15))
            Dim fine As Date = CalcolaPeriodoFine(inizio).AddDays(-1)
            Dim uscite As Double = 0
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "SELECT Importo FROM Transazioni WHERE Data >= @inizio AND Data <= @fine"
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@inizio", inizio.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@fine", fine.ToString("yyyy-MM-dd"))
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim imp As Double = Convert.ToDouble(reader("Importo"))
                            If imp < 0 Then uscite += Math.Abs(imp)
                        End While
                    End Using
                End Using
            End Using
            usciteMese.Add(uscite)
            nomiMesi.Add(fine.ToString("MMM", CultureInfo.CurrentCulture))
        Next

        cartesianChartUscite.Series = New ISeries() {
            New LiveChartsCore.SkiaSharpView.ColumnSeries(Of Double) With {
                .Name = "Uscite",
                .Values = usciteMese
            }
        }
        cartesianChartUscite.XAxes = New List(Of Axis) From {
            New Axis With {.Labels = nomiMesi}
        }
    End Sub

    Private Sub PopolaGraficoRisparmio(anno As Integer)
        Dim risparmiMese As New List(Of Double)
        Dim nomiMesi As New List(Of String)
        For mese = 1 To 12
            Dim inizio As Date = CalcolaPeriodoInizio(New DateTime(anno, mese, 15))
            Dim fine As Date = CalcolaPeriodoFine(inizio).AddDays(-1)
            Dim entrate As Double = 0
            Dim uscite As Double = 0
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "SELECT Importo FROM Transazioni WHERE Data >= @inizio AND Data <= @fine"
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@inizio", inizio.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@fine", fine.ToString("yyyy-MM-dd"))
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim imp As Double = Convert.ToDouble(reader("Importo"))
                            If imp > 0 Then entrate += imp
                            If imp < 0 Then uscite += Math.Abs(imp)
                        End While
                    End Using
                End Using
            End Using
            risparmiMese.Add(entrate - uscite)
            nomiMesi.Add(fine.ToString("MMM", CultureInfo.CurrentCulture))
        Next

        cartesianChartRisparmio.Series = New ISeries() {
            New LiveChartsCore.SkiaSharpView.LineSeries(Of Double) With {
                .Name = "Risparmi",
                .Values = risparmiMese,
                .GeometrySize = 0 ' solo linea, niente punti
            }
        }
        cartesianChartRisparmio.XAxes = New List(Of Axis) From {
            New Axis With {.Labels = nomiMesi}
        }
    End Sub

    ' --- LOGICA PERIODI CON WEEKEND ---
    Private Function CalcolaPeriodoInizio(dataValuta As Date) As Date
        Dim giorno23 As Date = New DateTime(dataValuta.Year, dataValuta.Month, GIORNO_PERIODO)
        Select Case giorno23.DayOfWeek
            Case DayOfWeek.Saturday : giorno23 = giorno23.AddDays(-1)
            Case DayOfWeek.Sunday : giorno23 = giorno23.AddDays(-2)
        End Select
        If dataValuta >= giorno23 Then
            Return giorno23
        Else
            Dim mesePrec As Date = giorno23.AddMonths(-1)
            Select Case mesePrec.DayOfWeek
                Case DayOfWeek.Saturday : mesePrec = mesePrec.AddDays(-1)
                Case DayOfWeek.Sunday : mesePrec = mesePrec.AddDays(-2)
            End Select
            Return mesePrec
        End If
    End Function

    Private Function CalcolaPeriodoFine(dataInizio As Date) As Date
        Dim giorno23Fine As Date = New DateTime(dataInizio.Year, dataInizio.Month, GIORNO_PERIODO).AddMonths(1)
        Select Case giorno23Fine.DayOfWeek
            Case DayOfWeek.Saturday : giorno23Fine = giorno23Fine.AddDays(-1)
            Case DayOfWeek.Sunday : giorno23Fine = giorno23Fine.AddDays(-2)
        End Select
        Return giorno23Fine
    End Function

    Private Sub Label1_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub
End Class
