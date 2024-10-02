using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NineMensMorris
{
    public class Node
    {
        public readonly Vector2Int BoardCoord;
        public readonly Vector3 LocalPos;
        private readonly List<Vector2Int> edgeDirections;
        private readonly BoardManager boardManager;

        //Associated Monobehaviours
        NodeMono mono;
        Token token;

        //Properties
        public List<Vector2Int> EdgeDirections => new(edgeDirections);
        public NodeMono Mono => mono;
        public Token Token => token;

        public Node(Vector2Int coord, Vector3 localPos, List<Vector2Int> localEdgeDirections, BoardManager manager)
        {
            BoardCoord = coord;
            boardManager = manager;
            LocalPos = localPos;
            edgeDirections = localEdgeDirections;
        }

        public override string ToString()
        {
            return $"Node-{BoardCoord}";
        }

        public void SetupMono(NodeMono nodeMono)
        {
            mono = nodeMono;
            mono.Setup(this);
        }

        #region Token Handling

        public void UnlinkToken()
        {
            token = null;
        }

        public void LinkToken(Token token)
        {
            this.token = token;
            token.transform.SetParent(mono.transform);
        }

        #endregion

        public void HandleNodeClicked()
        {
            boardManager.HandleNodeClicked(this);
        }
    }
}

