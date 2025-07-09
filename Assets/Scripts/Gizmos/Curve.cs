using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve : MonoBehaviour
{
    public Transform startTransform;
    public Transform endTransform;

    private Vector3[] curve = new Vector3[20];

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 start = startTransform.position;
        Vector3 end = endTransform.position;

        Vector3 mid = Vector3.up*0.6f + (start+end)/2f;

        for(float i = 0; i < 1f; i+= 0.05f)
        {
            curve[(int)(i*20)] = (1-i) * ((1-i)*start + i*mid) + i*((1-i)*mid + i*end);
        }   
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for(int i = 0; i < curve.Length; i++)
        {
            Gizmos.DrawSphere(curve[i], 0.02f);
        }
    }
}
