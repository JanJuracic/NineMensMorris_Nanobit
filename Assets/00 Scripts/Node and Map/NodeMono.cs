using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineMensMorris;
using UnityEngine.EventSystems;

namespace NineMensMorris
{
    [RequireComponent(typeof(Collider2D))]
    public class NodeMono : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        Node node;

        [Header("Components")]
        [SerializeField] SpriteMask holeMask;
        [SerializeField] Collider2D interactionCollider;

        [Header("Animation Settings")]
        [Header("Shake")]
        [SerializeField] private float shakeDur = 0.5f;
        [SerializeField][Range(0.05f, 1f)] private float shakeStrength = 0.2f;
        [SerializeField][Range(10, 20f)] private float shakeFrequency = 10.0f;

        Vector3 localPos;
        List<EdgeRenderer> edgeRenderers = new();

        bool nodeIsSelectable = false;
        bool tokenIsSelectable = false;

        public void Setup(Node myNode)
        {
            node = myNode;
            name = $"NodeMono: {node.BoardCoord}";
            localPos = transform.localPosition;
        }

        public void AddEdgeRenderer(EdgeRenderer edgeRenderer)
        {
            edgeRenderers.Add(edgeRenderer);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            node.HandleNodeClicked();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (nodeIsSelectable == false && tokenIsSelectable == false) return;

            //Handle node visuals
            holeMask.transform.localScale = Vector3.one * 0.5f;
            holeMask.enabled = true;

            //Handle token visuals
            if (node.Token != null)
            {
                node.Token.HandleHovered(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (nodeIsSelectable == false && tokenIsSelectable == false) return;

            //Handle node visuals
            holeMask.transform.localScale = Vector3.one;
            holeMask.enabled = true;

            //Handle token visuals
            if (node.Token != null)
            {
                node.Token.HandleHovered(false);
            }
        }

        public void UpdateNodeIsSelectable(bool nodeIsSelectable)
        {
            this.nodeIsSelectable = nodeIsSelectable;

            //Handle node visuals
            holeMask.enabled = nodeIsSelectable;
        }

        public void UpdateTokenIsSelectable(bool tokenIsSelectable, bool isFriendly)
        {
            this.tokenIsSelectable = tokenIsSelectable;

            //Handle token visuals
            if (node.Token != null)
            {
                node.Token.SetSelectabalityAndFriendliness(tokenIsSelectable, isFriendly);
            }
        }

        public void Shake()
        {
            StartCoroutine(Co_Shake());
        }

        private IEnumerator Co_Shake()
        {
            float timeElapsed = 0f;

            while (timeElapsed < shakeDur)
            {
                float xOffset = Mathf.PerlinNoise(Time.time * shakeFrequency, 0.0f) * 2.0f - 1.0f;
                float yOffset = Mathf.PerlinNoise(0.0f, Time.time * shakeFrequency) * 2.0f - 1.0f;
                Vector3 offset = new Vector3(xOffset, yOffset, 0);

                transform.localPosition = localPos + (offset * shakeStrength);
                foreach (EdgeRenderer edgeRenderer in edgeRenderers)
                {
                    edgeRenderer.UpdateLineEndpoints();
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = localPos;
            
            foreach (EdgeRenderer edgeRenderer in edgeRenderers)
            {
                edgeRenderer.UpdateLineEndpoints();
            }
        }

    }
}



