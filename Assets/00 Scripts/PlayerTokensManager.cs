using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace NineMensMorris
{
    public class PlayerTokensManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] TextMeshPro counterDisplay;

        [Header("Prefabs")]
        [SerializeField] Token tokenPrefab;

        [Header("Visual Settings")]
        [SerializeField] float heightBetweenTokensInPile;

        [Header("Movement Animation")]

        [Header("Slide")]
        [SerializeField] AnimationCurve slideCurve;
        [SerializeField][Range(1f, 3f)] float slideRelativeSpeed = 1f;

        PlayerData player;
        List<Token> tokensInSupply = new();
        List<Token> tokensOnBoard = new();

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
                if (child.GetComponent<Token>() != null)
                {
                    objectsToDestroy.Add(child.gameObject);
                }
            }
            foreach (GameObject child in objectsToDestroy)
            {
                Destroy(child);
            }

            for (int i = 0; i < count; i++)
            {
                InstantiateTokenToPile(i);
            }

            //Setup counter
            counterDisplay.text = count.ToString();
            counterDisplay.color = player.Color;

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

            //Update counter
            if (tokensInSupply.Count > 0)
            {
                counterDisplay.text = tokensInSupply.Count.ToString();
            }
            else counterDisplay.text = string.Empty;

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
}


