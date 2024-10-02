using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineMensMorris;
using System.Linq;

namespace NineMensMorris
{
    public class BatchAnimator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] BoardManager bm;

        [Header("Animation Settings")]
        [SerializeField][Range(0.1f, 1)] float millShakeStrengthFactor = 0.3f;
        [SerializeField][Range(0.1f, 1)] float millShakeDurFactor = 0.3f;

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

        public void AnimateNewMillNodes(List<Node> newNodes)
        {
            if (newNodes.Count == 0) return;

            StartCoroutine(Co_AnimateMills(newNodes));
        }

        private IEnumerator Co_AnimateMills(List<Node> newNodes)
        {
            while (true)
            {
                if (newNodes.All(n => n.Token.IsMoving == false))
                {
                    foreach (Node node in newNodes)
                    {
                        node.Mono.Shake(millShakeStrengthFactor, millShakeDurFactor);
                    }
                    break;
                }
              
                yield return null;
            }
        }
    }
}


