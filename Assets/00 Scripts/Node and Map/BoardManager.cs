using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace NineMensMorris 
{
    public class BoardManager : MonoBehaviour
    {
        Dictionary<Vector3Int, Node> nodeMap = new();
      
        [Header("Board Visuals")]
        [SerializeField] float ringOffset = 1f;

        [Header("Node Map Settings")]
        [SerializeField] int ringCount = 3;
        [SerializeField] bool includeDiagonalConnections = false;
        [SerializeField] bool includeCenterNode;

        [Header("Prefabs")]
        [SerializeField] NodeMono nodeMonoPrefab;
        [SerializeField] EdgeRenderer edgeRendererPrefab;

        [Space]

        [Header("Unity Events")]
        [SerializeField] UnityEvent<Node> OnNodeClicked;


        #region NodeMap Generation

        private void CreateNewNodesAndMap()
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
            if (includeCenterNode)
            {
                var coordinate = Vector3Int.zero;
                var edgeDirections = GetEdgeDirectionsFromWorldCoord(coordinate);
                var localPos = GetLocalPosition(coordinate);

                Node centerNode = new Node(coordinate, localPos, edgeDirections, this);
                nodeMap.Add(coordinate, centerNode);
            }

            //Populate map with nodes
            for (int ring = 1; ring <= ringCount; ring++)
            {
                for (int x = -1; x <= 1; x++) //This assumes 3 nodes per side of ring
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue; //There is no center node in a ring, all nodes are on the rim.

                        var coordinate = new Vector3Int(x, y, ring);
                        var edgeDirections = GetEdgeDirectionsFromWorldCoord(coordinate);
                        var localPos = GetLocalPosition(coordinate);

                        Node node = new Node(coordinate, localPos, edgeDirections, this);

                        nodeMap.Add(coordinate, node);
                    }
                }
            }

            Vector3 GetLocalPosition(Vector3Int coordinate)
            {
                float posX = coordinate.x * coordinate.z * ringOffset;
                float posY = coordinate.y * coordinate.z * ringOffset;
                return new Vector3(posX, posY, 0);
            }
        }

        private List<Vector3Int> GetEdgeDirectionsFromWorldCoord(Vector3Int worldCoord)
        {
            List<Vector3Int> localCoordsForEdges = new();

            int lowestRingIndex = 1;
            int highestRingIndex = ringCount;

            if (worldCoord == Vector3Int.zero) //If is center node
            {
                //Cardinal edges
                localCoordsForEdges.Add(Vector3Int.up + Vector3Int.forward);
                localCoordsForEdges.Add(Vector3Int.down + Vector3Int.forward);
                localCoordsForEdges.Add(Vector3Int.left + Vector3Int.forward);
                localCoordsForEdges.Add(Vector3Int.right + Vector3Int.forward);

                if (includeDiagonalConnections)
                {
                    localCoordsForEdges.Add(Vector3Int.up + Vector3Int.left + Vector3Int.forward);
                    localCoordsForEdges.Add(Vector3Int.up + Vector3Int.right + Vector3Int.forward);
                    localCoordsForEdges.Add(Vector3Int.down + Vector3Int.left + Vector3Int.forward);
                    localCoordsForEdges.Add(Vector3Int.down + Vector3Int.right + Vector3Int.forward);
                }
            }
            else if (IsCornerNode(worldCoord))
            {
                //Edges on same ring
                localCoordsForEdges.Add(new Vector3Int(-worldCoord.x, 0, 0));
                localCoordsForEdges.Add(new Vector3Int(0, -worldCoord.y, 0));

                if (includeDiagonalConnections)
                {
                    AddRingToRingConnections(worldCoord);
                }
            }
            else //Middle of a side of the square ring
            {
                //Edges on the same ring
                if (worldCoord.x == 0)
                {
                    localCoordsForEdges.Add(new Vector3Int(1, 0, 0));
                    localCoordsForEdges.Add(new Vector3Int(-1, 0, 0));
                }
                else if (worldCoord.y == 0)
                {
                    localCoordsForEdges.Add(new Vector3Int(0, 1, 0));
                    localCoordsForEdges.Add(new Vector3Int(0, -1, 0));
                }

                //Edges between rings
                AddRingToRingConnections(worldCoord);
            }
            return localCoordsForEdges;

            void AddRingToRingConnections(Vector3Int worldCoord)
            {
                //Handle lower rings
                if (includeCenterNode && lowestRingIndex == worldCoord.z)
                {
                    localCoordsForEdges.Add(-worldCoord);
                }
                else if (lowestRingIndex < worldCoord.z)
                {
                    localCoordsForEdges.Add(new Vector3Int(0, 0, -1));
                }

                //Handle upper rings
                if (worldCoord.z < highestRingIndex)
                {
                    localCoordsForEdges.Add(new Vector3Int(0, 0, 1));
                }
            }

            bool IsCornerNode(Vector3Int worldCoord)
            {
                if (Mathf.Abs(worldCoord.x) - Mathf.Abs(worldCoord.y) == 0) return true;
                else return false;
            }
        }

        #endregion

        public void HandleNodeClicked(Node node)
        {
            OnNodeClicked.Invoke(node);
        }

        public void SetupBoard()
        {
            CreateNewNodesAndMap();

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
                foreach (Vector3Int edgeDirection in node.EdgeDirections)
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
                er.SetupEndNodes(nodes[0].NodeMono, nodes[1].NodeMono);
            }
        }

        private Node GetNode(Vector3Int coordinate)
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
            foreach (Vector3Int edge in node.EdgeDirections)
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
            foreach (Vector3Int direction in firstNode.EdgeDirections)
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
                Vector3Int currentDirection = direction;
                Node lastValidNode = secondNode;
                bool reversedDirection = false; //We will reverse direction once, because our first node could have been in the middle of a mill.

                for (int i = 0; i < numOfTokensForMill - 2; i++) // -2 because the first node and second node are already added.
                {
                    Vector3Int coordinateToCheck = lastValidNode.BoardCoord + currentDirection;
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


