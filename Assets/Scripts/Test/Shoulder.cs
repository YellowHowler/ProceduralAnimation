using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoulder : MonoBehaviour
{
    [SerializeField] private Transform armJoint1;
    [SerializeField] private Transform armJoint2;
    [SerializeField] private Transform bodyJoint;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 vect1 = armJoint2.position - armJoint1.position;
        Vector3 vect2 = bodyJoint.right;

        Vector3 dir = Vector3.Cross(vect1, vect2);

        transform.rotation = Quaternion.LookRotation(dir);
        Debug.DrawLine(transform.position, transform.position + vect1 * 4, Color.blue);
        Debug.DrawLine(transform.position, transform.position + vect2 * 4, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + dir * 4, Color.red);
    }
}
