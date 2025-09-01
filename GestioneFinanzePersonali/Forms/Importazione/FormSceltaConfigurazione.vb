Public Class FormSceltaConfigurazione
    Private configurazioni As List(Of String)
    Private configurazioneScelta As ConfigurazioneImportazione = Nothing
    Private creaNuova As Boolean = False

    Public ReadOnly Property ConfigurazioneSelezionata As ConfigurazioneImportazione
        Get
            Return configurazioneScelta
        End Get
    End Property

    Public ReadOnly Property CreaNuovaConfigurazione As Boolean
        Get
            Return creaNuova
        End Get
    End Property

    Public Sub New()
        InitializeComponent()
        Me.Text = "Gestione Configurazioni Importazione"
        Me.Size = New Size(600, 400)
        Me.StartPosition = FormStartPosition.CenterScreen

        configurazioni = GestoreConfigurazioni.GetConfigurazioni()
        CreaInterfaccia()
    End Sub

    Private Sub CreaInterfaccia()
        Dim pnlTop As New Panel()
        pnlTop.Dock = DockStyle.Top
        pnlTop.Height = 60
        pnlTop.BackColor = Color.FromArgb(240, 248, 255)

        Dim lblTitolo As New Label()
        lblTitolo.Text = "Scegli una configurazione esistente o creane una nuova:"
        lblTitolo.Location = New Point(20, 20)
        lblTitolo.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        lblTitolo.AutoSize = True
        pnlTop.Controls.Add(lblTitolo)

        Dim lstConfigurazioni As New ListBox()
        lstConfigurazioni.Name = "lstConfigurazioni"
        lstConfigurazioni.Dock = DockStyle.Fill
        lstConfigurazioni.Font = New Font("Segoe UI", 10)
        For Each config In configurazioni
            lstConfigurazioni.Items.Add(config)
        Next
        AddHandler lstConfigurazioni.DoubleClick, AddressOf LstConfigurazioni_DoubleClick

        Dim pnlBottom As New Panel()
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Height = 60

        Dim btnNuova As New Button()
        btnNuova.Text = "Nuova Configurazione"
        btnNuova.Location = New Point(20, 15)
        btnNuova.Size = New Size(150, 35)
        btnNuova.BackColor = Color.FromArgb(40, 180, 100)
        btnNuova.ForeColor = Color.White
        btnNuova.FlatStyle = FlatStyle.Flat
        AddHandler btnNuova.Click, AddressOf BtnNuova_Click
        pnlBottom.Controls.Add(btnNuova)

        Dim btnCarica As New Button()
        btnCarica.Text = "Carica Selezionata"
        btnCarica.Location = New Point(200, 15)
        btnCarica.Size = New Size(130, 35)
        btnCarica.BackColor = Color.FromArgb(50, 120, 200)
        btnCarica.ForeColor = Color.White
        btnCarica.FlatStyle = FlatStyle.Flat
        AddHandler btnCarica.Click, AddressOf BtnCarica_Click
        pnlBottom.Controls.Add(btnCarica)

        Dim btnElimina As New Button()
        btnElimina.Text = "Elimina"
        btnElimina.Location = New Point(350, 15)
        btnElimina.Size = New Size(80, 35)
        btnElimina.BackColor = Color.FromArgb(220, 60, 60)
        btnElimina.ForeColor = Color.White
        btnElimina.FlatStyle = FlatStyle.Flat
        AddHandler btnElimina.Click, AddressOf BtnElimina_Click
        pnlBottom.Controls.Add(btnElimina)

        Dim btnAnnulla As New Button()
        btnAnnulla.Text = "Annulla"
        btnAnnulla.Location = New Point(460, 15)
        btnAnnulla.Size = New Size(80, 35)
        btnAnnulla.DialogResult = DialogResult.Cancel
        pnlBottom.Controls.Add(btnAnnulla)


        Me.Controls.Add(lstConfigurazioni)
        Me.Controls.Add(pnlTop)
        Me.Controls.Add(pnlBottom)
    End Sub

    Private Sub BtnNuova_Click(sender As Object, e As EventArgs)
        creaNuova = True
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtnCarica_Click(sender As Object, e As EventArgs)
        Dim lst = CType(Me.Controls.Find("lstConfigurazioni", True)(0), ListBox)
        If lst.SelectedItem IsNot Nothing Then
            configurazioneScelta = GestoreConfigurazioni.CaricaConfigurazione(lst.SelectedItem.ToString())
            If configurazioneScelta IsNot Nothing Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If
        Else
            MessageBox.Show("Seleziona una configurazione!", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub BtnElimina_Click(sender As Object, e As EventArgs)
        Dim lst = CType(Me.Controls.Find("lstConfigurazioni", True)(0), ListBox)
        If lst.SelectedItem IsNot Nothing Then
            If MessageBox.Show($"Eliminare la configurazione '{lst.SelectedItem}'?", "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                GestoreConfigurazioni.EliminaConfigurazione(lst.SelectedItem.ToString())
                lst.Items.Remove(lst.SelectedItem)
            End If
        End If
    End Sub

    Private Sub LstConfigurazioni_DoubleClick(sender As Object, e As EventArgs)
        BtnCarica_Click(sender, e)
    End Sub
End Class
