using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

        [Header("Entity Collections")]
        [SerializeField] List<EdgeRenderer> lineControllers = new();
        [SerializeField] List<NodeMono> nodeMonos = new();


        #region NodeMap Generation

        public void CreateNewNodesAndMap()
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
                Node centerNode = new Node(Vector3Int.zero);
                centerNode.SetupEdgeDirections(GetEdgeDirectionsFromWorldCoord(Vector3Int.zero));
                nodeMap.Add(Vector3Int.zero, centerNode);
            }

            //Populate map with nodes
            for (int ring = 1; ring <= ringCount; ring++)
            {
                for (int x = -1; x <= 1; x++) //This assumes 3 nodes per side of ring
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue; //There is no center node in a ring, all nodes are on the rim.

                        Vector3Int worldCoordOfNode = new Vector3Int(x, y, ring);
                        Node node = new Node(worldCoordOfNode);

                        node.SetupEdgeDirections(GetEdgeDirectionsFromWorldCoord(worldCoordOfNode));

                        //Calculate local position, used to place NodeMonos
                        float posX = x * ring * ringOffset;
                        float posY = y * ring * ringOffset;
                        Vector3 localPos = new Vector3(posX, posY, 0);
                        node.SetupLocalPosition(localPos);

                        nodeMap.Add(worldCoordOfNode, node);
                    }
                }
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


        [ContextMenu("Set Up Board")]
        public void SetUpBoard()
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
                        nodeMap[node.WorldCoord + edgeDirection]
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

    }
}


