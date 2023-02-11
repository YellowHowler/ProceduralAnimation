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

    [SerializeField] private Transform armCastPos;
    [SerializeField] private Transform legCastPos;
    private Transform castPos;

    private float arm1Length;

    private Vector3 arm1TargetPos;
    private Vector3 arm1FinalTargetPos;

    private Vector3[] targetPos;

    private Vector3 legPrevTargetPos;
    private Vector3 legTargetPos;
    
    private Rigidbody rb;

    RaycastHit hit;

    private bool isMoving = false;
    private bool isRotating;

    [SerializeField] private Transform test;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        targetPos = new Vector3[4];
        arm1Length = arm1LeafPos.gameObject.GetComponent<ArmKinematics>().CompleteLength;

        if(Physics.Raycast(origin: armCastPos.position,
                                direction: (-transform.up + rb.velocity*0.7f),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            arm1TargetPos = hit.point;
            arm1FinalTargetPos = hit.point;

            targetPos[1] = hit.point;
            targetPos[0] = hit.point;
        }
        if(Physics.Raycast(origin: legCastPos.position,
                                direction: (-transform.up + rb.velocity*0.7f),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            targetPos[3] = hit.point;
            targetPos[2] = hit.point;
        }

        legPrevTargetPos = arm1FinalTargetPos;
    }
    
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
    	float verticalInput = Input.GetAxis("Vertical");

    	Vector3 movement = transform.forward * verticalInput;
    	movement = movement.normalized * speed * Time.deltaTime;

        {
            transform.Rotate(new Vector3(0, horizontalInput * rb.velocity.magnitude * 0.05f,0));
            rb.AddForce(movement*5, ForceMode.Impulse);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 2f);
            float ang = Mathf.Acos(Vector3.Dot(transform.forward, rb.velocity) / (transform.forward.magnitude * rb.velocity.magnitude));
            if(ang < Mathf.PI/2)
            {
                dir = -1;
                castPos = armCastPos;
            }
            else   
            {
                dir = 1;
                castPos = legCastPos;
            }
        }

        if(!isMoving)
        {
            if(Physics.Raycast(origin: castPos.position,
                                direction: (-transform.up + rb.velocity*0.7f),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
            {
                Debug.DrawRay(castPos.position,(-transform.up + rb.velocity*0.7f), Color.green, 0.05f, true);
                test.position = hit.point;
                arm1TargetPos = hit.point;
            }

            
            if(Vector3.Distance(arm1FinalTargetPos, arm1TargetPos) > 3f)
            {
                StartCoroutine(StartMove());
            }
        }
    }

    private IEnumerator StartMove()
    {
        WaitForSeconds sec = new WaitForSeconds(0.3f);
        WaitForSeconds sec2 = new WaitForSeconds(0.1f);

        isMoving = true;
        
        if(dir == -1)
        {
            targetPos[3] = targetPos[2];
            targetPos[2] = targetPos[1];
            targetPos[1] = targetPos[0];
            targetPos[0] = arm1TargetPos;

            arm1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[2], targetPos[0], transform);
        }
        else
        {
            targetPos[0] = targetPos[1];
            targetPos[1] = targetPos[2];
            targetPos[2] = targetPos[3];
            targetPos[3] = arm1TargetPos;

            leg1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[1], targetPos[3], transform);
        }
        arm1FinalTargetPos = arm1TargetPos;

        yield return sec;
        yield return sec2;

        if(dir == -1) leg2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[3]-transform.forward*0.1f, targetPos[1]-transform.forward*0.1f, transform);
        else arm2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[0]+transform.forward*0.1f, targetPos[2]+transform.forward*0.1f, transform);

        yield return sec;

        if(Physics.Raycast(origin: castPos.position,
                                direction: (-transform.up + rb.velocity*0.7f),
                                hitInfo: out hit,
                                maxDistance: Mathf.Infinity,
                                layerMask: IgnoreLayers.value))
        {
            Debug.DrawRay(castPos.position,(-transform.up + rb.velocity*0.7f), Color.green, 0.05f, true);

            if(dir == -1)
            {
                targetPos[3] = targetPos[2];
                targetPos[2] = targetPos[1];
                targetPos[1] = targetPos[0];
                targetPos[0] = hit.point;
            }
            else
            {
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

        if(dir == -1) arm2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[2], targetPos[0], transform);
        else leg2LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[1], targetPos[3], transform);

        yield return sec;
        yield return sec2;

        if(dir == -1) leg1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[3]-transform.forward*0.1f, targetPos[1]-transform.forward*0.1f, transform);
        else arm1LeafPos.gameObject.GetComponent<ArmKinematics>().ChangeTarget(targetPos[0]+transform.forward*0.1f, targetPos[2]+transform.forward*0.1f, transform);

        yield return sec;

        isMoving = false;
    }
}