Public Class FormSceltaSet

    Private set1 As FormGestioneSetCategorie.SetCategoria
    Private set2 As FormGestioneSetCategorie.SetCategoria

    Public Property SetSelezionato As FormGestioneSetCategorie.SetCategoria

    Public Sub New(primoSet As FormGestioneSetCategorie.SetCategoria, secondoSet As FormGestioneSetCategorie.SetCategoria)
        InitializeComponent()
        set1 = primoSet
        set2 = secondoSet
    End Sub

    Private Sub FormSceltaSet_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Imposta informazioni Set 1
        rbSet1.Text = set1.NomeCompleto
        lblInfoSet1.Text = $"Pattern: {set1.NumeroPattern}" & vbCrLf &
                          $"Transazioni: {set1.NumeroTransazioni}" & vbCrLf &
                          $"Ultimo utilizzo: {If(set1.UltimoUtilizzo.HasValue, set1.UltimoUtilizzo.Value.ToString("dd/MM/yyyy"), "Mai")}" & vbCrLf &
                          $"Parole: {String.Join(", ", set1.Parole.Take(5))}{If(set1.Parole.Count > 5, "...", "")}"

        ' Imposta informazioni Set 2
        rbSet2.Text = set2.NomeCompleto
        lblInfoSet2.Text = $"Pattern: {set2.NumeroPattern}" & vbCrLf &
                          $"Transazioni: {set2.NumeroTransazioni}" & vbCrLf &
                          $"Ultimo utilizzo: {If(set2.UltimoUtilizzo.HasValue, set2.UltimoUtilizzo.Value.ToString("dd/MM/yyyy"), "Mai")}" & vbCrLf &
                          $"Parole: {String.Join(", ", set2.Parole.Take(5))}{If(set2.Parole.Count > 5, "...", "")}"

        ' Seleziona automaticamente il set con più transazioni
        If set2.NumeroTransazioni > set1.NumeroTransazioni Then
            rbSet2.Checked = True
            rbSet1.Checked = False
        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        SetSelezionato = If(rbSet1.Checked, set1, set2)
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
