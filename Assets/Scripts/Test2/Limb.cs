using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnumDef;

public class Limb : MonoBehaviour
{
    private LineRenderer lr;

    //values
    public LimbType type;
    //----------------------------------------------------------------


    //Transform and Limb references
    public Transform target;
    public Transform pole;

    public Transform root;
    public Transform leaf;
     
    protected Transform[] bones;

    public Limb alt; //other arm or leg
    //----------------------------------------------------------------


    //constants
    private int chainLength = 2;
    
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

    protected Quaternion targetInitRot;
    protected Quaternion endInitRot;
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
    
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.SetWidth(0.1f, 0.1f);

        bones = new Transform[chainLength + 1];

        targetInitRot = target.rotation;
        endInitRot = transform.rotation;
        var current = transform;
        length = 0;

        for (int i = chainLength - 1; i >= 0; i--)
        {
            length += (current.position - current.parent.position).magnitude;
            bones[i + 1] = current;
            bones[i] = current.parent;

            current = current.parent;
        }
        if (bones[0] == null)
            throw new UnityException("The chain value is longer than the ancestor chain!");
    }

    private void Start()
    {
        target.position = CastRayDown(root.position, Vector3.zero);
    }
  
    private void LateUpdate()
    {
        var lastBone = bones[bones.Length - 1];
       
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            for (var i = bones.Length - 1; i >= 0; i--)
            {
                if (i == bones.Length - 1)
                {
                    bones[i].rotation = target.rotation * Quaternion.Inverse(targetInitRot) * endInitRot;
                }
                else
                {
                    bones[i].rotation = Quaternion.FromToRotation(lastBone.position - bones[i].position, target.position - bones[i].position) * bones[i].rotation;
            
                    if (pole != null && i + 2 <= bones.Length - 1)
                    {
                        var plane = new Plane(bones[i + 2].position - bones[i].position, bones[i].position);
                        var projectedpole = plane.ClosestPointOnPlane(pole.position);
                        var projectedBone = plane.ClosestPointOnPlane(bones[i + 1].position);
                        if ((projectedBone - bones[i].position).sqrMagnitude > 0.01f)
                        {
                            var angle = Vector3.SignedAngle(projectedBone - bones[i].position, projectedpole - bones[i].position, plane.normal);
                            bones[i].rotation = Quaternion.AngleAxis(angle, plane.normal) * bones[i].rotation;
                        }
                    }
                }
               
                if ((lastBone.position - target.position).sqrMagnitude < delta * delta)
                    break;
            }
        }
        
        Vector3[] points = new Vector3[bones.Length];

        for(int i = 0; i < bones.Length; i++)
            points[i] = bones[i].position;

        lr.SetPositions(points);
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
            bodyScript.cycleProgress = Mathf.Clamp(bodyScript.cycleProgress + 0.024f, 0, 1);

            yield return sec;

            if(i >= 0.65f && !changedMoving) 
            {
                bodyScript.isMoving = false;
                changedMoving = true;
            }
        }
    }
}
