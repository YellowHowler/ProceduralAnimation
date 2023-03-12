using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    [SerializeField] private Transform[] nodes;

    private Vector3[] positions;
    private Quaternion[] rotations; 

    private int l = 150;
    private Vector3 prevPos; //previous position of node[0] (root)
    */

    private void Start()
    {
        StartCoroutine(WaitStart());
    }
    
    private void Update()
    {
        
    }

    private void Initialize()
    {
        positions = new Vector3[l];
        rotations = new Quaternion[l];

        positions[0] = nodes[0].position;
        rotations[0] = nodes[0].rotation;

        for(int i = 1; i < l; i++)
        {
            positions[i] = positions[i-1] - nodes[0].forward*speed;
            rotations[i] = rotations[0];
        }

        prevPos = nodes[0].position;
    }

    private IEnumerator WaitStart()
    {
        yield return new WaitForSeconds(2);
        Initialize();
    }

    private void FixedUpdate()
    {
        if(prevPos != nodes[0].position)
        {
            for(int i = l-1; i >= 1; i--)
            {
                positions[i] = positions[i-1];
                rotations[i] = rotations[i-1];
            }

            positions[0] = nodes[0].position;
            rotations[0] = nodes[0].rotation;

            for(int i = 1; i < nodes.Length; i++)
            {
                nodes[i].position = positions[distances[i-1]];
                nodes[i].rotation = rotations[distances[i-1]];
            }
        }

        prevPos = nodes[0].position;
    }
}
