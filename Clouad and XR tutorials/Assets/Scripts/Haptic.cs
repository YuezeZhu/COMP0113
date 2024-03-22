using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class Haptic
{
    [Range(0.0f, 1.0f)]
    public float intensity;
    public float duration;

    public void TriggerHaptic(BaseInteractionEventArgs eventArgs)
    {
        if(eventArgs.interactorObject is XRBaseControllerInteractor controllerInteractor) 
        {
            TriggerHaptic(controllerInteractor.xrController);
        }
    }
    public void TriggerHaptic(XRBaseController controller)
    {
        if(intensity > 0.0f)
        {
            controller.SendHapticImpulse(intensity, duration);
        }
    }
}

[Serializable]
public class IronHeadHaptic : Haptic
{
    [SerializeField]
    public GameObject iron;
    [SerializeField]
    public float hotTemperature;

    public new void TriggerHaptic(BaseInteractionEventArgs eventArgs)
    {
        if (eventArgs.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            TriggerHaptic(controllerInteractor.xrController);
        }
    }
    public new void TriggerHaptic(XRBaseController controller)
    {
        if (intensity > 0.0f && iron.GetComponent<SolderingIron>().temperature >= hotTemperature)
        {
            controller.SendHapticImpulse(intensity, duration);
        }
    }
}
