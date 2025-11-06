using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void QuitApp()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        Debug.Log("Chiusura applicazione...");
    }
}