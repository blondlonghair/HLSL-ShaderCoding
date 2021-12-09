using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject target;
    public Material mat;

    private Vector3 pos = new Vector3(0,0,-9);
    private float degree;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        lineRenderer.SetPosition(0, target.transform.position);
        lineRenderer.SetPosition(1, transform.position);

        float distance = Vector3.Distance(target.transform.position, gameObject.transform.position);

        degree += 1;
        pos.x += Mathf.Cos(degree * Mathf.PI / 180) / 5;
        transform.position = pos;

        mat.SetFloat("_LineLength", distance * 0.2f);
    }
}
