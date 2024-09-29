using NineMensMorris;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    PlayerData player;
    PlayerTokensManager manager;

    [Header("Components")]
    [SerializeField] SpriteRenderer tokenVisual;
    [SerializeField] SpriteRenderer shadowVisual;

    [Header("Movement Settings")]
    [SerializeField] AnimationCurve slideCurve;
    [SerializeField][Range(0f, 1f)] float slideFactor = 0.3f;

    //Properties
    public PlayerData Player => player;

    public void Setup(PlayerData playerData, PlayerTokensManager tokenManager)
    {
        player = playerData;
        tokenVisual.color = player.Color;
        manager = tokenManager;
    }

    public void DestroyToken()
    {
        manager.HandleTokenDestroyed(this);

        Destroy(gameObject);
        //Do animation;
    }

    public void SlideTo(Node node)
    {
        StartCoroutine(Co_SlideTo(node.NodeMono.transform.position));
    }

    private IEnumerator Co_SlideTo(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;  
        float exponentialLerp = 0f;

        while (true)
        {
            exponentialLerp = Mathf.Lerp(exponentialLerp, 1, slideFactor);
            float animationLerp = slideCurve.Evaluate(exponentialLerp);

            transform.position = Vector3.LerpUnclamped(startPos, targetPos, animationLerp);

            if (exponentialLerp > 0.995f)
            {
                transform.position = targetPos;
                break;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}