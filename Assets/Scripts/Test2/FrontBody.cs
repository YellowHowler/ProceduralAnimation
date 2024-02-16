using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnumDef;

public class FrontBody : MonoBehaviour
{
    //limbs
    public Limb arm1;
    public Limb arm2;
    public Limb leg1;
    public Limb leg2;
    //----------------------------------------------------------------

    //Transform references
    public Transform legBody;
    //----------------------------------------------------------------


    //temp Transform references
    public Transform rayTarget;
    public Transform projectionTarget;
    //----------------------------------------------------------------


    //properties
    private float maxDist = 3f;
    //----------------------------------------------------------------


    //states
    public bool isMoving{get;set;}
    private Limb curLimb{get;set;}
    private Limb prevLimb{get;set;}
    public float cycleProgress{get;set;}
    private Quaternion initRot;
    //----------------------------------------------------------------


    //constants
    private Vector3 nullVector3 = new Vector3(0, 10000, 0);
    //----------------------------------------------------------------

    //raycast parameters
    [SerializeField] private LayerMask IgnoreLayers; 
    private Ray ray;
    private RaycastHit hit;
    private Vector3 aimPos; 
    private Vector3 frontPos;
    private Vector3 prevFrontPos;
    //----------------------------------------------------------------


    //utility structs & functions
    struct RayInfo
    {
        public Vector3 origin;
        public Vector3 offset;

        public RayInfo(Vector3 _origin, Vector3 _offset)
        {
            origin = _origin;
            offset = _offset;
        }
    }
    private Vector3 CastRayDown(Vector3 rayOrigin, Vector3 rayOffset, bool firstCall = true)
    {
        Vector3 returnPoint = nullVector3;

        if(rayOffset.magnitude < 0.1f && !firstCall) return nullVector3;
        if(Physics.Raycast(origin: rayOrigin,
                                direction: -transform.up + rayOffset,
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            Debug.DrawRay(rayOrigin, -transform.up + rayOffset, Color.green);
            returnPoint = hit.point;
        }
        else 
        {
            return CastRayDown(rayOrigin, rayOffset*0.7f, false);
        }

        if(Vector3.Distance(returnPoint, rayOrigin) > maxDist * 1.5f)
        {
            return CastRayDown(rayOrigin, rayOffset*0.7f, false);
        }
        
        return returnPoint;
    }

    private bool AreVectorsSame(Vector3 v1, Vector3 v2)
    {
        return Vector3.Distance(v1, v2) <= 0.001f;
    }

    /*
    private Limb GetCurLimb(int ind)
    {
        switch(ind)
        {
            case 1:
                return arm1;
            case 2: 
                return leg2;
            case 3:
                return arm2;
            case 4:
                return leg1;
            default:
                return null;
        }
    }
    */

    private bool IsInFront (Vector3 a, Vector3 b, Vector3 forwardDir) //returns true if a is in front of b
    {
        return Vector3.Dot(forwardDir, a-b) >= 0.0f;
    }
    
    //----------------------------------------------------------------

    private Limb GetBackLimb(LimbType type)
    {
        if(type == LimbType.Arm)
        {
            if(IsInFront(arm1.leaf.position, arm2.leaf.position, transform.forward)) return arm2;
            return arm1;
        }

        if(IsInFront(leg1.leaf.position, leg2.leaf.position, legBody.forward)) return leg2;
        return leg1;
    }

    private void MoveArm(Vector3 target, Limb limb)
    {
        if(isMoving == true) return;
        isMoving = true;

        limb.ChangeTarget(limb.leaf.position, target, transform);
    }
    
    void Start()
    {
        maxDist = arm1.length * 0.8f;

        frontPos = CastRayDown(transform.position, new Vector3(0, 0, 0));
        prevFrontPos = frontPos;

        prevLimb = leg2;
        curLimb = leg2;
        initRot = transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float hor = Input.GetAxis("Horizontal");
    	float vert = Input.GetAxis("Vertical");

        Limb backArm = GetBackLimb(LimbType.Arm);
        Limb backLeg = GetBackLimb(LimbType.Leg);

        if(isMoving == false) //when limb still
        {
            if(vert != 0) //when moving
            {
                //getting the next limb to move
                prevLimb = curLimb;

                switch(prevLimb.type)
                {
                    case LimbType.Arm:
                        curLimb = backLeg;
                        break;
                    case LimbType.Leg:
                        curLimb = backArm;
                        break;
                    default:
                        if(Vector3.Distance(backArm.leaf.position, backLeg.leaf.position) <= 0.7f)
                        {
                            curLimb = backArm;
                        }
                        else
                        {
                            curLimb = backLeg;
                        }
                        break;
                }


                /*
                if(Vector3.Distance(backArm.leaf.position, backLeg.leaf.position) <= 0.7f) //move arm
                {
                    if(curLimb == leg1) curLimb = arm1;
                    else if (curLimb == leg2) curLimb = arm2;
                    else curLimb = backArm;
                }
                else
                {
                    if(curLimb == arm1) curLimb = leg2;
                    else if (curLimb == arm2) curLimb = leg1;
                    else curLimb = backLeg;
                }
                */


                //getting the target position
                if(curLimb.type == LimbType.Arm) //if arm get new position forward
                {
                    Vector3 rayOffset = (hor*transform.right + vert*transform.forward*5).normalized * 1.2f;
                    aimPos = CastRayDown(transform.position, rayOffset + (curLimb.root.position - transform.position) * 0.6f); 

                    cycleProgress = 0; //body position progession percent relative to two paws
                    initRot = transform.rotation;

                    prevFrontPos = frontPos;
                    frontPos = CastRayDown(transform.position, rayOffset); 

                    rayTarget.position = aimPos;
                }
                else //if leg aim for position right begind arm
                {
                    aimPos = backArm.leaf.position;
                }


                //moving limb
                MoveArm(aimPos, curLimb);
            }
        }
        else //when moving limb
        {
            Vector3 frontDir = frontPos - prevFrontPos;
            //print(prevFrontPos + " " + frontPos);
            
            Debug.DrawRay(prevFrontPos, frontDir, Color.red);

            transform.rotation = Quaternion.Slerp(initRot, Quaternion.LookRotation(frontDir, Vector3.up), cycleProgress);

            if(curLimb.type == LimbType.Arm)
            {
                /*
                Vector3 projectedPos = Vector3.ProjectOnPlane(curLimb.leaf.position, transform.up) + Vector3.Dot(curLimb.alt.leaf.position, transform.up) * transform.up;
                projectionTarget.position = projectedPos;
                transform.position = (projectedPos + curLimb.leaf.position) / 2 + transform.up * maxDist*0.9f;
                */
                transform.position = Vector3.Lerp(curLimb.alt.leaf.position, aimPos, cycleProgress) + transform.up * maxDist*0.9f;
            }
            else
            {
                //transform.position = (arm1.leaf.position + arm2.leaf.position) / 2 + transform.up * maxDist*0.9f;
                transform.position = Vector3.Lerp(backArm.leaf.position, backArm.alt.leaf.position, cycleProgress) + transform.up * maxDist*0.9f;
            }
            
        }
    }
}
