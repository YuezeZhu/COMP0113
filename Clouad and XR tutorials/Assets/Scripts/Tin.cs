using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.UI.GridLayoutGroup;

public class Tin : MonoBehaviour
{
    public BaseTin tin;
    private float meltingPoint;
    public bool flux = false;
    public float volume {get; private set;}

    private void Awake()
    {
        volume = 0.0f;
    }

    private void Start()
    {
        meltingPoint = SceneManager.instance.tinMeltingPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Iron Head")
        {
            GameObject ironObject = other.transform.parent.gameObject;
            ironObject.gameObject.GetComponent<SolderingIron>().soldering = true;
            ironObject.gameObject.GetComponent<SolderingIron>().TouchTin(meltingPoint);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Iron Head")
        {
            GameObject ironObject = other.transform.parent.gameObject;
            float ironTemp = ironObject.gameObject.GetComponent<SolderingIron>().temperature;
            if (ironTemp >= meltingPoint)
            {
               volume += (ironTemp - meltingPoint)*Time.deltaTime;
            }
            Debug.Log("iron temp measured from Tin logic: " + ironTemp);
            Debug.Log("volume melted: " + volume);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Iron Head" && volume > 0.0f)
        {
            GameObject ironObject = other.transform.parent.gameObject;
            ironObject.gameObject.GetComponent<SolderingIron>().soldering = false;
            ironObject.gameObject.GetComponent<SolderingIron>().LeaveTin();
            AttachToIron(ironObject);
        }
    }

    private void AttachToIron(GameObject iron)
    {
        float ironTemp = iron.GetComponent<SolderingIron>().temperature;
        Transform tinIronAttach = iron.transform.Find("TinTransform").transform;
        Debug.Log("New tin separated");
        GameObject newTin = Instantiate(gameObject, tinIronAttach);
        newTin.transform.localPosition = new Vector3(0, 0, 0);
        newTin.AddComponent<SeparatedTin>();
        newTin.GetComponent<SeparatedTin>().meltingPoint = meltingPoint;
        newTin.GetComponent<SeparatedTin>().flux = flux;
        newTin.GetComponent<SeparatedTin>().melted = true;
        newTin.GetComponent<SeparatedTin>().amount = volume;
        newTin.GetComponent<SeparatedTin>().temperature = ironTemp;
        Destroy(newTin.GetComponent<Tin>());
        if (newTin.GetComponent<XRGrabInteractable>() != null)
            newTin.GetComponent<XRGrabInteractable>().enabled = false;
        newTin.GetComponent<Rigidbody>().isKinematic = true;
        newTin.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
            //Destroy(newTin.GetComponent<XRGrabInteractable>());
        volume = 0.0f;
        newTin.tag = "Melted Tin";
        newTin.gameObject.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
    }
}
