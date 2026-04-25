using UnityEngine;

public class S_KeyReset : MonoBehaviour
{
    private Vector3 originalPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // Record the key's starting position
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        // Verify the position and reset if z is -10 or lower
        if (transform.position.z <= -10f) {
            transform.position = originalPosition;
        }
    }
}
