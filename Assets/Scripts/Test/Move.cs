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

    [SerializeField] private FollowBody nextBody;

    public bool isMoving{get;set;}

    private int curArm = 1;
    private int curDir = 0;

    private Vector3 castPos;
    private Vector3 bodyRayPos;
    private Vector3 aimPos;

    private float optBodyHeight = 2f;
    private float maxLegDist = 1.2f;

    private Vector3 nullVector3;

    private Vector3 CastRayDown(Vector3 rayOffset)
    {
        if(Physics.Raycast(origin: transform.position,
                                direction: -transform.up + rayOffset,
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            return hit.point;
        }
        else return nullVector3;
    }

    private void MoveArm(Vector3 target)
    {
        isMoving = true;

        StartCoroutine(MoveBody(target));
        if(curArm == 1)
            arm1.gameObject.GetComponent<Arm>().ChangeTarget(arm1.position, target, transform);
        else if(curArm == 2)
            arm2.gameObject.GetComponent<Arm>().ChangeTarget(arm2.position, target, transform);
    }

    private IEnumerator MoveBody(Vector3 target)
    {
        Vector3 frontPos, backPos;
        frontPos = target;
        if(curArm == 1)
            backPos = arm1.position;
        else
            backPos = arm2.position;
        
        WaitForSeconds sec = new WaitForSeconds(0.06f);

        Quaternion curRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(curDir * (frontPos - backPos));
        temp.rotation = targetRot;

        Vector3 curPos = transform.position;
        Vector3 targetPos = frontPos + temp.up*optBodyHeight;

        //nextBody.MoveBody(Vector3.Distance(targetPos, curPos) , 0.06f, curDir, targetPos);

        for(float i = 0; i < 1; i+= 0.1f)
        {
            transform.position = Vector3.Lerp(curPos, targetPos, i);
            if(i < 0.5f) transform.rotation = Quaternion.Slerp(curRot, targetRot, i*2);
            yield return sec;
        }
    }

    private int giveDir(float v)
    {
        if(v == 0) return 0;
        if(v > 0) return 1;
        return -1;
    }

    void Awake()
    {
        nullVector3 = new Vector3(0, 1000, 0);

        bodyRayPos = CastRayDown(Vector3.zero);

        if(bodyRayPos != nullVector3)
        {
            transform.position = bodyRayPos + transform.up*optBodyHeight;
            arm1.gameObject.GetComponent<Arm>().ChangeTargetPos(bodyRayPos);
            arm2.gameObject.GetComponent<Arm>().ChangeTargetPos(bodyRayPos);
        }
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
    	float verticalInput = Input.GetAxis("Vertical");

        Vector3 rayOffset = Vector3.zero + (horizontalInput*transform.right*0.3f + verticalInput*transform.forward).normalized * maxLegDist;
        aimPos = CastRayDown(rayOffset); 

        mouseRay.position = aimPos;

        int dir = giveDir(verticalInput);

        if(verticalInput != 0 && !isMoving)
        {
            if(curDir == dir) curArm = 3 - curArm;
            
            curDir = dir;

            MoveArm(aimPos);
        }
        else if (verticalInput == 0 && !isMoving && Vector3.Distance(arm1.position, arm2.position) > 0.5f)
        {
            if(curArm == 1) 
            {
                curArm = 2;
                MoveArm(arm1.position);
            }
            else
            {
                curArm = 1;
                MoveArm(arm2.position);
            }
        }
    }
}
