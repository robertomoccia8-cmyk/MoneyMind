Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Configuration
Imports System.IO

Module Program
    Public ServiceProvider As IServiceProvider
    Private _host As IHost
    
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        
        Try
            ' Configura il sistema di dependency injection e logging
            ConfigureServices()
            
            ' Inizializza il database
            InitializeDatabase()
            
            ' Avvia l'applicazione
            Dim mainForm = ServiceProvider.GetRequiredService(Of FormFinanza)()
            Application.Run(mainForm)
            
        Catch ex As Exception
            MessageBox.Show($"Errore critico durante l'avvio dell'applicazione: {ex.Message}", 
                          "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If _host IsNot Nothing Then
                _host.Dispose()
            End If
        End Try
    End Sub
    
    Private Sub ConfigureServices()
        Dim builder = Host.CreateDefaultBuilder()
        
        builder.ConfigureAppConfiguration(Sub(context, config)
            config.SetBasePath(Directory.GetCurrentDirectory())
            config.AddJsonFile("appsettings.json", optional:=False, reloadOnChange:=True)
            config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional:=True)
            config.AddEnvironmentVariables()
        End Sub)
        
        builder.ConfigureServices(Sub(context, services)
            Dim configuration = context.Configuration
            
            ' Registra configurazione
            services.AddSingleton(configuration)
            
            ' Registra logging
            services.AddLogging(Sub(logging)
                logging.ClearProviders()
                logging.AddConsole()
                logging.AddDebug()
                
                ' Aggiungi file logging personalizzato
                Dim logPath = configuration.GetValue("Logging:File:Path", "Logs/app.log")
                Dim minLevel = [Enum].Parse(Of FileLogger.LogLevel)(
                    configuration.GetValue("Logging:File:MinLevel", "Information"))
                
                Dim fullLogPath = Path.Combine(Application.StartupPath, logPath)
                Dim fileLogger = New FileLogger(fullLogPath, minLevel)
                services.AddSingleton(Of ILogger)(fileLogger)
            End Sub)
            
            ' Registra i repository
            Dim connectionString = configuration.GetConnectionString("Default") ??
                                 configuration.GetValue("Database:ConnectionString", "Data Source=Data/finanze.db;Version=3;")
            
            services.AddSingleton(Of ITransazioneRepository)(Function(sp)
                Return New TransazioneRepository(connectionString)
            End Function)
            
            ' Registra i servizi business
            services.AddScoped(Of TransazioneService)()
            
            ' Registra servizi di sicurezza
            services.AddSingleton(Of ISecureConfigurationManager)(Function(sp)
                Dim logger = sp.GetRequiredService(Of ILogger)()
                Dim configPath = configuration.GetValue("Security:SecureConfigPath", "config.enc")
                Dim fullConfigPath = Path.Combine(Application.StartupPath, configPath)
                Return New SecureConfigurationManager(logger, fullConfigPath)
            End Function)
            
            ' Registra i form
            services.AddTransient(Of FormFinanza)()
            services.AddTransient(Of FormAnalisi)()
            services.AddTransient(Of FormGrafici)()
            services.AddTransient(Of FormImpostazioniGenerali)()
            services.AddTransient(Of FormGestionePattern)()
            
            ' Registra servizi AI
            services.AddScoped(Of GptClassificatoreTransazioni)()
            services.AddScoped(Of GooglePlacesWrapper)()
            
            ' Registra altri servizi
            services.AddScoped(Of ClassificatoreTransazioni)()
            services.AddScoped(Of ImportatoreUniversale)()
            services.AddScoped(Of GestoreStipendi)()
        End Sub)
        
        _host = builder.Build()
        ServiceProvider = _host.Services
    End Sub
    
    Private Sub InitializeDatabase()
        Try
            Dim logger = ServiceProvider.GetRequiredService(Of ILogger)()
            logger.LogInformation("Inizializzazione database...")
            
            ' Inizializza il database usando il nuovo sistema
            DatabaseManager.InitializeDatabase()
            
            ' Inizializza le tabelle per le API keys
            GestoreApiKeys.InizializzaTabella()
            
            logger.LogInformation("Database inizializzato con successo")
            
        Catch ex As Exception
            Dim logger = ServiceProvider.GetRequiredService(Of ILogger)()
            logger.LogCritical(ex, "Errore critico nell'inizializzazione del database")
            Throw
        End Try
    End Sub
End Module