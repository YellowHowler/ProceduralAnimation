using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBody : MonoBehaviour
{
    [SerializeField] Transform prevBody;
    [SerializeField] Transform nextBody;

    private float dist; //distance from prevBody
    private bool started = false;

    void Awake()
    {
        dist = Vector3.Distance(prevBody.position, transform.position);
        print(dist);
        StartCoroutine(Detach());        
    }

    private IEnumerator Detach()
    {
        yield return new WaitForSeconds(1.5f);
        transform.parent = null;
        started = true;
    }

    void Update()
    {
        if(started) transform.LookAt(prevBody.position);
    }

    public void MoveBody(float distance, float interval, int direction, Vector3 aimPos)
    {
        if(started) StartCoroutine(MoveBodyCor(distance, interval, direction, aimPos));
    }

    private IEnumerator MoveBodyCor(float distance, float interval, int direction, Vector3 aimPos)
    {
        WaitForSeconds sec = new WaitForSeconds(interval);

        Vector3 curPos = transform.position;
        Vector3 targetPos = aimPos - transform.forward*dist/2 - prevBody.forward*dist/2;
        //Vector3 targetPos = prevBody.position + direction * transform.forward*dist * 2;

        if(nextBody != null) nextBody.gameObject.GetComponent<FollowBody>().MoveBody(distance, interval, direction, targetPos);

        for(float i = 0; i <= 1; i+= 0.1f)
        {
            transform.position = Vector3.Lerp(curPos, targetPos, i);
            yield return sec;
        }

        yield break;
    }
}
