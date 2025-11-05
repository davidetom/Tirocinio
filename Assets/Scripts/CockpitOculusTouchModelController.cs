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

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            fast = !fast;
        }

        //Debug.Log($"(Controller) -> Comandi letti dai joystick: l.x = {l.x}, l.y = {l.y}, r.x = {r.x}, r.y = {r.y}, fast = {this.fast}");

        if (l != null && r != null && (l != Vector2.zero || r != Vector2.zero))
            SendCommand(l, r);

        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            imageSaver.SaveImage();
        }
    }

    private void SendCommand(Vector2 l, Vector2 r)
    {
        var command = FormattableString.Invariant(
            $"stick {r.x:F2} {r.y:F2} {l.x:F2} {l.y:F2} {fast}"
        );
        Debug.Log($"(Controller) -> Comando da controller rilevato: {command}");
        commandManager.SetStickCommand(command);
    }
}
