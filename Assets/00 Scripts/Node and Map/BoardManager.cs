using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace NineMensMorris 
{
    public class BoardManager : MonoBehaviour
    {
        Dictionary<Vector2Int, Node> nodeMap = new();
      
        [Header("Board Visuals")]
        [SerializeField] float ringOffset = 1f;

        [Header("Prefabs")]
        [SerializeField] NodeMono nodeMonoPrefab;
        [SerializeField] EdgeRenderer edgeRendererPrefab;

        [Space]

        [Header("Unity Events")]
        [SerializeField] UnityEvent<Node> OnNodeClicked;


        #region NodeMap Generation

        private void CreateNewNodesAndMap(BoardAndRulesData data)
        {
            nodeMap.Clear();

            List<GameObject> objectsToDestroy = new();
            foreach (Transform child in transform)
            {
                objectsToDestroy.Add(child.gameObject);
            }
            foreach (GameObject child in objectsToDestroy)
            {
                Destroy(child);
            }

            //Handle center node
            if (data.IncludeCenterNode)
            {
                var coordinate = Vector2Int.zero;
                var edgeDirections = GetEdgeDirectionsFromWorldCoord(coordinate, 0, data);
                var localPos = GetLocalPosition(coordinate);

                Node centerNode = new Node(coordinate, localPos, edgeDirections, this);
                nodeMap.Add(coordinate, centerNode);
            }

            //Populate map with nodes
            for (int ring = 1; ring <= data.RingCount; ring++)
            {
                for (int x = -1; x <= 1; x++) //This assumes 3 nodes per side of ring
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue; //There is no center node in a ring, all nodes are on the rim.

                        var coordinate = new Vector2Int(x * ring, y * ring);
                        var edgeDirections = GetEdgeDirectionsFromWorldCoord(coordinate, ring, data);
                        var localPos = GetLocalPosition(coordinate);

                        Node node = new Node(coordinate, localPos, edgeDirections, this);

                        nodeMap.Add(coordinate, node);
                    }
                }
            }

            Vector3 GetLocalPosition(Vector2Int coordinate)
            {
                float posX = coordinate.x * ringOffset;
                float posY = coordinate.y * ringOffset;
                return new Vector3(posX, posY, 0);
            }
        }

        private List<Vector2Int> GetEdgeDirectionsFromWorldCoord(Vector2Int boardCoord, int ring, BoardAndRulesData data)
        {
            List<Vector2Int> results = new();

            int lowestRingIndex = data.IncludeCenterNode ? 0 : 1;
            int highestRingIndex = data.RingCount;

            if (boardCoord == Vector2Int.zero) //If it is center node
            {
                //Cardinal edges
                results.Add(Vector2Int.up);
                results.Add(Vector2Int.down);
                results.Add(Vector2Int.left);
                results.Add(Vector2Int.right);

                if (data.IncludeDiagonalConnections)
                {
                    results.Add(Vector2Int.up + Vector2Int.left);
                    results.Add(Vector2Int.up + Vector2Int.right);
                    results.Add(Vector2Int.down + Vector2Int.left);
                    results.Add(Vector2Int.down + Vector2Int.right);
                }
            }
            else if (IsCornerNode(boardCoord))
            {
                //Edges on same ring
                results.Add(new Vector2Int(-boardCoord.x, 0));
                results.Add(new Vector2Int(0, -boardCoord.y));

                if (data.IncludeDiagonalConnections)
                {
                    AddRingToRingConnections(boardCoord);
                }
            }
            else //Middle of a side of the square ring
            {
                //Edges on the same ring
                if (boardCoord.x == 0)
                {
                    results.Add(new Vector2Int(1 * ring, 0));
                    results.Add(new Vector2Int(-1 * ring, 0));
                }
                else if (boardCoord.y == 0)
                {
                    results.Add(new Vector2Int(0, 1 * ring));
                    results.Add(new Vector2Int(0, -1 * ring));
                }

                AddRingToRingConnections(boardCoord);
            }
            return results;

            void AddRingToRingConnections(Vector2Int boardCoord)
            {
                int deltaX = boardCoord.x == 0 ? 0 : (int)Mathf.Sign(boardCoord.x);
                int deltaY = boardCoord.y == 0 ? 0 : (int)(Mathf.Sign(boardCoord.y));

                //Connection to outer ring
                if (lowestRingIndex < ring)
                {
                    results.Add(new Vector2Int(-deltaX, -deltaY));
                }

                //Connection to inner ring
                if (ring < highestRingIndex)
                {
                    results.Add(new Vector2Int(deltaX, deltaY));
                }
            }

            bool IsCornerNode(Vector2Int boardCoord)
            {
                if (Mathf.Abs(boardCoord.x) - Mathf.Abs(boardCoord.y) == 0) return true;
                else return false;
            }
        }

        #endregion

        public void HandleNodeClicked(Node node)
        {
            OnNodeClicked.Invoke(node);
        }

        public void SetupBoard(BoardAndRulesData data)
        {
            CreateNewNodesAndMap(data);

            //Create NodeMonos
            foreach (Node node in nodeMap.Values)
            {
                //Create Node Monos
                NodeMono nodeMono = Instantiate(nodeMonoPrefab, node.LocalPos + transform.position, transform.rotation, transform);
                node.SetupMono(nodeMono);
            }

            //Create EdgeRenderers
            List<HashSet<Node>> edgeNodeSets = new();
            foreach (Node node in nodeMap.Values)
            {
                foreach (Vector2Int edgeDirection in node.EdgeDirections)
                {
                    HashSet<Node> edgeNodeSet = new()
                    {
                        node,
                        nodeMap[node.BoardCoord + edgeDirection]
                    };

                    //Ensure we have no duplicate edges. Edge A->B is equal to Edge B->A.
                    if (edgeNodeSets.All(nodeSet => !edgeNodeSet.SetEquals(nodeSet)))
                    {
                        edgeNodeSets.Add(edgeNodeSet);
                    }
                }
            }

            foreach (HashSet<Node> set in edgeNodeSets)
            {
                EdgeRenderer er = Instantiate(edgeRendererPrefab, transform.position, transform.rotation, transform);
                Node[] nodes = set.ToArray();

                nodes[0].Mono.AddEdgeRenderer(er);
                nodes[1].Mono.AddEdgeRenderer(er);

                er.SetupEndNodes(nodes[0].Mono, nodes[1].Mono);
                er.UpdateLineEndpoints();
            }
        }

        private Node GetNode(Vector2Int coordinate)
        {
            try 
            {
                return nodeMap[coordinate];
            }
            catch 
            { 
                return null;
            }
        }

        public List<Node> GetConnectingNodes(Node node)
        {
            List<Node> result = new();
            foreach (Vector2Int edge in node.EdgeDirections)
            {
                Node connectingNode = GetNode(edge + node.BoardCoord);
                if (connectingNode != null) result.Add(connectingNode);
            }
            return result;
        }

        public List<Node> GetAllNodes()
        {
            return nodeMap.Values.ToList();
        }

        public List<Node> GetAllEmptyNodes()
        {
            return nodeMap.Values
                .Where(n => n.Token == null)
                .ToList();
        }

        public List<Node> GetAllFullNodes()
        {
            return nodeMap.Values
                .Where (n => n.Token != null)
                .ToList();
        }

        public List<Node> GetAllPlayerTokenNodes(PlayerData targetPlayer)
        {
            return GetAllNodes()
                .Where(n => n.Token != null)
                .Where(n => n.Token.Player == targetPlayer)
                .ToList();
        }

        public List<HashSet<Node>> GetNewMills(Node firstNode, PlayerData activePlayer, int numOfTokensForMill)
        {
            List<HashSet<Node>> foundMills = new();

            if (numOfTokensForMill < 2)
            {
                throw new Exception($"Number of tokens per mill is too low ({numOfTokensForMill}), must be at least 2.");
            }

            //Check each connecting node to target node. For each direction 1 mill could be found.
            foreach (Vector2Int direction in firstNode.EdgeDirections)
            {
                Node secondNode = GetNode(firstNode.BoardCoord + direction);

                if (NodeHasFriendlyToken(secondNode) == false) continue;

                HashSet<Node> mill = new()
                {
                    firstNode,
                    secondNode
                };

                if (EnoughNodesInMill(mill)) //Could be true if numOfTokensForMill is 2
                {
                    foundMills.Add(mill);
                    continue;
                }

                //Find the rest of connecting tokens in the direction of the edge
                Vector2Int currentDirection = direction;
                Node lastValidNode = secondNode;
                bool reversedDirection = false; //We will reverse direction once, because our first node could have been in the middle of a mill.

                for (int i = 0; i < numOfTokensForMill - 2; i++) // -2 because the first node and second node are already added.
                {
                    Vector2Int coordinateToCheck = lastValidNode.BoardCoord + currentDirection;
                    Node node = GetNode(coordinateToCheck);

                    if (NodeHasFriendlyToken(node))
                    {
                        mill.Add(node);

                        if (EnoughNodesInMill(mill))
                        {
                            foundMills.Add(mill);
                            break;
                        }
                        else lastValidNode = node;
                    }
                    else
                    {
                        if (reversedDirection)
                        {
                            //We have tried reversing direction once already.
                            //There are no more potential nodes to check.
                            //Abandon this mill.
                            break;
                        }

                        currentDirection = -currentDirection;
                        lastValidNode = firstNode;
                        reversedDirection = true;
                        i -= 1;
                    }
                }
            }

            //Remove any identical mills. 
            //Two identical mills could be created if the first token in a 3 token mill is in the middle.
            List<HashSet<Node>> uniqueMills = new();
            foreach (HashSet<Node> mill in foundMills)
            {
                if (uniqueMills.Any(m => m.SetEquals(mill))) continue;
                else uniqueMills.Add(mill);
            }

            return foundMills;

            bool NodeHasFriendlyToken(Node node)
            {
                if (node == null) return false;
                if (node.Token == null) return false;
                if (node.Token.Player != activePlayer) return false;
                return true;
            }

            bool EnoughNodesInMill(HashSet<Node> mill)
            {
                if (mill.Count == numOfTokensForMill) return true;
                else return false;
            }
        }
    }
}


