using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace NineMensMorris
{
    public class GameEndWindow : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] TextMeshProUGUI winDisplay;
        [SerializeField] ModalWindowController modalWindow;

        [Header("Settings")]
        [SerializeField][Range(0f, 2f)] float openingDelay = 2f;

        public void AnnounceWinner(PlayerData winningPlayer)
        {
            winDisplay.text = $"{winningPlayer} wins!";
            StartCoroutine(Co_OpenWithDelay());
        }

        public void AnnounceDraw()
        {
            winDisplay.text = $"Draw!\nNo one wins...";
            StartCoroutine(Co_OpenWithDelay());
        }

        private IEnumerator Co_OpenWithDelay()
        {
            float timeElapsed = 0f;
            while (true)
            {
                timeElapsed += Time.deltaTime;
                if (timeElapsed > openingDelay) break;
                yield return null;
            }   
            modalWindow.ToggleActive();
        }
    }


}

