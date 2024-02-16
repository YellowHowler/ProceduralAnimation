using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
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

    //

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

    private void Start()
    {
        //Target.position = transform.position;
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

    public void ChangeTargetPos(Vector3 pos)
    {
        Target.position = pos;
    }

    public void ChangeTarget(Vector3 oldPos, Vector3 newPos, Transform body)
    {
        StartCoroutine(ChangeTargetCor(oldPos, newPos, body));
    }

    private IEnumerator ChangeTargetCor(Vector3 oldPos, Vector3 newPos, Transform body)
    {
        Move bodyScript = body.gameObject.GetComponent<Move>();

        bodyScript.isMoving = true;
        WaitForSeconds sec = new WaitForSeconds(0.02f);
        mid = body.up*2f + (newPos + oldPos)/2;

        bool changedMoving = false;

        for(float i = 0; i < 1; i+= 0.05f)
        {
            Target.position = (1-i) * ((1-i)*oldPos + i*mid) + i*((1-i)*mid + i*newPos); //bezier curve for moving leg upwards
            //bodyNode.Rotate(0, 0, AngleDir(bodyNode.forward, root.position - bodyNode.position, bodyNode.up) * 10f * i);

            if(i >= 0.8f && changedMoving == false)
            {
                bodyScript.isMoving = false; 
                changedMoving = true;
            }

            yield return sec;
        }

         //bodyScript.isMoving = false; 
    }

    private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);
		
		if (dir > 0f) {
			return 1f;
		} else if (dir < 0f) {
			return -1f;
		} else {
			return 0f;
		}
	}
}
