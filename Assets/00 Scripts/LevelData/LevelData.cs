using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/LevelData", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [Header("ID")]
    [SerializeField] string levelName;
    
    [Header("Token Rules")]
    [SerializeField] int numOfTokensForMill = 3;
    [SerializeField] int tokensPerPlayer = 10;
    [SerializeField] int maxTokensForFlying = 3;

    [Header("Board Data")]
    [SerializeField] int ringCount = 3;
    [SerializeField] bool includeDiagonalConnections = false;
    [SerializeField] bool includeCenterNode;

    //Properties
    public string LevelName => levelName;
    public int NumOfTokensForMill => numOfTokensForMill;
    public int TokensPerPlayer => tokensPerPlayer;
    public int MaxTokensForFlying => maxTokensForFlying;
    public int RingCount => ringCount;
    public bool IncludeDiagonalConnections => includeDiagonalConnections;
    public bool IncludeCenterNode => includeCenterNode;

    public void Setup(
        int tokensMill, 
        int tokensTotal, 
        int tokensFlying,
        int rings,
        bool diagonals,
        bool center
        )
    {
        numOfTokensForMill = tokensMill;
        tokensPerPlayer = tokensTotal;
        maxTokensForFlying = tokensFlying;
        ringCount = rings;
        includeDiagonalConnections = diagonals;
        includeCenterNode = center;
    }
}
