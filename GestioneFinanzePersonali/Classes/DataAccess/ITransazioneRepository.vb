Imports System.Data
Imports GestioneFinanzePersonali.Models

Namespace DataAccess
    Public Interface ITransazioneRepository
    Function GetAllAsync() As Task(Of List(Of Transazione))
    Function GetByIdAsync(id As Integer) As Task(Of Transazione)
    Function GetByDateRangeAsync(startDate As DateTime, endDate As DateTime) As Task(Of List(Of Transazione))
    Function GetByCategoriaAsync(macroCategoria As String, categoria As String) As Task(Of List(Of Transazione))
    Function SearchByDescrizioneAsync(searchTerm As String) As Task(Of List(Of Transazione))
    Function InsertAsync(transazione As Transazione) As Task(Of Integer)
    Function UpdateAsync(transazione As Transazione) As Task(Of Boolean)
    Function DeleteAsync(id As Integer) As Task(Of Boolean)
    Function GetStatisticheMensiliAsync(year As Integer) As Task(Of DataTable)
    End Interface
End Namespace