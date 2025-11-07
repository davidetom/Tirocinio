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

    public float emergencyBatteryPower = 5f;
    public float emergencyWifiPower = 10f;

    private float startTime;
    private float countdownTime = 5f;
    private bool countdownFinished = false;

    void Start()
    {
        startTime = Time.time;
        Debug.Assert(commandManager != null);
        Debug.Assert(cockpitController != null);
        Debug.Assert(emergencyPanel != null);
        Debug.Assert(objsToDisable != null);
    }

    void Update()
    {
        CountDown();
    }

    public void StatusCheck(float battery, float wifi)
    {
        if (!countdownFinished) return;
        if (battery < emergencyBatteryPower || wifi < emergencyWifiPower) Emergency();
    }

    public void CountDown()
    {
        if (Time.time - startTime >= countdownTime) countdownFinished = true;
    }

    public void Emergency()
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
        for (int i = 0; i < objsToDisable.Length; i++) objsToDisable[i].SetActive(false);
    }

    /*
    public void Restart()
    {
        // Riattiva Controlli
        cockpitController.EnableControls();

        // Avvia il protocollo di riavvio
        RestartProtocol();
    }
    
    private void RestartProtocol()
    {
        emergencyPanel.SetActive(false);
        for (int i = 0; i < objsToDisable.Length; i++) objsToDisable[i].SetActive(true);
    }
    */
}
