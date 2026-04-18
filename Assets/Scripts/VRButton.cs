using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRButton : MonoBehaviour
{
    public float pressDepth = 0.02f;
    private Vector3 startPos;
    private bool pressed = false;
    
    void OnPressed()
    {
        Debug.Log("Physical button pressed");
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = startPos.y - transform.localPosition.y;

        if (!pressed && distance > pressDepth)
        {
            pressed = true;
            OnPressed();
        }

        if (pressed && distance < pressDepth * 0.5f)
        {
            pressed = false;
        }
    }
}
