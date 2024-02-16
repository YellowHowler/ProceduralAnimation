using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    // test
    [SerializeField] Transform mouseRay;
    [SerializeField] Transform temp;
    //test 

    [SerializeField] private LayerMask IgnoreLayers; 

    private Ray ray;
    private RaycastHit hit;

    [SerializeField] private Transform arm1;
    [SerializeField] private Transform arm2;
    [SerializeField] private Transform leg1;
    [SerializeField] private Transform leg2;

    [SerializeField] private Transform body;
    [SerializeField] private Transform legBody;

    [SerializeField] private FollowBody nextBody;

    public bool isMoving{get;set;}

    private int curArm = 1;
    private int curDir = 0;

    private Vector3 castPos;
    private Vector3 bodyRayPos;
    private Vector3 aimPos;
    private Vector3 aimPosLeg;

    private float optBodyHeight = 2f;
    private float maxLegDist = 2f;

    private Vector3 nullVector3;

    private RayInfo pastRay;

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

    // 1: arm1, 2: leg2, 3: arm2, 4: leg1

    private Vector3 CastRayDown(Vector3 rayOrigin, Vector3 rayOffset)
    {
        if(rayOffset.magnitude < 0.01f) return nullVector3;
        if(Physics.Raycast(origin: rayOrigin,
                                direction: -transform.up + rayOffset,
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            return hit.point;
        }
        else return CastRayDown(rayOrigin, rayOffset*0.7f);
    }

    private void MoveArm(Vector3 target, int armNum)
    {
        print(curArm);

        if(isMoving == true) return;
        isMoving = true;

        StartCoroutine(MoveBody(target));
        if(armNum == 1)
            arm1.gameObject.GetComponent<Arm>().ChangeTarget(arm1.position, target, transform);
        else if(armNum == 3)
            arm2.gameObject.GetComponent<Arm>().ChangeTarget(arm2.position, target, transform);
        else if(armNum == 2)
            leg2.gameObject.GetComponent<Arm>().ChangeTarget(leg2.position, target, transform);
        else
            leg1.gameObject.GetComponent<Arm>().ChangeTarget(leg1.position, target, transform);
    }

    private IEnumerator MoveBody(Vector3 target)
    {
        Vector3 frontPos, backPos;
        frontPos = target;

        bool movingArm = curArm == 1 || curArm == 3;

        if(curArm == 1)
            backPos = arm1.position;
        else if (curArm == 3)
            backPos = arm2.position;
        else 
        {
            if(!IsInFront(arm1.position, arm2.position, transform.forward))
            {
                frontPos = arm1.position;
                backPos = arm2.position;
            }
            else
            {
                frontPos = arm2.position;
                backPos = arm1.position;
            }
        }
        
        WaitForSeconds sec = new WaitForSeconds(0.04f);

        Quaternion curRot = transform.rotation;
        Vector3 upDir = Vector3.Cross(CastRayDown(transform.position, transform.right) - CastRayDown(transform.position, -transform.right), frontPos - backPos);
        Quaternion targetRot = Quaternion.LookRotation(frontPos - backPos, -upDir);
        temp.rotation = targetRot;

        Vector3 curPos = transform.position;
        Vector3 targetPos = Vector3.zero;

        if(movingArm)
        {
            targetPos = (backPos + frontPos) / 2 + temp.up*optBodyHeight;
        }
        else
        {
            targetPos = frontPos + temp.up*optBodyHeight;
        }
        

        //nextBody.MoveBody(Vector3.Distance(targetPos, curPos) , 0.06f, curDir, targetPos);

        for(float i = 0; i <= 1; i+= 0.05f)
        {
            transform.position = Vector3.Lerp(curPos, targetPos, i);
            if(Vector3.Distance(frontPos, backPos) > 0.1f)
            {
                if(i <= 0.5f) transform.rotation = Quaternion.Slerp(curRot, targetRot, i*2.5f);
            }
            yield return sec;
        }
    }

    private int GiveDir(float v)
    {
        if(v == 0) return 0;
        if(v > 0) return 1;
        return -1;
    }

    private bool IsInFront (Vector3 a, Vector3 b, Vector3 forwardDir) //returns true if a is in front of b
    {
        return Vector3.Dot(forwardDir, a-b) < 0.0f;
    }

    void Awake()
    {
        nullVector3 = new Vector3(0, 1000, 0);

        bodyRayPos = CastRayDown(transform.position, Vector3.zero);

        if(bodyRayPos != nullVector3)
        {
            transform.position = bodyRayPos + transform.up*optBodyHeight;
            arm1.gameObject.GetComponent<Arm>().ChangeTargetPos(bodyRayPos);
            arm2.gameObject.GetComponent<Arm>().ChangeTargetPos(bodyRayPos);
        }

        pastRay = new RayInfo(transform.position, Vector3.zero);
    }
    
    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
    	float verticalInput = Input.GetAxis("Vertical");

        int dir = GiveDir(verticalInput);

        if(isMoving == false)
        {
            if(verticalInput != 0) //when moving
            {
                Vector3 rayOffset = Vector3.zero + (horizontalInput*Mathf.Abs(verticalInput)*transform.right + verticalInput*transform.forward*5).normalized * maxLegDist;
                aimPos = CastRayDown(transform.position, rayOffset); 
                aimPosLeg = CastRayDown(legBody.position, rayOffset);

                if(Vector3.Distance(aimPos, transform.position) > 3.5f) return;

                mouseRay.position = aimPos;

                if(curDir == dir) curArm = (curArm)%4 + 1;
                
                curDir = dir;

                if(curArm == 1 || curArm == 3) 
                {
                    MoveArm(aimPos, curArm);
                }
                else if(curArm == 2) 
                {
                    if(Vector3.Distance(arm2.position, legBody.position) < 0.1f) // when front leg's previos position is not far away from leg
                    {
                        MoveArm(arm2.position, curArm);
                    }
                    else
                    {
                        MoveArm(aimPosLeg, curArm);
                    }
                }
                else 
                {
                    if(Vector3.Distance(arm1.position, legBody.position) < 0.1f) // when front leg's previos position is not far away from leg
                    {
                        MoveArm(arm1.position, curArm);
                    }
                    else
                    {
                        MoveArm(aimPosLeg, curArm);
                    }
                }
            }
            else if (verticalInput == 0 && Vector3.Distance(arm1.position, arm2.position) > 1f) //when stopped, match front leg positions
            {
                Vector3 rayOffset = Vector3.zero + transform.forward * maxLegDist * 0.7f;
                aimPos = CastRayDown(transform.position, rayOffset); 
                aimPosLeg = CastRayDown(legBody.position, rayOffset);

                mouseRay.position = aimPos;

                if(curArm == 1 || curArm == 2) 
                {
                    curArm = 3;
                    MoveArm(aimPos, curArm);
                    curArm = 1;
                }
                else
                {
                    curArm = 1;
                    MoveArm(aimPos, curArm);
                    curArm = 3;
                }
            }
            else if (verticalInput == 0 && Vector3.Distance(leg1.position, leg2.position) > 1f) //when stopped, match back leg positions
            {
                if(IsInFront(leg2.position, leg1.position, legBody.forward)) 
                {
                    curArm = 2;
                    MoveArm(leg1.position, curArm);
                    curArm = 1;
                }
                else
                {
                    curArm = 4;
                    MoveArm(leg2.position, curArm);
                    curArm = 3;
                }
            }
        }
    }
}
