using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

[RequireComponent(typeof(CockpitStickController), typeof(CockpitCommandManager), typeof(CockpitEffect))]
public class CockpitSwitchBoard : MonoBehaviour
{
    private NgoEngine engine;
    private CockpitStickController stickController;
    private CockpitCommandManager commandManager;
    private CockpitEffect effect;

    [SerializeField]
    private Turntable turntable;

    [SerializeField]
    private VideoContainerDisplayPositions displayPositions;

    [SerializeField]
    private Interactable monitorButton;

    [SerializeField]
    private Interactable recordingButton;

    [SerializeField]
    private Interactable calibrateButton;

    // Comandi
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

    private void Start()
    {
        engine = NgoEngine.GetInstance();
        stickController = GetComponent<CockpitStickController>();
        commandManager = GetComponent<CockpitCommandManager>();
        effect = GetComponent<CockpitEffect>();

        Debug.Assert(stickController != null);
        Debug.Assert(commandManager != null);
        Debug.Assert(effect != null);
        Debug.Assert(turntable != null);
        Debug.Assert(displayPositions != null);
        Debug.Assert(monitorButton != null);
        Debug.Assert(recordingButton != null);
        Debug.Assert(calibrateButton != null);
    }


    public bool FPV
    {
        get
        {
            return !turntable.AutoTurn;
        }

        set
        {
            if (value)
            {
                displayPositions.Center();
                monitorButton.gameObject.SetActive(false);
                turntable.AutoTurn = false;
                stickController.RotationSpeed = RotationSpeed.Slow;
            }
            else
            {
                displayPositions.Bottom();
                turntable.AutoTurn = true;
                monitorButton.gameObject.SetActive(true);
                monitorButton.IsToggled = false;
                stickController.RotationSpeed = RotationSpeed.Fast;
            }
        }
    }

    public bool Fast
    {
        get
        {
            return stickController.Fast;
        }

        set
        {
            stickController.Fast = value;
            turntable.Size = value ? Size.Small : Size.Normal;
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
        effect.Takeoff();
        calibrateButton.IsToggled = false;
        turntable.Calibrate = false;
    }

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
            lx = 1f;
        else if (down)
            lx = -1f;
        else
            lx = 0f;
            
        if (rotateCW)
            ly = 1f;
        else if (rotateCCW)
            ly = -1f;
        else
            ly = 0f;
        
        var command = $"stick {rx:F2} {ry:F2} {lx:F2} {ly:F2} {stickController.Fast}";
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
