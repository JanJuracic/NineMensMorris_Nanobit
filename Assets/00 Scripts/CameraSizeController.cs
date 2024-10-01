using UnityEngine;

public class CameraSizeController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Camera cam;

    [Header("Settings")]
    [SerializeField][Range(0f, 3f)] float sizeOffset;

    float targetHeight;
    float targetWidth;

    #if UNITY_EDITOR
    public void Update()
    {
        UpdateCameraSize(targetHeight, targetWidth);
    }
    #endif

    public void UpdateCameraSize(float targetHeight, float targetWidth)
    {
        this.targetHeight = targetHeight;
        this.targetWidth = targetWidth;

        float aspectRatio = cam.aspect;
        float heightBasedSize = targetHeight + sizeOffset; 
        float widthBasedSize = (targetWidth + sizeOffset) / aspectRatio;
        cam.orthographicSize = Mathf.Max(heightBasedSize, widthBasedSize);

    }
}
