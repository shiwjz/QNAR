using UnityEngine;

// Keeps a 3D Quad covering the camera view on different devices.
// Attach to the HomeBackground Quad (not the ARCamera).
public class HomeBackgroundFit : MonoBehaviour
{
    public Camera arCamera;
    [Min(0.5f)] public float distance = 4f;
    [Range(1f, 1.3f)] public float overscan = 1.05f;
    [Min(0.1f)] public float fallbackImageAspect = 0.545f;

    void LateUpdate()
    {
        if (arCamera == null)
            return;

        float depth = Mathf.Max(distance, arCamera.nearClipPlane + 0.1f);

        Vector3 bottomLeft = arCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
        Vector3 bottomRight = arCamera.ViewportToWorldPoint(new Vector3(1f, 0f, depth));
        Vector3 topLeft = arCamera.ViewportToWorldPoint(new Vector3(0f, 1f, depth));

        float viewWidth = Vector3.Distance(bottomLeft, bottomRight);
        float viewHeight = Vector3.Distance(bottomLeft, topLeft);

        float imageAspect = fallbackImageAspect;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            Texture texture = meshRenderer.sharedMaterial.mainTexture;
            if (texture != null && texture.height > 0)
                imageAspect = (float)texture.width / texture.height;
        }

        // Fill the screen without stretching: crop any excess off-screen.
        float height = Mathf.Max(viewHeight, viewWidth / imageAspect) * overscan;
        float width = height * imageAspect;

        transform.position = arCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
      transform.rotation = arCamera.transform.rotation;
        transform.localScale = new Vector3(width, height, 1f);
    }
}
