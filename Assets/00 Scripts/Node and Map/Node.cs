using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineMensMorris;

namespace NineMensMorris
{
    public class Node
    {
        public readonly Vector2Int BoardCoord;
        public readonly Vector3 LocalPos;
        private readonly List<Vector2Int> edgeDirections;
        private readonly BoardManager boardManager;

        //Associated Monobehaviours
        NodeMono myNodeMono;
        Token myToken;

        //Properties
        public List<Vector2Int> EdgeDirections => new(edgeDirections);
        public NodeMono NodeMono => myNodeMono;
        public Token Token => myToken;

        public Node(Vector2Int coord, Vector3 localPos, List<Vector2Int> localEdgeDirections, BoardManager manager)
        {
            BoardCoord = coord;
            boardManager = manager;
            LocalPos = localPos;
            edgeDirections = localEdgeDirections;
        }

        public override string ToString()
        {
            return $"Node({BoardCoord}";
        }

        public void SetupMono(NodeMono nodeMono)
        {
            this.myNodeMono = nodeMono;
            myNodeMono.Setup(this);
        }

        #region Token Handling

        public void UnlinkToken()
        {
            myToken = null;
        }

        public void LinkToken(Token token)
        {
            myToken = token;

            token.transform.SetParent(myNodeMono.transform);
            token.SlideTo(this);
        }

        public void DestroyToken()
        {
            
        }

        #endregion

        public void HandleNodeClicked()
        {
            boardManager.HandleNodeClicked(this);
        }
    }
}

