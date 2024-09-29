using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineMensMorris;

public class BatchAnimator : MonoBehaviour
{
    [SerializeField] BoardManager bm;

    public void MarkValidNodesForMovement(List<Node> validNNodes)
    {
        foreach (Node node in bm.GetAllNodes())
        {
            node.Mono.UpdateIsUsable(validNNodes.Contains(node));
        }
    }
}
