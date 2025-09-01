Imports System.Security.Cryptography
Imports System.Text
Imports System.Data.SQLite

Public Class GestoreApiKeys
    Private Shared ReadOnly EncryptionKey As String = "ChiaveSegretaPerFinanze2024!" ' In produzione usa una chiave più sicura

    Public Shared Sub InizializzaTabella()
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd As New SQLiteCommand("
                CREATE TABLE IF NOT EXISTS ApiKeys (
                    Nome TEXT PRIMARY KEY,
                    ChiaveCriptata TEXT NOT NULL,
                    DataModifica DATETIME DEFAULT CURRENT_TIMESTAMP
                )", conn)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Sub SalvaChiaveApi(nome As String, chiave As String)
        Dim chiaveCriptata = CriptaChiave(chiave)
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd As New SQLiteCommand("
                INSERT OR REPLACE INTO ApiKeys (Nome, ChiaveCriptata, DataModifica) 
                VALUES (@nome, @chiave, @data)", conn)
                cmd.Parameters.AddWithValue("@nome", nome)
                cmd.Parameters.AddWithValue("@chiave", chiaveCriptata)
                cmd.Parameters.AddWithValue("@data", DateTime.Now)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Function CaricaChiaveApi(nome As String) As String
        Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using cmd As New SQLiteCommand("SELECT ChiaveCriptata FROM ApiKeys WHERE Nome = @nome", conn)
                cmd.Parameters.AddWithValue("@nome", nome)
                Dim risultato = cmd.ExecuteScalar()
                If risultato IsNot Nothing Then
                    Return DecriptaChiave(risultato.ToString())
                End If
            End Using
        End Using
        Return String.Empty
    End Function

    Public Shared Function VerificaChiaveEsiste(nome As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(CaricaChiaveApi(nome))
    End Function
    Private Shared Function DerivaChiave() As Byte()
        Using sha As SHA256 = SHA256.Create()
            Dim keyBytes = Encoding.UTF8.GetBytes(EncryptionKey)
            Return sha.ComputeHash(keyBytes)   ' 32 byte garantiti
        End Using
    End Function

    Private Shared Function CriptaChiave(plainText As String) As String
        If String.IsNullOrEmpty(plainText) Then Return ""
        Dim data = Encoding.UTF8.GetBytes(plainText)
        Using aes As Aes = Aes.Create()
            aes.Key = DerivaChiave()
            aes.IV = New Byte(15) {}    ' 16 byte di zero
            Using encryptor = aes.CreateEncryptor()
                Dim encrypted = encryptor.TransformFinalBlock(data, 0, data.Length)
                Return Convert.ToBase64String(encrypted)
            End Using
        End Using
    End Function

    Private Shared Function DecriptaChiave(cipherText As String) As String
        If String.IsNullOrEmpty(cipherText) Then Return ""
        Try
            Dim data = Convert.FromBase64String(cipherText)
            Using aes As Aes = Aes.Create()
                aes.Key = DerivaChiave()
                aes.IV = New Byte(15) {}  ' 16 byte di zero
                Using decryptor = aes.CreateDecryptor()
                    Dim decrypted = decryptor.TransformFinalBlock(data, 0, data.Length)
                    Return Encoding.UTF8.GetString(decrypted)
                End Using
            End Using
        Catch
            Return ""
        End Try
    End Function
End Class
