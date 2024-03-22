using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SolderStation : MonoBehaviour
{
    public Transform targetTransform; // target transform
    private bool isAttached = false;
    private Collider holderCollider;

    private void Start()
    {
        GameObject iron = GameObject.Find("Soldering_Iron"); // Only support one iron
        XRGrabInteractable grabInteractable = iron.GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(EnableTrigger);
        holderCollider = gameObject.GetComponent<Collider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Iron")) // if collide with iron
        {
            AttachIron(other.gameObject);
        }
    }
    private void EnableTrigger(BaseInteractionEventArgs args)
    {
        holderCollider.enabled = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Iron"))
        {
            isAttached = false;
            other.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

    private void AttachIron(GameObject iron)
    {
        if (!isAttached)
        {
            isAttached = true;
            iron.GetComponent<XRGrabInteractable>().enabled = false;
            iron.transform.position = targetTransform.position;
            iron.transform.rotation = targetTransform.rotation;

            iron.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            holderCollider.enabled = false;
            iron.GetComponent<XRGrabInteractable>().enabled = true;
        }
    }
}

