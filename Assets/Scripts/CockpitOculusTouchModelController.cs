using UnityEngine;
using System;

[RequireComponent(typeof(CockpitCommandManager), typeof(CockpitSwitchBoard))]
public class CockpitOculusTouchModelController : MonoBehaviour
{
    private CockpitCommandManager commandManager;
    private CockpitSwitchBoard switchBoard;
    private ImageSaver imageSaver;
    [HideInInspector]
    public bool fast = false;

    // Soglia angolare (in gradi) per distinguere direzioni principali
    [SerializeField] private float angleThreshold = 30f;
    // Deadzone minima per evitare rumore quando il joystick è quasi fermo
    [SerializeField] private float deadzone = 0.15f;

    private bool controlsEnabled = true;
    //private float imageSaveCooldown = 0.75f;
    //private float lastImageSaveTime = -1f;

    private void Start()
    {
        commandManager = GetComponent<CockpitCommandManager>();
        switchBoard = GetComponent<CockpitSwitchBoard>();
        imageSaver = GetComponent<ImageSaver>();

        Debug.Assert(commandManager != null);
        Debug.Assert(switchBoard != null);
        Debug.Assert(imageSaver != null);
    }

    private void Update()
    {
        if (!controlsEnabled) return;
        if (OVRInput.GetActiveController() != OVRInput.Controller.Touch)
        {
            return;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            Debug.Log("(Controller) -> Land button cliccato...");
            commandManager.Land();
            return;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Debug.Log("(Controller) -> TakeOff button cliccato...");
            switchBoard.Takeoff();
            return;
        }

        Vector2 l = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        Vector2 r = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);

        // Applica deadzone classica
        if (l.magnitude < deadzone)
            l = Vector2.zero;

        if (r.magnitude < deadzone)
            r = Vector2.zero;

        // Applica correzione direzionale al thumbstick sinistro
        if (l != Vector2.zero)
            l = ApplyDirectionalDeadzone(l);

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            fast = !fast;
        }

        //Debug.Log($"(Controller) -> Comandi letti dai joystick: l.x = {l.x}, l.y = {l.y}, r.x = {r.x}, r.y = {r.y}, fast = {this.fast}");

        if (l != null && r != null && (l != Vector2.zero || r != Vector2.zero))
            SendCommand(l, r);

        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            commandManager.SetExtraCommand("picture");
            /* Verifica se è passato abbastanza tempo dall'ultimo salvataggio
            if (Time.time - lastImageSaveTime >= imageSaveCooldown)
            {
                imageSaver.SaveImage();
                lastImageSaveTime = Time.time;
                Debug.Log($"(Controller) -> Salvataggio immagine avviato. Prossimo salvataggio disponibile tra {imageSaveCooldown} secondi.");
            }
            else
            {
                float remainingTime = imageSaveCooldown - (Time.time - lastImageSaveTime);
                Debug.LogWarning($"(Controller) -> Salvataggio immagine in cooldown. Attendi ancora {remainingTime:F2} secondi.");
            }*/
        }
    }

    private Vector2 ApplyDirectionalDeadzone(Vector2 input)
    {
        // Calcola l’angolo in gradi
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

        // Direzioni principali in gradi
        // Destra = 0°, Su = 90°, Sinistra = 180°/-180°, Giù = -90°
        if (Mathf.Abs(Mathf.DeltaAngle(angle, 0f)) < angleThreshold)
            return new Vector2(input.x, 0f); // Destra
        if (Mathf.Abs(Mathf.DeltaAngle(angle, 90f)) < angleThreshold)
            return new Vector2(0f, input.y); // Su
        if (Mathf.Abs(Mathf.DeltaAngle(angle, 180f)) < angleThreshold ||
            Mathf.Abs(Mathf.DeltaAngle(angle, -180f)) < angleThreshold)
            return new Vector2(input.x, 0f); // Sinistra
        if (Mathf.Abs(Mathf.DeltaAngle(angle, -90f)) < angleThreshold)
            return new Vector2(0f, input.y); // Giù

        // Se fuori da tutte le direzioni principali, lascia com'è
        return input;
    }

    private void SendCommand(Vector2 l, Vector2 r)
    {
        var command = FormattableString.Invariant(
            $"stick {r.x:F2} {r.y:F2} {l.x:F2} {l.y:F2} {fast}"
        );
        Debug.Log($"(Controller) -> Comando da controller rilevato: {command}");
        commandManager.SetStickCommand(command);
    }

    public void EnableControls() { controlsEnabled = true; }
    public void DisableControls() { controlsEnabled = false; }
}