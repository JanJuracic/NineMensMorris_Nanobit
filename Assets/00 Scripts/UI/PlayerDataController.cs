using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NineMensMorris
{ 
    public class PlayerDataController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] PlayerData myPlayer;
        [SerializeField] PlayerData otherPlayer;

        [Header("Components")]
        [SerializeField] TMP_InputField nameInput;
        [SerializeField] TMP_Dropdown colorDropdown;
        [SerializeField] Image tokenImage;

        private void Awake()
        {
            myPlayer.LoadPlayerDataFromPreferences();
            nameInput.text = myPlayer.Name;
            colorDropdown.value = myPlayer.ColorIndex;
            AttemptToChangeColor(myPlayer.ColorIndex);
        }

        public void AttemptToChangeName(string newName)
        {
            newName = newName.Trim();
            if (newName == otherPlayer.Name)
            {
                nameInput.text = myPlayer.Name;
            }
            else
            {
                nameInput.text = newName;
                myPlayer.UpdateName(newName);
            }
        }

        public void AttemptToChangeColor(int colorIndex)
        {
            Color newColor = colorDropdown.options[colorIndex].color;
            if (otherPlayer.Color == newColor)
            {
                colorDropdown.value = myPlayer.ColorIndex;
            }
            else
            {
                myPlayer.UpdateColor(newColor, colorIndex);
                UpdateTokenImageColorToCurrent();
            }
        }

        private void UpdateTokenImageColorToCurrent()
        {
            tokenImage.color = myPlayer.Color;
        }
    }
}


