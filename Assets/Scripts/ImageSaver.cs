using System.Collections;
using System.IO;
using UnityEngine;

public class ImageSaver : MonoBehaviour
{
    [SerializeField]
    private CockpitCommandManager commandManager;

    // Percorso della cartella delle immagini
    private string imageDirectory;

    // --- Informazioni confermate dai tuoi script ---
    // Queste sono le dimensioni e il formato corretti dei dati grezzi
    private const int ImageWidth = 960;
    private const int ImageHeight = 720;
    private const TextureFormat ImageFormat = TextureFormat.RGB24;
    // ---------------------------------------------

    // Variabile per assicurarsi che non ci siano più coroutine di salvataggio attive contemporaneamente
    private Coroutine currentSaveCoroutine;

    void Start()
    {
        Debug.Assert(commandManager != null);
        SetupImageDirectory();
    }

    /// <summary>
    /// Crea la cartella Images se non esiste, nello stesso percorso della cartella Logs
    /// </summary>
    private void SetupImageDirectory()
    {
        try
        {
            imageDirectory = Path.Combine(Application.persistentDataPath, "Images");
            
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
                Debug.Log($"[ImageSaver] Cartella immagini creata: {imageDirectory}");
            }
            else
            {
                Debug.Log($"[ImageSaver] Cartella immagini già esistente: {imageDirectory}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ImageSaver] Errore nella creazione della cartella Images: {e.Message}");
        }
    }

    /// <summary>
    /// Metodo pubblico per avviare il processo di salvataggio dell'immagine con retry.
    /// Ferma qualsiasi salvataggio precedente in corso.
    /// </summary>
    public void SaveImage()
    {
        if (currentSaveCoroutine != null)
        {
            StopCoroutine(currentSaveCoroutine);
            Debug.LogWarning("[ImageSaver] Un'operazione di salvataggio precedente è stata interrotta.");
        }
        currentSaveCoroutine = StartCoroutine(Co_SaveImageWithRetry());
    }

    /// <summary>
    /// Coroutine per tentare di salvare l'immagine, ritentando per un certo periodo
    /// o finché l'immagine non è disponibile.
    /// </summary>
    private IEnumerator Co_SaveImageWithRetry()
    {
        float startTime = Time.time;
        float timeout = 0.75f; // Tempo massimo per attendere l'immagine (es. 0.75 secondi)
        byte[] rawBytes = null;

        Debug.Log("[ImageSaver] Tentativo di recuperare l'immagine dal drone...");

        // Tenta di recuperare l'immagine finché non arriva o scade il timeout
        while (rawBytes == null || rawBytes.Length == 0)
        {
            rawBytes = commandManager.SavePicture();

            if (rawBytes != null && rawBytes.Length > 0)
            {
                Debug.Log($"[ImageSaver] Immagine ricevuta dopo {Time.time - startTime:F2} secondi.");
                break; // Esci dal loop, l'immagine è arrivata
            }

            if (Time.time - startTime > timeout)
            {
                Debug.LogWarning($"[ImageSaver] Timeout di {timeout:F2} secondi raggiunto. Nessun dato immagine ricevuto.");
                yield break; // Esci dalla coroutine
            }

            // Attendi il prossimo frame prima di ritentare
            yield return null; 
        }

        // Se l'immagine è stata ricevuta, procedi al salvataggio
        if (rawBytes != null && rawBytes.Length > 0)
        {
            Texture2D texture = null;
            try
            {
                // 1. Crea una Texture2D temporanea con le dimensioni e il formato corretti
                texture = new Texture2D(ImageWidth, ImageHeight, ImageFormat, false);

                // 2. Carica i dati grezzi dei pixel nella texture
                texture.LoadRawTextureData(rawBytes);
                texture.Apply(); // Applica i dati alla texture

                // 3. CAPOVOLGI L'IMMAGINE VERTICALMENTE (SOLUZIONE AL PROBLEMA DI ROTAZIONE)
                FlipTextureVertically(texture);

                // 4. Codifica la texture in un formato file valido (PNG)
                byte[] encodedBytes = texture.EncodeToPNG();
                // O se preferisci JPG: byte[] encodedBytes = texture.EncodeToJPG(90); 

                if (encodedBytes == null || encodedBytes.Length == 0)
                {
                    Debug.LogError("[ImageSaver] Errore durante la codifica dell'immagine in PNG/JPG.");
                    yield break;
                }

                // 5. Genera un nome file (con millisecondi per evitare sovrascritture)
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                
                // 6. Imposta l'estensione corretta
                string fileExtension = "png"; // Cambia in "jpg" se usi EncodeToJPG
                string fileName = $"drone_photo_{timestamp}.{fileExtension}";
                string filePath = Path.Combine(imageDirectory, fileName);
                
                // 7. Scrivi i byte *codificati* su file
                File.WriteAllBytes(filePath, encodedBytes);
                
                Debug.Log($"[ImageSaver] Immagine salvata con successo: {filePath}");
                Debug.Log($"[ImageSaver] Dimensione file: {encodedBytes.Length} bytes");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ImageSaver] Errore durante il salvataggio dell'immagine: {e.Message}");
                if (texture != null)
                {
                    Debug.LogError($"[ImageSaver] Dettagli: Dati ricevuti {rawBytes.Length} bytes. La texture si aspettava {texture.GetRawTextureData().Length} bytes.");
                }
            }
            finally
            {
                // 8. FONDAMENTALE: Distruggi la texture temporanea per liberare memoria
                if (texture != null)
                {
                    Destroy(texture);
                }
                currentSaveCoroutine = null; // Reset della coroutine attiva
            }
        }
    }

    /// <summary>
    /// Capovolge verticalmente i pixel di una Texture2D.
    /// </summary>
    /// <param name="texture">La Texture2D da capovolgere.</param>
    private void FlipTextureVertically(Texture2D texture)
    {
        // Ottiene tutti i colori (pixels) dalla texture
        Color[] pixels = texture.GetPixels();
        Color[] flippedPixels = new Color[pixels.Length];

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                // Mappa il pixel dalla riga 'y' alla riga 'texture.height - 1 - y'
                flippedPixels[x + (texture.height - 1 - y) * texture.width] = pixels[x + y * texture.width];
            }
        }
        // Imposta i pixel capovolti nella texture e applica le modifiche
        texture.SetPixels(flippedPixels);
        texture.Apply();
    }
}