using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyProtocol : MonoBehaviour
{
    [SerializeField]
    private CockpitCommandManager commandManager;
    [SerializeField]
    private CockpitOculusTouchModelController cockpitController;
    [SerializeField]
    private GameObject emergencyPanel;
    [SerializeField]
    private GameObject[] objsToDisable;

    void Start()
    {
        Debug.Assert(commandManager != null);
        Debug.Assert(cockpitController != null);
        Debug.Assert(emergencyPanel != null);
        Debug.Assert(objsToDisable != null);
    }

    public void Emergency()
    {
        // Disattiva Controlli e forza atterraggio
        cockpitController.DisableControls();
        commandManager.Land();

        // Avvia il protocollo di emergenza
        StartEmergencyProtocol();
    }

    public void Restart()
    {
        // Riattiva Controlli
        cockpitController.EnableControls();

        // Avvia il protocollo di riavvio
        RestartProtocol();
    }

    [ContextMenu("Protocollo di Emergenza")]
    private void StartEmergencyProtocol()
    {
        emergencyPanel.SetActive(true);
        for (int i = 0; i < objsToDisable.Length; i++) objsToDisable[i].SetActive(false);
    }
    
    [ContextMenu("Protocollo di Riavvio")]
    private void RestartProtocol()
    {
        emergencyPanel.SetActive(false);
        for (int i = 0; i < objsToDisable.Length; i++) objsToDisable[i].SetActive(true);
    }
}
