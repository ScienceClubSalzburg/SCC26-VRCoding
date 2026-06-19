using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // Neu für das neue Input System

/// <summary>
/// Manages a 3x3 sliding puzzle with interactive tiles
/// </summary>
public class SlidingPuzzleManager : MonoBehaviour
{
    [SerializeField] private int gridSize = 3;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float tileMoveSpeed = 5f;
    [SerializeField] private PuzzleTile tilePrefab;
    [SerializeField] private int[] solutionOrder = { 1, 4, 7, 2, 3, 6, 5, 8, 0 };

    private int[,] puzzleGrid;
    private Dictionary<int, Transform> tileTransforms = new Dictionary<int, Transform>();
    private Vector2Int emptyPosition;
    private int moveCount = 0;
    private bool isSolving = false;

    void Start()
    {
        InitializePuzzle();
    }

    void Update()
    {
        HandleDesktopInput();
    }

    /// <summary>
    /// Initializes the puzzle grid and creates tiles
    /// </summary>
    void InitializePuzzle()
    {
        puzzleGrid = new int[gridSize, gridSize];
        int tileNumber = 1;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                puzzleGrid[x, y] = tileNumber;
                tileNumber++;
            }
        }

        puzzleGrid[gridSize - 1, gridSize - 1] = 0;
        emptyPosition = new Vector2Int(gridSize - 1, gridSize - 1);

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int number = puzzleGrid[x, y];
                if (number != 0)
                {
                    CreateTile(number, x, y);
                }
            }
        }
    }

    /// <summary>
    /// Creates a single tile from the prefab
    /// </summary>
    void CreateTile(int number, int gridX, int gridY)
    {
        Vector3 worldPos = GridToWorldPosition(gridX, gridY);
        PuzzleTile tileScript = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);

        tileScript.gameObject.name = $"Tile_{number}";
        tileScript.SetTileNumber(number);
        tileScript.Initialize(number, this);

        tileTransforms[number] = tileScript.transform;
    }

    /// <summary>
    /// Converts grid coordinates to world positions
    /// </summary>
    Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        float x = (gridX - (gridSize - 1) / 2f) * tileSize;
        float y = ((gridSize - 1) / 2f - gridY) * tileSize;
        return transform.position + new Vector3(x, y, 0);
    }

    /// <summary>
    /// Called when a tile is selected
    /// </summary>
    public void OnTileSelected(int tileNumber)
    {
        Vector2Int tilePos = FindTilePosition(tileNumber);

        if (IsAdjacentToEmpty(tilePos))
        {
            MoveTile(tilePos);
        }
    }

    /// <summary>
    /// Finds the grid position of a tile
    /// </summary>
    Vector2Int FindTilePosition(int tileNumber)
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (puzzleGrid[x, y] == tileNumber)
                    return new Vector2Int(x, y);
            }
        }
        return Vector2Int.zero;
    }

    /// <summary>
    /// Checks if a tile is adjacent to the empty space
    /// </summary>
    bool IsAdjacentToEmpty(Vector2Int position)
    {
        return Vector2Int.Distance(position, emptyPosition) == 1f;
    }

    /// <summary>
    /// Moves a tile to the empty space
    /// </summary>
    void MoveTile(Vector2Int tilePos)
    {
        if (isSolving) return;

        int tileNumber = puzzleGrid[tilePos.x, tilePos.y];

        puzzleGrid[emptyPosition.x, emptyPosition.y] = tileNumber;
        puzzleGrid[tilePos.x, tilePos.y] = 0;

        Vector3 newWorldPos = GridToWorldPosition(emptyPosition.x, emptyPosition.y);
        StartCoroutine(AnimateTile(tileTransforms[tileNumber], newWorldPos));

        moveCount++;
        emptyPosition = tilePos;

        if (IsSolved())
        {
            OnPuzzleSolved();
        }
    }

    /// <summary>
    /// Animates tile movement
    /// </summary>
    System.Collections.IEnumerator AnimateTile(Transform tile, Vector3 targetPos)
    {
        float elapsedTime = 0f;
        float duration = 1f / tileMoveSpeed;
        Vector3 startPos = tile.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            tile.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            yield return null;
        }

        tile.position = targetPos;
    }

    /// <summary>
    /// Checks if puzzle is solved
    /// </summary>
    bool IsSolved()
    {
        int index = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (puzzleGrid[x, y] != solutionOrder[index])
                    return false;
                index++;
            }
        }
        return true;
    }

    /// <summary>
    /// Called when puzzle is solved
    /// </summary>
    void OnPuzzleSolved()
    {
        isSolving = true;
        Debug.Log($"✓ Puzzle solved in {moveCount} moves!");
    }

    /// <summary>
    /// Handles input using the new Input System
    /// </summary>
    void HandleDesktopInput()
    {
        // Prüft, ob die Maus existiert und die linke Maustaste in diesem Frame gedrückt wurde
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PuzzleTile tile = hit.collider.GetComponent<PuzzleTile>();
                if (tile != null)
                {
                    tile.OnClicked();
                }
            }
        }
    }

    /// <summary>
    /// Resets the puzzle
    /// </summary>
    public void ResetPuzzle()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        tileTransforms.Clear();
        moveCount = 0;
        isSolving = false;

        InitializePuzzle();
    }
}
