using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace NineMensMorris
{
    public class PlayerInfoDisplayer : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameDisplay;
        [SerializeField] TextMeshProUGUI infoDisplay;

        PlayerData player;

        public void SetupPlayer(PlayerData player)
        {
            this.player = player;
            player.PlayerInfo = this;

            nameDisplay.text = player.Name;
            nameDisplay.color = player.Color;
            infoDisplay.text = "";
            infoDisplay.color = player.Color;
        }

        public void UpdateActive(bool isActive)
        {
            if (isActive)
            {
                nameDisplay.alpha = 1f;
                infoDisplay.alpha = 1f;
            }
            else
            {
                nameDisplay.alpha = 0.4f;
                infoDisplay.alpha = 0.4f;
            }
        }

        public void UpdateInfoText(string text)
        {
            infoDisplay.text = text;
        }
    }
}


