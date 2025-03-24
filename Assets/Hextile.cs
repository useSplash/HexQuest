using System;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public Vector2Int axialCoord; 
    public float height = 1.0f;

    #region Pathfinding Variables
    [HideInInspector]
    public List<HexTile> neighbors = new List<HexTile>(); // References to neighbor tiles
    public HexTile previousHextile;

    public int hCost;
    public int gCost;
    public int fCost;

    public TextMesh hCostText;
    public TextMesh gCostText;
    public TextMesh fCostText;
    #endregion

    public bool isValid;
    public bool isOccupied;
    

    public void SetColor(Color color)
    {
        if (isValid) GetComponent<MeshRenderer>().material.color = color;
    }
    public void SetHCost(int cost = 0)
    {
        hCost = cost;
        hCostText.text = cost.ToString();

        SetFCost(gCost + hCost);
    }
    public void SetGCost(int cost = 0)
    {
        gCost = cost;
        gCostText.text = cost.ToString();

        SetFCost(gCost + hCost);
    }
    public void SetFCost(int cost = 0)
    {
        fCost = cost;
        fCostText.text = cost.ToString();
    }

    public void ResetTile()
    {
        hCost = 0;
        gCost = 0;
        fCost = 0;

        hCostText.text = "";
        gCostText.text = "";
        fCostText.text = "";

        previousHextile = null;
        
        SetColor(Color.white);
    }

    Vector3Int AxialToCube(Vector2Int axial)
    {
        int x = axial.x;
        int z = axial.y;
        int y = -x - z;
        return new Vector3Int(x, y, z);
    }

    public int GetDistance(HexTile target)
    {
        // Convert axial to cube first, then get distance
        Vector3Int aCube = AxialToCube(axialCoord);
        Vector3Int bCube = AxialToCube(target.axialCoord);
        return Mathf.Max(Mathf.Abs(aCube.x - bCube.x), Mathf.Abs(aCube.y - bCube.y), Mathf.Abs(aCube.z - bCube.z));
    }
}
