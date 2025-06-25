using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    public int playerNumber = 1; // 1 or 2
    public int health = 40;
    public Tile currentTile;

    public float moveCooldown = 0.2f;
    private float lastMoveTime;

    public HealthBar healthBar;

   
    private Vector2Int startingPosition;
    public KeyCode restartKey;// different for each player

   

    void Start()
    {
        if (currentTile != null)
            startingPosition = currentTile.gridPosition;

        if (playerNumber == 1)
            restartKey = KeyCode.R;
        else if (playerNumber == 2)
            restartKey = KeyCode.Keypad0;
    }
    public void MoveTo(Tile tile)
    {
        if (!tile.IsWalkable())
            return;

        transform.position = tile.transform.position;
        currentTile = tile;
        health += tile.healthImpact;
        health = Mathf.Clamp(health, 0, 40);

        if (healthBar != null)
            healthBar.SetHealth(health);

        Debug.Log($"Player {playerNumber} moved to {tile.gridPosition}, health: {health}");

        if (tile.tileType == TileType.Goal)
        {
            Debug.Log($"🎉 Player {playerNumber} reached the goal!");
        }

        if (health <= 0)
        {
            Debug.Log($"💀 Player {playerNumber} died.");
        }
    }

    void Update()
    {
        if (Time.time - lastMoveTime < moveCooldown) return;

        Vector2Int direction = Vector2Int.zero;

        if (playerNumber == 2)
        {
            if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;
        }
        else if (playerNumber == 1)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;
        }

        if (direction != Vector2Int.zero)
        {
            TryMove(direction);
            lastMoveTime = Time.time;
        }

   
    }

    void TryMove(Vector2Int dir)
    {
        Vector2Int newPos = currentTile.gridPosition + dir;

        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (newPos.x < 0 || newPos.x >= gridManager.width || newPos.y < 0 || newPos.y >= gridManager.height)
            return;

        Tile targetTile = gridManager.GetTileAt(newPos);
        if (targetTile != null && targetTile.IsWalkable())
        {
            MoveTo(targetTile);
        }
    }

    public void ForceReset(Tile[,] grid)
    {
        Tile startTile = grid[startingPosition.x, startingPosition.y];
        MoveTo(startTile);
        health = 40;
        if (healthBar != null)
            healthBar.SetHealth(health);
       
    }
}

