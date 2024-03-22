using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance { get; private set; }

    [SerializeField]
    public float roomTemperature = 20.0f;
    [SerializeField]
    public float tinMeltingPoint = 190.0f;
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
}
