using UnityEngine;

[RequireComponent(typeof(CockpitCommandManager), typeof(CockpitStickController), typeof(CockpitSwitchBoard))]
public class CockpitOculusTouchModelController : MonoBehaviour
{
    private CockpitStickController stickController;
    private CockpitCommandManager commandManager;
    private CockpitSwitchBoard switchBoard;

    [SerializeField]
    private Transform tello;

    private void Start()
    {
        stickController = GetComponent<CockpitStickController>();
        commandManager = GetComponent<CockpitCommandManager>();
        switchBoard = GetComponent<CockpitSwitchBoard>();

        Debug.Assert(commandManager != null);
        Debug.Assert(stickController != null);
        Debug.Assert(switchBoard != null);
        Debug.Assert(tello != null);
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

        Debug.Log($"(Controller) -> Comandi letti dai joystick: l.x = {l.x}, l.y = {l.y}, r.x = {r.x}, r.y = {r.y}");

        float m = 0.2f;
        m += 0.4f * OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger);
        m += 0.4f * OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);

        bool fast = m > 0.9f;

        if (l != null && r != null && (l != Vector2.zero || r != Vector2.zero))
            SendCommand(l, r, fast);

        /*

        stickController.Fast = fast;

        float ly = Mathf.Abs(l.y) > 0.5f ? l.y : 0f;
        tello.localPosition = new Vector3(GetValue(r.x, m), GetValue(ly, m), GetValue(r.y, m));

        float rotation = 0f;
        if (Mathf.Abs(l.x) > 0.5f)
        {
            rotation = l.x > 0f ? 0.5f : -0.5f;
        }
        stickController.UpdateRotation(rotation);
        stickController.IsOperating = true;

        */

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            commandManager.SetExtraCommand("picture");
        }

        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            switchBoard.Recording = !switchBoard.Recording;
        }

    }

    private void SendCommand(Vector2 l, Vector2 r, bool fast)
    {
        var command = $"stick {r.x:F2} {r.y:F2} {l.x:F2} {l.y:F2} {fast}";
        Debug.Log($"(Controller) -> Comando da controller rilevato: {command}");
        commandManager.SetStickCommand(command);
    }

    private float GetValue(float v, float m)
    {
        return v * m;
    }

}
