using System.IO;
using UnityEngine;

public class ImageSaver : MonoBehaviour
{
    [SerializeField]
    private CockpitCommandManager commandManager;

    private byte[] byteImage;
    
    // Percorso della cartella delle immagini
    private string imageDirectory;

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
            // Crea il percorso della cartella Images accanto a quella Logs
            imageDirectory = Path.Combine(Application.persistentDataPath, "Images");
            
            // Crea la cartella se non esiste
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
    /// Salva l'immagine ricevuta dal drone nella cartella Images
    /// </summary>
    public void SaveImage()
    {
        try
        {
            // Richiedi l'immagine dal drone
            byteImage = commandManager.SavePicture();
            
            if (byteImage == null || byteImage.Length == 0)
            {
                Debug.LogWarning("[ImageSaver] Nessuna immagine ricevuta dal drone o immagine vuota.");
                return;
            }
            
            // Genera un nome file con timestamp
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"drone_photo_{timestamp}.jpg";
            string filePath = Path.Combine(imageDirectory, fileName);
            
            // Scrivi i byte dell'immagine su file
            File.WriteAllBytes(filePath, byteImage);
            
            Debug.Log($"[ImageSaver] Immagine salvata con successo: {filePath}");
            Debug.Log($"[ImageSaver] Dimensione immagine: {byteImage.Length} bytes");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ImageSaver] Errore durante il salvataggio dell'immagine: {e.Message}");
        }
    }
}