using UnityEngine;

public class S_ForceLOD : MonoBehaviour
{
    void Awake() {
        foreach (var lodGroup in FindObjectsOfType<LODGroup>()) {
            lodGroup.ForceLOD(0); // LOD1 = second LOD
        }
    }
}
