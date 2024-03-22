using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StationButton : MonoBehaviour
{
    // put this script for the button colldier
    // put button visual as the visual target
    public Transform buttonVisualTarget;
    public enum ButtonType { ONOFF, Count };
    public ButtonType buttonType;
    public float pushAngleThres = 45.0f;
    public Vector3 localAxis;
    private Vector3 originalButtonPos;
    private Vector3 offset;
    private bool pushed;
    public bool on { get; set; }
    public int count { get; set; }
    private Transform pokeAttachTransform;
    private XRBaseInteractable button;
    private bool isPushingButton = false;

    // Network
    NetworkContext context;
    bool lastOnState;
    int lastCount;

    // Start is called before the first frame update
    void Start()
    {
        originalButtonPos = buttonVisualTarget.localPosition;
        button = gameObject.GetComponent<XRBaseInteractable>();
        button.hoverEntered.AddListener(PushRedButton);
        button.hoverExited.AddListener(LeaveRedButton);
        button.selectEntered.AddListener(Pushed);
        on = false;
        count = 0;
        context = NetworkScene.Register(this);
    }

    public void PushRedButton(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRPokeInteractor)
        {
            XRPokeInteractor interactor = (XRPokeInteractor)(args.interactorObject);
            pokeAttachTransform = interactor.attachTransform;
            offset = buttonVisualTarget.position - pokeAttachTransform.position;

            float pushAngle = Vector3.Angle(offset, buttonVisualTarget.TransformDirection(localAxis));
            if (pushAngle < pushAngleThres)
            {
                isPushingButton = true;
                pushed = false;
            }
        }
    }

    public void LeaveRedButton(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRPokeInteractor)
        {
            pushed = false;
            isPushingButton = false;
            if(buttonType == ButtonType.ONOFF)
            {
                on = !on;
            }
            if (buttonType == ButtonType.Count)
            {
                count++;
            }
        }
    }
    public void Pushed(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRPokeInteractor)
        {
            pushed = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (pushed) return;
        //buttonVisualTarget.gameObject.SetActive(!on);
        if (isPushingButton)
        {
            Vector3 localTargetPosition = buttonVisualTarget.InverseTransformPoint(pokeAttachTransform.position + offset);
            Vector3 ClampedPosition = Vector3.Project(localTargetPosition, localAxis);
            buttonVisualTarget.position = buttonVisualTarget.TransformPoint(ClampedPosition);
        }
        else
        {
            buttonVisualTarget.localPosition = originalButtonPos;
        }
        CheckNetworkUpdate();
    }


    // Network
    private struct Message
    {
        public bool on;
        public int count;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        on = m.on;
        lastOnState = on;
        count = m.count;
        lastCount = count;
    }

    private void CheckNetworkUpdate()
    {
        if (lastOnState != on)
        {
            lastOnState = on;
            context.SendJson(new Message()
            {
                on = on,
                count = count
            });
        }
        if (lastCount != count)
        {
            lastCount = count;
            context.SendJson(new Message()
            {
                on = on,
                count = count
            });
        }

    }
}
