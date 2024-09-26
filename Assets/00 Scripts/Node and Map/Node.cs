using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineMensMorris;

namespace NineMensMorris
{
    public class Node
    {
        public readonly Vector3Int WorldCoord;

        //Variables
        Vector3 localPos = Vector3.zero;
        List<Vector3Int> edgeDirections = new();
        NodeMono myNodeMono;

        //Properties
        public Vector3 LocalPos => localPos;
        public List<Vector3Int> EdgeDirections => new(edgeDirections);
        public NodeMono NodeMono => myNodeMono;

        public Node(Vector3Int worldCoord)
        {
            WorldCoord = worldCoord;
        }

        public void SetupEdgeDirections(List<Vector3Int> localEdgeDirections)
        {
            edgeDirections.AddRange(localEdgeDirections);
        }

        public void SetupLocalPosition(Vector3 localPos)
        {
            this.localPos = localPos;
        }

        public void SetupMono(NodeMono nodeMono)
        {
            this.myNodeMono = nodeMono;
            myNodeMono.Setup(this);
        }
    }
}

