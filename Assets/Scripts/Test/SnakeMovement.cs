using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    [SerializeField] private Transform[] BodyParts;
    private List<Vector3> positions = new List<Vector3>();

    public int gap = 10;

    private Vector3 prevPos;

    /*
    [SerializeField] private Transform[] nodes;
    private int[] distances; // distance in fixed frames (50 per second)
    [SerializeField] private float speed; //meters per second / 50

    private Vector3[] positions;
    private Quaternion[] rotations; 

    private int l = 0;
    private Vector3 prevPos; //previous position of node[0] (root)
    */

    private void Start()
    {
        prevPos = transform.position;
        //StartCoroutine(WaitStart());
    }
    
    private void Update()
    {
            positions.Insert(0, transform.position);
            //if(positions.Count > 100) positions.RemoveAt(positions.Count - 1);

            int ind = 0;
            foreach (var body in BodyParts) {
                Vector3 point = positions[Mathf.Clamp(ind * gap, 0, positions.Count - 1)];

                // Move body towards the point along the snakes path
                Vector3 moveDirection = point - body.transform.position;
                body.position += transform.position - prevPos;

                // Rotate body towards the point along the snakes path
                body.LookAt(point);

                ind++;
            }

        prevPos = transform.position;
    }

    /*
    private void Initialize()
    {
        distances = new int[nodes.Length - 1];

        for(int i = 0; i < distances.Length; i++)
        {
            distances[i] = (int)(Vector3.Distance(nodes[i+1].position, nodes[i].position) / speed);
            l += distances[i];
        }

        l++;

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
        yield return new WaitForSeconds(3);
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
    */


}
