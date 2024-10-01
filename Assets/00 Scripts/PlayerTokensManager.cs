using NineMensMorris;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTokensManager : MonoBehaviour
{
    List<Token> tokensInSupply = new();
    List<Token> tokensOnBoard = new();

    [Header("Prefabs")]
    [SerializeField] Token tokenPrefab;

    [Header("Visual Settings")]
    [SerializeField] float heightBetweenTokensInPile;

    [Header("Movement Animation")]

    [Header("Slide")]
    [SerializeField] AnimationCurve slideCurve;
    [SerializeField][Range(1f, 3f)] float slideRelativeSpeed = 1f;

    PlayerData player;

    public int LivingTokensCount => tokensInSupply.Count + tokensOnBoard.Count;
    public int TokensInSupplyCount => tokensInSupply.Count;

    private void OnDisable()
    {
        player.TokenManager = null;
    }

    public void SetupPlayerData(PlayerData player)
    {
        this.player = player;
        player.TokenManager = this;
    }

    public void InstantiateNewTokens(int count)
    {
        tokensInSupply.Clear();
        tokensOnBoard.Clear();

        List<GameObject> objectsToDestroy = new();
        foreach (Transform child in transform)
        {
            objectsToDestroy.Add(child.gameObject);
        }
        foreach (GameObject child in objectsToDestroy)
        {
            Destroy(child);
        }

        for (int i = 0; i < count; i++)
        {
            InstantiateTokenToPile(i);
        }

        StartCoroutine(Co_SlideIntoView());
    }

    private void InstantiateTokenToPile(int i)
    {
        Vector3 pos = transform.position + (Vector3.up * heightBetweenTokensInPile * i);
        Token token = Instantiate(tokenPrefab, pos, transform.rotation, transform);
        token.SetupData(player, this);
        token.SetSortingOrder(i);
        tokensInSupply.Add(token);
    }

    public Token SendTopTokenToNode(Node node)
    {
        Token topToken = tokensInSupply.Last();
        tokensInSupply.Remove(topToken);
        tokensOnBoard.Add(topToken);
        return topToken;
    }

    public void HandleTokenDestroyed(Token token)
    {
        tokensOnBoard.Remove(token);
    }

    private IEnumerator Co_SlideIntoView()
    {
        float distanceFromCenter = (Vector3.zero - transform.position).magnitude;

        Vector3 startPos = transform.position + (transform.up * distanceFromCenter * 2f);
        Vector3 endPos = transform.position;

        float t = 0f;

        while (true)
        {
            t = Mathf.MoveTowards(t, 1, slideRelativeSpeed * Time.deltaTime);
            float animationLerpValue = slideCurve.Evaluate(t);

            transform.position = Vector3.LerpUnclamped(startPos, endPos, animationLerpValue);

            if (t > 0.995f)
            {
                transform.position = endPos;
                break;
            }

            yield return null;
        }
    }
}
