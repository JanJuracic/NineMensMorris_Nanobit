using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineMensMorris;

public class BatchAnimator : MonoBehaviour
{
    [SerializeField] BoardManager bm;

    public void MarkValidNodesForMovement(List<Node> validNodes)
    {
        foreach (Node node in bm.GetAllNodes())
        {
            bool nodeIsSelectable = validNodes.Contains(node);
            node.Mono.UpdateNodeIsSelectable(nodeIsSelectable);
        }
    }

    public void MarkValidTokenNodesForSelection(List<Node> validNodes, bool isFriendly)
    {
        foreach (Node node in bm.GetAllNodes())
        {
            bool tokenIsSelectable = validNodes.Contains(node);
            node.Mono.UpdateTokenIsSelectable(tokenIsSelectable, isFriendly);
        }
    }

    public void MarkSelectedTokenNode(Node selectedNode)
    {
        foreach (Node node in bm.GetAllFullNodes())
        {
            node.Token.HandleSelected(selectedNode == node);
        }
    }
}
