using System;
using UnityEngine;


public class CockpitCommandManager : MonoBehaviour
{
    private NgoEngine engine;

    private int landCount;
    private bool takeoff;
    private string stickCommand;
    private string extraCommand;

    private void Start()
    {
        engine = NgoEngine.GetInstance();
    }

    private void LateUpdate()
    {
        if (landCount > 0)
        {
            Debug.Log("Landing iniziato...");
            engine.EntryCommand("land");
            landCount--;
            return;
        }

        if (takeoff)
        {
            Debug.Log("TakeOff iniziato...");
            engine.EntryCommand("takeoff");
            takeoff = false;
            return;
        }

        if (!string.IsNullOrEmpty(extraCommand))
        {
            Debug.Log("Comando di movimento extra da inviare...");
            engine.EntryCommand(extraCommand);
            extraCommand = "";
            return;
        }

        if (!string.IsNullOrEmpty(stickCommand))
        {
            Debug.Log($"Comando di movimento da controller da inviare: {stickCommand}");
            engine.EntryCommand(stickCommand);
            return;
        }
    }

    public void Land()
    {
        Debug.Log("Land button cliccato e comando ricevuto...");
        landCount = 5;
        takeoff = false;
        stickCommand = "";
        extraCommand = "";
    }

    public void Takeoff()
    {
        Debug.Log("Comando di TakeOff ricevuto...");
        takeoff = true;
    }

    public void SetStickCommand(string stickComm)
    {
        Debug.Log($"StickCommand: {stickComm}");
        this.stickCommand = stickComm;
    }

    public void SetExtraCommand(string extraComm)
    {
        if(!IsMoving())
        {
            this.extraCommand = extraComm;
        }
    }
    private bool IsMoving()
    {
        string[] vals = stickCommand.Split();

        if(vals.Length < 2)
        {
            return false;
        }

        for(int i = 1; i < vals.Length; i++)
        {
            if (float.TryParse(vals[i], out float num) && num > 0f)
            {
                return true;
            }
        }
        return false;
    }
}
