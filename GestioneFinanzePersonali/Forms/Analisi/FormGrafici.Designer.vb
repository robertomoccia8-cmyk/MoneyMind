<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormGrafici
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata in Progettazione Windows Form.  
    'Non modificarla mediante l'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormGrafici))
        Me.cartesianChartUscite = New LiveChartsCore.SkiaSharpView.WinForms.CartesianChart()
        Me.cartesianChartEntrate = New LiveChartsCore.SkiaSharpView.WinForms.CartesianChart()
        Me.cartesianChartRisparmio = New LiveChartsCore.SkiaSharpView.WinForms.CartesianChart()
        Me.cmbAnnoGrafici = New System.Windows.Forms.ComboBox()
        Me.btnAggiornaGrafici = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.Tab_EntrateUscite = New System.Windows.Forms.TabPage()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Tab_Risparmi = New System.Windows.Forms.TabPage()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.Panel1.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.Tab_EntrateUscite.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Tab_Risparmi.SuspendLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'cartesianChartUscite
        '
        Me.cartesianChartUscite.Dock = System.Windows.Forms.DockStyle.Fill
        Me.cartesianChartUscite.Location = New System.Drawing.Point(53, 330)
        Me.cartesianChartUscite.MatchAxesScreenDataRatio = False
        Me.cartesianChartUscite.Name = "cartesianChartUscite"
        Me.cartesianChartUscite.Size = New System.Drawing.Size(1508, 322)
        Me.cartesianChartUscite.TabIndex = 1
        '
        'cartesianChartEntrate
        '
        Me.cartesianChartEntrate.Dock = System.Windows.Forms.DockStyle.Fill
        Me.cartesianChartEntrate.Location = New System.Drawing.Point(53, 3)
        Me.cartesianChartEntrate.MatchAxesScreenDataRatio = False
        Me.cartesianChartEntrate.Name = "cartesianChartEntrate"
        Me.cartesianChartEntrate.Size = New System.Drawing.Size(1508, 321)
        Me.cartesianChartEntrate.TabIndex = 0
        '
        'cartesianChartRisparmio
        '
        Me.cartesianChartRisparmio.Dock = System.Windows.Forms.DockStyle.Fill
        Me.cartesianChartRisparmio.Location = New System.Drawing.Point(3, 3)
        Me.cartesianChartRisparmio.MatchAxesScreenDataRatio = False
        Me.cartesianChartRisparmio.Name = "cartesianChartRisparmio"
        Me.cartesianChartRisparmio.Size = New System.Drawing.Size(1564, 655)
        Me.cartesianChartRisparmio.TabIndex = 0
        '
        'cmbAnnoGrafici
        '
        Me.cmbAnnoGrafici.FormattingEnabled = True
        Me.cmbAnnoGrafici.Location = New System.Drawing.Point(12, 12)
        Me.cmbAnnoGrafici.Name = "cmbAnnoGrafici"
        Me.cmbAnnoGrafici.Size = New System.Drawing.Size(150, 28)
        Me.cmbAnnoGrafici.TabIndex = 1
        '
        'btnAggiornaGrafici
        '
        Me.btnAggiornaGrafici.Location = New System.Drawing.Point(168, 12)
        Me.btnAggiornaGrafici.Name = "btnAggiornaGrafici"
        Me.btnAggiornaGrafici.Size = New System.Drawing.Size(100, 30)
        Me.btnAggiornaGrafici.TabIndex = 2
        Me.btnAggiornaGrafici.Text = "Aggiorna"
        Me.btnAggiornaGrafici.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.cmbAnnoGrafici)
        Me.Panel1.Controls.Add(Me.btnAggiornaGrafici)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1578, 50)
        Me.Panel1.TabIndex = 3
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.Tab_EntrateUscite)
        Me.TabControl1.Controls.Add(Me.Tab_Risparmi)
        Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControl1.Location = New System.Drawing.Point(0, 50)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(1578, 694)
        Me.TabControl1.TabIndex = 4
        '
        'Tab_EntrateUscite
        '
        Me.Tab_EntrateUscite.Controls.Add(Me.TableLayoutPanel1)
        Me.Tab_EntrateUscite.Location = New System.Drawing.Point(4, 29)
        Me.Tab_EntrateUscite.Name = "Tab_EntrateUscite"
        Me.Tab_EntrateUscite.Padding = New System.Windows.Forms.Padding(3)
        Me.Tab_EntrateUscite.Size = New System.Drawing.Size(1570, 661)
        Me.Tab_EntrateUscite.TabIndex = 0
        Me.Tab_EntrateUscite.Text = "Entrate - Uscite"
        Me.Tab_EntrateUscite.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.cartesianChartUscite, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.cartesianChartEntrate, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.PictureBox1, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.PictureBox2, 0, 0)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 3)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(1564, 655)
        Me.TableLayoutPanel1.TabIndex = 2
        '
        'PictureBox1
        '
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(3, 330)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(44, 322)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 2
        Me.PictureBox1.TabStop = False
        '
        'Tab_Risparmi
        '
        Me.Tab_Risparmi.Controls.Add(Me.cartesianChartRisparmio)
        Me.Tab_Risparmi.Location = New System.Drawing.Point(4, 29)
        Me.Tab_Risparmi.Name = "Tab_Risparmi"
        Me.Tab_Risparmi.Padding = New System.Windows.Forms.Padding(3)
        Me.Tab_Risparmi.Size = New System.Drawing.Size(1570, 661)
        Me.Tab_Risparmi.TabIndex = 1
        Me.Tab_Risparmi.Text = "Risparmi"
        Me.Tab_Risparmi.UseVisualStyleBackColor = True
        '
        'PictureBox2
        '
        Me.PictureBox2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), System.Drawing.Image)
        Me.PictureBox2.Location = New System.Drawing.Point(3, 3)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(44, 321)
        Me.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox2.TabIndex = 3
        Me.PictureBox2.TabStop = False
        '
        'FormGrafici
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1578, 744)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "FormGrafici"
        Me.Text = "Grafici"
        Me.Panel1.ResumeLayout(False)
        Me.TabControl1.ResumeLayout(False)
        Me.Tab_EntrateUscite.ResumeLayout(False)
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Tab_Risparmi.ResumeLayout(False)
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents cartesianChartEntrate As LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
    Friend WithEvents cmbAnnoGrafici As ComboBox
    Friend WithEvents btnAggiornaGrafici As Button
    Friend WithEvents cartesianChartUscite As LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
    Friend WithEvents cartesianChartRisparmio As LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
    Friend WithEvents Panel1 As Panel
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents Tab_EntrateUscite As TabPage
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents Tab_Risparmi As TabPage
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
End Class
