using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoardAndRules/BoardAndRulesData", fileName = "BoardAndRulesData")]
public class BoardAndRulesData : ScriptableObject
{
    [Header("Token Rules")]
    [SerializeField] int numOfTokensForMill = 3;
    [SerializeField] int tokensPerPlayer = 10;
    [SerializeField] int maxTokensForFlying = 3;

    [Header("Board Data")]
    [SerializeField] int ringCount = 3;
    [SerializeField] bool includeDiagonalConnections = false;
    [SerializeField] bool includeCenterNode;

    //Properties
    public int NumOfTokensForMill => numOfTokensForMill;
    public int TokensPerPlayer => tokensPerPlayer;
    public int MaxTokensForFlying => maxTokensForFlying;
    public int RingCount => ringCount;
    public bool IncludeDiagonalConnections => includeDiagonalConnections;
    public bool IncludeCenterNode => includeCenterNode;

}
