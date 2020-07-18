using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{

    private Tilemap tilemap;

    public void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public Vector2 GetTileMidPoint(Vector2 worldPointerClickPosition)
    {
        Vector2 ret = Vector2.negativeInfinity;

        Vector3Int tileCoords = tilemap.WorldToCell(worldPointerClickPosition);

        if (tilemap.HasTile(tileCoords))
        {
            //tilemap.SetTileFlags(tileCoords, TileFlags.None);
            //Debug.Log("LOCK STATE: " + tilemap.GetTileFlags(tileCoords).ToString());
            //tilemap.SetColor(tileCoords, Color.blue);

            //Center of the tile
            ret.x = tilemap.CellToWorld(tileCoords).x;//
            ret.y = tilemap.CellToWorld(tileCoords).y + 0.3f;// + tilemap.size.y/3;
        }

        Debug.Log("pointer: " + worldPointerClickPosition.ToString() + " tileCoords: " + tileCoords.ToString() + " return: " + ret.ToString());

        return ret;
    }

}
