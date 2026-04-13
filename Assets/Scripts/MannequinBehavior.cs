using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MannequinBehavior : MonoBehaviour
{
    public bool bCanMove = false;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Cannot move");
            bCanMove = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Collision exit");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Can move");
            bCanMove = true;
        }
    }
}
