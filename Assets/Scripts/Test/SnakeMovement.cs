using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    [SerializeField] private Transform[] nodes;
    private float[] distances;

    private Vector3[] positions;
    private Quaternion[] rotations; 

    private int l = 500;
    private Vector3 prevPos; //previous position of node[0] (root)

    private bool started = false;

    private void Start()
    {
        StartCoroutine(WaitStart());
    }

    private void Initialize()
    {
        positions = new Vector3[l];
        rotations = new Quaternion[l];

        distances = new float[nodes.Length-1];

        positions[0] = nodes[0].position;
        rotations[0] = nodes[0].rotation;

        for(int i = 1; i < nodes.Length; i++)
        {
            distances[i-1] = Vector3.Distance(nodes[i].position, nodes[i-1].position);
        }

        for(int i = 1; i < l; i++)
        {
            positions[i] = positions[i-1] - nodes[0].forward*0.08f;
            rotations[i] = rotations[0];
        }

        prevPos = nodes[0].position;

        started = true;

        StartCoroutine(MoveNodes());
    }

    private IEnumerator WaitStart()
    {
        yield return new WaitForSeconds(2);

        for(int i = 1; i < nodes.Length; i++)
        {
            nodes[i].parent = null;
        }

        Initialize();
    }

    private IEnumerator MoveNodes()
    {
        WaitForSeconds sec = new WaitForSeconds(0.05f);
        /*
        if(started && transform.position != positions[0])
        {
            for(int i = l-1; i >= 1; i--)
            {
                positions[i] = positions[i-1];
                rotations[i] = rotations[i-1];
            }

            positions[0] = transform.position;
            rotations[0] = transform.rotation;

            float dist = 0;
            int j = 0;

            for(int i = 1; i < l; i++)
            {
                dist += Vector3.Distance(positions[i], positions[i-1]);

                if(Mathf.Abs(distances[j] - dist) < 0.1f)
                {
                    nodes[j+1].position = positions[i];
                    nodes[j+1].LookAt(nodes[j]);
                    j++;

                    if(j == nodes.Length - 1) break;
                }
            }

            print(j);
        }
        */

        while(true)
        {
            if(transform.position != prevPos)
            {
                for (int i = 1; i < nodes.Length; i++)
                {
                    Vector3 targetPos = nodes[i - 1].position;
                    targetPos = Vector3.Lerp(nodes[i].position, targetPos, (Vector3.Distance(transform.position, prevPos) * distances[i-1]));
                    nodes[i].position = nodes[i-1].position - (nodes[i-1].position - targetPos).normalized*distances[i-1];
                    nodes[i].LookAt(nodes[i - 1].position);
                }
            }

            prevPos = nodes[0].position;

            yield return sec;
        }
    }
}
