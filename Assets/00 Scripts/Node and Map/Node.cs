using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineMensMorris;

namespace NineMensMorris
{
    public class Node
    {
        public readonly Vector3Int BoardCoord;
        public readonly Vector3 LocalPos;
        private readonly List<Vector3Int> edgeDirections;
        private readonly BoardManager boardManager;

        //Associated Monobehaviours
        NodeMono myNodeMono;
        Token myToken;

        //Properties
        public List<Vector3Int> EdgeDirections => new(edgeDirections);
        public NodeMono NodeMono => myNodeMono;
        public Token Token => myToken;

        public Node(Vector3Int coord, Vector3 localPos, List<Vector3Int> localEdgeDirections, BoardManager manager)
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

            //TODO: Handle movement on Token
            token.transform.position = myNodeMono.transform.position;
            token.transform.SetParent(myNodeMono.transform);
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

