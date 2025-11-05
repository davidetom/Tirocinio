using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CockpitStateView : MonoBehaviour
{
    private static Color LightOK = new Color(1f, 1f, 0f, 0.15f);
    private static Color LightNG = new Color(0f, 0f, 0f, 0f);

    private NgoEngine engine;

    [SerializeField]
    private TMP_Text command;

    [SerializeField]
    private TMP_Text notice;

    // BATTERY
    [SerializeField]
    private GameObject batteryIndicatorObject;
    private IProgressIndicator batteryIndicator;

    [SerializeField]
    private Renderer batteryProgressRenderer;

    // WIFI
    [SerializeField]
    private GameObject wifiIndicatorObject;
    private IProgressIndicator wifiIndicator;

    [SerializeField]
    private Renderer wifiProgressRenderer;

    // COMPASS
    [SerializeField]
    private Transform arrowObject;
    [SerializeField]
    private List<GameObject> directionIndicators;

    [SerializeField]
    private Message message;
    private string lastCommand;
    private string lastNotice;

    [SerializeField]
    private Renderer lightStrength;

    public bool EnableDirectionMessage { get; set; } = false;

    private async void Start()
    {
        Debug.Assert(command != null);
        Debug.Assert(notice != null);
        Debug.Assert(batteryIndicatorObject != null);
        Debug.Assert(batteryProgressRenderer != null);
        Debug.Assert(wifiIndicatorObject != null);
        Debug.Assert(wifiProgressRenderer != null);
        Debug.Assert(arrowObject != null);
        Debug.Assert(directionIndicators != null);
        Debug.Assert(message != null);

        engine = NgoEngine.GetInstance();
        command.text = "";
        notice.text = "";

        batteryIndicator = batteryIndicatorObject.GetComponent<IProgressIndicator>();
        wifiIndicator = wifiIndicatorObject.GetComponent<IProgressIndicator>();
        await batteryIndicator.OpenAsync();
        batteryIndicator.Progress = 0f;
        await wifiIndicator.OpenAsync();
        wifiIndicator.Progress = 0f;
    }

    private void Update()
    {
        command.text = engine.GetSentCommand();
        notice.text = engine.GetNotice();
        UpdateMessage(Translate(command.text, lastCommand), notice.text);

        UpdateBatteryState(engine.GetState("bat"));
        UpdateWifiState(engine.GetState("wifi"));
        UpdateCompass(engine.GetState("yaw"));
        UpdateLightStrength(engine.GetState("lit", -1f));
    }

    private void UpdateLightStrength(float value)
    {
        if (lightStrength == null)
        {
            return;
        }

        lightStrength.material.color = (value == 0f) ? LightOK : LightNG;
    }

    private void UpdateMessage(string command, string notice)
    {
        if (!string.IsNullOrEmpty(notice) && !string.Equals(lastNotice, notice))
        {
            lastNotice = notice;
            message.Show(notice);
            return;
        }

        if (!string.IsNullOrEmpty(command) && !string.Equals(lastCommand, command))
        {
            lastCommand = command;
            message.Show(lastCommand);
        }
    }

    private string Translate(string message, string defaultCommand)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "";
        }

        if (!message.StartsWith("stick"))
        {
            return message;
        }

        if (!EnableDirectionMessage)
        {
            return "";
        }

        try
        {
            var s = message.Split(' ');
            var rx = new StickCommand(s[1], "left", "right", defaultCommand);
            var ry = new StickCommand(s[2], "backward", "forward", defaultCommand);
            var lx = new StickCommand(s[3], "ccw", "cw", defaultCommand);
            var ly = new StickCommand(s[4], "down", "up", defaultCommand);

            var value = rx.select(ry.select(ly));
            if (!value.IsZero())
            {
                return value.ToString();
            }

            if (!lx.IsZero())
            {
                return lx.ToString();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return "";

    }

    private void UpdateBatteryState(float value)
    {
        var bat = value / 100;
        batteryIndicator.Progress = bat;
        Color color = Color.red;
        if (bat > 0.5f)
        {
            color = Color.green;
        }
        else if (bat > 0.2f)
        {
            color = Color.yellow;
        }
        batteryProgressRenderer.material.color = color;
    }

    private void UpdateWifiState(float value)
    {
        var wifi = value / 100;
        wifiIndicator.Progress = wifi;
        Color color = Color.red;
        if (wifi > 0.5f)
        {
            color = Color.green;
        }
        else if (wifi > 0.2f)
        {
            color = Color.yellow;
        }
        wifiProgressRenderer.material.color = color;
    }

    private void UpdateCompass(float value)
    {
        var yaw = value;
        arrowObject.localEulerAngles = new Vector3(arrowObject.localEulerAngles.x, arrowObject.localEulerAngles.y, -yaw);
        
        int index = Mathf.FloorToInt(yaw / 45f);
        index = Mathf.Clamp(index, 0, directionIndicators.Count - 1);
        for (int i = 0; i < directionIndicators.Count; i++)
        {
            if (i != index) directionIndicators[index].SetActive(false);
            else directionIndicators[index].SetActive(true);
        }
    }

    private class StickCommand
    {
        private readonly string name;
        private readonly float value;
        private readonly bool isDefault;

        public StickCommand(string textValue, string a, string b, string c)
        {
            var value = float.Parse(textValue);
            this.value = Mathf.Abs(value);
            name = value < 0 ? a : b;
            isDefault = String.Equals(name, c);
        }

        public StickCommand select(StickCommand command)
        {
            if (isDefault)
            {
                return value + 0.1f > command.value ? this : command;
            }

            return value > command.value ? this : command;
        }

        public override string ToString()
        {
            return name;
        }

        public bool IsZero()
        {
            return this.value < 0.1f;
        }
    }
}
