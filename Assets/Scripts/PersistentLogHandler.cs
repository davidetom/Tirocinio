using System.IO;
using UnityEngine;

/// <summary>
/// Questo script intercetta tutti i log di Unity (Debug.Log, Debug.Warning, etc.)
/// e li scrive in un file di testo timestampato nella cartella Application.persistentDataPath.
/// </summary>
public class PersistentLogHandler : MonoBehaviour
{
    /// <summary>
    /// Prefisso per il nome del file di log. Il timestamp verrà aggiunto a questo.
    /// </summary>
    [HideInInspector]
    public string logFileNamePrefix = "debuglog";

    // Oggetto per scrivere sul file
    private StreamWriter streamWriter;
    // Percorso completo della cartella dei log
    private string logDirectory;
    // Percorso completo del file di log corrente
    private string logFilePath;

    /// <summary>
    /// Impostiamo un pattern Singleton per assicurarci che esista un solo logger
    /// e che persista tra le scene.
    /// </summary>
    public static PersistentLogHandler Instance { get; private set; }

    void Awake()
    {
        // Pattern Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Rende questo oggetto persistente tra le scene

            // Configura e avvia il logger
            SetupLogger();
        }
        else
        {
            // Esiste già un'istanza, distruggi questo duplicato
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Sottoscrivi l'evento per ricevere i messaggi di log
        // Questo è il cuore del sistema
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // Rimuovi la sottoscrizione quando l'oggetto è disabilitato o distrutto
        Application.logMessageReceived -= HandleLog;
    }

    void OnDestroy()
    {
        // Assicurati di chiudere il file stream quando l'app si chiude o l'oggetto viene distrutto
        if (streamWriter != null)
        {
            streamWriter.WriteLine($"--- Log terminato alle: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter = null;
        }
    }

    /// <summary>
    /// Configura le cartelle e il file di log all'avvio.
    /// </summary>
    private void SetupLogger()
    {
        try
        {
            // 1. Definisci la cartella dei log dentro il persistentDataPath
            // Esempio su Quest: /storage/emulated/0/Android/data/com.tuacompagnia.tuaapp/files/Logs
            logDirectory = Path.Combine(Application.persistentDataPath, "Logs");

            // 2. Crea la cartella se non esiste
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 3. Crea un nome file unico con un timestamp
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{logFileNamePrefix}_{timestamp}.txt";
            logFilePath = Path.Combine(logDirectory, fileName);

            // 4. Apri un flusso di scrittura (StreamWriter) per il file
            // Il 'true' significa "append" (aggiungi in fondo), anche se qui apriamo un file nuovo
            streamWriter = new StreamWriter(logFilePath, true);
            streamWriter.AutoFlush = true; // Assicura che i dati vengano scritti subito

            // Scrivi un'intestazione nel log per sapere quando è iniziata la sessione
            streamWriter.WriteLine($"--- Log avviato alle: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
            streamWriter.WriteLine($"Piattaforma: {Application.platform}");
            streamWriter.WriteLine($"Device: {SystemInfo.deviceName}");
            streamWriter.WriteLine("--------------------------------------------------\n");

            // Logga un messaggio per confermare che il logger è attivo
            Debug.Log($"[PersistentLogHandler] Avviato. Scrittura log su: {logFilePath}");
        }
        catch (System.Exception e)
        {
            // Se c'è un errore nell'aprire il file, loggalo nella console (anche se non verrà salvato)
            Debug.LogError($"[PersistentLogHandler] Errore nell'inizializzazione del logger: {e.Message}");
            streamWriter = null; // Assicurati che non proviamo a scrivere su un file non valido
        }
    }

    /// <summary>
    /// Chiamato per ogni Debug.Log, Debug.Warning, Debug.Error, etc.
    /// </summary>
    /// <param name="logString">Il messaggio di log.</param>
    /// <param name="stackTrace">La traccia dello stack (importante per gli errori).</param>
    /// <param name="type">Il tipo di log (Log, Warning, Error, Exception, Assert).</param>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (streamWriter == null)
        {
            return; // Non fare nulla se il logger non è stato inizializzato correttamente
        }

        try
        {
            // Formatta il messaggio da scrivere sul file
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            string formattedLog = $"[{timestamp}] [{type}] {logString}";

            // Scrivi il messaggio sul file
            streamWriter.WriteLine(formattedLog);

            // Se è un errore o un'eccezione, includi anche lo stack trace
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                streamWriter.WriteLine($"Stack Trace:\n{stackTrace}");
            }

            // Aggiungi una riga vuota per leggibilità
            streamWriter.WriteLine(); 
        }
        catch (System.Exception e)
        {
            // Gestisce eventuali errori di scrittura sul file
            Debug.LogError($"[PersistentLogHandler] Errore durante la scrittura sul file di log: {e.Message}");
        }
    }
}