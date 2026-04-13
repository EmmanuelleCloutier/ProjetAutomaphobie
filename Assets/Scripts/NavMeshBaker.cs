using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    private NavMeshSurface surface;

    void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
        Debug.Log("NavMesh bake terminé !");
    }
}