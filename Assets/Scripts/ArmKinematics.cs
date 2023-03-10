using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmKinematics : MonoBehaviour
{
    [SerializeField] private int ChainLength = 2;
    [SerializeField] public Transform Target;
    [SerializeField] public Transform root;

    protected Quaternion TargetInitialRotation;
    protected Quaternion EndInitialRotation;

    [SerializeField] private Transform Pole;
    [SerializeField] public float CompleteLength;
    [SerializeField] private int Iterations = 10;
    [SerializeField] private float Delta = 0.001f;
    
    protected Transform[] Bones;

    private Vector3 mid;
    private bool inAir = false;

    private LineRenderer lr;
    
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.SetWidth(0.5f, 0.5f);

        Bones = new Transform[ChainLength + 1];

        TargetInitialRotation = Target.rotation;
        EndInitialRotation = transform.rotation;
        var current = transform;
        CompleteLength = 0;
        for (int i = ChainLength - 1; i >= 0; i--)
        {
            CompleteLength += (current.position - current.parent.position).magnitude;
            Bones[i + 1] = current;
            Bones[i] = current.parent;

            current = current.parent;
        }
        if (Bones[0] == null)
            throw new UnityException("The chain value is longer than the ancestor chain!");
    }
  
    private void LateUpdate()
    {
        var lastBone = Bones[Bones.Length - 1];
       
        for (int iteration = 0; iteration < Iterations; iteration++)
        {
            for (var i = Bones.Length - 1; i >= 0; i--)
            {
                if (i == Bones.Length - 1)
                {
                    Bones[i].rotation = Target.rotation * Quaternion.Inverse(TargetInitialRotation) * EndInitialRotation;
                }
                else
                {
                    Bones[i].rotation = Quaternion.FromToRotation(lastBone.position - Bones[i].position, Target.position - Bones[i].position) * Bones[i].rotation;
            
                    if (Pole != null && i + 2 <= Bones.Length - 1)
                    {
                        var plane = new Plane(Bones[i + 2].position - Bones[i].position, Bones[i].position);
                        var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                        var projectedBone = plane.ClosestPointOnPlane(Bones[i + 1].position);
                        if ((projectedBone - Bones[i].position).sqrMagnitude > 0.01f)
                        {
                            var angle = Vector3.SignedAngle(projectedBone - Bones[i].position, projectedPole - Bones[i].position, plane.normal);
                            Bones[i].rotation = Quaternion.AngleAxis(angle, plane.normal) * Bones[i].rotation;
                        }
                    }
                }
               
                if ((lastBone.position - Target.position).sqrMagnitude < Delta * Delta)
                    break;
            }
        }
        
        Vector3[] points = new Vector3[Bones.Length];

        for(int i = 0; i < Bones.Length; i++)
            points[i] = Bones[i].position;

        lr.SetPositions(points);
    }

    private void FixedUpdate() 
    {
        if(!inAir)
        {
            
        }
    }

    public void ChangeTarget(Vector3 oldPos, Vector3 newPos, Transform body, bool front)
    {
        StartCoroutine(ChangeTargetCor(oldPos, newPos, body, front));
    }

    private IEnumerator ChangeTargetCor(Vector3 oldPos, Vector3 newPos, Transform body, bool front)
    {
        MoveBody bodyScript = body.gameObject.GetComponent<MoveBody>();

        inAir = true;
        bodyScript.isMoving = true;
        WaitForSeconds sec = new WaitForSeconds(0.013f);
        //mid = body.gameObject.GetComponent<MoveBody>().dir * Vector3.Cross((newPos-oldPos).normalized, (root.position - body.position))*3.5f + newPos;
        mid = body.up*1f + (newPos + oldPos)/2;

        Quaternion curRotation = body.rotation;
        Quaternion lookRotation = bodyScript.lookRotation;

        //body.gameObject.GetComponent<MoveBody>().speed = 6f;
        print(bodyScript.speed);

        for(float i = 0; i < 1; i+= 0.05f)
        {
            Target.position = (1-i) * ((1-i)*oldPos + i*mid) + i*((1-i)*mid + i*newPos);
            
            if(i <= 0.5f) body.rotation = Quaternion.Slerp(curRotation, lookRotation, i * 2);

            yield return sec;
        }

        if(front == (bodyScript.dir == -1)){
            Vector3 curPosition = body.position;
            Vector3 targetPosition = body.up * 2f + (newPos + oldPos)/2 + body.forward*0.5f;   
            float bodySpeed = bodyScript.speed;

            bodyScript.speed = 0;

            for(float i = 0; i < 1; i+= 0.1f)
            {
                body.position = Vector3.Lerp(curPosition, targetPosition, i);
                yield return sec;
            }

            bodyScript.speed = bodySpeed;
        }

        bodyScript.isMoving = false;

        //body.gameObject.GetComponent<MoveBody>().speed = 8f;

        // while(Vector3.Distance(Target.position, newPos) > 0.01f)
        // {
        //     Target.position = Vector3.MoveTowards(Target.position, newPos, 0.05f*35);
        //     //body.position = new Vector3(body.position.x, body.position.y + Vector3.MoveTowards(Target.position, newPos, 0.05f*10).y - Target.position.y, body.position.z);
        //     yield return sec;
        // }

        //body.gameObject.GetComponent<MoveBody>().speed = 1.5f;
        //print(body.gameObject.GetComponent<MoveBody>().speed);
        //yield return new WaitForSeconds(0.2f);

        if(bodyScript.dir == -1) body.gameObject.GetComponent<Rigidbody>().AddForce(body.forward*5, ForceMode.Impulse);
        else if (bodyScript.dir == 1) body.gameObject.GetComponent<Rigidbody>().AddForce(-body.forward*5, ForceMode.Impulse);

        inAir = false;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(mid, 0.1f);
    }
}
