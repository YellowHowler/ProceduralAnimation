using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBodyNodes : MonoBehaviour
{
    private bool initialized = false;
    private bool updating = false;
    private bool updatingPos = false;

    [SerializeField] private float nodeDist;
    [SerializeField] private Transform[] nodes;

    private Transform head;

    private Vector3[] positions;
    private int positionLength = 100;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);

        Initialize();
    }

    void Initialize()
    {
        head = nodes[0];
        positions = new Vector3[positionLength];

        positions[0] = head.position;

        for(int i = 1; i < positionLength; i++)
        {
            positions[i] = positions[i-1] - head.forward.normalized * 0.05f;
        }

        UpdateNodePos();

        initialized = true;

        StartCoroutine(MoveBody());
    }

    void UpdateNodePos()
    {   
        updating = true;

        int nodeInd = 1;
        print("ah");
        float totDist = 0;

        for(int i = 1; i < positionLength; i++)
        {
            totDist += Vector3.Distance(positions[i], positions[i-1]);

            if(totDist >= nodeDist) 
            {
                print("move " + nodeInd);
                nodes[nodeInd].gameObject.GetComponent<Rigidbody>().position = positions[i];
                nodes[nodeInd].gameObject.transform.LookAt(nodes[nodeInd-1]);
                totDist = 0;

                print(totDist + " " + i);
                nodeInd++;

                if(nodeInd >= nodes.Length) break;
            }
        }

        updating = false;
    }

    void Update()
    {
        
    }

    private IEnumerator MoveBody()
    {
        WaitForSeconds sec = new WaitForSeconds(0.02f);

        while(true)
        {
            if(Vector3.Distance(positions[0], head.position) >= 0.1f && updatingPos == false)
            {
                updatingPos = true;
                for(int i = positionLength - 1; i >= 1; i--)
                {
                    positions[i] = positions[i-1];
                }

                positions[0] = head.position;

                if(!updating) UpdateNodePos();

                updatingPos = false;
            }

            yield return sec;
        }
    }
}
