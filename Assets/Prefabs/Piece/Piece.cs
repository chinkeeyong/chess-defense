using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{

    //Important Elements to be placed first in the editor.

    public enum PlayerColor { White, Black };
    public PlayerColor playerColor = PlayerColor.White;

    public enum ChessPieceType { King, Queen, Bishop, Rook, Knight, Pawn };
    public ChessPieceType chessPieceType = ChessPieceType.Pawn;

    public Vector2Int boardPosition = new Vector2Int(0, 0);

    public bool snapToBoardPosition = true;
    public bool highlightOnMouseOver = true;


    // Runtime variables

    Color glowColor = new Color();
    bool draggable = false;
    bool dragging = false;
    bool highlighted = false;
    bool hasMoved = false;
    List<Vector2Int> validMoves = new List<Vector2Int>();


    // Static values

    static Vector2 boardOrigin = new Vector2(0.24f, 1.58f);
    static Vector2 boardOriginForMouse = new Vector2(0.24f, 1.62f);
    static Vector2 displacementPerX = new Vector2(0.24f, -0.18f);
    static Vector2 displacementPerY = new Vector2(-0.24f, -0.18f);
    static Vector3 mouseDragOffset = new Vector3(0f, 0.1f);
    static int BoardXMin = 0;
    static int BoardXMax = 7;
    static int BoardYMin = 1;
    static int BoardYMax = 9;
    static int BoardYPlayerMax = 8;
    static float zDepthPerY = 0.01f;
    static float glowPulseSpeed = 5f;


    // Resources

    GameObject shadow;
    GameObject glow;
    SpriteRenderer glowSpriteRenderer;
    GameObject body;
    GameController gameController;
    HighlightTilemap highlightTilemap;
    public Sprite white_king;
    public Sprite white_queen;
    public Sprite white_bishop;
    public Sprite white_rook;
    public Sprite white_knight;
    public Sprite white_pawn;
    public Sprite black_king;
    public Sprite black_queen;
    public Sprite black_bishop;
    public Sprite black_rook;
    public Sprite black_knight;
    public Sprite black_pawn;
    public Sprite glow_king;
    public Sprite glow_queen;
    public Sprite glow_bishop;
    public Sprite glow_rook;
    public Sprite glow_knight;
    public Sprite glow_pawn;

    // Awake is called when the script is loaded, before Start. We initialize the piece here.
    private void Awake()
    {
        // Initialize all local object references.
        shadow = transform.GetChild(0).gameObject;
        glow = transform.GetChild(1).gameObject;
        glowSpriteRenderer = glow.GetComponent<SpriteRenderer>();
        body = transform.GetChild(2).gameObject;

        // Initialize all world object references.
        gameController = GameObject.Find("/GameController").GetComponent<GameController>();
        highlightTilemap = GameObject.Find("/Grid/Highlight Tilemap").GetComponent<HighlightTilemap>();

        // Not glowing initially
        glow.SetActive(false);
    }

    public void Initialize()
    {

        // Initialize this chess piece's type.
        SetChessPieceType(chessPieceType);

        // Add this chess piece to the global array.
        gameController.pieces.Add(this);
    }

    public void Start()
    {
        GetValidMoves();
    }

    // Update is called once per frame
    void Update()
    {
        // Snap to board position
        if (snapToBoardPosition && !dragging)
        {
            transform.position = boardOrigin + (boardPosition.x * displacementPerX) + (boardPosition.y * displacementPerY);
        }

        // Set z depth
        float newZ = transform.position.y * zDepthPerY;
        if (dragging)
        {
            newZ -= 2f;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, newZ);

        // Make glow glowy
        if (glow.activeInHierarchy)
        {
            float glowMagnitude = 0.5f + (Mathf.Abs(Mathf.Sin(Time.unscaledTime * glowPulseSpeed)) * 0.5f);
            glowSpriteRenderer.color = new Color(glowColor.r * glowMagnitude, glowColor.g * glowMagnitude, glowColor.b * glowMagnitude);
        }
    }


    private void OnMouseEnter()
    {
        if (highlightOnMouseOver && !highlighted)
        {
            Highlight();
        }
    }


    private void OnMouseExit()
    {
        if (highlighted)
        {
            Unhighlight();
        }
    }


    private void OnMouseUp()
    {
        if (dragging)
        {
            dragging = false;
            shadow.SetActive(true);
            
            // Move the mouse to the new tile if it's a valid move.
            Vector2Int _hoveredTile = GetMouseHoveredTile();
            if (validMoves.Contains(_hoveredTile))
            {
                boardPosition = _hoveredTile;
                hasMoved = true;

                // Capture enemy piece, if it's in that space
                Piece _capturedPieceIfAny = null;
                foreach (Piece _piece in gameController.pieces)
                {
                    if (_piece.playerColor != playerColor && _piece.boardPosition == boardPosition)
                    {
                        _capturedPieceIfAny = _piece;
                        break;
                    }
                }

                if (_capturedPieceIfAny != null)
                {
                    gameController.Capture(_capturedPieceIfAny);
                }

                // Now refresh valid moves of all friendly pieces
                foreach (Piece _piece in gameController.pieces)
                {
                     _piece.GetValidMoves();
                }

                // And update UI
                Unhighlight();
                Highlight();
            }
        }
    }


    private void OnMouseDown()
    {
        if (draggable)
        {
            dragging = true;
            shadow.SetActive(false);
        }
    }


    private void OnMouseDrag()
    {
        if (dragging)
        {
            Vector3 mouseScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(mouseScreenPoint) + mouseDragOffset;
            mousePosition.z = 0f;
            transform.position = mousePosition;
        }
    }

    private void SetChessPieceType(ChessPieceType t)
    {
        switch (t)
        {
            case ChessPieceType.King:
                if (playerColor == PlayerColor.White) { body.GetComponent<SpriteRenderer>().sprite = white_king; }
                else { body.GetComponent<SpriteRenderer>().sprite = black_king; }
                glowSpriteRenderer.sprite = glow_king;
                break;

            case ChessPieceType.Queen:
                if (playerColor == PlayerColor.White) { body.GetComponent<SpriteRenderer>().sprite = white_queen; }
                else { body.GetComponent<SpriteRenderer>().sprite = black_queen; }
                glowSpriteRenderer.sprite = glow_queen;
                break;

            case ChessPieceType.Bishop:
                if (playerColor == PlayerColor.White) { body.GetComponent<SpriteRenderer>().sprite = white_bishop; }
                else { body.GetComponent<SpriteRenderer>().sprite = black_bishop; }
                glowSpriteRenderer.sprite = glow_bishop;
                break;

            case ChessPieceType.Rook:
                if (playerColor == PlayerColor.White) { body.GetComponent<SpriteRenderer>().sprite = white_rook; }
                else { body.GetComponent<SpriteRenderer>().sprite = black_rook; }
                glowSpriteRenderer.sprite = glow_rook;
                break;

            case ChessPieceType.Knight:
                if (playerColor == PlayerColor.White) { body.GetComponent<SpriteRenderer>().sprite = white_knight; }
                else { body.GetComponent<SpriteRenderer>().sprite = black_knight; }
                glowSpriteRenderer.sprite = glow_knight;
                break;

            case ChessPieceType.Pawn:
                if (playerColor == PlayerColor.White) { body.GetComponent<SpriteRenderer>().sprite = white_pawn; }
                else { body.GetComponent<SpriteRenderer>().sprite = black_pawn; }
                glowSpriteRenderer.sprite = glow_pawn;
                break;

            default:
                Debug.Log("Error: Tried to set Piece " + name + " to an undefined chessPieceType.");
                break;
        }

        switch (playerColor)
        {
            case PlayerColor.White:
                glowColor = Color.green;
                draggable = true;
                break;

            case PlayerColor.Black:
                glowColor = Color.red;
                draggable = false;
                break;

            default:
                Debug.Log("Error: Tried to set Piece " + name + " to an undefined playerColor.");
                break;
        }
    }


    private void GetValidMoves()
    {
        // Get the base movement for each chess piece type.
        List<Vector2Int> _baseValidMoves = new List<Vector2Int>();
        switch (chessPieceType)
        {
            case ChessPieceType.King: // Kings can move one space in any direction.
                _baseValidMoves.Add(new Vector2Int(1, -1));
                _baseValidMoves.Add(new Vector2Int(1, 0));
                _baseValidMoves.Add(new Vector2Int(1, 1));
                _baseValidMoves.Add(new Vector2Int(-1, -1));
                _baseValidMoves.Add(new Vector2Int(-1, 0));
                _baseValidMoves.Add(new Vector2Int(-1, 1));
                _baseValidMoves.Add(new Vector2Int(0, 1));
                _baseValidMoves.Add(new Vector2Int(0, -1));
                // TODO: Add castling.
                break;

            case ChessPieceType.Queen: // Queens can move up to 7 squares in any direction, obstructed by pieces.
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                break;

            case ChessPieceType.Bishop: // Bishops can move up to 7 squares diagonally, obstructed by pieces.
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                break;

            case ChessPieceType.Rook: // Rooks can move up to 7 squares horizontally or vertically, obstructed by pieces.
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_moveOnBoard)) { break; }
                }
                break;

            case ChessPieceType.Knight: // Knights can move in an L shape, jumping to any square.
                _baseValidMoves.Add(new Vector2Int(2, -1));
                _baseValidMoves.Add(new Vector2Int(2, 1));
                _baseValidMoves.Add(new Vector2Int(-2, -1));
                _baseValidMoves.Add(new Vector2Int(-2, 1));
                _baseValidMoves.Add(new Vector2Int(-1, 2));
                _baseValidMoves.Add(new Vector2Int(1, 2));
                _baseValidMoves.Add(new Vector2Int(-1, -2));
                _baseValidMoves.Add(new Vector2Int(1, -2));
                break;

            case ChessPieceType.Pawn: // Pawns can only move one space forward. "Forward" changes depending on if the pawn is white or black.
                if (playerColor == PlayerColor.White)
                {
                    if (!PotentialMoveIsObstructed(new Vector2Int(0, -1)))
                    {
                        _baseValidMoves.Add(new Vector2Int(0, -1));
                        if (!PotentialMoveIsObstructed(new Vector2Int(0, -2)) && !hasMoved)
                        {
                            _baseValidMoves.Add(new Vector2Int(0, -2)); // Double move on first move.
                        }
                    }

                    // Add potential captures.
                    foreach (Piece _piece in gameController.pieces)
                    {
                        if (_piece.playerColor != playerColor && _piece.boardPosition == boardPosition + new Vector2Int(-1, -1))
                        {
                            _baseValidMoves.Add(new Vector2Int(-1, -1));
                        }
                    }
                    foreach (Piece _piece in gameController.pieces)
                    {
                        if (_piece.playerColor != playerColor && _piece.boardPosition == boardPosition + new Vector2Int(1, -1))
                        {
                            _baseValidMoves.Add(new Vector2Int(1, -1));
                        }
                    }
                }
                else // Pawn is black
                {
                    if (!PotentialMoveIsObstructed(new Vector2Int(0, 1)))
                    {
                        _baseValidMoves.Add(new Vector2Int(0, 1));
                        if (!PotentialMoveIsObstructed(new Vector2Int(0, 2)) && !hasMoved)
                        {
                            _baseValidMoves.Add(new Vector2Int(0, 2)); // Double move on first move.
                        }
                    }

                    // Add potential captures.
                    foreach (Piece _piece in gameController.pieces)
                    {
                        if (_piece.playerColor != playerColor && _piece.boardPosition == boardPosition + new Vector2Int(-1, 1))
                        {
                            _baseValidMoves.Add(new Vector2Int(-1, 1));
                        }
                    }
                    foreach (Piece _piece in gameController.pieces)
                    {
                        if (_piece.playerColor != playerColor && _piece.boardPosition == boardPosition + new Vector2Int(1, 1))
                        {
                            _baseValidMoves.Add(new Vector2Int(1, 1));
                        }
                    }
                }
                break;

            default:
                Debug.Log("Error: Tried to get valid moves for Piece " + name + ", but chessPieceType is undefined.");
                break;
        }

        validMoves.Clear();

        // Now that we have the normal valid movement for the chess pieces, apply it to this piece's position and add it to valid moves.
        foreach(Vector2Int v in _baseValidMoves)
        {
            Vector2Int _moveOnBoard = v + boardPosition;


            // Cull if it's out of bounds.
            if (_moveOnBoard.x < BoardXMin || _moveOnBoard.x > BoardXMax || _moveOnBoard.y < BoardYMin || _moveOnBoard.y > BoardYMax)
            {
                continue;
            }

            // Cull the last row for players.
            if (playerColor == PlayerColor.White && _moveOnBoard.y > BoardYPlayerMax)
            {
                continue;
            }

            // Cull it if the space is already occupied by a friendly piece.
            bool _cullMe = false;
            foreach (Piece _piece in gameController.pieces)
            {
                if (_piece.playerColor == playerColor && _piece.boardPosition == _moveOnBoard) {
                    _cullMe = true;
                    break;
                }
            }
            if (_cullMe)
            {
                continue;
            }

            validMoves.Add(_moveOnBoard);
        }
    }

    private bool PotentialMoveIsObstructed(Vector2Int _potentialMove)
    {
        foreach (Piece _piece in gameController.pieces)
        {
            if (_piece.boardPosition == boardPosition + _potentialMove)
            {
                return true;
            }
        }
        return false;
    }

    private void Highlight()
    {
        highlighted = true;
        glow.SetActive(true);
        foreach(Vector2Int v in validMoves)
        {
            if (playerColor == PlayerColor.White)
            {
                highlightTilemap.HighlightFriendlyTile(v);
            }
            else
            {
                highlightTilemap.HighlightEnemyTile(v);
            }
        }
    }

    private void Unhighlight()
    {
        highlighted = false;
        glow.SetActive(false);
        highlightTilemap.ClearAllTiles();
    }

    public Vector2Int GetMouseHoveredTile()
    {
        Vector3 _mouseScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 _mousePosition3D = Camera.main.ScreenToWorldPoint(_mouseScreenPoint);
        Vector2 _mousePosition = new Vector2(_mousePosition3D.x - boardOriginForMouse.x, boardOriginForMouse.y - _mousePosition3D.y);
        GridLayout _gridLayout = highlightTilemap.transform.parent.GetComponentInParent<GridLayout>();
        Vector3Int _cellPosition = _gridLayout.WorldToCell(_mousePosition);
        return new Vector2Int(_cellPosition.x, _cellPosition.y);
    }
}
