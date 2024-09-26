using NineMensMorris;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EdgeRenderer : MonoBehaviour
{
    LineRenderer lr;

    NodeMono firstNodeMono;
    NodeMono secondNodeMono;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void SetupEndNodes(NodeMono firstNode, NodeMono secondNode)
    {
        firstNodeMono = firstNode;
        secondNodeMono = secondNode;
        UpdateLineEndpoints();
    }

    public void UpdateLineEndpoints()
    {
        lr.SetPosition(0, firstNodeMono.transform.position);
        lr.SetPosition(1, secondNodeMono.transform.position);
    }
}
