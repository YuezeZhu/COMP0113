using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour 
{
    private GameObject attractor;
    public float separateForce = 0.5f;
    public float alignForce = 0.2f;
    public float cohesionForce = 0.1f;
    public float attractingForce = 0.1f;
    public float attractingRadius = 5;
    public float radius = 3;
    public float maxSpeed = 5;
    public float maxForce = 1;
    private Vector3 randomPos;
    private GameObject flock;
    private Vector3 acceleration;
    private Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        attractor = GameObject.FindGameObjectWithTag("attract boids");
        flock = GameObject.Find("flock");
        velocity = Random.insideUnitSphere*maxSpeed;
        randomPos = new Vector3(Random.Range(-31f, 23f), Random.Range(-17f, 35f), Random.Range(-40f, 37f));
        transform.position = randomPos;

        float ratio = maxSpeed / 25;
        acceleration *= ratio;
        separateForce *= ratio;
        alignForce *= ratio;
        cohesionForce *= ratio;
        attractingForce *= ratio;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        acceleration = new Vector3(0,0,0);
        Attraction();
        Separation();
        Align();
        Cohesion();
        velocity += acceleration;
        if(velocity.magnitude > 0.01)
        {
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
        }
        velocity = velocity.normalized*maxSpeed;
        transform.position += velocity/60;
        Wrap();
    }

    public Vector3 getVel()
    {
        return velocity;
    }
    private void Wrap()
    {
        //x
        if (transform.position.x > 23)
        {
            transform.position = new Vector3(-31, transform.position.y, transform.position.z);
        }
        if (transform.position.x < -31)
        {
            transform.position = new Vector3(23, transform.position.y, transform.position.z);
        }
        //y
        if (transform.position.y > 35)
        {
            transform.position = new Vector3(transform.position.x, -17, transform.position.z);
        }
        if (transform.position.y < -17)
        {
            transform.position = new Vector3(transform.position.x, 35, transform.position.z);
        }
        //z
        if (transform.position.z > 37)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y,-40);
        }
        if (transform.position.z < -40)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 37);
        }
    }

    private void Align()
    {
        Vector3 avg = new Vector3(0,0,0);
        List<GameObject> list = flock.GetComponent<FlockControl>().getBoidsList();
        int total = list.Count;
        for (int i = 0; i < total; i++)
        {
            GameObject other = list[i];
            if (other == null || other == gameObject)
            {
                continue;
            }
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance < radius)
            {
                //Debug.Log(other);
                avg += other.GetComponent<Boids>().getVel();
            }
        }
        //Debug.Log(totalSteering);
        if (total > 1)
        {
            avg = avg / (total-1);
            avg -= velocity;
            acceleration += Vector3.ClampMagnitude(avg *  alignForce, maxForce);
        }
    }

    private void Cohesion()
    {
        Vector3 avgPos = new Vector3(0, 0, 0); 
        List<GameObject> list = flock.GetComponent<FlockControl>().getBoidsList();
        int total = list.Count;
        for (int i = 0; i < total; i++)
        {
            GameObject other = list[i];
            if (other == null || other == gameObject)
            {
                continue;
            }
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance < radius)
            {
                //Debug.Log(other);
                avgPos += other.transform.position;
            }
        }
        //Debug.Log(totalSteering);
        if (total > 1)
        {
            avgPos = avgPos / (total - 1);
            avgPos -= transform.position;
            avgPos -= velocity;
            acceleration += Vector3.ClampMagnitude(avgPos * cohesionForce/100, maxForce);
        }
    }
    private void Separation()
    {
        Vector3 totalSteering = new Vector3(0, 0, 0);
        List<GameObject> list = flock.GetComponent<FlockControl>().getBoidsList();
        int total = list.Count;
        for (int i = 0; i < total; i++)
        {
            GameObject other = list[i];
            if (other == null || other == gameObject)
            {
                continue;
            }
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if(distance < radius)
            {
                //Debug.Log(other);
                Vector3 awayVelocity =  (transform.position - other.transform.position)/Mathf.Pow(distance/10,3);
                totalSteering += awayVelocity;
            }
        }
        //Debug.Log(totalSteering);
        if(total > 1)
        {
            totalSteering = totalSteering/(total - 1);
            totalSteering -= velocity;
            acceleration += Vector3.ClampMagnitude(totalSteering * separateForce, maxForce);
        }
    }
    private void Attraction()
    {
        if (attractor == null)
        {
            return;
        }
        float distance = Vector3.Distance(transform.position, attractor.transform.position);
        if (distance > attractingRadius)
        {
            return;
        }
        Vector3 steering = attractor.transform.position;
        steering -= transform.position;
        steering -= velocity;
        acceleration += Vector3.ClampMagnitude(steering * attractingForce, maxForce);
    }
    private void Avoid()
    {

    }
}
