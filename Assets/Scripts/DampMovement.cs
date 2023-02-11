using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DampMovement : MonoBehaviour
{
    [SerializeField] private Transform follow;
    [SerializeField] private Rigidbody mainFollow;
    private float d;

    private Quaternion prevRot;
    private Quaternion targetRot;

    private Vector3 targetPos;

    private Rigidbody followRb;
    
    void Start()
    {
        d = Vector3.Distance(follow.position, transform.position);
        prevRot = transform.rotation;

        StartCoroutine(RecordPos());
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, (mainFollow.velocity.magnitude-1.9f)/18);

        targetPos = follow.position + follow.forward * -0.7f * follow.localScale.z/2;
        transform.position += targetPos - (transform.position + transform.forward * transform.localScale.z/2);
    }

    private IEnumerator RecordPos()
    {
        WaitForSeconds sec = new WaitForSeconds(0.1f);

        while(true){
            targetRot = follow.rotation;
            yield return sec;
            prevRot = follow.rotation;
        }
    }
}
