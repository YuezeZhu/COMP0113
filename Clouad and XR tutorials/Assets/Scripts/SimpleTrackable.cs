using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SimpleTrackable : MonoBehaviour
{
    NetworkContext context;
    public bool owner;


    private void Awake()
    {
        owner = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
        XRGrabInteractable grabInteractable = gameObject.GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(ComponentSelected);
        grabInteractable.selectExited.AddListener(ComponentDeselected);
    }

    private void ComponentSelected(BaseInteractionEventArgs args)
    {
        owner = true;
    }

    private void ComponentDeselected(BaseInteractionEventArgs args)
    {
        owner = false;
    }
    Vector3 lastPosition;
    Quaternion lastRotation;

    // Update is called once per frame
    void Update()
    {
        if(owner)
        {
            lastPosition = transform.localPosition;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                rotation = transform.localRotation
            });
        }
        //if (lastPosition != transform.localPosition)
        //{
        //    lastPosition = transform.localPosition;
        //    context.SendJson(new Message()
        //    {
        //        position = transform.localPosition,
        //        rotation = transform.localRotation
        //    });
        //}
        //if (lastRotation != transform.localRotation)
        //{
        //    lastRotation = transform.localRotation;
        //    context.SendJson(new Message()
        //    {
        //        position = transform.localPosition,
        //        rotation = transform.localRotation
        //    });
        //}
    }

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;
        transform.localRotation = m.rotation;

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
    }
}
