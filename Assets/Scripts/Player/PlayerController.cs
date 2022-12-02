using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

public class PlayerController : MonoBehaviour
{
    private Recorder recorder;
    private InputDevice leftHand;

    [HideInInspector] public float speakerAmp;

    // Start is called before the first frame update
    void Start()
    {
        recorder = FindObjectOfType<Recorder>();
        recorder.TransmitEnabled= false;

        if(recorder.IsCurrentlyTransmitting)

        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    // Update is called once per frame
    void Update()
    {
        if(leftHand!= null)
        {
            bool triggerValue;
            if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
            {
                EnableVoiceChat();
            }
            else
            {
                DisableVoiceChat();
            }

            if (recorder.IsCurrentlyTransmitting)
            {
                speakerAmp = recorder.LevelMeter.CurrentPeakAmp * 1000f;
                if(speakerAmp > 0.1f)
                {
                    Debug.LogFormat("<color=green>IsCurrentlyTransmitting {0}</color>", speakerAmp.ToString("F3"));
                }
            }
            else
            {
                speakerAmp = 0;
            }
        }
    }

    public void EnableVoiceChat()
    {
        if(recorder != null && !recorder.TransmitEnabled)
        {
            recorder.TransmitEnabled = true;
        }
    }

    public void DisableVoiceChat()
    {
        if (recorder != null && recorder.TransmitEnabled)
        {
            recorder.TransmitEnabled = false;
        }
    }
}
