using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHandler : MonoBehaviour
{
    public HexGridManager gridManager;
    public TokenHandler token;

    void Start()
    {
        gridManager.GenerateGrid();
        token.currentTile = gridManager.GetTileAt(token.startTileAxialCoordinates);
        token.SnapToHex(token.currentTile);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                if (tile)
                {
                    if (token.currentTile.GetDistance(tile) > 0)
                    StartCoroutine(token.TravelRoute(gridManager.FindPath(token.currentTile, tile)));
                }
            }
        }
    }
}
