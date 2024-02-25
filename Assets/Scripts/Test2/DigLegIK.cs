using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnumDef;

public class DigLegIK : InverseKinematics
{
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
    }

/*
    protected void UpdateJoints()
    {
        Vector3 newTarget = bones[0].position + Vector3.ClampMagnitude(target.position - bones[0].position, length*0.9f);
        float targetDist = Vector3.Distance(newTarget, bones[0].position);

        float l1 = Vector3.Distance(bones[0].position, bones[1].position);
        float l2 = Vector3.Distance(bones[1].position, bones[2].position);
        float l3 = Vector3.Distance(bones[0].position, bones[1].position);

        if(0 <= targetDist && targetDist <= length * 0.4f)
        {
            //bones[0].position + Vector3.Lerp(bones[2].position - bones[0].position,)
        }
        else if(targetDist <= 0.7f)
        {

        }
        else
        {

        }
    }
    */

    protected void DrawLimbLine()
    {
        Vector3[] points = new Vector3[bones.Length];
        
        for(int i = 0; i < bones.Length; i++)
        {
            points[i] = bones[i].position;
        }

        Vector3 zero = bones[0].position;
        Vector3 one = bones[1].position;
        Vector3 two = bones[2].position;

        points[1] = zero + Vector3.Reflect(two - one, zero-two);

        lr.SetPositions(points);
    }
}
