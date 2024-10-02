using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameEndWindow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winDisplay;
    [SerializeField] ModalWindowController modalWindow;

    public void AnnounceWinner(PlayerData winningPlayer)
    {
        winDisplay.text = $"{winningPlayer} wins!";
        modalWindow.ToggleActive();
    }

    public void AnnounceDraw()
    {
        winDisplay.text = $"Draw!\nNo one wins...";
        modalWindow.ToggleActive();
    }
}
