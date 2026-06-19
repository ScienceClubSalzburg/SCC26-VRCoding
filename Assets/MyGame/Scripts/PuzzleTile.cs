using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // Neu für das neue Input System

/// <summary>
/// Einzelne Puzzlekachel mit Interaktionsunterstützung
/// </summary>
public class PuzzleTile : MonoBehaviour
{
    private int tileNumber;
    private SlidingPuzzleManager puzzleManager;
    private Color originalColor;
    private Color hoverColor;
    private bool isHovered = false;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
            hoverColor = originalColor + new Color(0.2f, 0.2f, 0.2f, 0f);
        }
    }

    void Update()
    {
        HandleHoverWithNewInput();
    }

    /// <summary>
    /// Ersetzt OnMouseEnter/OnMouseExit für das neue Input System
    /// </summary>
    private void HandleHoverWithNewInput()
    {
        if (Mouse.current == null || Camera.main == null || meshRenderer == null) return;

        // Strahl von der Mausposition in die Welt schießen
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Prüfen, ob genau diese Kachel getroffen wurde
            if (hit.collider == GetComponent<Collider>())
            {
                if (!isHovered)
                {
                    isHovered = true;
                    meshRenderer.material.color = hoverColor;
                }
                return;
            }
        }

        // Wenn der Mauszeiger nicht mehr auf dieser Kachel ist
        if (isHovered)
        {
            isHovered = false;
            meshRenderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// Initializes the tile with number and puzzle manager reference
    /// </summary>
    public void Initialize(int number, SlidingPuzzleManager manager)
    {
        tileNumber = number;
        puzzleManager = manager;
    }

    /// <summary>
    /// Sets the tile number text
    /// </summary>
    public void SetTileNumber(int number)
    {
        TMP_Text textMesh = GetComponentInChildren<TMP_Text>();
        if (textMesh != null)
        {
            textMesh.text = number.ToString();
        }
    }

    /// <summary>
    /// Called on mouse click
    /// </summary>
    public void OnClicked()
    {
        if (puzzleManager != null)
            puzzleManager.OnTileSelected(tileNumber);
    }

    /// <summary>
    /// Gets the tile number
    /// </summary>
    public int GetTileNumber()
    {
        return tileNumber;
    }
}
