using UnityEngine;

public class MannequinBehavior : MonoBehaviour
{
    public bool bCanMove = false;

    /// <summary>
    /// Call this when a raycast from the player first hits this mannequin.
    /// </summary>
    /// <param name="other">The GameObject whose raycast entered.</param>
    public void OnRaycastEnter(GameObject other)
    {
        Debug.Log("Raycast Enter");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Cannot move");
            bCanMove = false;
        }
    }

    /// <summary>
    /// Call this when a raycast from the player stops hitting this mannequin.
    /// </summary>
    /// <param name="other">The GameObject whose raycast exited.</param>
    public void OnRaycastExit(GameObject other)
    {
        Debug.Log("Raycast Exit");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Can move");
            bCanMove = true;
        }
    }
}
