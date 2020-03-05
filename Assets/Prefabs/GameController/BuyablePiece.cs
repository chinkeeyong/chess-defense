using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyablePiece : Piece

{
    Vector3 startingBoardPosition;
    public int price = 0;
    bool buyable = false;

    Vector2Int[] basePossibleSpawnPositions = {
        new Vector2Int (0, 8),
        new Vector2Int (1, 8),
        new Vector2Int (2, 8),
        new Vector2Int (3, 8),
        new Vector2Int (4, 8),
        new Vector2Int (5, 8),
        new Vector2Int (6, 8),
        new Vector2Int (7, 8),
        new Vector2Int (0, 7),
        new Vector2Int (1, 7),
        new Vector2Int (2, 7),
        new Vector2Int (3, 7),
        new Vector2Int (4, 7),
        new Vector2Int (5, 7),
        new Vector2Int (6, 7),
        new Vector2Int (7, 7)
    };

    List<Vector2Int> possibleSpawnPositions = new List<Vector2Int>();

    new void Start()
    {
        startingBoardPosition = transform.position;
        glowColor = Color.green;
    }

    new void OnMouseDown()
    {
        if (gameController.gamePhase == GameController.GamePhase.PLAYER_TO_MOVE &&
            !gameController.playerIsDraggingSomething &&
            buyable)
        {
            gameController.playerIsDraggingSomething = true;
            dragging = true;
            shadow.SetActive(false);

            foreach (Vector2Int v in possibleSpawnPositions)
            {
                highlightTilemap.HighlightFriendlyTile(v);
            }
        }
    }

    new void OnMouseUp()
    {
        if (dragging)
        {
            gameController.playerIsDraggingSomething = false;
            dragging = false;
            shadow.SetActive(true);
            highlightTilemap.LoadTilesFromCache();
            transform.position = startingBoardPosition;

            // Spawn in a new piece at hovered tile if it's valid.
            Vector2Int _hoveredTile = GetMouseHoveredBoardPosition();
            if (possibleSpawnPositions.Contains(_hoveredTile))
            {
                Piece _newPiece = null;
                _newPiece = gameController.InstantiatePiece(PlayerColor.White, chessPieceType, _hoveredTile.x, _hoveredTile.y);
                if (_newPiece != null)
                {
                    gameController.dinars -= price;
                    _newPiece.GetComponent<Animation>().Play("SpawnAnimation");
                    gameController.gamePhase = GameController.GamePhase.PLAYER_JUST_BOUGHT_PIECE;
                }
            }
        }
    }

    new void OnMouseEnter()
    {
        if (buyable)
        {
            Highlight();
        }
    }


    public void UpdateBuyability()
    {
        buyable = gameController.dinars >= price;

        if (buyable)
        {
            body.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            body.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }
    }


    public void UpdatePossibleSpawnPositions()
    {
        possibleSpawnPositions.Clear();

        foreach (Vector2Int _position in basePossibleSpawnPositions)
        {
            // Cull spaces that are obstructed
            if (PotentialMoveIsObstructed(_position, Vector2Int.zero))
            {
                continue;
            }

            possibleSpawnPositions.Add(_position);
        }
    }
}
