Imports System.Runtime.CompilerServices

Public Module DateTimeExtensions
    
    <Extension>
    Public Function IsWeekend(dateValue As DateTime) As Boolean
        Return dateValue.DayOfWeek = DayOfWeek.Saturday OrElse dateValue.DayOfWeek = DayOfWeek.Sunday
    End Function
    
    <Extension>
    Public Function IsWeekday(dateValue As DateTime) As Boolean
        Return Not dateValue.IsWeekend()
    End Function
    
    <Extension>
    Public Function StartOfMonth(dateValue As DateTime) As DateTime
        Return New DateTime(dateValue.Year, dateValue.Month, 1)
    End Function
    
    <Extension>
    Public Function EndOfMonth(dateValue As DateTime) As DateTime
        Return dateValue.StartOfMonth().AddMonths(1).AddDays(-1)
    End Function
    
    <Extension>
    Public Function StartOfWeek(dateValue As DateTime, Optional firstDayOfWeek As DayOfWeek = DayOfWeek.Monday) As DateTime
        Dim daysFromFirstDay = (7 + dateValue.DayOfWeek - firstDayOfWeek) Mod 7
        Return dateValue.AddDays(-daysFromFirstDay).Date
    End Function
    
    <Extension>
    Public Function EndOfWeek(dateValue As DateTime, Optional firstDayOfWeek As DayOfWeek = DayOfWeek.Monday) As DateTime
        Return dateValue.StartOfWeek(firstDayOfWeek).AddDays(6)
    End Function
    
    <Extension>
    Public Function StartOfYear(dateValue As DateTime) As DateTime
        Return New DateTime(dateValue.Year, 1, 1)
    End Function
    
    <Extension>
    Public Function EndOfYear(dateValue As DateTime) As DateTime
        Return New DateTime(dateValue.Year, 12, 31)
    End Function
    
    <Extension>
    Public Function StartOfQuarter(dateValue As DateTime) As DateTime
        Dim quarterStartMonth = ((dateValue.Month - 1) \ 3) * 3 + 1
        Return New DateTime(dateValue.Year, quarterStartMonth, 1)
    End Function
    
    <Extension>
    Public Function EndOfQuarter(dateValue As DateTime) As DateTime
        Return dateValue.StartOfQuarter().AddMonths(3).AddDays(-1)
    End Function
    
    <Extension>
    Public Function GetQuarter(dateValue As DateTime) As Integer
        Return (dateValue.Month - 1) \ 3 + 1
    End Function
    
    <Extension>
    Public Function GetWeekOfYear(dateValue As DateTime, Optional firstDayOfWeek As DayOfWeek = DayOfWeek.Monday) As Integer
        Dim culture = Globalization.CultureInfo.CurrentCulture
        Dim calendarWeekRule = If(firstDayOfWeek = DayOfWeek.Monday, 
                                 Globalization.CalendarWeekRule.FirstFourDayWeek,
                                 Globalization.CalendarWeekRule.FirstDay)
        
        Return culture.Calendar.GetWeekOfYear(dateValue, calendarWeekRule, firstDayOfWeek)
    End Function
    
    <Extension>
    Public Function ToItalianFormat(dateValue As DateTime) As String
        Return dateValue.ToString("dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture)
    End Function
    
    <Extension>
    Public Function ToItalianDateTimeFormat(dateValue As DateTime) As String
        Return dateValue.ToString("dd/MM/yyyy HH:mm:ss", Globalization.CultureInfo.InvariantCulture)
    End Function
    
    <Extension>
    Public Function ToShortItalianFormat(dateValue As DateTime) As String
        Return dateValue.ToString("dd/MM/yy", Globalization.CultureInfo.InvariantCulture)
    End Function
    
    <Extension>
    Public Function ToMonthYearFormat(dateValue As DateTime) As String
        Return dateValue.ToString("MM/yyyy", Globalization.CultureInfo.InvariantCulture)
    End Function
    
    <Extension>
    Public Function IsInRange(dateValue As DateTime, startDate As DateTime, endDate As DateTime) As Boolean
        Return dateValue >= startDate AndAlso dateValue <= endDate
    End Function
    
    <Extension>
    Public Function GetBusinessDaysUntil(startDate As DateTime, endDate As DateTime) As Integer
        If startDate >= endDate Then
            Return 0
        End If
        
        Dim businessDays = 0
        Dim currentDate = startDate.Date
        
        While currentDate < endDate.Date
            If currentDate.IsWeekday() Then
                businessDays += 1
            End If
            currentDate = currentDate.AddDays(1)
        End While
        
        Return businessDays
    End Function
    
    <Extension>
    Public Function AddBusinessDays(startDate As DateTime, businessDays As Integer) As DateTime
        Dim currentDate = startDate.Date
        Dim addedDays = 0
        
        While addedDays < businessDays
            currentDate = currentDate.AddDays(1)
            If currentDate.IsWeekday() Then
                addedDays += 1
            End If
        End While
        
        Return currentDate
    End Function
    
    <Extension>
    Public Function GetSeason(dateValue As DateTime) As TransactionSeasonality
        Dim month = dateValue.Month
        
        Select Case month
            Case 12, 1, 2
                Return TransactionSeasonality.Winter
            Case 3, 4, 5
                Return TransactionSeasonality.Spring
            Case 6, 7, 8
                Return TransactionSeasonality.Summer
            Case 9, 10, 11
                Return TransactionSeasonality.Autumn
            Case Else
                Return TransactionSeasonality.Unknown
        End Select
    End Function
    
    <Extension>
    Public Function IsHoliday(dateValue As DateTime) As Boolean
        ' Italian holidays
        Dim year = dateValue.Year
        
        ' Fixed holidays
        Dim fixedHolidays = New DateTime() {
            New DateTime(year, 1, 1),   ' Capodanno
            New DateTime(year, 1, 6),   ' Epifania
            New DateTime(year, 4, 25),  ' Festa della Liberazione
            New DateTime(year, 5, 1),   ' Festa del Lavoro
            New DateTime(year, 6, 2),   ' Festa della Repubblica
            New DateTime(year, 8, 15),  ' Ferragosto
            New DateTime(year, 11, 1),  ' Ognissanti
            New DateTime(year, 12, 8),  ' Immacolata
            New DateTime(year, 12, 25), ' Natale
            New DateTime(year, 12, 26)  ' Santo Stefano
        }
        
        ' Check fixed holidays
        If fixedHolidays.Contains(dateValue.Date) Then
            Return True
        End If
        
        ' Easter-based holidays (simplified calculation)
        Dim easter = CalculateEaster(year)
        Dim easterHolidays = New DateTime() {
            easter.AddDays(1),  ' Lunedì dell'Angelo
            easter.AddDays(39), ' Ascensione (some regions)
            easter.AddDays(50)  ' Lunedì di Pentecoste (some regions)
        }
        
        Return easterHolidays.Contains(dateValue.Date)
    End Function
    
    Private Function CalculateEaster(year As Integer) As DateTime
        ' Simplified Easter calculation (Gregorian calendar)
        Dim a = year Mod 19
        Dim b = year \ 100
        Dim c = year Mod 100
        Dim d = b \ 4
        Dim e = b Mod 4
        Dim f = (b + 8) \ 25
        Dim g = (b - f + 1) \ 3
        Dim h = (19 * a + b - d - g + 15) Mod 30
        Dim i = c \ 4
        Dim k = c Mod 4
        Dim l = (32 + 2 * e + 2 * i - h - k) Mod 7
        Dim m = (a + 11 * h + 22 * l) \ 451
        Dim n = (h + l - 7 * m + 114) \ 31
        Dim p = (h + l - 7 * m + 114) Mod 31
        
        Return New DateTime(year, n, p + 1)
    End Function
    
    <Extension>
    Public Function ToRelativeString(dateValue As DateTime) As String
        Dim now = DateTime.Now
        Dim difference = now - dateValue
        
        If difference.TotalDays < 1 Then
            If difference.TotalHours < 1 Then
                If difference.TotalMinutes < 1 Then
                    Return "Proprio ora"
                Else
                    Return $"{Math.Floor(difference.TotalMinutes)} minuti fa"
                End If
            Else
                Return $"{Math.Floor(difference.TotalHours)} ore fa"
            End If
        ElseIf difference.TotalDays < 7 Then
            Return $"{Math.Floor(difference.TotalDays)} giorni fa"
        ElseIf difference.TotalDays < 30 Then
            Return $"{Math.Floor(difference.TotalDays / 7)} settimane fa"
        ElseIf difference.TotalDays < 365 Then
            Return $"{Math.Floor(difference.TotalDays / 30)} mesi fa"
        Else
            Return $"{Math.Floor(difference.TotalDays / 365)} anni fa"
        End If
    End Function
    
End Module