using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    // test
    [SerializeField] Transform mouseRay;
    //test 

    [SerializeField] private LayerMask IgnoreLayers; 

    private Ray ray;
    private RaycastHit hit;

    [SerializeField] private Transform arm1;
    [SerializeField] private Transform arm2;
    [SerializeField] private Transform leg1;
    [SerializeField] private Transform leg2;

    public bool isMoving{get;set;}

    private int curArm = 1;
    private int curDir = 0;

    private Vector3 castPos;
    private Vector3 bodyRayPos;
    private Vector3 aimPos;

    private float optBodyHeight = 1.6f;
    private float maxLegDist = 1f;

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

    private void MoveArm()
    {
        isMoving = true;

        StartCoroutine(MoveBody());
        if(curArm == 1)
            arm1.gameObject.GetComponent<Arm>().ChangeTarget(arm1.position, aimPos, transform);
        else if(curArm == 2)
            arm2.gameObject.GetComponent<Arm>().ChangeTarget(arm2.position, aimPos, transform);
    }

    private IEnumerator MoveBody()
    {
        Vector3 frontPos, backPos;
        frontPos = aimPos;
        if(curArm == 1)
            backPos = arm1.position;
        else
            backPos = arm2.position;

        WaitForSeconds sec = new WaitForSeconds(0.05f);

        Quaternion curRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(curDir * (frontPos - backPos));

        for(float i = 0; i < 1; i+= 0.2f)
        {
            transform.rotation = Quaternion.Slerp(curRot, targetRot, i);
            yield return sec;
        }

        //yield return new WaitForSeconds(0.1f);

        Vector3 curPos = transform.position;
        Vector3 targetPos = frontPos + transform.up*optBodyHeight;

        for(float i = 0; i < 1; i+= 0.1f)
        {
            transform.position = Vector3.Lerp(curPos, targetPos, i);
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
        }
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
    	float verticalInput = Input.GetAxis("Vertical");

        Vector3 rayOffset = Vector3.zero + (horizontalInput*transform.right*0.5f + verticalInput*transform.forward).normalized * maxLegDist;
        aimPos = CastRayDown(rayOffset); 

        mouseRay.position = aimPos;

        int dir = giveDir(verticalInput);

        if(verticalInput != 0 && !isMoving)
        {
            if(curDir == dir) curArm = 3 - curArm;
            
            curDir = dir;

            MoveArm();
        }
    
        print(curArm);
    }
}
