Public Module ApplicationConstants
    ' Database constants
    Public Const DEFAULT_CONNECTION_STRING As String = "Data Source=Data/finanze.db;Version=3;"
    Public Const DATABASE_FOLDER As String = "Data"
    Public Const DATABASE_FILE As String = "finanze.db"
    
    ' Logging constants
    Public Const LOG_FOLDER As String = "Logs"
    Public Const LOG_FILE As String = "GestioneFinanze.log"
    Public Const MAX_LOG_FILE_SIZE As Long = 10 * 1024 * 1024 ' 10MB
    Public Const MAX_LOG_FILES_COUNT As Integer = 5
    
    ' Security constants
    Public Const CONFIG_FILE As String = "config.enc"
    Public Const ENCRYPTION_KEY_SOURCE As String = "Machine"
    
    ' API constants
    Public Const OPENAI_API_KEY As String = "OpenAI_ApiKey"
    Public Const GOOGLE_API_KEY As String = "Google_ApiKey"
    Public Const CLAUDE_API_KEY As String = "Claude_ApiKey"
    
    ' OpenAI configuration
    Public Const OPENAI_BASE_URL As String = "https://api.openai.com/v1"
    Public Const OPENAI_MODEL As String = "gpt-4-turbo-preview"
    Public Const OPENAI_MAX_TOKENS As Integer = 1000
    Public Const OPENAI_TIMEOUT_SECONDS As Integer = 30
    
    ' Claude configuration
    Public Const CLAUDE_BASE_URL As String = "https://api.anthropic.com/v1"
    Public Const CLAUDE_MODEL As String = "claude-sonnet-4-0"
    Public Const CLAUDE_MAX_TOKENS As Integer = 1000
    Public Const CLAUDE_TIMEOUT_SECONDS As Integer = 30
    
    ' Google Places configuration
    Public Const GOOGLE_PLACES_BASE_URL As String = "https://maps.googleapis.com/maps/api/place"
    Public Const GOOGLE_PLACES_TIMEOUT_SECONDS As Integer = 15
    
    ' UI constants
    Public Const DEFAULT_DATE_FORMAT As String = "dd/MM/yyyy"
    Public Const DEFAULT_CURRENCY_FORMAT As String = "€ #,##0.00"
    Public Const DEFAULT_REFRESH_INTERVAL_SECONDS As Integer = 30
    
    ' Import/Export constants
    Public Const MAX_IMPORT_ERRORS As Integer = 100
    Public Const IMPORT_PREVIEW_MAX_RECORDS As Integer = 100
    Public ReadOnly SUPPORTED_FILE_EXTENSIONS() As String = {".csv", ".xlsx", ".xls", ".txt"}
    
    ' Batch processing constants
    Public Const MAX_CONCURRENT_AI_REQUESTS As Integer = 5
    Public Const MAX_CONCURRENT_DB_OPERATIONS As Integer = 10
    Public Const BATCH_SIZE As Integer = 100
    
    ' Validation constants
    Public Const MIN_TRANSACTION_AMOUNT As Decimal = 0.01D
    Public Const MAX_TRANSACTION_AMOUNT As Decimal = 999999999.99D
    Public Const MAX_DESCRIPTION_LENGTH As Integer = 500
    Public Const MAX_CATEGORY_LENGTH As Integer = 100
    
    ' Feature flags
    Public Const ENABLE_AI_CLASSIFICATION_DEFAULT As Boolean = True
    Public Const ENABLE_GOOGLE_PLACES_LOOKUP_DEFAULT As Boolean = True
    Public Const ENABLE_ADVANCED_ANALYTICS_DEFAULT As Boolean = True
    Public Const ENABLE_AUTO_BACKUP_DEFAULT As Boolean = False
    Public Const ENABLE_DARK_MODE_DEFAULT As Boolean = False
    
    ' Rate limiting
    Public Const OPENAI_RATE_LIMIT_PER_MINUTE As Integer = 20
    Public Const GOOGLE_RATE_LIMIT_PER_MINUTE As Integer = 100
    Public Const CLAUDE_RATE_LIMIT_PER_MINUTE As Integer = 15
End Module