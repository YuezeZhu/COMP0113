using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;

public class FIRE : MonoBehaviour
{

    private ParticleSystem smoke;

    NetworkContext context;
    bool fire;
    bool lastFire;
    private void Start()
    {
        smoke = GetComponentInChildren<ParticleSystem>();
        smoke.Stop();
        context = NetworkScene.Register(this);
        fire = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Iron"))
        {
            if(collision.collider.gameObject.GetComponent<SolderingIron>().temperature > 100)
            {
                smoke.Play();
                fire = true;
            }
        }
    }
    //private void OnCollisionExit(Collision collision)
    //{

    //    if (collision.collider.CompareTag("Iron"))
    //    {
    //        smoke.Stop();
    //        fire = false;
    //    }
    //}
    private void Update()
    {
        if (lastFire != fire)
        {
            lastFire = fire;
            context.SendJson(new Message()
            {
                fire=fire
            });
        }
    }
    private struct Message
    {
        public bool fire;
    }
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();
        fire = m.fire;
        if (m.fire)
        {
            smoke.Play();
        }
        else
        {
            smoke.Stop();
        }
    }
}
