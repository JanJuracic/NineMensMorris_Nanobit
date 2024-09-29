using NineMensMorris;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTokensManager : MonoBehaviour
{
    [SerializeField] List<Token> tokensInSupply = new();
    [SerializeField] List<Token> tokensOnBoard = new();

    [Header("Data")]
    [SerializeField] PlayerData player;

    [Header("Prefabs")]
    [SerializeField] Token tokenPrefab;

    [Header("Visual Settings")]
    [SerializeField] float heightBetweenTokensInPile;

    public int LivingTokensCount => tokensInSupply.Count + tokensOnBoard.Count;
    public int TokensInSupplyCount => tokensInSupply.Count;


    private void OnEnable()
    {
        player.TokenManager = this;   
    }

    private void OnDisable()
    {
        player.TokenManager = null;
    }

    public void InstantiateTokens(int count)
    {
        for (int i = 0; i < count; i++)
        {
            InstantiateTokenToPile();
        }
    }

    private void InstantiateTokenToPile()
    {
        Token token = Instantiate(tokenPrefab, transform.position, transform.rotation, transform);
        token.Setup(player, this);
        tokensInSupply.Add(token);
    }

    public void SendTopTokenToNode(Node node)
    {
        Token topToken = tokensInSupply[0];
        node.LinkToken(topToken);
        tokensInSupply.Remove(topToken);
        tokensOnBoard.Add(topToken);
    }

    public void HandleTokenDestroyed(Token token)
    {
        tokensOnBoard.Remove(token);
    }
}
