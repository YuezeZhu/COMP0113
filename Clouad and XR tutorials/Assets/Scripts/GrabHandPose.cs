using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;
using Ubiq.Messaging;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GrabHandPose : MonoBehaviour
{
    [SerializeField]
    private GameObject avatarManager;
    public HandData rightHandPose;
    public HandData leftHandPose;
    private Vector3 startHandPos;
    private Vector3 endHandPos;
    private Quaternion startHandRot;
    private Quaternion endHandRot;

    private Quaternion[] startFingerRot;
    private Quaternion[] endFingerRot;
    // Network
    NetworkContext context;
    bool grabbed;
    bool hand; //true left, false right
    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(SetUpPose);
        grabInteractable.selectExited.AddListener(UnsetPose);
        rightHandPose.gameObject.SetActive(false);
        leftHandPose.gameObject.SetActive(false);
        context = NetworkScene.Register(this);
    }

    public GameObject FindInChildren(GameObject go, string name)
    {
        return (from x in go.GetComponentsInChildren<Transform>()
                where x.gameObject.name == name
                select x.gameObject).First();
    }

    public void SetUpPose(BaseInteractionEventArgs args)
    {
        if(args.interactorObject is XRDirectInteractor)
        {
            HandData handData;

            Debug.Log(avatarManager);
            if (args.interactorObject.transform.CompareTag("Left hand"))
            {
                handData = FindInChildren(avatarManager, "Left Hand Model").GetComponent<HandData>();
                hand = true;
            }
            else if(args.interactorObject.transform.CompareTag("Right hand"))
            {
                //Debug.Log(FindInChildren(avatarManager, "Right Hand Model"));
                handData = FindInChildren(avatarManager, "Right Hand Model").GetComponent<HandData>();
                hand = false;
            }
            else
            {
                return;
            }
            //HandData handData = args.interactorObject.transform.GetComponentInChildren<HandData>();
            handData.animator.enabled = false;

            if (handData.modelType == HandData.HandModelType.Right)
            {
                SetHandDataValues(handData, rightHandPose);
            }
            else
            {
                SetHandDataValues(handData, leftHandPose);
            }
            SetHandData(handData, endHandPos, endHandRot, endFingerRot);

            // Network
            grabbed = true;
            CheckNetworkUpdate();
        }
    }

    public void SetHandDataValues(HandData h1, HandData h2 )
    {
        startHandPos = new Vector3(h1.root.localPosition.x/h1.root.localScale.x,
                                   h1.root.localPosition.y/h1.root.localScale.y, 
                                   h1.root.localPosition.z/h1.root.localScale.z);
        endHandPos = new Vector3(h2.root.localPosition.x/h2.root.localScale.x,
                                 h2.root.localPosition.y/h2.root.localScale.y,
                                 h2.root.localPosition.z/h2.root.localScale.z);



        startHandRot = h1.root.localRotation;
        endHandRot = h2.root.localRotation;

        startFingerRot = new Quaternion[h1.fingerBones.Length];
        endFingerRot = new Quaternion[h2.fingerBones.Length];

        for ( int i = 0; i < h1.fingerBones.Length; i++ )
        {
            startFingerRot[i] = h1.fingerBones[i].localRotation;
            endFingerRot[i] = h2.fingerBones[i].localRotation;
        }
    }

    public void SetHandData(HandData h, Vector3 newPos, Quaternion newRot, Quaternion[] newBonesRot)
    {
        h.root.localPosition = newPos;
        h.root.localRotation = newRot;

        for (int i = 0; i < h.fingerBones.Length; i++)
        {
            h.fingerBones[i].localRotation = newBonesRot[i];
        }
    }

    public void UnsetPose(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor)
        {
            HandData handData;
            if (args.interactorObject.transform.CompareTag("Left hand"))
            {
                handData = FindInChildren(avatarManager, "Left Hand Model").GetComponent<HandData>();
                hand = true;
            }
            else if (args.interactorObject.transform.CompareTag("Right hand"))
            {
                handData = FindInChildren(avatarManager, "Right Hand Model").GetComponent<HandData>();
                hand = false;
            }
            else
            {
                return;
            }
            //HandData handData = args.interactorObject.transform.GetComponentInChildren<HandData>();
            handData.animator.enabled = true;

            SetHandData(handData, startHandPos, startHandRot, startFingerRot);

            // Network
            grabbed = false;
            CheckNetworkUpdate();
        }
    }

    
    private struct Message
    {
        public bool grabbed;
        public bool hand;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();
        Debug.Log("Grab hand, message received");
        grabbed = m.grabbed;
        hand = m.hand;
        //XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabbed)
        {
            HandData handData;
            if (hand)
            {
                handData = FindInChildren(avatarManager, "Left Hand Model").GetComponent<HandData>();
            }
            else
            {
                handData = FindInChildren(avatarManager, "Right Hand Model").GetComponent<HandData>();
            }
            handData.animator.enabled = false;

            if (handData.modelType == HandData.HandModelType.Right)
            {
                SetHandDataValues(handData, rightHandPose);
            }
            else
            {
                SetHandDataValues(handData, leftHandPose);
            }
            SetHandData(handData, endHandPos, endHandRot, endFingerRot);

        }
        else
        {
            HandData handData;
            if (hand)
            {
                handData = FindInChildren(avatarManager, "Left Hand Model").GetComponent<HandData>();
            }
            else
            {
                handData = FindInChildren(avatarManager, "Right Hand Model").GetComponent<HandData>();
            }
            //HandData handData = args.interactorObject.transform.GetComponentInChildren<HandData>();
            handData.animator.enabled = true;

            SetHandData(handData, startHandPos, startHandRot, startFingerRot);
        }
    }

    private void CheckNetworkUpdate()
    {
        // here this method is invoked
        context.SendJson(new Message()
        {
            grabbed = grabbed,
            hand = hand
        });
    }

#if UNITY_EDITOR
    public void MirrorPose(HandData poseToMirror, HandData poseFromMirror)
    {
        Vector3 mirroredPos = poseFromMirror.root.localPosition;
        mirroredPos.x *= -1;
        
        Quaternion mirroredRot = poseFromMirror.root.localRotation;
        //mirroredRot.y *= -1;
        mirroredRot.z *= -1;
        //mirroredRot.x *= -1;


        poseToMirror.root.localPosition = mirroredPos;
        poseToMirror.root.localRotation = mirroredRot;

        for(int i = 0; i<poseFromMirror.fingerBones.Length; i++)
        {
            poseToMirror.fingerBones[i].localRotation = poseFromMirror.fingerBones[i].localRotation;
        }
    }



    [MenuItem("Tools/Mirror left hand from right")]
    public static void MirrorRightPos()
    {
        GrabHandPose handPose = Selection.activeGameObject.GetComponent<GrabHandPose>();
        handPose.MirrorPose(handPose.leftHandPose, handPose.rightHandPose);
    }

#endif
}
