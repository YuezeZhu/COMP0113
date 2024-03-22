using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandData : MonoBehaviour
{
    public enum HandModelType {Left, Right}
    public HandModelType modelType;
    public Transform root;
    public Animator animator;
    public Transform[] fingerBones;
}
