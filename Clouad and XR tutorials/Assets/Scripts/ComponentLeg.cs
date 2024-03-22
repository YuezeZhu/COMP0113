using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;


public class ComponentLeg : MonoBehaviour
{
    public tinVolumeRange volumeRequirement; // this need to be move to a separate component
    public Transform tinAttach;
    //public float tinVolume { get;private set; }
    public float tinVolume;
    public GameObject mainComponent;
    public bool isTinned { get; private set; }
    public bool isSoldered { get; set; }
    // Network
    NetworkContext context;
    Vector3 lastPosition;
    bool lastTinState;
    private void Start()
    {
        context = NetworkScene.Register(this);
        tinVolume = 0;
    }
    private void Update()
    {
        if (!isTinned && mainComponent.GetComponent<Components>().isAttached)
        {
            Solder();
        }
        CheckNetworkUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Melted Tin"))
        {
            Debug.Log("added");
            AddTin(other.gameObject);
            //other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private void AddTin(GameObject tin)
    {
        if (mainComponent.GetComponent<Components>().isAttached)
        {
            tin.transform.SetParent(gameObject.transform);
            tin.transform.localPosition = tinAttach.localPosition;
            tin.transform.localRotation = Random.rotation;
            tin.GetComponent<Collider>().enabled = false; // avoid multiple detection
            float volume = tin.GetComponent<SeparatedTin>().amount;
            tinVolume += volume;
            //Debug.Log("attached volume: " + tinVolume);
        }
    }

    private void Solder()
    {
        if (volumeRequirement.min <= tinVolume && tinVolume <= volumeRequirement.max)
        {
            isTinned = true;
        }
    }

    // Network
    private struct Message
    {
        public Vector3 position;
        public bool isTinned;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;
        isTinned = m.isTinned;

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = transform.localPosition;
        lastTinState = isTinned;
    }

    private void CheckNetworkUpdate()
    {
        if (lastPosition != transform.localPosition)
        {
            lastPosition = transform.localPosition;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                isTinned = isTinned
            });
        }
        if (lastTinState != isTinned)
        {
            lastTinState = isTinned;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                isTinned = isTinned
            });
        }

    }
#if UNITY_EDITOR

    [MenuItem("Tools/component debug")]
    public static void ComponentDebug()
    {
        Debug.Log("is solder: " + Selection.activeGameObject.GetComponent<ComponentLeg>().isTinned);
        Debug.Log("attached volume: " + Selection.activeGameObject.GetComponent<ComponentLeg>().isTinned);
    }
#endif
}
