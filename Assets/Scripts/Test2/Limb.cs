using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnumDef;

public class Limb : MonoBehaviour
{
    //values
    public LimbType type;
    //----------------------------------------------------------------


    //Transform and Limb references
    private Transform target;

    public Transform root;
    public Transform leaf;
     
    protected Transform[] bones;

        //public Transform modelLeaf;
     
        //protected Transform[] modelBones;

    public Limb alt; //other arm or leg
    //----------------------------------------------------------------


    //constants
    public int chainLength = 2;
    
    [HideInInspector] public float length;
    private int iterations = 10;
    private float delta = 0.001f;
    //----------------------------------------------------------------


    //raycast parameters
    [SerializeField] private LayerMask IgnoreLayers; 
    private RaycastHit hit;
    //----------------------------------------------------------------


    //temp variables
    private Vector3 mid;
    private bool inAir = false;
    //----------------------------------------------------------------

    private Vector3 CastRayDown(Vector3 rayOrigin, Vector3 rayOffset)
    {
        if(Physics.Raycast(origin: rayOrigin,
                                direction: -transform.up + rayOffset,
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            return hit.point;
        }
        else 
        {
            print("null");
            return new Vector3(0, 10000, 0);
        }
    }

    private void Start()
    {
        length = GetComponent<InverseKinematics>().length;
        target = GetComponent<InverseKinematics>().target;
        target.position = CastRayDown(root.position, Vector3.zero);
    }

    public void ChangeTargetPos(Vector3 pos)
    {
        target.position = pos;
    }

    public void ChangeTarget(Vector3 oldPos, Vector3 newPos, Transform body)
    {
        StartCoroutine(ChangeTargetCor(oldPos, newPos, body));
    }

    private IEnumerator ChangeTargetCor(Vector3 oldPos, Vector3 newPos, Transform body)
    {
        bool changedMoving = false;

        FrontBody bodyScript = body.gameObject.GetComponent<FrontBody>();

        bodyScript.isMoving = true;
        WaitForSeconds sec = new WaitForSeconds(0.02f);
        mid = body.up*0.3f + (newPos + oldPos)/2;

        for(float i = 0; i < 1; i+= 0.05f)
        {
            target.position = (1-i) * ((1-i)*oldPos + i*mid) + i*((1-i)*mid + i*newPos);
            bodyScript.cycleProgressArm = Mathf.Clamp(bodyScript.cycleProgressArm + 0.039f, 0, 1);
            bodyScript.cycleProgressLeg = Mathf.Clamp(bodyScript.cycleProgressLeg + 0.039f, 0, 1);

            if(bodyScript.cycleProgressArm ==1 ) print("hi");

            yield return sec;

            if(i >= 0.65f && !changedMoving) 
            {
                bodyScript.isMoving = false;
                changedMoving = true;
            }
        }
    }
}
