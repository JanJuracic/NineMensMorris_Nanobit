using NineMensMorris;
using NUnit.Framework.Constraints;
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

    [Header("Movement Animation Settings")]
    [Header("Slide")]
    [SerializeField] AnimationCurve slideCurve;
    [SerializeField][Range(1f, 3f)] float slideRelativeSpeed = 1f;

    [Header("Fly")]
    [SerializeField] AnimationCurve flyCurve;
    [SerializeField][Range(0.1f, 1f)] float flyHeightToLengthRatio = 0.5f;
    [SerializeField][Range(12, 20f)] float flySpeedPerSecond = 15f;

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
        StartCoroutine(Co_SlideTo(node.Mono.transform));
    }

    public void FlyTo(Node node)
    {
        StartCoroutine(Co_FlyTo(node.Mono.transform));
    }

    private IEnumerator Co_SlideTo(Transform targetTr)
    {
        Vector3 startPos = transform.position;  
        float t = 0f;

        while (true)
        {
            t = Mathf.MoveTowards(t, 1, slideRelativeSpeed * Time.deltaTime);            
            float animationLerpValue = slideCurve.Evaluate(t);

            transform.position = Vector3.LerpUnclamped(startPos, targetTr.position, animationLerpValue);

            if (t > 0.995f)
            {
                transform.position = targetTr.position;
                break;
            }

            yield return null;
        }
    }

    private IEnumerator Co_FlyTo(Transform targetTr)
    {
        Vector3 startPos = transform.position;
        Vector3 visualOriginLocPos = tokenVisual.transform.localPosition;

        float distanceToTarget = (startPos - targetTr.position).magnitude;
        float flyHeight = distanceToTarget * flyHeightToLengthRatio;

        float t = 0f;
        float distanceCrossed = 0f;
        while (true)
        {
            distanceCrossed = Mathf.MoveTowards(distanceCrossed, distanceToTarget, flySpeedPerSecond * Time.deltaTime);
            t = distanceCrossed / distanceToTarget;

            //Move token
            transform.position = Vector3.Lerp(startPos, targetTr.position, t);

            //Move token visual upwards along curve
            float animationLerpValue = flyCurve.Evaluate(t);
            float tokenVisHeight = Mathf.LerpUnclamped(0, flyHeight, animationLerpValue);
            tokenVisual.transform.localPosition = new Vector3(visualOriginLocPos.x, tokenVisHeight, visualOriginLocPos.z);

            if (t > 0.995f)
            {
                transform.position = targetTr.position;
                tokenVisual.transform.localPosition = visualOriginLocPos;
                break;
            }

            yield return null;
        }
    }
}