using UnityEngine;

public class CameraSizeController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Camera cam;

    [Header("Settings")]
    [SerializeField][Range(1f, 3f)] float sizeFactor = 1.2f;

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
        float heightBasedSize = targetHeight; 
        float widthBasedSize = targetWidth / aspectRatio;
        cam.orthographicSize = Mathf.Max(heightBasedSize * sizeFactor, widthBasedSize * sizeFactor);

    }
}
