using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockControl : MonoBehaviour
{
    [SerializeField]
    private GameObject boid;
    public int number = 40;
    private List<GameObject> boidsList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < number; i++)
        {
            GameObject newBoid = Instantiate(boid);
            boidsList.Add(newBoid);
        }
    }
    public List<GameObject> getBoidsList()
    {
        return boidsList;
    }


}
