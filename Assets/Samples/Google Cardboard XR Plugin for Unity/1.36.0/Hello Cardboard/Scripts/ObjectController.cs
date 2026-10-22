using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectController : MonoBehaviour
{
    public Material InactiveMaterial;
    public Material GazedAtMaterial;

    private Renderer _myRenderer;
    private Vector3 _startingPosition;
    private bool _isGazedAt = false;

    public void Start()
    {
        _startingPosition = transform.parent.localPosition;
        _myRenderer = GetComponent<Renderer>();
        SetMaterial(false);
    }

    public void Update()
    {
        // Rotate the treasure continuously so it looks alive
        transform.Rotate(Vector3.up * 45f * Time.deltaTime, Space.Self);

        // If the player is looking at the treasure and presses Interact (Button East B/Circle, Right Trigger, or E/Enter key)
        if (_isGazedAt)
        {
            bool interactPressed = false;

            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                interactPressed = true;
            }

            if (Gamepad.current != null && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame))
            {
                interactPressed = true;
            }

            if (interactPressed)
            {
                OnPointerClick();
            }
        }
    }

    public void TeleportRandomly()
    {
        // Pick a random sibling shape (QuadSphere, TriSphere, or Icosahedron) and activate it
        int sibIdx = transform.GetSiblingIndex();
        int numSibs = transform.parent.childCount;
        sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
        GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

        // Move treasure to a new nearby position around the player
        Vector3 randomOffset = new Vector3(Random.Range(-6f, 6f), Random.Range(1.0f, 2.5f), Random.Range(3f, 8f));
        transform.parent.localPosition = _startingPosition + randomOffset;

        randomSib.SetActive(true);
        gameObject.SetActive(false);
        SetMaterial(false);
    }

    public void OnPointerEnter()
    {
        _isGazedAt = true;
        SetMaterial(true);
    }

    public void OnPointerExit()
    {
        _isGazedAt = false;
        SetMaterial(false);
    }

    public void OnPointerClick()
    {
        TeleportRandomly();
    }

    private void SetMaterial(bool gazedAt)
    {
        if (InactiveMaterial != null && GazedAtMaterial != null)
        {
            _myRenderer.material = gazedAt ? GazedAtMaterial : InactiveMaterial;
        }
    }
}