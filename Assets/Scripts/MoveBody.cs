using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBody : MonoBehaviour
{
    [SerializeField] private LayerMask IgnoreLayers; 

    [SerializeField] public float speed = 0.35f;
    [HideInInspector] public float dir;

    [SerializeField] private Transform arm1LeafPos;
    [SerializeField] private Transform arm2LeafPos;
    [SerializeField] private Transform leg1LeafPos;
    [SerializeField] private Transform leg2LeafPos;

    private Transform arm1RootPos;
    private Transform arm2RootPos;
    private Transform leg1RootPos;
    private Transform leg2RootPos;

    [SerializeField] private Transform armCastPos;
    [SerializeField] private Transform legCastPos;
    private Transform castPos;

    private float armLength;
    private float offsetAmount = 1f;

    [HideInInspector] public Quaternion lookRotation;

    private Vector3 armTargetPos;
    private Vector3 arm1FinalTargetPos;

    private Vector3 legTargetPos;

    private Vector3[] targetPos;
    
    private Rigidbody rb;

    RaycastHit hit;

    [HideInInspector] public bool isMoving = false;
    private bool isRotating;

    private int lastMoved = 4;

    [SerializeField] private Transform test1;
    [SerializeField] private Transform test2;
    [SerializeField] private Transform test3;
    [SerializeField] private Transform test4;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        arm1RootPos = arm1LeafPos.gameObject.GetComponent<ArmKinematics>().root;
        arm2RootPos = arm2LeafPos.gameObject.GetComponent<ArmKinematics>().root;
        leg1RootPos = leg1LeafPos.gameObject.GetComponent<ArmKinematics>().root;
        leg2RootPos = leg2LeafPos.gameObject.GetComponent<ArmKinematics>().root;

        targetPos = new Vector3[4];
        armLength = Vector3.Distance(arm1LeafPos.gameObject.GetComponent<ArmKinematics>().root.position, arm1LeafPos.position) * offsetAmount;

        if(Physics.Raycast(origin: armCastPos.position,
                                direction: (-transform.up + rb.velocity*0.9f),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            armTargetPos = hit.point;
            arm1FinalTargetPos = hit.point;

            targetPos[1] = hit.point;
            targetPos[0] = hit.point;
        }
        if(Physics.Raycast(origin: legCastPos.position,
                                direction: (-transform.up + rb.velocity*0.9f),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            targetPos[3] = hit.point;
            targetPos[2] = hit.point;
        }
    }
    
    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
    	float verticalInput = Input.GetAxis("Vertical");

    	Vector3 movement = transform.forward * verticalInput;
    	movement = movement.normalized * speed * Time.deltaTime;

        test1.position = targetPos[0];
        test2.position = targetPos[1];
        test3.position = targetPos[2];
        test4.position = targetPos[3];

        lookRotation = Quaternion.LookRotation((targetPos[0] - targetPos[3]));

        {
            rb.AddForce(movement*5, ForceMode.Impulse);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 2f);
            float ang = Mathf.Acos(Vector3.Dot(transform.forward, rb.velocity) / (transform.forward.magnitude * rb.velocity.magnitude));
            //ang < Mathf.PI/2
            if(verticalInput > 0)
            {
                dir = -1;
                castPos = armCastPos;
            }
            else if (verticalInput < 0)  
            {
                dir = 1;
                castPos = legCastPos;
            }
            else 
            {
                dir = 0;
            }
        }

        Vector3 rayOffset = (new Vector3(-horizontalInput * 0.5f, 0, 0) + rb.velocity*0.9f).normalized * 1.5f;

        if(!isMoving && dir != 0)
        {
            if(Physics.Raycast(origin: armCastPos.position,
                                direction: (-transform.up + rayOffset),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
            {
                Debug.DrawRay(castPos.position,(-transform.up + rayOffset), Color.green, 0.05f, true);
                armTargetPos = hit.point;
            }

            if(Physics.Raycast(origin: legCastPos.position,
                                direction: (-transform.up + rayOffset),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
            {
                Debug.DrawRay(castPos.position,(-transform.up + rayOffset), Color.green, 0.05f, true);
                legTargetPos = hit.point;
            }

            if(dir == -1)
            {
                if(Vector3.Distance(arm1FinalTargetPos, armTargetPos) > 3f)
                {
                    //StartCoroutine(StartMove());
                }

                if (Vector3.Distance(arm1LeafPos.position, armTargetPos) > armLength && lastMoved == 4)
                {
                    targetPos[3] = targetPos[2];
                    targetPos[2] = targetPos[1];
                    targetPos[1] = targetPos[0];
                    targetPos[0] = armTargetPos;

                    arm1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[2], targetPos[0], transform, true);

                    lastMoved = 1;
                }
                else if (Vector3.Distance(leg2LeafPos.position, legTargetPos) > armLength && lastMoved == 1)
                {
                    leg2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[3]-transform.forward*0.1f, targetPos[1]-transform.forward*0.1f, transform, false);

                    lastMoved = 2;
                }
                else if (Vector3.Distance(arm2RootPos.position, armTargetPos) > armLength && lastMoved == 2)
                {
                    targetPos[3] = targetPos[2];
                    targetPos[2] = targetPos[1];
                    targetPos[1] = targetPos[0];
                    targetPos[0] = armTargetPos;

                    arm2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[2], targetPos[0], transform, true);

                    lastMoved = 3;
                }
                else if (Vector3.Distance(leg1LeafPos.position, legTargetPos) > armLength && lastMoved == 3)
                {
                    leg1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[3]-transform.forward*0.1f, targetPos[1]-transform.forward*0.1f, transform, false);

                    lastMoved = 4;
                }
            }

            //arm1 leg2 arm2 leg1
            //1    2    3    4
            else
            {
                if (Vector3.Distance(leg1LeafPos.position, legTargetPos) > armLength && lastMoved == 4)
                {
                    targetPos[0] = targetPos[1];
                    targetPos[1] = targetPos[2];
                    targetPos[2] = targetPos[3];
                    targetPos[3] = legTargetPos;

                    leg1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[1]-transform.forward*0.1f, targetPos[3]-transform.forward*0.1f, transform, false);

                    lastMoved = 3;
                }
                else if (Vector3.Distance(leg2LeafPos.position, legTargetPos) > armLength && lastMoved == 3)
                {
                    arm2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[0], targetPos[2], transform, true);

                    lastMoved = 2;
                }
                else if (Vector3.Distance(arm2LeafPos.position, armTargetPos) > armLength && lastMoved == 2)
                {
                    targetPos[0] = targetPos[1];
                    targetPos[1] = targetPos[2];
                    targetPos[2] = targetPos[3];
                    targetPos[3] = legTargetPos;

                    leg2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[1]-transform.forward*0.1f, targetPos[3]-transform.forward*0.1f, transform, false);

                    lastMoved = 1;
                }
                else if (Vector3.Distance(arm1LeafPos.position, legTargetPos) > armLength && lastMoved == 1)
                {
                    arm1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[0], targetPos[2], transform, true);

                    lastMoved = 4;
                }
            }
        }
    }

    private IEnumerator StartMove()
    {
        WaitForSeconds sec = new WaitForSeconds(0.3f);
        WaitForSeconds sec2 = new WaitForSeconds(0.2f);

        isMoving = true;
        
        if(dir == -1)
        {
            if(Vector3.Distance(hit.point, arm1LeafPos.gameObject.GetComponent<ArmKinematics>().root.position) > armLength*offsetAmount) yield break;
            targetPos[3] = targetPos[2];
            targetPos[2] = targetPos[1];
            targetPos[1] = targetPos[0];
            targetPos[0] = armTargetPos;

            arm1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[2], targetPos[0], transform, true);
        }
        else
        {
            if(Vector3.Distance(hit.point, leg1LeafPos.gameObject.GetComponent<ArmKinematics>().root.position) > armLength*offsetAmount) yield break;

            targetPos[0] = targetPos[1];
            targetPos[1] = targetPos[2];
            targetPos[2] = targetPos[3];
            targetPos[3] = armTargetPos;

            leg1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[1], targetPos[3], transform, false);
        }
        arm1FinalTargetPos = armTargetPos;

        yield return sec;
        yield return sec2;

        if(dir == -1) leg2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[3]-transform.forward*0.1f, targetPos[1]-transform.forward*0.1f, transform, false);
        else arm2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[0]+transform.forward*0.1f, targetPos[2]+transform.forward*0.1f, transform, true);

        yield return sec;

        if(Physics.Raycast(origin: castPos.position,
                                direction: (-transform.up + rb.velocity*0.9f),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            Debug.DrawRay(castPos.position,(-transform.up + rb.velocity*0.9f), Color.green, 0.05f, true);

            if(dir == -1)
            {
                if(Vector3.Distance(hit.point, arm2LeafPos.gameObject.GetComponent<ArmKinematics>().root.position) > armLength*offsetAmount) yield break;

                targetPos[3] = targetPos[2];
                targetPos[2] = targetPos[1];
                targetPos[1] = targetPos[0];
                targetPos[0] = hit.point;
            }
            else
            {
                if(Vector3.Distance(hit.point, leg2LeafPos.gameObject.GetComponent<ArmKinematics>().root.position) > armLength*offsetAmount) yield break;
    
                targetPos[0] = targetPos[1];
                targetPos[1] = targetPos[2];
                targetPos[2] = targetPos[3];
                targetPos[3] = hit.point;
            }
        }
        else
        {
            yield break;
        }

        if(dir == -1) arm2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[2], targetPos[0], transform, true);
        else leg2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[1], targetPos[3], transform, false);

        yield return sec;
        yield return sec2;

        if(dir == -1) leg1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[3]-transform.forward*0.1f, targetPos[1]-transform.forward*0.1f, transform, false);
        else arm1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[0]+transform.forward*0.1f, targetPos[2]+transform.forward*0.1f, transform, true);

        yield return sec;

        isMoving = false;
    }
}