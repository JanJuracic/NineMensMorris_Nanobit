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
        Node myNode;

        [Header("Components")]
        [SerializeField] SpriteMask holeMask;
        [SerializeField] Collider2D interactionCollider;

        public void Setup(Node node)
        {
            myNode = node;
            name = $"NodeMono: {node.BoardCoord}";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            myNode.HandleNodeClicked();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            holeMask.enabled = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            holeMask.enabled = true;
        }
    }
}



