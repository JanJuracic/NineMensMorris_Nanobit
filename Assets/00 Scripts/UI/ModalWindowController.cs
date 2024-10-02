using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalWindowController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] RectTransform modalWindow;
    [SerializeField] Animator animator;

    [Header("Interaction Blockers")]
    [SerializeField] RectTransform backgroundBlocker;
    [SerializeField] RectTransform interactionBlocker;

    [Header("Animation")]
    [SerializeField] AnimationClip OpenClip;
    [SerializeField] AnimationClip CloseClip;

    bool isOpen = false;

    public void ToggleActive()
    {
        isOpen = !isOpen;
        OpenCloseAnimation(isOpen);
    }

    public void OpenCloseAnimation(bool opening)
    {
        if (opening) animator.Play(OpenClip.name);
        else animator.Play(CloseClip.name);
    }

    #region Animation Trigger Methods

    public void ActivateBackgroundBlocker()
    {
        backgroundBlocker.gameObject.SetActive(true);
    }
    public void DeactivateBackgroundBlocker()
    {
        backgroundBlocker.gameObject.SetActive(false);
    }

    public void ActivateInteractionBlocker()
    {
        interactionBlocker.gameObject.SetActive(true);
    }

    public void DeactivateInteractionBlocker()
    {
        interactionBlocker.gameObject.SetActive(false);
    }

    #endregion
}
