using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CockpitCommandManager))]
public class CockpitHandGestureSensor : MonoBehaviour
{
    public new bool enabled
    {
        get { return base.enabled; }
        set
        {
            ResetCurrentGesture();
            base.enabled = value;
        }
    }

    enum HandGesture
    {
        None, OneUp, TwoUp, ThreeUp, FourUp
    }
    private static Color Default = Color.gray;
    private static Color Picture = Color.white;

    private Color currentColor = Default;
    private HandGesture currentGesture = HandGesture.None;
    private float confirmationTime = float.MaxValue;

    private IMixedRealityHand[] hands = new IMixedRealityHand[] { };
    private Renderer[] renderers = new Renderer[] { };

    private CockpitCommandManager commandManager;

    private void Start()
    {
        ResetCurrentGesture();
        commandManager = GetComponent<CockpitCommandManager>();
        Debug.Assert(commandManager != null);
    }

    private void Update()
    {
        UpdateHands();
        UpdateCurrentGesture();
        UpdateColors();
    }

    private void UpdateCurrentGesture()
    {
        if (Time.time > confirmationTime)
        {
            FollowTheGesture();
        }

        foreach (var hand in hands)
        {
            if (hand != null)
                UpdateCurrentGesture(hand);
        }
    }

    private bool FollowTheGesture()
    {
        if (currentGesture == HandGesture.TwoUp)
        {
            commandManager.SetExtraCommand("picture");
            currentColor = Picture;
            Invoke("ResetColor", 2f);
        }
        else
        {
            return false;
        }

        ResetCurrentGesture();
        return true;
    }

    private void ResetColor()
    {
        currentColor = Default;
    }

    private void ResetCurrentGesture()
    {
        confirmationTime = float.MaxValue;
        currentGesture = HandGesture.None;
    }

    private void UpdateCurrentGesture(IMixedRealityHand hand)
    {
        HandGesture gesture = GetHandGesture(hand);
        if (gesture == HandGesture.None)
        {
            confirmationTime = float.MaxValue;
        }
        else if (gesture != currentGesture)
        {
            confirmationTime = Time.time + 1f;
        }
        currentGesture = gesture;
    }

    private HandGesture GetHandGesture(IMixedRealityHand hand)
    {
        // Questo controllo è ridondante se chiamato da UpdateCurrentGesture, ma è una buona pratica mantenerlo
        if (hand == null) return HandGesture.None;

        // MODIFICA: Aggiunto un controllo per assicurarsi che il visualizzatore della mano sia pronto prima di usarlo.
        // Questo è il punto che causava l'errore.
        bool visualizerReady = hand.Visualizer != null && hand.Visualizer.GameObjectProxy != null;

        if (!IsTracked(hand))
        {
            if (visualizerReady)
            {
                hand.Visualizer.GameObjectProxy.SetActive(false);
            }
            return HandGesture.None;
        }
        else
        {
            if (visualizerReady)
            {
                hand.Visualizer.GameObjectProxy.SetActive(true);
            }
        }

        var indexIsStanding = IsStanding(hand, TrackedHandJoint.IndexTip, TrackedHandJoint.IndexDistalJoint, TrackedHandJoint.IndexMiddleJoint, TrackedHandJoint.Palm);
        if (!indexIsStanding)
        {
            return HandGesture.None;
        }

        var middleIsStanding = IsStanding(hand, TrackedHandJoint.MiddleTip, TrackedHandJoint.MiddleDistalJoint, TrackedHandJoint.MiddleMiddleJoint, TrackedHandJoint.Palm);
        if (!middleIsStanding)
        {
            return HandGesture.OneUp;
        }

        var ringStanding = IsStanding(hand, TrackedHandJoint.RingTip, TrackedHandJoint.RingDistalJoint, TrackedHandJoint.RingMiddleJoint, TrackedHandJoint.Palm);
        if (!ringStanding)
        {
            return HandGesture.TwoUp;
        }

        var pinkyStanding = IsStanding(hand, TrackedHandJoint.PinkyTip, TrackedHandJoint.PinkyDistalJoint, TrackedHandJoint.PinkyMiddleJoint, TrackedHandJoint.Palm);
        if (!pinkyStanding)
        {
            return HandGesture.ThreeUp;
        }
        else
        {
            return HandGesture.FourUp;
        }
    }

    private bool IsStanding(IMixedRealityHand hand, TrackedHandJoint tip, TrackedHandJoint distal, TrackedHandJoint middle, TrackedHandJoint parm)
    {
        var a = GetHeight(hand, tip);
        var b = GetHeight(hand, distal);
        var c = GetHeight(hand, middle);
        var d = GetHeight(hand, parm);

        // Se una qualsiasi delle giunture non è tracciata, GetHeight restituisce float.MinValue, quindi questo controllo fallirà in modo sicuro.
        return a > b && b > c && c > d && (a - d > 0.05f);
    }

    private float GetHeight(IMixedRealityHand hand, TrackedHandJoint joint)
    {
        if (hand.TryGetJoint(joint, out MixedRealityPose pose))
        {
            return pose.Position.y;
        }
        else
        {
            return float.MinValue;
        }
    }

    private bool IsTracked(IMixedRealityHand hand)
    {
        // hand != null viene già controllato prima, ma per sicurezza lo lasciamo.
        return hand != null && hand.Enabled && hand.TrackingState == TrackingState.Tracked;
    }

    private void UpdateColors()
    {
        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = currentColor;
            }
        }
    }

    private void UpdateHands()
    {
        var newhands = FindHands();
        if (IsValidCache(newhands))
        {
            return;
        }

        hands = newhands;
        renderers = new Renderer[hands.Length];
        for (int i = 0; i < hands.Length; i++)
        {
            // MODIFICA: Aggiunto un controllo per evitare errori se il visualizzatore non è pronto durante l'aggiornamento della cache.
            if (hands[i].Visualizer != null && hands[i].Visualizer.GameObjectProxy != null)
            {
                renderers[i] = hands[i].Visualizer.GameObjectProxy.GetComponentInChildren<Renderer>();
            }
        }
    }

    private bool IsValidCache(IMixedRealityHand[] newhands)
    {
        if (hands.Length == 0)
        {
            return false;
        }

        if (hands.Length != newhands.Length)
        {
            return false;
        }

        for (int i = 0; i < hands.Length; i++)
        {
            if (hands[i] != newhands[i])
            {
                return false;
            }
        }
        return true;
    }

    private IMixedRealityHand[] FindHands()
    {
        List<IMixedRealityHand> hands = new List<IMixedRealityHand>();
        // Usiamo l'operatore null-conditional (?) per sicurezza
        var controllers = CoreServices.InputSystem?.DetectedControllers;

        if (controllers == null)
        {
            return hands.ToArray();
        }

        foreach (IMixedRealityController detectedController in controllers)
        {
            if (detectedController is IMixedRealityHand hand)
            {
                hands.Add(hand);
            }
        }
        return hands.ToArray();
    }
}