using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public int radius = 5;
    public float hexSize = 1f;

    [HideInInspector]
    public Dictionary<Vector2Int, HexTile> hexMap = new Dictionary<Vector2Int, HexTile>();

    // Axial directions (q, r)
    private readonly Vector2Int[] axialDirections = new Vector2Int[]
    {
        new Vector2Int(+1,  0), // east
        new Vector2Int(+1, -1), // northeast
        new Vector2Int( 0, -1), // northwest
        new Vector2Int(-1,  0), // west
        new Vector2Int(-1, +1), // southwest
        new Vector2Int( 0, +1)  // southeast
    };

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                if (tile)
                {
                    tile.SetColor(Color.black);
                    tile.isValid = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) {ResetTiles();}
    }

    public void GenerateGrid()
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                Vector2Int axialCoord = new Vector2Int(q, r);
                Vector3 worldPos = AxialToWorld(axialCoord);

                GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                HexTile tile = tileObj.GetComponent<HexTile>();

                Renderer rend = tileObj.GetComponent<Renderer>();
                rend.material = new Material(rend.sharedMaterial); // unique material instance

                tile.ResetTile();
                tile.axialCoord = axialCoord;
                tile.isValid = true;

                hexMap[axialCoord] = tile;
            }
        }
        AssignNeighbors();
    }

    void AssignNeighbors()
    {
        foreach (var pair in hexMap)
        {
            HexTile tile = pair.Value;
            foreach (Vector2Int dir in axialDirections)
            {
                Vector2Int neighborCoord = tile.axialCoord + dir;
                if (hexMap.TryGetValue(neighborCoord, out HexTile neighborTile))
                {
                    tile.neighbors.Add(neighborTile);
                }
            }
        }
    }

    // Input Start and End HexTiles 
    // Get A* path
    public List<HexTile> FindPath(HexTile startTile, HexTile endTile)
    {
        if (!endTile.isValid) {return null;}

        List<HexTile> toSearch = new List<HexTile>() {startTile};
        List<HexTile> processed = new List<HexTile>();

        while (toSearch.Any()) 
        {
            HexTile current = toSearch[0];
            foreach (HexTile t in toSearch)
            {
                if (t.fCost < current.fCost || 
                    t.fCost == current.fCost && t.hCost < current.hCost) 
                    {current = t;}
            }

            processed.Add(current);
            toSearch.Remove(current);

            if (current == endTile) {
                var currentPathTile = endTile;
                var path = new List<HexTile>();

                while (currentPathTile != startTile){
                    path.Add(currentPathTile);
                    // currentPathTile.SetColor(Color.green);
                    currentPathTile = currentPathTile.previousHextile;
                }

                return path;
            }

            foreach (HexTile neighbor in current.neighbors)
            {
                if (neighbor.isValid && !processed.Contains(neighbor)) {
                    bool inSearch = toSearch.Contains(neighbor);
                    int costToNeighbor = current.gCost + current.GetDistance(neighbor);

                    if (!inSearch || costToNeighbor < neighbor.gCost){
                        neighbor.SetGCost(costToNeighbor);
                        neighbor.previousHextile = current;

                        if (!inSearch) {
                            neighbor.SetHCost(neighbor.GetDistance(endTile));
                            toSearch.Add(neighbor);
                        }
                    }
                }
            }
        }

        return null;
    }

    // Input Axial Coordinates 
    // Get corresponding world location
    Vector3 AxialToWorld(Vector2Int axial){
        float x = hexSize * Mathf.Sqrt(3f) * (axial.x + axial.y / 2f);
        float z = hexSize * 1.5f * axial.y;
        return new Vector3(x, 0, z);
    }

    // Input Axial Coordinates 
    // Get corresponding tile
    public HexTile GetTileAt(Vector2Int coord){
        return hexMap.TryGetValue(coord, out HexTile tile) ? tile : null;
    }

    // Input Starting tile and a range
    // Get all hexes within that range
    public List<HexTile> GetTilesInRange(HexTile tile, int range){
        
        List<HexTile> result = new List<HexTile>();

        for (int dq = -range; dq <= range; dq++)
        {
            int dr1 = Mathf.Max(-range, -dq - range);
            int dr2 = Mathf.Min(range, -dq + range);
            for (int dr = dr1; dr <= dr2; dr++)
            {
                Vector2Int coord = tile.axialCoord + new Vector2Int(dq, dr);
                if (hexMap.TryGetValue(coord, out HexTile outTile))
                {
                    result.Add(outTile);
                }
            }
        }
        return result;
    }

    // Input Starting tile, end tile and a range 
    // Get all hexes within a cone in that direction range

    public List<HexTile> GetConeFromTo(HexTile startTile, HexTile endTile, int range, float coneAngleDegrees)
    {
        if (startTile == endTile) {return null;}
        
        List<HexTile> coneTiles = new List<HexTile>();
        List<HexTile> inRange = GetTilesInRange(startTile, range);

        Vector3 startPos = AxialToWorld(startTile.axialCoord);
        Vector3 endPos = AxialToWorld(endTile.axialCoord);
        Vector3 forwardDir = (endPos - startPos).normalized;

        float halfAngle = coneAngleDegrees / 2f;

        foreach (HexTile tile in inRange)
        {
            if (tile == startTile) continue;

            Vector3 tilePos = AxialToWorld(tile.axialCoord);
            Vector3 dirToTile = (tilePos - startPos).normalized;

            float angle = Vector3.Angle(forwardDir, dirToTile);

            if (angle <= halfAngle)
            {
                coneTiles.Add(tile);
            }
        }

        return coneTiles;
    }

    // Reset Pathfinding Value and Color
    public void ResetTiles()
    {
        foreach (var pair in hexMap) 
        {   
                pair.Value.ResetTile();
        }
    }
}
