using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
//using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;

public class CircuitBoard : MonoBehaviour
{
    // script for one circuit board slot
    public float slotTemperature { get; private set; }
    private float roomTemp, ironSetTemp;
    private bool isHeating;
    public GameObject componentLeg;
    private float tinMeltingPoint;
    Material indicatorMat;

    //Network
    NetworkContext context;
    public bool lastHeating;
    private void Start()
    {
        slotTemperature = SceneManager.instance.roomTemperature;
        isHeating = false;
        roomTemp = SceneManager.instance.roomTemperature;
        // For the slot indicator color
        indicatorMat = GetComponent<MeshRenderer>().material;
        GetComponent<MeshRenderer>().enabled = false;
        tinMeltingPoint = SceneManager.instance.tinMeltingPoint;
        context = NetworkScene.Register(this);
    }
    private void Update()
    {
        TransferHeat();
        Solder();
        CheckNetworkUpdate();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Iron Head"))
        {
            Debug.Log("entered indicator");
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            GameObject ironObject = other.transform.parent.gameObject;
            TransferHeat(ironObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Iron Head"))
        {
            isHeating = false;
            //gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    private void TransferHeat(GameObject iron)
    {
        isHeating = true;
        float ironTemp = iron.GetComponent<SolderingIron>().temperature;
        ironSetTemp = iron.GetComponent<SolderingIron>().setTemperature;
        slotTemperature = Mathf.Lerp(slotTemperature, ironTemp, Time.deltaTime);
        Debug.Log("iron temp measured from slot (" + gameObject.name + ") logic: " + ironTemp);
        Debug.Log("board slot temp: " + slotTemperature);
        SetIndicator();
    }
    private void TransferHeat()
    {
        if (!isHeating)
        {
            slotTemperature = Mathf.Lerp(slotTemperature, SceneManager.instance.roomTemperature, Time.deltaTime);
            SetIndicator();
        }
    }

    void Solder()
    {
        ComponentLeg thisLegScript = componentLeg.GetComponent<ComponentLeg>();
        if (!thisLegScript.isSoldered)
        {
            if (thisLegScript.isTinned && slotTemperature >= tinMeltingPoint)
            {
                thisLegScript.isSoldered = true;
            }
        }
    }
    void SetIndicator()
    {
        ComponentLeg thisLegScript = componentLeg.GetComponent<ComponentLeg>();
        float amount = thisLegScript.tinVolume;
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        //if (thisLegScript.volumeRequirement.WithinRange(amount) == 0)
        //{
        // Color - temperature
        Vector3 color = RainbowMap(slotTemperature, roomTemp, ironSetTemp);
        Color tempColor = new Color(color.x, color.y, color.z);
        indicatorMat.SetColor("_Temperature", tempColor);
        // Alpha - amount
        float ceil = (thisLegScript.volumeRequirement.max - thisLegScript.volumeRequirement.min) * 0.8f + thisLegScript.volumeRequirement.min;
        float percent = amount / ceil;
        percent = Mathf.Clamp(percent, 0, 0.3f);
        if (!thisLegScript.isSoldered)
        {
            indicatorMat.SetFloat("_amount", percent);
        }
        else
        {
            indicatorMat.SetFloat("_amount", 1);
        }
        //}
        //else
        //{
        //    // Color - temperature - bad - black
        //    Color tempColor = new Color(0,0,0);
        //    indicatorMat.SetColor("_Temperature", tempColor);
        //    // Alpha - amount
        //    float ceil = thisLegScript.volumeRequirement.min;
        //    float percent = amount / ceil;
        //    indicatorMat.SetFloat("_amount", percent);
        //}
    }

    private Vector3 RainbowMap(float v, float vmin, float vmax)
    {
        Vector3 c = new Vector3(1.0f, 1.0f, 1.0f); // white
        float dv;
        v = Mathf.Clamp(v, vmin, vmax);
        dv = vmax - vmin;

        if (v < (vmin + 0.25 * dv))
        {
            c.x = 0;
            c.y = 4 * (v - vmin) / dv;
        }
        else if (v < (vmin + 0.5 * dv))
        {
            c.x = 0;
            c.z = 1 + 4 * (vmin + 0.25f * dv - v) / dv;
        }
        else if (v < (vmin + 0.75 * dv))
        {
            c.x = 4 * (v - vmin - 0.5f * dv) / dv;
            c.z = 0;
        }
        else
        {
            c.y = 1 + 4 * (vmin + 0.75f * dv - v) / dv;
            c.z = 0;
        }

        return c;
    }

    //Network

    private struct Message
    {
        public bool isHeating;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        isHeating = m.isHeating;
    }

    private void CheckNetworkUpdate()
    {
        if (lastHeating != isHeating)
        {
            lastHeating = isHeating;
            context.SendJson(new Message()
            {
                isHeating = isHeating,
            });
        }
    }
}

