using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Players/Player")]
public class PlayerData : ScriptableObject
{
    [Header("Player Details")]
    [SerializeField] string playerName;
    [SerializeField] Color color;

    public string Name => playerName;
    public Color Color => color;
    public PlayerTokensManager TokenManager { get; set; }

    public void UpdateName(string newName)
    {
        playerName = newName;
    }

    public void UpdateColor(Color newColor)
    {
        color = newColor;
    }

    public override string ToString()
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hexColor}>{playerName}</color>";
    }
}
