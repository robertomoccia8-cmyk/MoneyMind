Imports NUnit.Framework
Imports Moq
Imports FluentAssertions
Imports System.IO

<TestFixture>
Public Class SecureConfigurationManagerTests
    Private _mockLogger As Mock(Of ILogger)
    Private _tempConfigPath As String
    Private _configManager As SecureConfigurationManager
    
    <SetUp>
    Public Sub Setup()
        _mockLogger = New Mock(Of ILogger)()
        _tempConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.enc")
        _configManager = New SecureConfigurationManager(_mockLogger.Object, _tempConfigPath)
    End Sub
    
    <TearDown>
    Public Sub TearDown()
        Try
            If File.Exists(_tempConfigPath) Then
                File.Delete(_tempConfigPath)
            End If
        Catch
            ' Ignora errori di pulizia
        End Try
        _configManager = Nothing
    End Sub
    
    <Test>
    Public Sub SetSecureValue_ShouldStoreAndRetrieveValue_WhenValidKeyValue()
        ' Arrange
        Dim key = "TestKey"
        Dim value = "TestValue123!"
        
        ' Act
        _configManager.SetSecureValue(key, value)
        Dim retrievedValue = _configManager.GetSecureValue(key)
        
        ' Assert
        retrievedValue.Should().Be(value)
    End Sub
    
    <Test>
    Public Sub SetSecureValue_ShouldCreateConfigFile_WhenFileDoesNotExist()
        ' Arrange
        Dim key = "TestKey"
        Dim value = "TestValue"
        
        ' Act
        _configManager.SetSecureValue(key, value)
        
        ' Assert
        File.Exists(_tempConfigPath).Should().BeTrue()
    End Sub
    
    <Test>
    Public Sub GetSecureValue_ShouldReturnNothing_WhenKeyDoesNotExist()
        ' Act
        Dim result = _configManager.GetSecureValue("NonExistentKey")
        
        ' Assert
        result.Should().BeNull()
    End Sub
    
    <Test>
    Public Sub HasValue_ShouldReturnTrue_WhenKeyExists()
        ' Arrange
        Dim key = "ExistingKey"
        Dim value = "SomeValue"
        _configManager.SetSecureValue(key, value)
        
        ' Act
        Dim result = _configManager.HasValue(key)
        
        ' Assert
        result.Should().BeTrue()
    End Sub
    
    <Test>
    Public Sub HasValue_ShouldReturnFalse_WhenKeyDoesNotExist()
        ' Act
        Dim result = _configManager.HasValue("NonExistentKey")
        
        ' Assert
        result.Should().BeFalse()
    End Sub
    
    <Test>
    Public Sub DeleteSecureValue_ShouldRemoveKey_WhenKeyExists()
        ' Arrange
        Dim key = "KeyToDelete"
        Dim value = "ValueToDelete"
        _configManager.SetSecureValue(key, value)
        
        ' Act
        _configManager.DeleteSecureValue(key)
        
        ' Assert
        _configManager.HasValue(key).Should().BeFalse()
        _configManager.GetSecureValue(key).Should().BeNull()
    End Sub
    
    <Test>
    Public Sub SetSecureValue_ShouldHandleSpecialCharacters()
        ' Arrange
        Dim key = "SpecialKey"
        Dim value = "Value with special chars: àèìòù €$£@#%^&*()[]{}|;:',.<>?/~`"
        
        ' Act
        _configManager.SetSecureValue(key, value)
        Dim retrievedValue = _configManager.GetSecureValue(key)
        
        ' Assert
        retrievedValue.Should().Be(value)
    End Sub
    
    <Test>
    Public Sub SetSecureValue_ShouldHandleLongValues()
        ' Arrange
        Dim key = "LongValueKey"
        Dim value = New String("A"c, 10000) ' 10KB string
        
        ' Act
        _configManager.SetSecureValue(key, value)
        Dim retrievedValue = _configManager.GetSecureValue(key)
        
        ' Assert
        retrievedValue.Should().Be(value)
    End Sub
    
    <Test>
    Public Sub SetSecureValue_ShouldOverwriteExistingValue()
        ' Arrange
        Dim key = "OverwriteKey"
        Dim originalValue = "OriginalValue"
        Dim newValue = "NewValue"
        
        ' Act
        _configManager.SetSecureValue(key, originalValue)
        _configManager.SetSecureValue(key, newValue)
        Dim retrievedValue = _configManager.GetSecureValue(key)
        
        ' Assert
        retrievedValue.Should().Be(newValue)
        retrievedValue.Should().NotBe(originalValue)
    End Sub
    
    <Test>
    Public Sub SetSecureValue_ShouldDeleteKey_WhenValueIsEmpty()
        ' Arrange
        Dim key = "KeyToEmpty"
        Dim value = "InitialValue"
        _configManager.SetSecureValue(key, value)
        
        ' Act
        _configManager.SetSecureValue(key, "")
        
        ' Assert
        _configManager.HasValue(key).Should().BeFalse()
    End Sub
    
    <Test>
    Public Sub SetSecureValue_ShouldThrowArgumentException_WhenKeyIsEmpty()
        ' Act & Assert
        Dim act As Action = Sub() _configManager.SetSecureValue("", "value")
        act.Should().Throw(Of ArgumentException)()
    End Sub
    
    <Test>
    Public Sub GetSecureValue_ShouldThrowArgumentException_WhenKeyIsEmpty()
        ' Act & Assert
        Dim act As Action = Sub() _configManager.GetSecureValue("")
        act.Should().Throw(Of ArgumentException)()
    End Sub
    
    <Test>
    Public Sub EncryptionDecryption_ShouldBeDeterministic_ForSameInput()
        ' Arrange
        Dim key = "DeterministicKey"
        Dim value = "TestValue"
        
        ' Act - Set and get multiple times
        _configManager.SetSecureValue(key, value)
        Dim result1 = _configManager.GetSecureValue(key)
        
        _configManager.SetSecureValue(key, value)
        Dim result2 = _configManager.GetSecureValue(key)
        
        ' Assert
        result1.Should().Be(value)
        result2.Should().Be(value)
        result1.Should().Be(result2)
    End Sub
    
    <Test>
    Public Sub ConfigPersistence_ShouldSurviveManagerRecreation()
        ' Arrange
        Dim key = "PersistenceKey"
        Dim value = "PersistentValue"
        
        ' Act - Set value with first manager
        _configManager.SetSecureValue(key, value)
        _configManager = Nothing
        
        ' Create new manager with same config path
        Dim newManager = New SecureConfigurationManager(_mockLogger.Object, _tempConfigPath)
        Dim retrievedValue = newManager.GetSecureValue(key)
        
        ' Assert
        retrievedValue.Should().Be(value)
    End Sub
End Class