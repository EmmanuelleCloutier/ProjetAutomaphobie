using Unity.XR.CoreUtils;
using UnityEngine;

public class S_PushBack : MonoBehaviour
{
    public XROrigin xrOrigin;

    void OnCollisionStay(Collision collision) {
        Vector3 pushDirection = collision.contacts[0].normal;
        xrOrigin.transform.position += pushDirection * 0.02f;
    }
}
