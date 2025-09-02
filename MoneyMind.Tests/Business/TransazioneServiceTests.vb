Imports NUnit.Framework
Imports Moq
Imports FluentAssertions
Imports MoneyMind.Models
Imports System.Threading.Tasks

<TestFixture>
Public Class TransazioneServiceTests
    Private _mockRepository As Mock(Of ITransazioneRepository)
    Private _mockLogger As Mock(Of ILogger)
    Private _service As TransazioneService
    
    <SetUp>
    Public Sub Setup()
        _mockRepository = New Mock(Of ITransazioneRepository)()
        _mockLogger = New Mock(Of ILogger)()
        _service = New TransazioneService(_mockRepository.Object, _mockLogger.Object)
    End Sub
    
    <Test>
    Public Async Function GetAllTransazioniAsync_ShouldReturnTransazioni_WhenRepositoryReturnsData() As Task
        ' Arrange
        Dim expectedTransazioni = New List(Of Transazione) From {
            New Transazione With {.ID = 1, .Descrizione = "Test 1", .Importo = 100D, .Data = DateTime.Today},
            New Transazione With {.ID = 2, .Descrizione = "Test 2", .Importo = -50D, .Data = DateTime.Today}
        }
        
        _mockRepository.Setup(Function(r) r.GetAllAsync()).ReturnsAsync(expectedTransazioni)
        
        ' Act
        Dim result = Await _service.GetAllTransazioniAsync()
        
        ' Assert
        result.Should().HaveCount(2)
        result.Should().BeEquivalentTo(expectedTransazioni)
        _mockRepository.Verify(Function(r) r.GetAllAsync(), Times.Once)
    End Function
    
    <Test>
    Public Async Function CreateTransazioneAsync_ShouldReturnNewId_WhenTransazioneIsValid() As Task
        ' Arrange
        Dim transazione = New Transazione With {
            .Descrizione = "Test transazione",
            .Importo = 100D,
            .Data = DateTime.Today
        }
        Dim expectedId = 42
        
        _mockRepository.Setup(Function(r) r.InsertAsync(It.IsAny(Of Transazione)())).ReturnsAsync(expectedId)
        
        ' Act
        Dim result = Await _service.CreateTransazioneAsync(transazione)
        
        ' Assert
        result.Should().Be(expectedId)
        _mockRepository.Verify(Function(r) r.InsertAsync(transazione), Times.Once)
    End Function
    
    <Test>
    Public Sub CreateTransazioneAsync_ShouldThrowArgumentNullException_WhenTransazioneIsNull()
        ' Act & Assert
        Dim act As Func(Of Task) = Async Function() Await _service.CreateTransazioneAsync(Nothing)
        act.Should().ThrowAsync(Of ArgumentNullException)()
    End Sub
    
    <Test>
    Public Sub CreateTransazioneAsync_ShouldThrowArgumentException_WhenDescrizioneIsEmpty()
        ' Arrange
        Dim transazione = New Transazione With {
            .Descrizione = "",
            .Importo = 100D,
            .Data = DateTime.Today
        }
        
        ' Act & Assert
        Dim act As Func(Of Task) = Async Function() Await _service.CreateTransazioneAsync(transazione)
        act.Should().ThrowAsync(Of ArgumentException)().WithMessage("*descrizione*")
    End Sub
    
    <Test>
    Public Sub CreateTransazioneAsync_ShouldThrowArgumentException_WhenImportoIsZero()
        ' Arrange
        Dim transazione = New Transazione With {
            .Descrizione = "Test",
            .Importo = 0D,
            .Data = DateTime.Today
        }
        
        ' Act & Assert
        Dim act As Func(Of Task) = Async Function() Await _service.CreateTransazioneAsync(transazione)
        act.Should().ThrowAsync(Of ArgumentException)().WithMessage("*importo*")
    End Sub
    
    <Test>
    Public Sub CreateTransazioneAsync_ShouldThrowArgumentException_WhenDataIsInFuture()
        ' Arrange
        Dim transazione = New Transazione With {
            .Descrizione = "Test",
            .Importo = 100D,
            .Data = DateTime.Now.AddDays(2)
        }
        
        ' Act & Assert
        Dim act As Func(Of Task) = Async Function() Await _service.CreateTransazioneAsync(transazione)
        act.Should().ThrowAsync(Of ArgumentException)().WithMessage("*futuro*")
    End Sub
    
    <Test>
    Public Async Function GetTransazioniByPeriodoAsync_ShouldReturnFilteredData_WhenDatesAreValid() As Task
        ' Arrange
        Dim startDate = DateTime.Today.AddDays(-7)
        Dim endDate = DateTime.Today
        Dim expectedTransazioni = New List(Of Transazione) From {
            New Transazione With {.ID = 1, .Descrizione = "Test", .Importo = 100D, .Data = DateTime.Today}
        }
        
        _mockRepository.Setup(Function(r) r.GetByDateRangeAsync(startDate, endDate)).ReturnsAsync(expectedTransazioni)
        
        ' Act
        Dim result = Await _service.GetTransazioniByPeriodoAsync(startDate, endDate)
        
        ' Assert
        result.Should().BeEquivalentTo(expectedTransazioni)
        _mockRepository.Verify(Function(r) r.GetByDateRangeAsync(startDate, endDate), Times.Once)
    End Function
    
    <Test>
    Public Sub GetTransazioniByPeriodoAsync_ShouldThrowArgumentException_WhenStartDateIsAfterEndDate()
        ' Arrange
        Dim startDate = DateTime.Today
        Dim endDate = DateTime.Today.AddDays(-1)
        
        ' Act & Assert
        Dim act As Func(Of Task) = Async Function() Await _service.GetTransazioniByPeriodoAsync(startDate, endDate)
        act.Should().ThrowAsync(Of ArgumentException)().WithMessage("*precedente*")
    End Sub
    
    <Test>
    Public Async Function GetSaldoTotaleAsync_ShouldReturnCorrectSum_WhenTransactionsExist() As Task
        ' Arrange
        Dim transazioni = New List(Of Transazione) From {
            New Transazione With {.Importo = 100D},
            New Transazione With {.Importo = -50D},
            New Transazione With {.Importo = 25D}
        }
        
        _mockRepository.Setup(Function(r) r.GetAllAsync()).ReturnsAsync(transazioni)
        
        ' Act
        Dim result = Await _service.GetSaldoTotaleAsync()
        
        ' Assert
        result.Should().Be(75D)
    End Function
    
    <Test>
    Public Async Function GetEntrateUscitePeriodoAsync_ShouldReturnCorrectValues() As Task
        ' Arrange
        Dim startDate = DateTime.Today.AddDays(-7)
        Dim endDate = DateTime.Today
        Dim transazioni = New List(Of Transazione) From {
            New Transazione With {.Importo = 100D},  ' Entrata
            New Transazione With {.Importo = 200D},  ' Entrata
            New Transazione With {.Importo = -50D},  ' Uscita
            New Transazione With {.Importo = -75D}   ' Uscita
        }
        
        _mockRepository.Setup(Function(r) r.GetByDateRangeAsync(startDate, endDate)).ReturnsAsync(transazioni)
        
        ' Act
        Dim result = Await _service.GetEntrateUscitePeriodoAsync(startDate, endDate)
        
        ' Assert
        result.Entrate.Should().Be(300D)
        result.Uscite.Should().Be(125D)
    End Function
    
    <Test>
    Public Async Function SearchTransazioniAsync_ShouldReturnResults_WhenSearchTermIsValid() As Task
        ' Arrange
        Dim searchTerm = "test"
        Dim expectedResults = New List(Of Transazione) From {
            New Transazione With {.ID = 1, .Descrizione = "Test transaction"}
        }
        
        _mockRepository.Setup(Function(r) r.SearchByDescrizioneAsync(searchTerm)).ReturnsAsync(expectedResults)
        
        ' Act
        Dim result = Await _service.SearchTransazioniAsync(searchTerm)
        
        ' Assert
        result.Should().BeEquivalentTo(expectedResults)
    End Function
    
    <Test>
    Public Sub SearchTransazioniAsync_ShouldThrowArgumentException_WhenSearchTermIsEmpty()
        ' Act & Assert
        Dim act As Func(Of Task) = Async Function() Await _service.SearchTransazioniAsync("")
        act.Should().ThrowAsync(Of ArgumentException)().WithMessage("*vuoto*")
    End Sub
End Class