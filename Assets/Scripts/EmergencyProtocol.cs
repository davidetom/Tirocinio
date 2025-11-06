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

    void Start()
    {
        Debug.Assert(commandManager != null);
        Debug.Assert(cockpitController != null);
        Debug.Assert(emergencyPanel != null);
    }

    public void EmergencyLand()
    {
        // Disattiva Controlli e forza atterraggio
        cockpitController.DisableControls();
        commandManager.Land();

        // Avvia il protocollo di emergenza
        StartEmergencyProtocol();
    }
    
    private void StartEmergencyProtocol()
    {
        emergencyPanel.SetActive(true);
    }
}
