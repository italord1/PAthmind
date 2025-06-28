using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty,
    Obstacle,
    Resource,
    Ground_Normal,
    Ground_Slow,
    Ground_Dangerous,
    Goal
}

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public TileType tileType;

    public int healthImpact;      // How much it drains or adds
    public float movementCost;    // Used by A*

    public void Init(Vector2Int pos, TileType type)
    {
        gridPosition = pos;
        tileType = type;
        name = $"Tile_{pos.x}_{pos.y}";

        SetTilePropertiesByType(type);
    }

    private void SetTilePropertiesByType(TileType type)
    {
        switch (type)
        {
            case TileType.Empty:
                healthImpact = 0;
                movementCost = 1f;
                break;
            case TileType.Obstacle:
                healthImpact = 0;
                movementCost = Mathf.Infinity; // Not walkable
                break;
            case TileType.Resource:
                healthImpact = +4;   // Increase healing amount
                movementCost = 1f;
                break;
            case TileType.Ground_Normal:
                healthImpact = -5;    // Increase damage so player loses more health
                movementCost = 1f;
                break;
            case TileType.Ground_Slow:
                healthImpact = -8;    // More damage + slower
                movementCost = 2f;
                break;
            case TileType.Ground_Dangerous:
                healthImpact = -10;   // High damage and slowest
                movementCost = 3f;
                break;
            case TileType.Goal:
                healthImpact = 0;
                movementCost = 1f;
                break;
        }
    }

    public bool IsWalkable()
    {
        return tileType != TileType.Obstacle;
    }

  
    public void Highlight(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}
