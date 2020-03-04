using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HighlightTilemap : MonoBehaviour
{
    public TileBase highlightfriendly;
    public TileBase highlightenemy;
    public TileBase highlightdanger;

    Tilemap tilemap;

    static float glowPulseSpeed = 5f;

    private void Start()
    {
        tilemap = gameObject.GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        float glowMagnitude = 0.5f + (Mathf.Abs(Mathf.Sin(Time.unscaledTime * glowPulseSpeed)) * 0.5f);
        gameObject.GetComponent<Tilemap>().color = new Color(glowMagnitude, glowMagnitude, glowMagnitude);
    }

    private Vector3Int ChessboardToGrid(Vector2Int v)
    {
        return new Vector3Int((4 - v.y), (3 - v.x), 0);
    }

    public void HighlightFriendlyTile(Vector2Int v)
    {
        tilemap.SetTile(ChessboardToGrid(v), highlightfriendly);
        tilemap.RefreshTile(ChessboardToGrid(v));
    }

    public void HighlightEnemyTile(Vector2Int v)
    {
        tilemap.SetTile(ChessboardToGrid(v), highlightenemy);
        tilemap.RefreshTile(ChessboardToGrid(v));
    }

    public void HighlightDangerTile(Vector2Int v)
    {
        tilemap.SetTile(ChessboardToGrid(v), highlightdanger);
        tilemap.RefreshTile(ChessboardToGrid(v));
    }

    public void ClearAllTiles()
    {
        tilemap.ClearAllTiles();
        tilemap.RefreshAllTiles();
    }
}
