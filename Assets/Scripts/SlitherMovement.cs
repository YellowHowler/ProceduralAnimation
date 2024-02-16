using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement2 : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform[] nodes;

    private int[] nodeInds;

    private Vector3[] posRecord;
    private int nodeCount;

    void Start()
    {
        posRecord = new Vector3[100];

        nodeCount = nodes.Length;

        nodeInds = new int[nodeCount];

        Vector3 bodyDir = (nodes[nodeCount - 1].position - head.position).normalized;
    }

    void FixedUpdate()
    {
        
    }
}
