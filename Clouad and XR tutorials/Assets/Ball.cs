using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Ball : MonoBehaviour
{
    XRGrabInteractable interactable;
    bool isHeld = false;
    NetworkContext context;
    Transform parent;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent;
        interactable = GetComponent<XRGrabInteractable>();
        interactable.firstSelectEntered.AddListener(OnPickedUp);
        interactable.lastSelectExited.AddListener(OnDropped);
        context = NetworkScene.Register(this);
    }

    void OnPickedUp(SelectEnterEventArgs ev)
    {
        Debug.Log("Picked up");
        isHeld = true;
    }

    void OnDropped(SelectExitEventArgs ev)
    {
        Debug.Log("Dropped");
        transform.parent = parent;
        isHeld = false;
    }

    private struct Message
    {
        public Vector3 position;
    }

    // Update is called once per frame
    void Update()
    {
        if(isHeld)
        {
            Message m = new Message();
            m.position = this.transform.localPosition;
            context.SendJson(m);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        var message = m.FromJson<Message>();
        transform.localPosition = message.position;
        GetComponent<Rigidbody>().isKinematic = true;
    }
}
