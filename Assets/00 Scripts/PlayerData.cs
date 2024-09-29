using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Players/Player")]
public class PlayerData : ScriptableObject
{
    [Header("Player Details")]
    [SerializeField] string playerName;
    [SerializeField] Color color;

    public string Name => name;
    public Color Color => color;

    public PlayerTokensManager TokenManager { get; set; }
}
