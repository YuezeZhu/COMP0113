using System;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class net : MonoBehaviour
{
    XRGrabInteractable interactable;
    NetworkContext context;
    bool isHeld;
    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
        interactable.firstSelectEntered.AddListener(OnPicked);
        interactable.lastSelectExited.AddListener(OnDrop);
        context = NetworkScene.Register(this);
    }

    void OnPicked(SelectEnterEventArgs ev)
    {
        isHeld = true;
    }
    private void OnDrop(SelectExitEventArgs ev)
    {
    }
    [Serializable]
    struct message
    {
        public Vector3 pos;
    }
    private void Update()
    {
        var m = new message();
        m.pos = transform.localPosition;
        if (isHeld)
        {
            context.SendJson(m);
        }
    }
    public void processMessage(ReferenceCountedSceneGraphMessage ms)
    {
        var message = ms.FromJson<message>();
        transform.localPosition = message.pos;
    }
}
