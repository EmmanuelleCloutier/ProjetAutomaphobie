using UnityEngine;

public class MannequinActivationZone : MonoBehaviour
{
    [Header("Zone")]
    public float activationRadius = 5f;      // Distance max d'activation
    public float activationAngle = 120f;     // Derrière/côté du joueur (0° = pile derrière)

    [Header("References")]
    public MannequinAI mannequin;
    public Transform playerHead; // Camera Rig / Head Transform

    private bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;

        float distance = Vector3.Distance(transform.position, playerHead.position);
        if (distance > activationRadius) return;

        // Vecteur du joueur VERS le mannequin
        Vector3 toMannequin = (transform.position - playerHead.position).normalized;

        // Vecteur "regard" du joueur
        Vector3 playerForward = playerHead.forward;

        // Dot product : -1 = derrière, 1 = devant
        float dot = Vector3.Dot(playerForward, toMannequin);

        // On active si le mannequin est sur le côté ou derrière
        // (dot < 0 = derrière, ajustable avec le seuil)
        float threshold = Mathf.Cos(activationAngle * 0.5f * Mathf.Deg2Rad);

        if (dot < threshold) // Le joueur a passé/dépasse le mannequin
        {
            hasTriggered = true;
            mannequin.Activate();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
