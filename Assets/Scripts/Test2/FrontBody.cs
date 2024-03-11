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
    public Transform testObj;
    //----------------------------------------------------------------


    //properties
    private float maxDist = 3f;
    private float bodyDist;
    private float height = 0.55f;
    private float moveSpeed = 0.75f;
    private float rotSpeed = 90f;
    //----------------------------------------------------------------


    //states
    private CatState curState;
    
    private float idleTime = 0;

    public bool isMoving{get;set;}
    private Limb curLimb{get;set;}
    private Limb prevLimb{get;set;}
    
    [HideInInspector] public LimbBodyInfo armBodyInfo;
    [HideInInspector] public LimbBodyInfo legBodyInfo;
    //----------------------------------------------------------------


    //constants
    private Vector3 nullVect = new Vector3(0, 10000, 0);
    //----------------------------------------------------------------

    //raycast parameters
    [SerializeField] private LayerMask IgnoreLayers; 
    private Ray ray;
    private RaycastHit hit;
    private Vector3 armAimPos; 
    private Vector3 legAimPos; 
    //----------------------------------------------------------------


    //utility structs & functions
    public struct RayInfo
    {
        public Vector3 origin;
        public Vector3 offset;

        public RayInfo(Vector3 _origin, Vector3 _offset)
        {
            origin = _origin;
            offset = _offset;
        }
    }

    public struct LimbBodyInfo
    {
        public float cycleProgress;
        public Quaternion initRot;
        public Vector3 frontPos;
        public Vector3 prevFrontPos;

        public LimbBodyInfo(float _cycleProgress, Quaternion _initRot, Vector3 _frontPos, Vector3 _prevFrontPos)
        {
            cycleProgress = _cycleProgress;
            initRot = _initRot;
            frontPos = _frontPos;
            prevFrontPos = _prevFrontPos;
        }
    }

    private Vector3 CastRayDown(Vector3 rayOrigin, Vector3 rayOffset, bool firstCall = true)
    {
        Vector3 returnPoint = nullVect;

        if(rayOffset.magnitude < 0.1f && !firstCall) return nullVect;
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

    private Vector3 CastRay(Vector3 rayOrigin, Vector3 rayDir)
    {
        Physics.Raycast(origin: rayOrigin,
                                direction: rayDir,
                                hitInfo: out hit,
                                maxDistance: maxDist * 1.5f,
                                layerMask: IgnoreLayers.value);
        Debug.DrawRay(rayOrigin, rayDir, Color.green);
    
        return hit.point;
    }

    private Vector3 ClampVector(Vector3 v, float min, float max)
    {
        v = Vector3.ClampMagnitude(v, max);

        if(v.magnitude < min)
            return v.normalized * min;
        
        return v;
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

    private Vector3 RemoveVectorComponent(Vector3 v, Vector3 dir)
    {
        return v - dir*Vector3.Dot(v, dir);
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

    private void MatchLimbPos(LimbType type)
    {
        float armForwardDist = RemoveVectorComponent(RemoveVectorComponent((arm1.leaf.position - arm2.leaf.position), transform.right), transform.up).magnitude;
        float legForwardDist = RemoveVectorComponent(RemoveVectorComponent((leg1.leaf.position - leg2.leaf.position), legBody.right), transform.up).magnitude;

        if(type == LimbType.Arm && armForwardDist >= 0.4f)
        {
            Limb backArm = GetBackLimb(LimbType.Arm);

            Vector3 rayDir = (backArm.alt.leaf.position + transform.forward * 0.2f) - transform.position;
            Vector3 rayDirVert = RemoveVectorComponent(rayDir, transform.right);
            
            Physics.Raycast(origin: transform.position,
                                direction: (rayDirVert - rayDir) + rayDirVert,
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value);

            armAimPos = hit.point;
            curLimb = backArm;

            MoveArm(armAimPos, curLimb, rayDirVert);
        }  
        else if(type == LimbType.Leg && legForwardDist >= 0.4f)
        {
            Limb backLeg = GetBackLimb(LimbType.Leg);

            Vector3 rayDir = (backLeg.alt.leaf.position + legBody.forward * 0.2f) - legBody.position;
            Vector3 rayDirVert = RemoveVectorComponent(rayDir, legBody.right);
            
            Physics.Raycast(origin: legBody.position,
                                direction: (rayDirVert - rayDir) + rayDirVert,
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value);

            legAimPos = hit.point;
            curLimb = backLeg;

            MoveArm(legAimPos, curLimb, rayDirVert);
        }

        curState = CatState.Still;
    }

    private void MoveArm(Vector3 target, Limb limb, Vector3 rayDir)
    {
        if(isMoving == true) return;
        isMoving = true;

        if(limb.type == LimbType.Arm)
        {
            armBodyInfo.cycleProgress = 0; //body position progession percent relative to two paws
            armBodyInfo.initRot = transform.rotation;
            armBodyInfo.prevFrontPos = armBodyInfo.frontPos;
            armBodyInfo.frontPos = CastRay(transform.position, rayDir); 
        }
        else
        {
            legBodyInfo.cycleProgress = 0; //body position progession percent relative to two paws
            legBodyInfo.initRot = legBody.rotation;
            legBodyInfo.prevFrontPos = legBodyInfo.frontPos;
            legBodyInfo.frontPos = CastRay(legBody.position, rayDir); 
        }

        limb.ChangeTarget(limb.leaf.position, target, transform);
    }

    private void MoveBody(Transform body, Vector3 pos)
    {
        body.position = Vector3.MoveTowards(body.position, pos, moveSpeed * Time.deltaTime);
    }

    private void RotateBody(Transform body, Quaternion rot)
    {
        body.rotation = Quaternion.RotateTowards(body.rotation, rot, rotSpeed * Time.deltaTime);
    }
    
    void Start()
    {
        maxDist = arm1.length * 1f;
        print(maxDist);

        bodyDist = Vector3.Distance(transform.position, legBody.position);

        prevLimb = leg2;
        curLimb = leg2;

        armBodyInfo = new LimbBodyInfo(0, transform.rotation, CastRayDown(transform.position, new Vector3(0, 0, 0)), CastRayDown(transform.position, new Vector3(0, 0, 0)));
        legBodyInfo = new LimbBodyInfo(0, legBody.rotation, CastRayDown(legBody.position, new Vector3(0, 0, 0)), CastRayDown(legBody.position, new Vector3(0, 0, 0)));
    }

    // Update is called once per frame
    void Update()
    {
        float hor = Input.GetAxis("Horizontal");
    	float vert = Input.GetAxis("Vertical");

        Limb backArm = GetBackLimb(LimbType.Arm);
        Limb backLeg = GetBackLimb(LimbType.Leg);

        if(isMoving == false) //when limb still
        {
            if(vert != 0 && curState != CatState.Adjust) //when moving
            {
                idleTime = 0;

                if(Input.GetKeyDown(KeyCode.LeftShift) == false) //when walking
                {
                    curState = CatState.Walk;

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

                    //getting the target position
                    if(curLimb.type == LimbType.Arm) //if arm get new position forward
                    {
                        Vector3 rayOffset = (hor*3*transform.right + vert*transform.forward*5).normalized * 2f;
                        armAimPos = CastRayDown(transform.position, rayOffset + (curLimb.root.position - transform.position) * 0.3f); 

                        rayTarget.position = armAimPos;
                        
                        //moving limb
                        MoveArm(armAimPos, curLimb, RemoveVectorComponent((armAimPos - transform.position), transform.right));
                    }
                    else //if leg aim for position right begind arm
                    {
                        legAimPos = backArm.leaf.position;

                        //moving limb
                        MoveArm(legAimPos, curLimb, RemoveVectorComponent((legAimPos - legBody.position), legBody.right));
                    }
                }
                else
                {
                    curState = CatState.Run;
                }
            }
            else //not moving
            {
                curState = CatState.Still;

                idleTime += Time.deltaTime;

                if(idleTime >= 3f)
                {
                    curState = CatState.Adjust;

                    if(curLimb.type == LimbType.Arm)
                    {
                        if(Vector3.Distance(armAimPos, backLeg.alt.leaf.position) <= bodyDist)
                        {
                            MatchLimbPos(LimbType.Leg);
                            MatchLimbPos(LimbType.Arm);
                        }
                        else
                        {
                            prevLimb = curLimb;
                            curLimb = backLeg;

                            Vector3 aimPos = CastRay(legBody.position, (backArm.alt.leaf.position - transform.forward*0.98f*bodyDist) - legBody.position);
                            if(IsInFront(aimPos, legAimPos, legBody.forward))
                            {
                                legAimPos = aimPos;
                                MoveArm(legAimPos, curLimb, RemoveVectorComponent((legAimPos - legBody.position), legBody.right));
                            }
                        }
                    }
                    else
                    {
                        if(Vector3.Distance(legAimPos, backArm.alt.leaf.position) >= bodyDist * 0.7f)
                        {
                            MatchLimbPos(LimbType.Arm);
                            MatchLimbPos(LimbType.Leg);
                        }
                        else
                        {
                            prevLimb = curLimb;
                            curLimb = backArm;

                            /*
                            Vector3 projectedTransPos = Vector3.ProjectOnPlane(transform.position, Vector3.Cross(arm1.leaf.position - arm2.leaf.position, armBodyInfo.frontPos - armBodyInfo.prevFrontPos));
                            float theta = Vector3.Angle(transform.forward, backLeg.alt.leaf.position - projectedTransPos);
                            float a = Vector3.Distance(backLeg.alt.leaf.position, projectedTransPos);
                            float b = bodyDist * 0.98f;
                            float c = a * Mathf.Cos(theta * Mathf.Deg2Rad) + Mathf.Sqrt(a*a*Mathf.Cos(theta * Mathf.Deg2Rad)*Mathf.Cos(theta * Mathf.Deg2Rad) + 4*b*b);
                            */

                            Vector3 aimPos = CastRay(transform.position, armAimPos + (bodyDist - Vector3.Distance(legAimPos, backArm.alt.leaf.position)) * transform.forward + (curLimb.root.position - transform.position) * 0.3f - transform.position);
                            if(IsInFront(aimPos, armAimPos, transform.forward))
                            {
                                armAimPos = aimPos;
                                MoveArm(armAimPos, curLimb, RemoveVectorComponent((armAimPos - transform.position), transform.right));
                            }
                        }
                    }
                }
            }
        }
        else //when moving limb
        {
            Vector3 frontDirArm = armBodyInfo.frontPos - armBodyInfo.prevFrontPos;
            Vector3 frontDirLeg = legBodyInfo.frontPos - legBodyInfo.prevFrontPos;
            //print(prevFrontPos + " " + frontPos);
            
            Debug.DrawRay(armBodyInfo.prevFrontPos, frontDirArm, Color.blue);

            RotateBody(transform, Quaternion.Slerp(armBodyInfo.initRot, Quaternion.LookRotation(frontDirArm, Vector3.up), armBodyInfo.cycleProgress));
        
            Vector3 legHorDir = leg1.root.position - leg2.root.position;
            RotateBody(legBody, Quaternion.Slerp(armBodyInfo.initRot, Quaternion.LookRotation(-Vector3.Cross(Vector3.Cross(frontDirLeg, legHorDir), legHorDir), Vector3.up), legBodyInfo.cycleProgress));

            Debug.DrawRay(legBody.transform.position, legBody.transform.forward, Color.red);

            if(curLimb.type == LimbType.Arm)
            {
                /*
                Vector3 projectedPos = Vector3.ProjectOnPlane(curLimb.leaf.position, transform.up) + Vector3.Dot(curLimb.alt.leaf.position, transform.up) * transform.up;
                projectionTarget.position = projectedPos;
                transform.position = (projectedPos + curLimb.leaf.position) / 2 + transform.up * maxDist*0.9f;
                */

                MoveBody(transform, Vector3.Lerp(curLimb.alt.leaf.position, armAimPos, armBodyInfo.cycleProgress) + transform.up * height);
                
                Vector3 legBodyPos = Vector3.Lerp(backLeg.leaf.position, backLeg.alt.leaf.position, legBodyInfo.cycleProgress) + legBody.transform.up * height;
                legBodyPos = ClampVector(legBodyPos - transform.position, bodyDist * 0.9f, bodyDist) + transform.position;
                MoveBody(legBody, legBodyPos);
            }
            else
            {
                //transform.position = (arm1.leaf.position + arm2.leaf.position) / 2 + transform.up * maxDist*0.9f;
                MoveBody(transform, Vector3.Lerp(backArm.leaf.position, backArm.alt.leaf.position, armBodyInfo.cycleProgress) + transform.up * height);

                Vector3 legBodyPos = Vector3.Lerp(curLimb.alt.leaf.position, legAimPos, legBodyInfo.cycleProgress) + legBody.transform.up * height;
                legBodyPos = ClampVector(legBodyPos - transform.position, bodyDist * 0.9f, bodyDist) + transform.position;
                MoveBody(legBody, legBodyPos);
            }
        }
    }
}
