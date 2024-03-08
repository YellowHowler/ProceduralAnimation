using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnumDef;

public class DigLegIK : InverseKinematics
{
    public LineRenderer lrTest;

    protected void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
        base.Start();
    }

    protected void LateUpdate()
    {
        UpdateJoints();
        DrawLimbLine();

        //DrawTestLine();
    }


    protected void UpdateJoints()
    {
        Vector3 zero = bones[0].position;
        Vector3 one = bones[1].position;
        Vector3 two = bones[2].position;
        Vector3 three = bones[3].position;

        Vector3 newTarget = zero + Vector3.ClampMagnitude(target.position - zero, length*0.9f);
        
        if((newTarget - zero).magnitude < length*0.3f)
        {
            newTarget = zero + (newTarget - zero).normalized*length*0.3f;
        }

        float targetDist = Vector3.Distance(newTarget, zero);

        float l1 = Vector3.Distance(zero, one);
        float l2 = Vector3.Distance(one, two);
        float l3 = Vector3.Distance(two, three);

        float cutoff1 = l1 * Mathf.Cos(Mathf.PI / 8) + (l2 + l3) * Mathf.Sin(Mathf.PI / 6.5f);
        float cutoff2 = l1 * Mathf.Cos(Mathf.PI / 8) + (l2 + l3) * Mathf.Sin(Mathf.PI / 3);

        float firstAng = 0;

        if(0 <= targetDist && targetDist <= cutoff1)
        {
            float angCos =  Mathf.Clamp(Mathf.Cos(Mathf.PI / 8) - (cutoff1 - targetDist)/l1, 0, 1);
            firstAng = Mathf.Acos(angCos);
            firstAng *= Mathf.Rad2Deg;
        }
        else if(targetDist <= cutoff2)
        {
            firstAng = Mathf.PI / 8 * Mathf.Rad2Deg;
        }
        else
        {
            float angCos = Mathf.Clamp(Mathf.Cos(Mathf.PI / 8) + (-cutoff2 + targetDist)/l1, 0, 1);
            firstAng = Mathf.Acos(angCos);
            firstAng *= Mathf.Rad2Deg;
        }

        Vector3 offset = (newTarget - zero).normalized * l1;
        Vector3 axis = Vector3.Cross(offset, pole.position - zero);
        Quaternion rotation = Quaternion.AngleAxis(-firstAng, axis);

        print(firstAng);

        bones[1].position = zero + (rotation * offset);

        var lastBone = bones[bones.Length - 1];
       
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            for (var i = bones.Length - 1; i >= 1; i--)
            {
                if (i == bones.Length - 1)
                {
                    bones[i].rotation = target.rotation * Quaternion.Inverse(targetInitRot) * endInitRot;
                }
                else
                {
                    bones[i].rotation = Quaternion.FromToRotation(lastBone.position - bones[i].position, newTarget - bones[i].position) * bones[i].rotation;
            
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
               
                if ((lastBone.position - newTarget).sqrMagnitude < delta * delta)
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

        /*
        Vector3 zero = bones[0].position;
        Vector3 one = bones[1].position;
        Vector3 two = bones[2].position;

        points[1] = zero + Vector3.Reflect(two - one, zero-two);
        */

        lr.SetPositions(points);
    }

    protected void DrawTestLine()
    {
        lrTest.SetWidth(0.07f, 0.07f);

        Vector3[] points = new Vector3[3];

        points[0] = bones[1].position;
        points[1] = bones[0].position;
        points[2] = bones[3].position;

        lrTest.SetPositions(points);
    }
}
