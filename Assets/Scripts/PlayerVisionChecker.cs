using UnityEngine;
using UnityEngine.XR;

public class PlayerVisionChecker : MonoBehaviour
{
    public static PlayerVisionChecker Instance;

    [Header("VR Cameras")]
    public Camera leftEyeCamera;
    public Camera rightEyeCamera;

    [Header("Settings")]
    public LayerMask obstacleMask; // Ce qui peut bloquer la vue

    void Awake() => Instance = this;

    /// <summary>
    /// Retourne true si le joueur voit l'objet cible
    /// </summary>
    public bool IsLookingAt(Vector3 targetPosition, float targetRadius = 0.5f)
    {
        return IsVisibleFromCamera(leftEyeCamera, targetPosition, targetRadius)
            || IsVisibleFromCamera(rightEyeCamera, targetPosition, targetRadius);
    }

    bool IsVisibleFromCamera(Camera cam, Vector3 worldPos, float radius)
    {
        if (cam == null) return false;

        // 1. Est-il dans le frustum ?
        Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);
        bool inFrustum = viewportPos.z > 0
            && viewportPos.x > -radius && viewportPos.x < 1 + radius
            && viewportPos.y > -radius && viewportPos.y < 1 + radius;

        if (!inFrustum) return false;

        // 2. Y a-t-il un obstacle entre le joueur et le mannequin ?
        Vector3 direction = worldPos - cam.transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(cam.transform.position, direction.normalized,
            out RaycastHit hit, distance, obstacleMask))
        {
            // Quelque chose bloque la vue
            return false;
        }

        return true;
    }
}
