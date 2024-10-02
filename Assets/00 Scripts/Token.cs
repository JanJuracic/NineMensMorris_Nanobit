using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace NineMensMorris
{
    public class Token : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] SortingGroup sortingGroup;
        [SerializeField] SpriteRenderer tokenVisual;
        [SerializeField] SpriteRenderer outlineVisual;
        [SerializeField] SpriteRenderer shadowVisual;

        [Header("VISUALS")]

        [Header("Movement Animation Settings")]

        [Header("Slide")]
        [SerializeField] AnimationCurve slideCurve;
        [SerializeField][Range(1f, 3f)] float slideRelativeSpeed = 1f;
        [SerializeField][Range(0.1f, 1f)] float slideShakeStrengthFactor = 0.6f;
        [SerializeField][Range(0.1f, 1f)] float slideShakeDurationFactor = 0.35f;
        [SerializeField] AudioClip slideSound;

        [Header("Fly")]
        [SerializeField] AnimationCurve flyCurve;
        [SerializeField][Range(0.1f, 1f)] float flyHeightToLengthRatio = 0.5f;
        [SerializeField][Range(12, 20f)] float flySpeedPerSecond = 15f;
        [SerializeField][Range(0.1f, 1f)] float landShakeStrengthFactor = 0.6f;
        [SerializeField][Range(0.1f, 1f)] float landShakeDurationFactor = 0.35f;
        [SerializeField] AudioClip jumpSound;
        [SerializeField] AudioClip landSound;

        [Header("Destroy")]
        [SerializeField][Range(0.1f, 3f)] float destroyDur;
        [SerializeField][Range(0.1f, 1f)] private float shakeStrength = 0.2f;
        [SerializeField][Range(10, 30f)] private float shakeFrequency = 10.0f;
        [SerializeField] AudioClip destroySound;

        [Header("Outline Colors")]
        [Header("Friendly")]
        [SerializeField] Color defaultFriendly;
        [SerializeField] Color hoveredFriendly;
        [SerializeField] Color selectedFriendly;
        [Header("Enemy")]
        [SerializeField] Color defaultEnemy;
        [SerializeField] Color hoveredEnemy;
        [SerializeField] Color selectedEnemy;

        PlayerData player;
        PlayerTokensManager manager;

        bool isSelectable = false;
        bool isSelected = false;
        bool isFriendly = true;

        bool isMoving = false;

        //Properties
        public PlayerData Player => player;
        public bool IsMoving => isMoving;

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
            StartCoroutine(Co_SlideTo(node));
        }

        public void FlyTo(Node node)
        {
            StartCoroutine(Co_FlyTo(node));
        }

        public void DestroyToken()
        {
            manager.HandleTokenDestroyed(this);
            StartCoroutine(Co_Destroy());
        }

        private IEnumerator Co_SlideTo(Node targetNode)
        {
            isMoving = true;

            Transform targetTr = targetNode.Mono.transform;
            NodeMono nodeMono = targetNode.Mono;

            Vector3 startPos = transform.position;  
            float t = 0f;

            SFXManager.Play(slideSound, transform);

            while (true)
            {
                t = Mathf.MoveTowards(t, 1, slideRelativeSpeed * Time.deltaTime);            
                float animationLerpValue = slideCurve.Evaluate(t);

                transform.position = Vector3.LerpUnclamped(startPos, targetTr.position, animationLerpValue);

                if (t > 0.995f)
                {
                    transform.position = targetTr.position;
                    nodeMono.Shake(slideShakeStrengthFactor, slideShakeDurationFactor);

                    break;
                }

                yield return null;
            }

            isMoving = false;
        }

        private IEnumerator Co_FlyTo(Node targetNode)
        {
            isMoving = true;

            Transform targetTr = targetNode.Mono.transform;
            NodeMono nodeMono = targetNode.Mono;

            Vector3 startPos = transform.position;
            Vector3 visualOriginLocPos = tokenVisual.transform.localPosition;

            float distanceToTarget = (startPos - targetTr.position).magnitude;
            float flyHeight = distanceToTarget * flyHeightToLengthRatio;

            SFXManager.Play(jumpSound, transform);

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

                    nodeMono.Shake(landShakeStrengthFactor, landShakeDurationFactor);
                    break;
                }

                yield return null;
            }

            SFXManager.Play(landSound, transform);

            isMoving = false;
        }

        private IEnumerator Co_Destroy()
        {
            shadowVisual.enabled = false;
            outlineVisual.enabled = false;

            Material mat = tokenVisual.material;
            Vector3 localPos = transform.localPosition;

            SFXManager.Play(destroySound, transform);

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
}

