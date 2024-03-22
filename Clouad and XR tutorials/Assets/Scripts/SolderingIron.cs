using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Ubiq.Messaging;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SolderingIron : MonoBehaviour
{
    [SerializeField]
    private float power = 30.0f;//{ get; set; }
    private float roomTemperature; 
    public IronHeadHaptic hotHeadTouchHaptic;
    [Range(0.0f, 1.0f)]
    public float solderHapticIntensity;
    public float solderHapticDuration=0.1f;
    [Range(0.0f, 1.0f)]
    public float cleaningHapticIntensity;
    [Range(0.0f, 1.0f)]
    public float cleanedHapticIntensity;
    public float cleaningHapticDuration=0.1f;

    public float setTemperature { get; set; } //{ get; set; }
    public bool on; // { get; set; }
    public bool soldering { get; set; }
    public float temperature { get; set; }
    public float clean { get; private set; } //0-100
    Material tipMat;
    public bool isGrabbed { get; private set; }
    public bool isTouchingTin { get; private set; }
    public bool isTouchingSponge { get; private set; }
    private XRBaseController grabbedController;
    private ParticleSystem ironSmoke;
    // Network
    NetworkContext context;
    public bool owner;
    Vector3 lastPosition;
    Quaternion lastRotation;
    bool lastSolderingState;
    float lastCleanState;
    private void Awake()
    {
        gameObject.tag = "Iron";
        soldering = false;
        owner = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        roomTemperature = SceneManager.instance.roomTemperature;
        temperature = roomTemperature; // room temperature???
        on = false;
        clean = 0;
        GameObject head = gameObject.transform.Find("Head_collider").gameObject;
        XRBaseInteractable headInteractable = head.GetComponent<XRSimpleInteractable>();
        //Debug.Log(head.name);
        headInteractable.hoverEntered.AddListener(hotHeadTouchHaptic.TriggerHaptic);
        XRBaseInteractable ironInteractable = GetComponent<XRBaseInteractable>();
        Debug.Log(ironInteractable);
        ironInteractable.selectEntered.AddListener(GetController);
        ironInteractable.selectExited.AddListener(LeaveController);



        // For the tip color
        // Use the tip material (second)
        tipMat = GetComponent<MeshRenderer>().materials[1];
        ironSmoke = GetComponentInChildren<ParticleSystem>();
        ironSmoke.Stop();
        isGrabbed = false;
        isTouchingSponge = false;
        isTouchingTin = false;
        context = NetworkScene.Register(this);
    }

    private void GetController(BaseInteractionEventArgs eventArgs)
    {
        if (eventArgs.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            isGrabbed = true;
            grabbedController = controllerInteractor.xrController;
            owner = true;
        }
    }
    private void LeaveController(BaseInteractionEventArgs eventArgs)
    {
        if (eventArgs.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            isGrabbed = false;
            owner = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Heating(on, Time.deltaTime);
        //Debug.Log("iron temp: " + temperature);
        SetTipColor();
        CheckNetworkUpdate();
        clean = Mathf.Clamp(clean, 0, 100);
        Haptic();
    }

    private void Haptic()
    {
        if (isGrabbed && isTouchingTin)
        {
            grabbedController.SendHapticImpulse(solderHapticIntensity, solderHapticDuration);
        }
        else if(isGrabbed && isTouchingSponge)
        {
            if (clean < 50)
            {
                grabbedController.SendHapticImpulse(cleaningHapticIntensity, cleaningHapticDuration);
            }
            else
            {
                grabbedController.SendHapticImpulse(cleanedHapticIntensity, 0.1f);
                isTouchingSponge = false;
            }
        }
    }

    void SetTipColor()
    {
        // set from heat and clean
        // temperature 0-1, clean 0-100

        float normalizedTemp = (temperature - roomTemperature) / (setTemperature-roomTemperature);
        //Debug.Log("norm temp: " + normalizedTemp);
        tipMat.SetFloat("_Temperature", normalizedTemp);
        tipMat.SetFloat("_Clean", clean);
    }
    void Heating(bool on, float dt)
    {
        // using simple logic P = heat/time
        // make sure takes around a minute for 30w power
        float change = on && (setTemperature>temperature) ? (0.3f * power) : -5.0f;
        change = dt * change;
        float newTemperature = temperature + change;
        if(newTemperature >= roomTemperature)
        {
            temperature = newTemperature;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sponge"))
        {
            clean += Time.deltaTime*30;
            isTouchingSponge = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sponge"))
        {
            isTouchingSponge = false;
        }
    }
    public void TouchTin(float meltingPoint)
    {
        if(temperature > meltingPoint)
        {
            ironSmoke.Play();
            isTouchingTin = true;
        }
    }
    public void LeaveTin()
    {
        clean -= 10;
        ironSmoke.Stop();
        isTouchingTin = false;
    }
    // Network
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool soldering;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;
        transform.localRotation = m.rotation;
        soldering = m.soldering;

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = transform.localPosition;
        lastSolderingState = soldering;
        lastRotation = transform.localRotation;
    }

    private void CheckNetworkUpdate()
    {

        if (owner)
        {
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                rotation = transform.localRotation,
                soldering = soldering
            });
        }
        //if (lastPosition != transform.localPosition)
        //{
        //    lastPosition = transform.localPosition;
        //    context.SendJson(new Message()
        //    {
        //        position = transform.localPosition,
        //        rotation = transform.localRotation,
        //        soldering = soldering
        //    });
        //}
        //if (lastSolderingState != soldering)
        //{
        //    lastSolderingState = soldering;
        //    context.SendJson(new Message()
        //    {
        //        position = transform.localPosition,
        //        rotation = transform.localRotation,
        //        soldering = soldering
        //    });
        //}
        //if (lastRotation != transform.localRotation)
        //{
        //    lastRotation = transform.localRotation;
        //    context.SendJson(new Message()
        //    {
        //        position = transform.localPosition,
        //        rotation = transform.localRotation,
        //        soldering = soldering
        //    });
        //}

    }

}
