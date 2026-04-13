using UnityEngine;
using UnityEngine.AI;

public class MannequinAI : MonoBehaviour
{
    public enum State { Dormant, Activated, Chasing, Frozen }

    [Header("Settings")]
    public float checkInterval = 0.1f;    // Fréquence de vérification (perf)
    public float deactivateDistance = 20f;
    public float lookCheckRadius = 0.6f; // Rayon de détection du regard sur le mannequin

    [Header("References")]
    public Transform playerHead;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Audio")]
    public AudioSource footstepAudio;

    private State currentState = State.Dormant;
    private float checkTimer;

    void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval) return;
        checkTimer = 0;

        switch (currentState)
        {
            case State.Dormant:
                // Rien — attend l'activation par MannequinActivationZone
                break;

            case State.Activated:
            case State.Chasing:
            case State.Frozen:
                UpdateActiveState();
                break;
        }
    }

    public void Activate()
    {
        if (currentState != State.Dormant) return;
        currentState = State.Activated;
        Debug.Log($"{name} activé !");
        // Son, animation de "réveil" si besoin
    }

    public bool IsActive()
    {
        return currentState != State.Dormant;
    }

    void UpdateActiveState()
    {
        float distance = Vector3.Distance(transform.position, playerHead.position);

        // Trop loin ? désactivation
        if (distance > deactivateDistance)
        {
            SetState(State.Dormant);
            return;
        }

        bool playerSees = PlayerVisionChecker.Instance.IsLookingAt(
            transform.position + Vector3.up, // Viser le torse/tête
            lookCheckRadius
        );

        if (playerSees)
        {
            SetState(State.Frozen);
        }
        else
        {
            SetState(State.Chasing);
        }
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case State.Chasing:
                agent.isStopped = false;
                agent.SetDestination(playerHead.position);
                animator?.SetBool("Moving", true);
                footstepAudio?.Play();
                break;

            case State.Frozen:
                agent.isStopped = true;
                // IMPORTANT : freeze la position exacte pour éviter le glissement
                agent.velocity = Vector3.zero;
                animator?.SetBool("Moving", false);
                footstepAudio?.Stop();
                break;

            case State.Dormant:
                agent.isStopped = true;
                animator?.SetBool("Moving", false);
                break;
        }
    }
}