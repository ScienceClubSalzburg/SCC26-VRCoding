using UnityEngine;
using UnityEngine.InputSystem; // Neu für das neue Input System

public class ResetButton : MonoBehaviour
{
    [SerializeField] private SlidingPuzzleManager puzzleManager;
    private bool isHovering = false;
    private Color originalColor;
    private Renderer buttonRenderer;

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            originalColor = buttonRenderer.material.color;
        }
    }

    void Update()
    {
        HandleInputWithNewSystem();
    }

    /// <summary>
    /// Verarbeitet Hover und Klick mit dem neuen Input System
    /// </summary>
    private void HandleInputWithNewSystem()
    {
        if (Mouse.current == null || Camera.main == null || buttonRenderer == null) return;

        // Strahl von der Mausposition schießen
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        bool hitThisCollider = false;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Prüfen, ob genau dieser Reset-Button getroffen wurde
            if (hit.collider == GetComponent<Collider>())
            {
                hitThisCollider = true;

                // 1. HOVER LOGIK (Ehemals OnMouseEnter)
                if (!isHovering)
                {
                    isHovering = true;
                    buttonRenderer.material.color = Color.white;
                }

                // 2. KLICK LOGIK (Ehemals OnMouseDown)
                if (Mouse.current.leftButton.wasPressedThisFrame && puzzleManager != null)
                {
                    puzzleManager.ResetPuzzle();
                }
            }
        }

        // 3. HOVER EXIT LOGIK (Ehemals OnMouseExit)
        if (!hitThisCollider && isHovering)
        {
            isHovering = false;
            buttonRenderer.material.color = originalColor;
        }
    }
}
