using NineMensMorris;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class Token : MonoBehaviour
{
    PlayerData player;
    PlayerTokensManager manager;

    [Header("Components")]
    [SerializeField] SortingGroup sortingGroup;
    [SerializeField] SpriteRenderer tokenVisual;
    [SerializeField] SpriteRenderer outlineVisual;
    [SerializeField] SpriteRenderer shadowVisual;

    [Header("VISUALS")]

    [Header("Movement Animation Settings")]

    [Header("Fall")]
    [SerializeField] AnimationCurve fallCurve;
    [SerializeField][Range(12, 20f)] float fallSpeedPerSecond = 15f;
    [SerializeField][Range(3, 10f)] float fallStartHeight = 3f;

    [Header("Slide")]
    [SerializeField] AnimationCurve slideCurve;
    [SerializeField][Range(1f, 3f)] float slideRelativeSpeed = 1f;

    [Header("Fly")]
    [SerializeField] AnimationCurve flyCurve;
    [SerializeField][Range(0.1f, 1f)] float flyHeightToLengthRatio = 0.5f;
    [SerializeField][Range(12, 20f)] float flySpeedPerSecond = 15f;

    [Header("Destroy")]
    [SerializeField][Range(0.1f, 3f)] float destroyDur;
    [SerializeField][Range(0.1f, 1f)] private float shakeStrength = 0.2f;
    [SerializeField][Range(10, 30f)] private float shakeFrequency = 10.0f;


    [Header("Outline Colors")]
    [Header("Friendly")]
    [SerializeField] Color defaultFriendly;
    [SerializeField] Color hoveredFriendly;
    [SerializeField] Color selectedFriendly;
    [Header("Enemy")]
    [SerializeField] Color defaultEnemy;
    [SerializeField] Color hoveredEnemy;
    [SerializeField] Color selectedEnemy;

    bool isSelectable = false;
    bool isSelected = false;
    bool isFriendly = true;

    //Properties
    public PlayerData Player => player;

    public void SetupData(PlayerData playerData, PlayerTokensManager tokenManager)
    {
        player = playerData;
        tokenVisual.color = player.Color;
        manager = tokenManager;
    }

    public void SetSortingOrder(int order)
    {
        sortingGroup.sortingOrder = order;
    }

    public void SetSelectabalityAndFriendliness(bool selectable, bool friendly)
    {
        isSelectable = selectable;
        isFriendly = friendly;
        outlineVisual.enabled = isSelectable;

        //Handle visuals for selectables
        if (isSelectable)
        {
            if (friendly) outlineVisual.color = defaultFriendly;
            else outlineVisual.color = defaultEnemy;
        }
    }

    public void HandleSelected(bool isSelected)
    {
        this.isSelected = isSelected;

        if (isSelected)
        {
            if (isFriendly) outlineVisual.color = selectedFriendly;
            else outlineVisual.color = selectedEnemy;
        }
        else
        {
            if (isFriendly) outlineVisual.color = defaultFriendly;
            else outlineVisual.color = defaultEnemy;
        }
    }

    public void HandleHovered(bool isHovered)
    {
        if (isSelected) return;

        if (isHovered)
        {
            if (isFriendly) outlineVisual.color = hoveredFriendly;
            else outlineVisual.color = hoveredEnemy;
        } 
        else
        {
            if (isFriendly) outlineVisual.color = defaultFriendly;
            else outlineVisual.color = defaultEnemy;
        }
    }

    public void SlideTo(Node node)
    {
        StartCoroutine(Co_SlideTo(node.Mono.transform));
    }

    public void FlyTo(Node node)
    {
        StartCoroutine(Co_FlyTo(node.Mono.transform));
    }

    public void DestroyToken()
    {
        manager.HandleTokenDestroyed(this);
        StartCoroutine(Co_Destroy());
    }

    private IEnumerator Co_Fall()
    {
        shadowVisual.enabled = false;
        tokenVisual.transform.localPosition = Vector3.up * fallStartHeight;
        Color tokenTransparent = new Color(tokenVisual.color.r, tokenVisual.color.g, tokenVisual.color.b, 0);
        Color tokenOpaque = new Color(tokenVisual.color.r, tokenVisual.color.g, tokenVisual.color.b, 1);
        tokenVisual.color = tokenTransparent;

        float t = 0f;
        float distanceCrossed = 0f;
        
        while (true)
        {
            distanceCrossed = Mathf.MoveTowards(distanceCrossed, fallStartHeight, fallSpeedPerSecond * Time.deltaTime);
            t = distanceCrossed / fallStartHeight;

            //Move token visual downwards along curve
            float animationLerpValue = fallCurve.Evaluate(t);
            float tokenVisHeight = Mathf.LerpUnclamped(fallStartHeight, 0, animationLerpValue);
            tokenVisual.transform.localPosition = new Vector3
                (tokenVisual.transform.localPosition.x, 
                tokenVisHeight, 
                tokenVisual.transform.localPosition.z);

            //Move alpha from transparent to opaque
            tokenVisual.color = Color.Lerp(tokenTransparent, tokenOpaque, t);

            if (t > 0.995f)
            {
                shadowVisual.enabled = true;
                tokenVisual.transform.localPosition = Vector3.zero;
                break;
            }

            yield return null;
        }
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

    private IEnumerator Co_Destroy()
    {
        shadowVisual.enabled = false;
        outlineVisual.enabled = false;

        Material mat = tokenVisual.material;
        Vector3 localPos = transform.localPosition;

        float timeElapsed = 0f;
        while (true)
        {
            //Handle growing
            transform.localScale = Vector3.one + (Vector3.one * (timeElapsed / destroyDur));

            //Handle shaking
            float xOffset = Mathf.PerlinNoise(Time.time * shakeFrequency, 0.0f) * 2.0f - 1.0f;
            float yOffset = Mathf.PerlinNoise(0.0f, Time.time * shakeFrequency) * 2.0f - 1.0f;
            Vector3 offset = new Vector3(xOffset, yOffset, 0);

            transform.localPosition = localPos + (offset * shakeStrength);
            transform.localPosition = localPos + (offset * shakeStrength);
            
            //Handle fading
            float fadeAmount = 1 - (timeElapsed / destroyDur);
            mat.SetFloat("_Fade", fadeAmount);

            if (timeElapsed > destroyDur )
            {
                mat.SetFloat("_Fade", 0f);
                Destroy(gameObject);
            }

            timeElapsed += Time.deltaTime;

            yield return null;
        }
    }


}