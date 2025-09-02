Imports System.Data
Imports System.Data.SQLite
Imports GestioneFinanzePersonali.Models

Namespace DataAccess
    Public Class TransazioneRepository
    Implements ITransazioneRepository
    
    Private ReadOnly _connectionString As String
    
    Public Sub New(connectionString As String)
        _connectionString = connectionString
    End Sub
    
    Public Async Function GetAllAsync() As Task(Of List(Of Transazione)) Implements ITransazioneRepository.GetAllAsync
        Dim transazioni As New List(Of Transazione)
        
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "SELECT * FROM Transazioni ORDER BY Data DESC"
            Using command As New SQLiteCommand(query, connection)
                Using reader = Await command.ExecuteReaderAsync()
                    While Await reader.ReadAsync()
                        transazioni.Add(MapToTransazione(reader))
                    End While
                End Using
            End Using
        End Using
        
        Return transazioni
    End Function
    
    Public Async Function GetByIdAsync(id As Integer) As Task(Of Transazione) Implements ITransazioneRepository.GetByIdAsync
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "SELECT * FROM Transazioni WHERE ID = @id"
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@id", id)
                Using reader = Await command.ExecuteReaderAsync()
                    If Await reader.ReadAsync() Then
                        Return MapToTransazione(reader)
                    End If
                End Using
            End Using
        End Using
        
        Return Nothing
    End Function
    
    Public Async Function GetByDateRangeAsync(startDate As DateTime, endDate As DateTime) As Task(Of List(Of Transazione)) Implements ITransazioneRepository.GetByDateRangeAsync
        Dim transazioni As New List(Of Transazione)
        
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "SELECT * FROM Transazioni WHERE Data BETWEEN @start AND @end ORDER BY Data DESC"
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"))
                command.Parameters.AddWithValue("@end", endDate.ToString("yyyy-MM-dd"))
                Using reader = Await command.ExecuteReaderAsync()
                    While Await reader.ReadAsync()
                        transazioni.Add(MapToTransazione(reader))
                    End While
                End Using
            End Using
        End Using
        
        Return transazioni
    End Function
    
    Public Async Function GetByCategoriaAsync(macroCategoria As String, categoria As String) As Task(Of List(Of Transazione)) Implements ITransazioneRepository.GetByCategoriaAsync
        Dim transazioni As New List(Of Transazione)
        
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "SELECT * FROM Transazioni WHERE MacroCategoria = @macro AND Categoria = @cat ORDER BY Data DESC"
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@macro", macroCategoria)
                command.Parameters.AddWithValue("@cat", categoria)
                Using reader = Await command.ExecuteReaderAsync()
                    While Await reader.ReadAsync()
                        transazioni.Add(MapToTransazione(reader))
                    End While
                End Using
            End Using
        End Using
        
        Return transazioni
    End Function
    
    Public Async Function SearchByDescrizioneAsync(searchTerm As String) As Task(Of List(Of Transazione)) Implements ITransazioneRepository.SearchByDescrizioneAsync
        Dim transazioni As New List(Of Transazione)
        
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "SELECT * FROM Transazioni WHERE Descrizione LIKE @term OR Causale LIKE @term ORDER BY Data DESC"
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@term", $"%{searchTerm}%")
                Using reader = Await command.ExecuteReaderAsync()
                    While Await reader.ReadAsync()
                        transazioni.Add(MapToTransazione(reader))
                    End While
                End Using
            End Using
        End Using
        
        Return transazioni
    End Function
    
    Public Async Function InsertAsync(transazione As Transazione) As Task(Of Integer) Implements ITransazioneRepository.InsertAsync
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "INSERT INTO Transazioni (Data, Importo, Descrizione, Causale, MacroCategoria, Categoria, Necessita, Frequenza, Stagionalita) 
                        VALUES (@data, @importo, @descrizione, @causale, @macro, @categoria, @necessita, @frequenza, @stagionalita);
                        SELECT last_insert_rowid();"
            
            Using command As New SQLiteCommand(query, connection)
                AddParametersFromTransazione(command, transazione)
                Return Convert.ToInt32(Await command.ExecuteScalarAsync())
            End Using
        End Using
    End Function
    
    Public Async Function UpdateAsync(transazione As Transazione) As Task(Of Boolean) Implements ITransazioneRepository.UpdateAsync
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "UPDATE Transazioni SET Data=@data, Importo=@importo, Descrizione=@descrizione, 
                        Causale=@causale, MacroCategoria=@macro, Categoria=@categoria, 
                        Necessita=@necessita, Frequenza=@frequenza, Stagionalita=@stagionalita,
                        DataModifica=CURRENT_TIMESTAMP WHERE ID=@id"
            
            Using command As New SQLiteCommand(query, connection)
                AddParametersFromTransazione(command, transazione)
                command.Parameters.AddWithValue("@id", transazione.ID)
                Return Await command.ExecuteNonQueryAsync() > 0
            End Using
        End Using
    End Function
    
    Public Async Function DeleteAsync(id As Integer) As Task(Of Boolean) Implements ITransazioneRepository.DeleteAsync
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "DELETE FROM Transazioni WHERE ID = @id"
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@id", id)
                Return Await command.ExecuteNonQueryAsync() > 0
            End Using
        End Using
    End Function
    
    Public Async Function GetStatisticheMensiliAsync(year As Integer) As Task(Of DataTable) Implements ITransazioneRepository.GetStatisticheMensiliAsync
        Dim dt As New DataTable()
        
        Using connection As New SQLiteConnection(_connectionString)
            Await connection.OpenAsync()
            
            Dim query = "SELECT strftime('%m', Data) as Mese,
                               SUM(CASE WHEN Importo > 0 THEN Importo ELSE 0 END) as Entrate,
                               SUM(CASE WHEN Importo < 0 THEN ABS(Importo) ELSE 0 END) as Uscite
                        FROM Transazioni 
                        WHERE strftime('%Y', Data) = @year
                        GROUP BY strftime('%m', Data)
                        ORDER BY Mese"
            
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@year", year.ToString())
                Using adapter As New SQLiteDataAdapter(command)
                    adapter.Fill(dt)
                End Using
            End Using
        End Using
        
        Return dt
    End Function
    
    Private Function MapToTransazione(reader As SQLiteDataReader) As Transazione
        Return New Transazione With {
            .ID = Convert.ToInt32(reader("ID")),
            .Data = Convert.ToDateTime(reader("Data")),
            .Importo = Convert.ToDecimal(reader("Importo")),
            .Descrizione = reader("Descrizione").ToString(),
            .Causale = If(reader("Causale") IsNot DBNull.Value, reader("Causale").ToString(), ""),
            .MacroCategoria = If(reader("MacroCategoria") IsNot DBNull.Value, reader("MacroCategoria").ToString(), ""),
            .Categoria = If(reader("Categoria") IsNot DBNull.Value, reader("Categoria").ToString(), ""),
            .Necessita = If(reader("Necessita") IsNot DBNull.Value, reader("Necessita").ToString(), ""),
            .Frequenza = If(reader("Frequenza") IsNot DBNull.Value, reader("Frequenza").ToString(), ""),
            .Stagionalita = If(reader("Stagionalita") IsNot DBNull.Value, reader("Stagionalita").ToString(), ""),
            .DataInserimento = If(reader("DataInserimento") IsNot DBNull.Value, Convert.ToDateTime(reader("DataInserimento")), DateTime.Now),
            .DataModifica = If(reader("DataModifica") IsNot DBNull.Value, Convert.ToDateTime(reader("DataModifica")), DateTime.Now)
        }
    End Function
    
    Private Sub AddParametersFromTransazione(command As SQLiteCommand, transazione As Transazione)
        command.Parameters.AddWithValue("@data", transazione.Data.ToString("yyyy-MM-dd"))
        command.Parameters.AddWithValue("@importo", transazione.Importo)
        command.Parameters.AddWithValue("@descrizione", transazione.Descrizione)
        command.Parameters.AddWithValue("@causale", If(String.IsNullOrEmpty(transazione.Causale), "", transazione.Causale))
        command.Parameters.AddWithValue("@macro", If(String.IsNullOrEmpty(transazione.MacroCategoria), "", transazione.MacroCategoria))
        command.Parameters.AddWithValue("@categoria", If(String.IsNullOrEmpty(transazione.Categoria), "", transazione.Categoria))
        command.Parameters.AddWithValue("@necessita", If(String.IsNullOrEmpty(transazione.Necessita), "", transazione.Necessita))
        command.Parameters.AddWithValue("@frequenza", If(String.IsNullOrEmpty(transazione.Frequenza), "", transazione.Frequenza))
        command.Parameters.AddWithValue("@stagionalita", If(String.IsNullOrEmpty(transazione.Stagionalita), "", transazione.Stagionalita))
    End Sub
    End Class
End Namespace