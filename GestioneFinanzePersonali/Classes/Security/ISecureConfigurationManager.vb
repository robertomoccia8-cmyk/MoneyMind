Namespace Security
    Public Interface ISecureConfigurationManager
    Function GetSecureValue(key As String) As String
    Sub SetSecureValue(key As String, value As String)
    Sub DeleteSecureValue(key As String)
    Function HasValue(key As String) As Boolean
    Sub InitializeEncryption()
    End Interface
End Namespace