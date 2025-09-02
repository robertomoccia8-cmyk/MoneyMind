Namespace Models
    Public Class Transazione
    Public Property ID As Integer
    Public Property Data As Date
    Public Property Importo As Decimal
    Public Property Descrizione As String
    Public Property Causale As String
    Public Property MacroCategoria As String
    Public Property Categoria As String
    Public Property Necessita As String
    Public Property Frequenza As String
    Public Property Stagionalita As String
    Public Property DataInserimento As DateTime
    Public Property DataModifica As DateTime

    Public Sub New()
        ' Valori di default
        ID = 0
        Data = Date.Today
        Importo = 0
        Descrizione = ""
        Causale = ""
        MacroCategoria = ""
        Categoria = ""
        Necessita = ""
        Frequenza = ""
        Stagionalita = ""
        DataInserimento = DateTime.Now
        DataModifica = DateTime.Now
    End Sub

    Public Sub New(descrizione As String, importo As Decimal, data As Date)
        Me.New()
        Me.Descrizione = descrizione
        Me.Importo = importo
        Me.Data = data
    End Sub
    End Class
End Namespace
