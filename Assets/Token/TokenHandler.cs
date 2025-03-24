using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenHandler : MonoBehaviour
{
    public HexTile currentTile;
    public Vector2Int startTileAxialCoordinates;

    public void SnapToHex(HexTile target)
    {
        currentTile.SetColor(Color.yellow);
        transform.position = 
                    target.gameObject.transform.position 
                    + new Vector3(0, target.height);
        currentTile = target;
        currentTile.SetColor(Color.red);
    }

    public IEnumerator TravelRoute(List<HexTile> path)
    {
        for (int i = path.Count-1; i >= 0; i--)
        {
            SnapToHex(path[i]);
            yield return new WaitForSeconds(1.0f);
        }
    }
}
