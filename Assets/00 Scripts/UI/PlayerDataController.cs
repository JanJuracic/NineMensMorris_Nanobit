using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        nameInput.text = myPlayer.Name;
        colorDropdown.value = GetCurrentColorIndex();
        UpdateTokenImageColorToCurrent();
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

    public void AttemptToChangeColor(int choiceIndex)
    {
        Color newColor = colorDropdown.options[choiceIndex].color;
        if (otherPlayer.Color == newColor)
        {
            colorDropdown.value = GetCurrentColorIndex();
        }
        else
        {
            myPlayer.UpdateColor(newColor);
            UpdateTokenImageColorToCurrent();
        }
    }

    private int GetCurrentColorIndex()
    {
        //Find previously set color index
        int previousColorIndex = 0;
        for (int i = 0; i < colorDropdown.options.Count; i++)
        {
            Color color = colorDropdown.options[i].color;
            if (color == myPlayer.Color)
            {
                previousColorIndex = i;
                break;
            }
        }
        return previousColorIndex;
    }

    private void UpdateTokenImageColorToCurrent()
    {
        tokenImage.color = myPlayer.Color;
    }
}
