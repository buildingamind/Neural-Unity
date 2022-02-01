using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalGenerator : MonoBehaviour
{
    public AnimationCurve signal;  // Graphs out the repeating rate input over time.
    // A line at 0.5 would be a rate of 0.5 seconds between firings constantly.
    // A rate from 1 to 0.1 would be an accelerating frequency
    // A rate from 0.1 to 1 would be a decelerating frequency
    public float amplifier = 1f; // Scale Signal Up
    public float signalRate = 1f;
    public float phase = 0f;  // Phase Shift. Should be a value between 0 and 1. 
}
