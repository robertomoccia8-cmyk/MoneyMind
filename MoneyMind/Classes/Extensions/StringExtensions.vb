Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Public Module StringExtensions
    
    <Extension>
    Public Function IsNullOrWhiteSpace(value As String) As Boolean
        Return String.IsNullOrWhiteSpace(value)
    End Function
    
    <Extension>
    Public Function IsValidEmail(email As String) As Boolean
        If String.IsNullOrWhiteSpace(email) Then
            Return False
        End If
        
        Try
            Dim emailPattern = "^[^@\s]+@[^@\s]+\.[^@\s]+$"
            Return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase)
        Catch
            Return False
        End Try
    End Function
    
    <Extension>
    Public Function ToTitleCase(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return value
        End If
        
        Return Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower())
    End Function
    
    <Extension>
    Public Function TruncateWithEllipsis(value As String, maxLength As Integer) As String
        If String.IsNullOrEmpty(value) OrElse value.Length <= maxLength Then
            Return value
        End If
        
        Return value.Substring(0, maxLength - 3) + "..."
    End Function
    
    <Extension>
    Public Function CleanCurrency(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return "0"
        End If
        
        ' Remove common currency symbols and whitespace
        Dim cleaned = Regex.Replace(value, "[€$£¥¢₹₽¥\s]", "")
        
        ' Replace comma with dot for decimal point
        cleaned = cleaned.Replace(",", ".")
        
        ' Remove any non-digit, non-dot, non-minus characters
        cleaned = Regex.Replace(cleaned, "[^0-9.\-]", "")
        
        Return If(String.IsNullOrEmpty(cleaned), "0", cleaned)
    End Function
    
    <Extension>
    Public Function ExtractKeywords(text As String, Optional minLength As Integer = 3) As List(Of String)
        If String.IsNullOrWhiteSpace(text) Then
            Return New List(Of String)()
        End If
        
        ' Remove special characters and split by whitespace
        Dim cleanText = Regex.Replace(text, "[^a-zA-ZÀ-ÿ0-9\s]", " ")
        Dim words = cleanText.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        
        ' Filter by minimum length and common stop words
        Dim stopWords = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "il", "la", "lo", "le", "gli", "un", "una", "di", "da", "in", "con", "su", "per", "tra", "fra",
            "a", "e", "o", "ma", "se", "che", "del", "della", "dei", "delle", "dal", "dalla", "dai", "dalle",
            "nel", "nella", "nei", "nelle", "sul", "sulla", "sui", "sulle", "col", "colla", "the", "and", "or",
            "but", "if", "of", "to", "for", "with", "on", "at", "by", "from", "as", "is", "are", "was", "were"
        }
        
        Return words.Where(Function(w) w.Length >= minLength AndAlso Not stopWords.Contains(w)) _
                   .Select(Function(w) w.ToLower()) _
                   .Distinct() _
                   .ToList()
    End Function
    
    <Extension>
    Public Function NormalizeSpacing(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return String.Empty
        End If
        
        ' Replace multiple consecutive whitespace characters with single space
        Return Regex.Replace(value.Trim(), "\s+", " ")
    End Function
    
    <Extension>
    Public Function ToSnakeCase(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return String.Empty
        End If
        
        ' Insert underscore before capital letters (except the first character)
        Dim result = Regex.Replace(value, "(?<!^)([A-Z])", "_$1")
        
        ' Convert to lowercase
        Return result.ToLower()
    End Function
    
    <Extension>
    Public Function ToPascalCase(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return String.Empty
        End If
        
        ' Split by non-alphanumeric characters
        Dim words = Regex.Split(value, "[^a-zA-Z0-9]")
        
        ' Capitalize first letter of each word
        Dim result = String.Join("", words.Where(Function(w) Not String.IsNullOrEmpty(w)) _
                                            .Select(Function(w) Char.ToUpper(w(0)) + w.Substring(1).ToLower()))
        
        Return result
    End Function
    
    <Extension>
    Public Function ContainsAny(value As String, searchTerms As String()) As Boolean
        If String.IsNullOrWhiteSpace(value) OrElse searchTerms Is Nothing OrElse searchTerms.Length = 0 Then
            Return False
        End If
        
        Return searchTerms.Any(Function(term) Not String.IsNullOrWhiteSpace(term) AndAlso 
                                            value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
    End Function
    
    <Extension>
    Public Function Mask(value As String, Optional visibleChars As Integer = 4) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return String.Empty
        End If
        
        If value.Length <= visibleChars Then
            Return New String("*"c, value.Length)
        End If
        
        Dim maskLength = value.Length - visibleChars
        Return New String("*"c, maskLength) + value.Substring(maskLength)
    End Function
    
End Module