Public Enum TransactionNecessityLevel
    Unknown = 0
    Essential = 1
    Important = 2
    Desirable = 3
End Enum

Public Enum TransactionFrequency
    Unknown = 0
    Daily = 1
    Weekly = 2
    Monthly = 3
    Quarterly = 4
    Yearly = 5
    Occasional = 6
End Enum

Public Enum TransactionSeasonality
    Unknown = 0
    AllYear = 1
    Spring = 2
    Summer = 3
    Autumn = 4
    Winter = 5
End Enum

Public Enum ImportStatus
    Pending = 0
    InProgress = 1
    Completed = 2
    Failed = 3
    Cancelled = 4
End Enum

Public Enum ValidationResult
    Valid = 0
    Warning = 1
    ValidationError = 2
    Critical = 3
End Enum

Public Enum SupportedFileType
    Csv = 0
    Excel = 1
    Text = 2
End Enum

Public Enum LogLevel
    Debug = 0
    Information = 1
    Warning = 2
    [Error] = 3
    Critical = 4
End Enum

Public Enum ApiProvider
    OpenAI = 0
    Claude = 1
    GooglePlaces = 2
End Enum

Public Enum ClassificationConfidence
    VeryLow = 0
    Low = 25
    Medium = 50
    High = 75
    VeryHigh = 90
End Enum