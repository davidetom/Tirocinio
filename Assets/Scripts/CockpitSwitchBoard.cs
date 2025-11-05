using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using System;

[RequireComponent(typeof(CockpitCommandManager))]
public class CockpitSwitchBoard : MonoBehaviour
{
    private NgoEngine engine;
    private CockpitCommandManager commandManager;
    private CockpitOculusTouchModelController cockpitController;

    [SerializeField]
    private VideoContainerDisplayPositions displayPositions;

    [SerializeField]
    private Interactable monitorButton;

    [SerializeField]
    private Interactable recordingButton;

    /* Comandi
    private float rx;
    private float ry;
    private float lx;
    private float ly;
    private bool left;
    private bool right;
    private bool forward;
    private bool back;
    private bool up;
    private bool down;
    private bool rotateCW;
    private bool rotateCCW;
    */
    
    private void Start()
    {
        engine = NgoEngine.GetInstance();
        commandManager = GetComponent<CockpitCommandManager>();
        cockpitController = GetComponent<CockpitOculusTouchModelController>();

        Debug.Assert(commandManager != null);
        Debug.Assert(cockpitController != null);
        Debug.Assert(displayPositions != null);
        Debug.Assert(monitorButton != null);
        Debug.Assert(recordingButton != null);
    }

    public bool Fast
    {
        get
        {
            return cockpitController.fast;
        }

        set
        {
            cockpitController.fast = value;
        }
    }

    public bool Recording
    {
        get
        {
            return recordingButton.IsToggled;
        }

        set
        {
            recordingButton.IsToggled = value;
            engine.SetRecording(value);
        }
    }

    public void Takeoff()
    {
        Debug.Log("TakeOff button cliccato...");
        commandManager.Takeoff();
    }

    /*
    void Update()
    {
        if (right)
            rx = 1f;
        else if (left)
            rx = -1f;
        else
            rx = 0f;

        if (forward)
            ry = 1f;
        else if (back)
            ry = -1f;
        else
            ry = 0f;

        if (up)
            ly = 1f;
        else if (down)
            ly = -1f;
        else
            ly = 0f;
            
        if (rotateCW)
            lx = 1f;
        else if (rotateCCW)
            lx = -1f;
        else
            lx = 0f;
        
        var command = FormattableString.Invariant(
            $"stick {rx:F2} {ry:F2} {lx:F2} {ly:F2} {cockpitController.fast}"
        );
        if (right || left || forward || back || up || down || rotateCW || rotateCCW)
            commandManager.SetStickCommand(command);
    }

    public void Left(bool setActive)
    {
        if (setActive)
            Debug.Log("Left button premuto...");
        else
            Debug.Log("Left button rilasciato...");
        left = setActive && !right;
    }

    public void Right(bool setActive)
    {
        if (setActive)
            Debug.Log("Right button premuto...");
        else
            Debug.Log("Right button rilasciato...");
        right = setActive && !left;
    }

    public void Forward(bool setActive)
    {
        if (setActive)
            Debug.Log("Forward button premuto...");
        else
            Debug.Log("Forward button rilasciato...");
        forward = setActive && !back;
    }

    public void Back(bool setActive)
    {
        if (setActive)
            Debug.Log("Back button premuto...");
        else
            Debug.Log("Back button rilasciato...");
        back = setActive && !forward;
    }

    public void Up(bool setActive)
    {
        if (setActive)
            Debug.Log("Up button premuto...");
        else
            Debug.Log("Up button rilasciato...");
        up = setActive && !down;
    }

    public void Down(bool setActive)
    {
        if (setActive)
            Debug.Log("Down button premuto...");
        else
            Debug.Log("Down button rilasciato...");
        down = setActive && !up;
    }

    public void RotateCW(bool setActive)
    {
        if (setActive)
            Debug.Log("RotateCW button premuto...");
        else
            Debug.Log("RotateCW button rilasciato...");
        rotateCW = setActive && !rotateCCW;
    }

    public void RotateCCW(bool setActive)
    {
        if (setActive)
            Debug.Log("RotateCCW button premuto...");
        else
            Debug.Log("RotateCCW button rilasciato...");
        rotateCCW = setActive && !rotateCW;
    }
    
    /* LEGENDA:

        stick rx ry lx ly speed
        rx: (left) -1 < 0 < 1 (right)
        ry: (backward) -1 < 0 < 1 (forward)
        lx: (down) -1 < 0 < 1 (up)
        ly: (ccw) -1 < 0 < 1 (cw)
        speed: 0 (slow) or 1 (fast)
    
    */
}
