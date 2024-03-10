using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnumDef;

public class InverseKinematics : MonoBehaviour
{
    protected LineRenderer lr;

    //values
    //----------------------------------------------------------------

    //Transform references
    public Transform target;
    public Transform pole;
    protected Transform[] bones;
    //----------------------------------------------------------------
    

    //constants
    public int chainLength = 2;
    
    [HideInInspector] public float length;
    protected int iterations = 10;
    protected float delta = 0.001f;
    //----------------------------------------------------------------


    //variables
    protected Quaternion targetInitRot;
    protected Quaternion endInitRot;
    //----------------------------------------------------------------

    protected void Awake()
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

    protected void Start()
    {
    }

    protected void LateUpdate()
    {
        UpdateJoints();
        DrawLimbLine();
    }

    protected void UpdateJoints()
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
    }

    protected void DrawLimbLine()
    {
        Vector3[] points = new Vector3[bones.Length];
        
        for(int i = 0; i < bones.Length; i++)
        {
            points[i] = bones[i].position;
        }

        lr.SetPositions(points);
    }
}
