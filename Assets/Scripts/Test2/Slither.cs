using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slither : MonoBehaviour //attach to first body
{
    public float bodyDist = 0.3f;
    public List<Transform> body = new List<Transform>();
    private List<Vector3> positions = new List<Vector3>();

    void Start()
    {
        for(int i = 0; i < body.Count; i++)
        {
            positions.Add(body[i].position);
        }
    }

    void FixedUpdate()
    {
        float curDist = Vector3.Distance(positions[0], transform.position);

        if(curDist >= bodyDist) //update only when moved certain amount
        {
            positions.Insert(0, transform.position);
            positions.RemoveAt(positions.Count - 1);
        }

        for(int i = 1; i < body.Count; i++)
        {
            Vector3 newPos = Vector3.Lerp(positions[i], positions[i-1], curDist/bodyDist);
            if(Vector3.Distance(newPos, body[i].position) <= bodyDist + 0.5f) body[i].position = newPos;
            body[i].rotation = Quaternion.LookRotation(body[i-1].position - body[i].position, transform.up);
        }
    }
}
