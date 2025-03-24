using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BoardHandler : MonoBehaviour
{
    public HexGridManager gridManager;
    public TokenHandler token;
    public TextMeshProUGUI debugText;
    public enum Mode
    {
        move,
        cone,
        areaLocal,
        areaTarget
    }
    Mode currentMode;

    void Start()
    {
        gridManager.GenerateGrid();
        token.currentTile = gridManager.GetTileAt(token.startTileAxialCoordinates);
        token.SnapToHex(token.currentTile);

        SwapMode(Mode.move);
    }

    void SwapMode(Mode newMode)
    {
        switch (newMode)
        {
            case Mode.move:
                currentMode = Mode.move;
                debugText.text = "Move";
            break;
            case Mode.areaLocal:
                currentMode = Mode.areaLocal;
                debugText.text = "Area";
            break;
            case Mode.cone:
                currentMode = Mode.cone;
                debugText.text = "Cone";
            break;
            default:
                currentMode = Mode.move;
                debugText.text = "Move";
            break;
        }
    }

    void Update()
    {
        gridManager.ResetTiles();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            HexTile tile = hit.collider.GetComponent<HexTile>();
            if (tile)
            {
                switch (currentMode)
                {
                    case Mode.move:
                        currentMode = Mode.move;
                        debugText.text = "Move";
                        ColorTiles(gridManager.FindPath(token.currentTile, tile));
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (token.currentTile.GetDistance(tile) > 0)
                            StartCoroutine(token.TravelRoute(gridManager.FindPath(token.currentTile, tile)));
                        }
                    break;
                    case Mode.areaLocal:
                        currentMode = Mode.areaLocal;
                        debugText.text = "Area Around 1";
                        ColorTiles(gridManager.GetTilesInRange(token.currentTile, 1));

                    break;
                    case Mode.cone:
                        currentMode = Mode.cone;
                        debugText.text = "Cone";
                        ColorTiles(gridManager.GetConeFromTo(token.currentTile, tile, 2, 180));
                    break;
                    default:
                        currentMode = Mode.move;
                        debugText.text = "Move";
                    break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwapMode(Mode.move);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            SwapMode(Mode.areaLocal);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwapMode(Mode.cone);
        }
    }

    void ColorTiles(List<HexTile> tiles)
    {
        foreach (HexTile tile in tiles)
        {
            tile.SetColor(Color.cyan);
        }
    }
}
