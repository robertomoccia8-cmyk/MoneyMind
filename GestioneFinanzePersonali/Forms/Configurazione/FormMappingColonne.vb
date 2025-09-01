Imports System.Windows.Forms

Public Class FormMappingColonne

    Private risultatoAnalisi As ImportatoreUniversale.RisultatoAnalisi
    Private mappingFinale As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)

    Public Property MappingColonne As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)
        Get
            Return mappingFinale
        End Get
        Set(value As Dictionary(Of ImportatoreUniversale.TipoColonna, Integer))
            mappingFinale = value
        End Set
    End Property

    Public Sub New(analisi As ImportatoreUniversale.RisultatoAnalisi)
        InitializeComponent()
        risultatoAnalisi = analisi
        mappingFinale = New Dictionary(Of ImportatoreUniversale.TipoColonna, Integer)
    End Sub

    Private Sub FormMappingColonne_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ImpostaInterfaccia()
        PopolaAnteprima()
        ImpostaMappingAutomatico()
    End Sub

    Private Sub ImpostaInterfaccia()
        Me.Text = "Mapping Colonne - Importazione File"
        Me.Size = New Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterParent

        ' Label info
        Dim lblInfo As New Label()
        lblInfo.Text = "Associa le colonne del file ai campi richiesti:"
        lblInfo.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblInfo.Location = New Point(20, 20)
        lblInfo.Size = New Size(400, 25)
        Me.Controls.Add(lblInfo)

        ' Panel per mapping
        Dim pnlMapping As New Panel()
        pnlMapping.Location = New Point(20, 60)
        pnlMapping.Size = New Size(740, 120)
        pnlMapping.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(pnlMapping)

        ' Mapping Data
        CreaControlloMapping(pnlMapping, "Data:", "cmbData", 10, 15)

        ' Mapping Importo  
        CreaControlloMapping(pnlMapping, "Importo:", "cmbImporto", 10, 50)

        ' Mapping Descrizione
        CreaControlloMapping(pnlMapping, "Descrizione:", "cmbDescrizione", 10, 85)

        ' DataGridView per anteprima
        Dim dgvAnteprima As New DataGridView()
        dgvAnteprima.Name = "dgvAnteprima"
        dgvAnteprima.Location = New Point(20, 200)
        dgvAnteprima.Size = New Size(740, 300)
        dgvAnteprima.ReadOnly = True
        dgvAnteprima.AllowUserToAddRows = False
        dgvAnteprima.AllowUserToDeleteRows = False
        dgvAnteprima.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        Me.Controls.Add(dgvAnteprima)

        ' Pulsanti
        Dim btnOk As New Button()
        btnOk.Name = "btnOk"
        btnOk.Text = "Importa"
        btnOk.Location = New Point(600, 520)
        btnOk.Size = New Size(80, 30)
        btnOk.BackColor = Color.Green
        btnOk.ForeColor = Color.White
        AddHandler btnOk.Click, AddressOf BtnOk_Click
        Me.Controls.Add(btnOk)

        Dim btnAnnulla As New Button()
        btnAnnulla.Text = "Annulla"
        btnAnnulla.Location = New Point(690, 520)
        btnAnnulla.Size = New Size(80, 30)
        btnAnnulla.DialogResult = DialogResult.Cancel
        Me.Controls.Add(btnAnnulla)
    End Sub

    Private Sub CreaControlloMapping(parent As Panel, etichetta As String, nomeCombo As String, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = etichetta
        lbl.Location = New Point(x, y)
        lbl.Size = New Size(80, 20)
        parent.Controls.Add(lbl)

        Dim cmb As New ComboBox()
        cmb.Name = nomeCombo
        cmb.Location = New Point(x + 90, y - 3)
        cmb.Size = New Size(200, 25)
        cmb.DropDownStyle = ComboBoxStyle.DropDownList

        ' Popola con le colonne disponibili
        cmb.Items.Add("-- Non assegnata --")
        For Each colonna In risultatoAnalisi.ColonneRilevate
            cmb.Items.Add($"Colonna {colonna.Indice + 1}: {colonna.Nome}")
        Next

        cmb.SelectedIndex = 0
        AddHandler cmb.SelectedIndexChanged, AddressOf ComboMapping_SelectedIndexChanged
        parent.Controls.Add(cmb)
    End Sub

    Private Sub PopolaAnteprima()
        Dim dgv = DirectCast(Me.Controls("dgvAnteprima"), DataGridView)
        dgv.Columns.Clear()

        ' Crea colonne
        For i = 0 To risultatoAnalisi.ColonneRilevate.Count - 1
            Dim colonna = risultatoAnalisi.ColonneRilevate(i)
            dgv.Columns.Add($"Col{i}", $"Colonna {i + 1}: {colonna.Nome}")
        Next

        ' Popola righe
        For Each riga In risultatoAnalisi.DatiAnteprima.Take(10)
            dgv.Rows.Add(riga)
        Next
    End Sub

    Private Sub ImpostaMappingAutomatico()
        Dim cmbData = DirectCast(Me.Controls.Find("cmbData", True)(0), ComboBox)
        Dim cmbImporto = DirectCast(Me.Controls.Find("cmbImporto", True)(0), ComboBox)
        Dim cmbDescrizione = DirectCast(Me.Controls.Find("cmbDescrizione", True)(0), ComboBox)

        ' Imposta automaticamente se possibile
        For Each colonna In risultatoAnalisi.ColonneRilevate
            Select Case colonna.Tipo
                Case ImportatoreUniversale.TipoColonna.Data
                    cmbData.SelectedIndex = colonna.Indice + 1
                Case ImportatoreUniversale.TipoColonna.Importo
                    cmbImporto.SelectedIndex = colonna.Indice + 1
                Case ImportatoreUniversale.TipoColonna.Descrizione
                    cmbDescrizione.SelectedIndex = colonna.Indice + 1
            End Select
        Next
    End Sub

    Private Sub ComboMapping_SelectedIndexChanged(sender As Object, e As EventArgs)
        ' Evidenzia la colonna selezionata nella griglia
        Dim combo = DirectCast(sender, ComboBox)
        Dim dgv = DirectCast(Me.Controls("dgvAnteprima"), DataGridView)

        ' Reset colori
        For Each col As DataGridViewColumn In dgv.Columns
            col.DefaultCellStyle.BackColor = Color.White
        Next

        ' Evidenzia colonna selezionata
        If combo.SelectedIndex > 0 Then
            Dim indiceColonna = combo.SelectedIndex - 1
            If indiceColonna < dgv.Columns.Count Then
                dgv.Columns(indiceColonna).DefaultCellStyle.BackColor = Color.LightYellow
            End If
        End If
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs)
        If ValidaMapping() Then
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Function ValidaMapping() As Boolean
        Dim cmbData = DirectCast(Me.Controls.Find("cmbData", True)(0), ComboBox)
        Dim cmbImporto = DirectCast(Me.Controls.Find("cmbImporto", True)(0), ComboBox)
        Dim cmbDescrizione = DirectCast(Me.Controls.Find("cmbDescrizione", True)(0), ComboBox)

        If cmbData.SelectedIndex = 0 OrElse cmbImporto.SelectedIndex = 0 OrElse cmbDescrizione.SelectedIndex = 0 Then
            MessageBox.Show("Devi assegnare tutte e tre le colonne richieste (Data, Importo, Descrizione).",
                          "Mapping Incompleto", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        ' Verifica che non ci siano duplicati
        Dim indiciUsati As New List(Of Integer) From {
            cmbData.SelectedIndex,
            cmbImporto.SelectedIndex,
            cmbDescrizione.SelectedIndex
        }

        If indiciUsati.Distinct().Count() < 3 Then
            MessageBox.Show("Non puoi assegnare la stessa colonna a campi diversi.",
                          "Mapping Duplicato", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        ' Salva mapping
        mappingFinale.Clear()
        mappingFinale(ImportatoreUniversale.TipoColonna.Data) = cmbData.SelectedIndex - 1
        mappingFinale(ImportatoreUniversale.TipoColonna.Importo) = cmbImporto.SelectedIndex - 1
        mappingFinale(ImportatoreUniversale.TipoColonna.Descrizione) = cmbDescrizione.SelectedIndex - 1

        Return True
    End Function

End Class
