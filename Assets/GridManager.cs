using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    private PlayerController player1;
    private PlayerController player2;

    public GameObject healthBarPrefab; 

    public int width = 12;
    public int height = 8;

    private Transform tilesContainer;
    private Transform playersContainer;
    private Transform uiContainer;

    public KeyCode player1RestartKey = KeyCode.R;
    public KeyCode player2RestartKey = KeyCode.Keypad0;

    [SerializeField] private LineRenderer pathLine;

    private bool player1Restarted = false;
    private bool player2Restarted = false;

    public GameObject winPopupPanel;
    public TMP_Text winMessageText;

    [System.Serializable]
    public struct TilePrefabEntry
    {
        public TileType type;
        public GameObject prefab;
    }

    public TilePrefabEntry[] tilePrefabsArray;
    private Dictionary<TileType, GameObject> tilePrefabsDict;

    private Tile[,] grid;
    void Start()
    {
        // Create containers
        tilesContainer = new GameObject("TilesContainer").transform;
        tilesContainer.parent = transform;

        playersContainer = new GameObject("PlayersContainer").transform;
        playersContainer.parent = transform;

        uiContainer = FindFirstObjectByType<Canvas>().transform; // UI stays under Canvas

        tilePrefabsDict = new Dictionary<TileType, GameObject>();
        foreach (var entry in tilePrefabsArray)
        {
            tilePrefabsDict[entry.type] = entry.prefab;
        }

        GenerateGrid();
    }
    void GenerateGrid()
    {
        do
        {
            grid = new Tile[width, height];

            Vector2 offset = new Vector2(width / 2f - 0.5f, height / 2f - 0.5f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType type = GetRandomTileType();

                    if (x == 0 && y == 0)
                        type = TileType.Empty;
                    else if (x == width - 1 && y == height - 1)
                        type = TileType.Goal;

                    GameObject prefab = tilePrefabsDict[type];

                    Vector3 position = new Vector3(x - offset.x, y - offset.y, 0);
                    GameObject tileGO = Instantiate(prefab, position, Quaternion.identity, tilesContainer);

                    Tile tile = tileGO.GetComponent<Tile>();
                    tile.Init(new Vector2Int(x, y), type);

                    grid[x, y] = tile;
                }
            }


        } while (FindPath(grid[0, 0], grid[width - 1, height - 1], false) == null); // make sure that there is a valid path


        // Spawn players under playersContainer
        SpawnPlayers();
    }


    void SpawnPlayers()
    {
        Tile startTile = grid[0, 0];
        Tile goalTile = grid[width - 1, height - 1];

        // First, spawn players so you have access to their Health
        foreach (Transform child in playersContainer)
            Destroy(child.gameObject);

        GameObject player1GO = Instantiate(player1Prefab, startTile.transform.position, Quaternion.identity, playersContainer);
        player1 = player1GO.GetComponent<PlayerController>();
        player1.playerNumber = 1;
        player1.MoveTo(startTile);

        GameObject player2GO = Instantiate(player2Prefab, startTile.transform.position, Quaternion.identity, playersContainer);
        player2 = player2GO.GetComponent<PlayerController>();
        player2.playerNumber = 2;
        player2.MoveTo(startTile);

        SetupHealthBar(player1);
        SetupHealthBar(player2);

        // Now that player1 exists and has health, check path without resources:
        List<Tile> pathWithoutResources = FindPath(startTile, goalTile, ignoreResources: true);

        if (pathWithoutResources == null)
        {
            Debug.Log("No path without resources exists, player must go through resources.");
        }
        else
        {
            int totalDamage = 0;
            foreach (var tile in pathWithoutResources)
            {
                totalDamage += Mathf.Max(0, Mathf.Abs(tile.healthImpact)); // sum damage ignoring healing
            }

            if (player1.health <= totalDamage)
            {
                Debug.Log("Player cannot survive path without resources, must use resources.");
            }
            else
            {
                Debug.Log("Player can survive without resources.");
            }
        }

        // Highlight normal path including resources
        List<Tile> path = FindPath(startTile, goalTile);
        HighlightPath(path);
    }


    public void HighlightPath(List<Tile> path)
    {
        if (path == null || path.Count == 0)
        {
            pathLine.positionCount = 0;
            return;
        }

        pathLine.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i].transform.position;
            pos.z = -0.1f; // Ensure the line appears in front of tiles
            pathLine.SetPosition(i, pos);
        }
    }

    void SetupHealthBar(PlayerController player)
    {
        GameObject hbGO = Instantiate(healthBarPrefab, Vector3.zero, Quaternion.identity, uiContainer);
        HealthBar hbScript = hbGO.GetComponent<HealthBar>();
        hbScript.target = player.transform;
        player.healthBar = hbScript;
    }
    TileType GetRandomTileType()
    {
        int rand = Random.Range(0, 6); // 6 tile types (excluding Goal)

        switch (rand)
        {
            case 0: return TileType.Empty;
            case 1: return TileType.Obstacle;
            case 2: return TileType.Resource;
            case 3: return TileType.Ground_Normal;
            case 4: return TileType.Ground_Slow;
            case 5: return TileType.Ground_Dangerous;
            default: return TileType.Empty;
        }
    }

    public Tile GetTileAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return null;

        return grid[pos.x, pos.y];
    }

    public void ResetBoard()
    {
        // Destroy all tiles only
        foreach (Transform child in tilesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in uiContainer)
        {
            if (child.GetComponent<HealthBar>() != null)
                Destroy(child.gameObject);
        }

        GenerateGrid(); // regenerates tiles and respawns players + health bars
    }

    void Update()
    {
        if (!player1Restarted && Input.GetKeyDown(player1RestartKey))
        {
            player1Restarted = true;
            RestartGameForPlayer(player1);
            RestartGameForPlayer(player2);
        }

        if (!player2Restarted && Input.GetKeyDown(player2RestartKey))
        {
            player2Restarted = true;
            RestartGameForPlayer(player1);
            RestartGameForPlayer(player2);
        }

        Debug.Log(player1.playerNumber);

        if (player1.currentTile.tileType == TileType.Goal)
            ShowWinPopup(1);
        else if (player2.currentTile.tileType == TileType.Goal)
            ShowWinPopup(2);

        if (player1.health <= 0)
            ShowWinPopup(2);
        else if (player2.health <= 0)
            ShowWinPopup(1);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(); 
        }
    }

    void RestartGameForPlayer(PlayerController player)
    {
        Debug.Log($"Player {player.playerNumber} restarted the board.");

        ResetBoard();

        player1.ForceReset(grid);
        player2.ForceReset(grid);

          
    }

    private List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = tile.gridPosition + dir;
            Tile neighborTile = GetTileAt(neighborPos);
            if (neighborTile != null && neighborTile.IsWalkable())
                neighbors.Add(neighborTile);
        }

        return neighbors;
    }

    public List<Tile> FindPath(Tile start, Tile goal, bool ignoreResources = false)
    {
        List<PathNode> openSet = new List<PathNode>();
        HashSet<Tile> closedSet = new HashSet<Tile>();

        PathNode startNode = new PathNode(start) { gCost = 0, hCost = Heuristic(start, goal) };
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get node with lowest fCost
            PathNode current = openSet[0];
            foreach (var node in openSet)
            {
                if (node.fCost < current.fCost || (node.fCost == current.fCost && node.hCost < current.hCost))
                    current = node;
            }

            if (current.tile == goal)
            {
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            closedSet.Add(current.tile);

            foreach (Tile neighbor in GetNeighbors(current.tile))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                if (ignoreResources && neighbor.tileType == TileType.Resource)
                    continue; // skip resources if ignoring

                int tileCost = (neighbor.healthImpact < 0 ? Mathf.Abs(neighbor.healthImpact) : 0) + 1;
                int tentativeGCost = current.gCost + tileCost; 

                PathNode neighborNode = openSet.Find(n => n.tile == neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new PathNode(neighbor);
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = Heuristic(neighbor, goal);
                    neighborNode.parent = current;
                    openSet.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = current;
                }
            }
        }

        return null; // No path found
    }

    private int Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.gridPosition.x - b.gridPosition.x) + Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
    }

    private List<Tile> ReconstructPath(PathNode node)
    {
        List<Tile> path = new List<Tile>();
        while (node != null)
        {
            path.Add(node.tile);
            node = node.parent;
        }
        path.Reverse();
        int totalHealthLoss = 0;
        string pathDebug = "A* path: ";
        foreach (var tile in path)
        {
            pathDebug += $"[{tile.gridPosition}] ";
            if (tile.healthImpact < 0)
                totalHealthLoss += Mathf.Abs(tile.healthImpact);
        }
        Debug.Log(pathDebug);
        Debug.Log($"Total health loss on path: {totalHealthLoss}");
        return path;
    }

    void ShowWinPopup(int winnerPlayerNumber)
    {
        winMessageText.text = $"Player {winnerPlayerNumber} Won!";
        winPopupPanel.SetActive(true);
    }

    public void RestartGameFromPopup()
    {
        winPopupPanel.SetActive(false);

        player1Restarted = false;
        player2Restarted = false;

        ResetBoard();
    }

    private class PathNode
    {
        public Tile tile;
        public PathNode parent;
        public int gCost; // Cost from start to this node
        public int hCost; // Heuristic cost to goal
        public int fCost => gCost + hCost;

        public PathNode(Tile tile)
        {
            this.tile = tile;
        }
    }
}


