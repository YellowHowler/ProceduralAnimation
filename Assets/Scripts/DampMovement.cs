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

    private Quaternion[] rotRecord;
    private Vector3[] posRecord;

    private Vector3 targetPos;

    private Rigidbody followRb;
    
    void Start()
    {
        d = Vector3.Distance(follow.position, transform.position);
        prevRot = transform.rotation;

        rotRecord = new Quaternion[10];
        posRecord = new Vector3[10];

        StartCoroutine(RecordPos());
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, (mainFollow.velocity.magnitude-1.9f)/18);

        targetPos = follow.position + follow.forward * -0.7f * follow.localScale.z/2;
        transform.position = Vector3.MoveTowards(transform.position, targetPos - transform.forward * transform.localScale.z/2, 6f * Time.deltaTime);
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

    private IEnumerator RecordMovement()
    {
        WaitForSeconds sec = new WaitForSeconds(0.1f);

        while(true){
            for(int i = 0; i < rotRecord.Length-1; i++)
            {
                rotRecord[i+1] = rotRecord[i];
                posRecord[i+1] = posRecord[i];
            }
            rotRecord[0] = follow.rotation;
            posRecord[0] = follow.position;

            transform.rotation = (rotRecord[8] * Quaternion.Inverse(rotRecord[9])) * transform.rotation;
            transform.position += posRecord[8] - posRecord[9];

            yield return sec;
        }
    }
}
