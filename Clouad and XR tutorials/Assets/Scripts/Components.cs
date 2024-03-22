using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


[System.Serializable]
public struct tinVolumeRange
{
    public float min;
    public float max;
    public int WithinRange(float v)
    {
        // Returns -1, 0, 1 for small, in range and large
        if((this.min > v) && (this.max > v))
        {
            return -1;
        }
        else if ((this.min <= v) && (this.max >= v))
        {
            return 0;
        }
        else
        {
            return 1;
        }

    }
}
public class Components : MonoBehaviour
{
    public Transform slotTargetTransform; // target transform
    //public Transform tinTargetTransform;
    //public tinVolumeRange volumeRequirement; // this need to be move to a separate component
    public Collider[] slotCollider;
    public bool isAttached { get; private set; }
    //private float tinVolume; 
    //public bool isSoldered { get; private set; }
    public GameObject[] legs;

    //Network
    NetworkContext context;
    public bool owner;
    Vector3 lastPosition;
    Quaternion lastRotation;
    bool selected = false;
    bool lastAttached;
    private Vector3 originalScale;
    private void Awake()
    {
        owner = false;
    }
    private void Start()
    {
        isAttached = false;
        //Debug.Log("starting parent: "+gameObject.transform.parent);
        //tinVolume = 0f;
        XRGrabInteractable grabInteractable = gameObject.GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(ComponentSelected);
        grabInteractable.selectExited.AddListener(ComponentDeselected);
        originalScale = transform.localScale;

        //boardCollider = gameObject.GetComponent<Collider>();
        context = NetworkScene.Register(this);

    }

    private void Update()
    {
        CheckLegs();
        CheckNetworkUpdate();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (slotCollider.Contains(other)) // if collide with its slot
        {
            //Debug.Log(gameObject.name + " detected its slot: " + other.gameObject);
            if (!isAttached)
            {
                AttachComponent();
                CheckNetworkUpdate();
            }
        }
        //if(other.CompareTag("Melted Tin"))
        //{
        //    Debug.Log("added");
        //    AddTin(other.gameObject);
        //}
    }
    private void ComponentSelected(BaseInteractionEventArgs args)
    {
        for (int i = 0; i < slotCollider.Length; i++)
        {
            slotCollider[i].enabled = true;
        }
        if(!isAttached)
        {
            gameObject.transform.parent = null;
            //Debug.Log("current parent: "+gameObject.transform.parent);
            transform.localScale = originalScale;
            slotTargetTransform.gameObject.SetActive(true);
            SetTransluscent();
        }
        owner = true;
        selected = true;
    }

    private void ComponentDeselected(BaseInteractionEventArgs args)
    {
        gameObject.transform.parent = null;
        slotTargetTransform.gameObject.SetActive(false);
        owner = false;
        selected = false;
    }
    private void OnTriggerExit(Collider other)
    {
        if (slotCollider.Contains(other))
        {
            gameObject.transform.parent = null;
            isAttached = false;
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            CheckNetworkUpdate();
        }
    }

    private void AttachComponent()
    {
        isAttached = true;
        gameObject.GetComponent<XRGrabInteractable>().enabled = false;
        gameObject.transform.SetParent(slotTargetTransform.parent, false);
        gameObject.transform.localPosition = slotTargetTransform.localPosition;
        gameObject.transform.localRotation = slotTargetTransform.localRotation;
        gameObject.transform.localScale = slotTargetTransform.localScale;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;
        // disable all colliders
        for(int i = 0; i < slotCollider.Length; i++)
        {
            slotCollider[i].enabled = false;
        }
        gameObject.GetComponent<XRGrabInteractable>().enabled = true;
        slotTargetTransform.gameObject.SetActive(false);
        Debug.Log("is attached: " + isAttached);
    }

    private void CheckLegs()
    {
        bool allSoldered = legs[0].GetComponent<ComponentLeg>().isSoldered;
        for(int i = 1; i<legs.Length; i++)
        {
            allSoldered &= legs[i].GetComponent<ComponentLeg>().isSoldered;
        }
        if (allSoldered)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            gameObject.GetComponent<XRGrabInteractable>().enabled = false;
            Debug.Log("Soldered: " + gameObject);
        }
    }
    private void SetTransluscent()
    {
        MeshRenderer[] renders = slotTargetTransform.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0;i < renders.Length; i++)
        {
            Material[] mats = renders[i].materials;
            for (int j = 0;j < mats.Length; j++)
            {
                Color color = mats[j].color;
                color.a = 0.4f;
                mats[j].color = color;
            }
        }
    }
    //private void AddTin(GameObject tin)
    //{
    //    if (isAttached)
    //    {
    //        tin.transform.SetParent(tinTargetTransform);
    //        tin.transform.localPosition = new Vector3(0,0,0);
    //        tin.GetComponent<Collider>().enabled = false; // avoid multiple detection
    //        float volume = tin.GetComponent<SeparatedTin>().amount;
    //        tinVolume += volume;
    //        //Debug.Log("attached volume: " + tinVolume);
    //    }
    //}
    //private void Solder()
    //{
    //    if (volumeRequirement.min <= tinVolume && tinVolume <= volumeRequirement.max)
    //    {
    //        isSoldered = true;
    //        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
    //        rb.constraints = RigidbodyConstraints.FreezeAll;
    //        gameObject.GetComponent<XRGrabInteractable>().enabled = false;
    //        //Debug.Log("is solder: " + isSoldered);
    //    }
    //}

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isAttached;
        public bool owner;
        public bool selected;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;
        transform.localRotation = m.rotation;

        // Make sure the logic in Update doesn't trigger as a result of this message
        //lastPosition = transform.localPosition;
        //lastRotation = transform.localRotation;
        //isAttached = m.isAttached;

        isAttached = m.isAttached;
        if(isAttached == false)
        {
            gameObject.transform.parent = null;
            isAttached = false;
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
        }
        else
        {
            AttachComponent();
        }
        if (m.owner)
        {
            owner = false;
        }
        selected = m.selected;
    }

    private void CheckNetworkUpdate()
    {
        if (owner)
        {
            //lastPosition = transform.localPosition;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                rotation = transform.localRotation,
                isAttached = isAttached,
                owner = owner
            });
        }
        if(lastAttached != isAttached)
        {
            lastAttached = isAttached;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                rotation = transform.localRotation,
                isAttached = isAttached,
                owner = owner
            });
        }
        if(selected)
        {
            for (int i = 0; i < slotCollider.Length; i++)
            {
                slotCollider[i].enabled = true;
            }
            if (!isAttached)
            {
                gameObject.transform.parent = null;
                //Debug.Log("current parent: "+gameObject.transform.parent);
                transform.localScale = originalScale;
                slotTargetTransform.gameObject.SetActive(true);
                SetTransluscent();
            }
        }
        else
        {
            if (!isAttached)
            {
                gameObject.transform.parent = null;
            }
            slotTargetTransform.gameObject.SetActive(false);
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
}
