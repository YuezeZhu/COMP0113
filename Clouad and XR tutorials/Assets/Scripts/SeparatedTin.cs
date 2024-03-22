using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeparatedTin : MonoBehaviour
{ 
    public float amount { get; set; }
    public bool melted { get; set; }
    public float temperature { get; set; }
    public float meltingPoint { get; set; }
    public bool flux { get; set; }

    public void TempChange(float dT)
    {
        temperature += dT;
        melted = temperature >=  meltingPoint ? true : false;
    }
}
