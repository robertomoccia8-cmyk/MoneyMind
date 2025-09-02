Imports GestioneFinanzePersonali.Models
Imports GestioneFinanzePersonali.DataAccess
Imports GestioneFinanzePersonali.Logging
Imports System.Data

Namespace Business
    Public Class TransazioneService
    Private ReadOnly _repository As ITransazioneRepository
    Private ReadOnly _logger As ILogger
    
    Public Sub New(repository As ITransazioneRepository, logger As ILogger)
        _repository = repository
        _logger = logger
    End Sub
    
    Public Async Function GetAllTransazioniAsync() As Task(Of List(Of Transazione))
        Try
            _logger.LogInformation("Recupero tutte le transazioni")
            Return Await _repository.GetAllAsync()
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel recupero delle transazioni")
            Throw
        End Try
    End Function
    
    Public Async Function GetTransazioniByPeriodoAsync(startDate As DateTime, endDate As DateTime) As Task(Of List(Of Transazione))
        Try
            ValidateDateRange(startDate, endDate)
            _logger.LogInformation("Recupero transazioni dal {StartDate} al {EndDate}", startDate, endDate)
            Return Await _repository.GetByDateRangeAsync(startDate, endDate)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel recupero transazioni per periodo")
            Throw
        End Try
    End Function
    
    Public Async Function GetTransazioniByCategoriaAsync(macroCategoria As String, categoria As String) As Task(Of List(Of Transazione))
        Try
            ValidateCategoria(macroCategoria, categoria)
            _logger.LogInformation("Recupero transazioni per categoria {MacroCategoria} > {Categoria}", macroCategoria, categoria)
            Return Await _repository.GetByCategoriaAsync(macroCategoria, categoria)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel recupero transazioni per categoria")
            Throw
        End Try
    End Function
    
    Public Async Function SearchTransazioniAsync(searchTerm As String) As Task(Of List(Of Transazione))
        Try
            If String.IsNullOrWhiteSpace(searchTerm) Then
                Throw New ArgumentException("Il termine di ricerca non può essere vuoto", NameOf(searchTerm))
            End If
            
            _logger.LogInformation("Ricerca transazioni con termine: {SearchTerm}", searchTerm)
            Return Await _repository.SearchByDescrizioneAsync(searchTerm)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nella ricerca transazioni")
            Throw
        End Try
    End Function
    
    Public Async Function CreateTransazioneAsync(transazione As Transazione) As Task(Of Integer)
        Try
            ValidateTransazione(transazione)
            _logger.LogInformation("Creazione nuova transazione per {Importo} del {Data}", transazione.Importo, transazione.Data)
            Return Await _repository.InsertAsync(transazione)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nella creazione transazione")
            Throw
        End Try
    End Function
    
    Public Async Function UpdateTransazioneAsync(transazione As Transazione) As Task(Of Boolean)
        Try
            ValidateTransazione(transazione)
            If transazione.ID <= 0 Then
                Throw New ArgumentException("ID transazione non valido", NameOf(transazione))
            End If
            
            _logger.LogInformation("Aggiornamento transazione ID {ID}", transazione.ID)
            Return Await _repository.UpdateAsync(transazione)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nell'aggiornamento transazione")
            Throw
        End Try
    End Function
    
    Public Async Function DeleteTransazioneAsync(id As Integer) As Task(Of Boolean)
        Try
            If id <= 0 Then
                Throw New ArgumentException("ID transazione non valido", NameOf(id))
            End If
            
            _logger.LogInformation("Eliminazione transazione ID {ID}", id)
            Return Await _repository.DeleteAsync(id)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nell'eliminazione transazione")
            Throw
        End Try
    End Function
    
    Public Async Function GetStatisticheMensiliAsync(year As Integer) As Task(Of DataTable)
        Try
            If year < 2000 OrElse year > DateTime.Now.Year + 1 Then
                Throw New ArgumentException("Anno non valido", NameOf(year))
            End If
            
            _logger.LogInformation("Recupero statistiche mensili per l'anno {Year}", year)
            Return Await _repository.GetStatisticheMensiliAsync(year)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel recupero statistiche mensili")
            Throw
        End Try
    End Function
    
    Public Async Function GetSaldoTotaleAsync() As Task(Of Decimal)
        Try
            Dim transazioni = Await _repository.GetAllAsync()
            Return transazioni.Sum(Function(t) t.Importo)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel calcolo saldo totale")
            Throw
        End Try
    End Function
    
    Public Async Function GetEntrateUscitePeriodoAsync(startDate As DateTime, endDate As DateTime) As Task(Of (Entrate As Decimal, Uscite As Decimal))
        Try
            ValidateDateRange(startDate, endDate)
            Dim transazioni = Await _repository.GetByDateRangeAsync(startDate, endDate)
            
            Dim entrate = transazioni.Where(Function(t) t.Importo > 0).Sum(Function(t) t.Importo)
            Dim uscite = Math.Abs(transazioni.Where(Function(t) t.Importo < 0).Sum(Function(t) t.Importo))
            
            Return (entrate, uscite)
        Catch ex As Exception
            _logger.LogError(ex, "Errore nel calcolo entrate/uscite periodo")
            Throw
        End Try
    End Function
    
    Private Sub ValidateTransazione(transazione As Transazione)
        If transazione Is Nothing Then
            Throw New ArgumentNullException(NameOf(transazione))
        End If
        
        If String.IsNullOrWhiteSpace(transazione.Descrizione) Then
            Throw New ArgumentException("La descrizione è obbligatoria", NameOf(transazione))
        End If
        
        If transazione.Importo = 0 Then
            Throw New ArgumentException("L'importo non può essere zero", NameOf(transazione))
        End If
        
        If transazione.Data > DateTime.Now.AddDays(1) Then
            Throw New ArgumentException("La data non può essere nel futuro", NameOf(transazione))
        End If
    End Sub
    
    Private Sub ValidateDateRange(startDate As DateTime, endDate As DateTime)
        If startDate > endDate Then
            Throw New ArgumentException("La data di inizio deve essere precedente alla data di fine")
        End If
        
        If (endDate - startDate).TotalDays > 3650 Then ' 10 anni
            Throw New ArgumentException("Il range di date non può superare 10 anni")
        End If
    End Sub
    
    Private Sub ValidateCategoria(macroCategoria As String, categoria As String)
        If String.IsNullOrWhiteSpace(macroCategoria) Then
            Throw New ArgumentException("La macro categoria è obbligatoria", NameOf(macroCategoria))
        End If
        
        If String.IsNullOrWhiteSpace(categoria) Then
            Throw New ArgumentException("La categoria è obbligatoria", NameOf(categoria))
        End If
    End Sub
    End Class
End Namespace