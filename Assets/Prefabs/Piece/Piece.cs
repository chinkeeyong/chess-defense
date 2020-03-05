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

    public AudioClip moveSound;


    // Runtime variables

    [HideInInspector] public Color glowColor = new Color();
    [HideInInspector] public bool dragging = false;
    bool highlighted = false;
    [HideInInspector] public bool alwaysHighlight = false;
    bool hasMoved = false;
    [HideInInspector] public bool canBeCapturedEnPassant = false;
    [HideInInspector] public List<Vector2Int> validMoves = new List<Vector2Int>();
    float animationPercentage = 0f;
    bool animatingMovementToAIPreferredMove = false;

    // Used to store info for AI
    Vector2Int aiPreferredMove = new Vector2Int(-1, -1);
    public bool hasBeenMovedThisTurn = false;

    // Static values

    Vector2 boardOrigin = new Vector2(0.24f, 1.58f);
    Vector2 boardOriginForMouse = new Vector2(0.24f, 1.62f);
    Vector2 displacementPerX = new Vector2(0.24f, -0.18f);
    Vector2 displacementPerY = new Vector2(-0.24f, -0.18f);
    [HideInInspector] public Vector3 mouseDragOffset = new Vector3(0f, 0.1f);
    int BoardXMin = 0;
    int BoardXMax = 7;
    int BoardYMin = 1;
    int BoardYMax = 9;
    int BoardYPlayerMax = 8;
    float zDepthPerY = 0.01f;
    [HideInInspector] public float glowPulseSpeed = 5f;
    float pieceMovementSpeed = 2f;
    float knightAnimationLerpSpeed = 3f;
    float knightJumpAnimationMagnitude = 0.5f;


    // Resources

    [HideInInspector] public GameObject shadow;
    [HideInInspector] public GameObject glow;
    SpriteRenderer glowSpriteRenderer;
    [HideInInspector] public GameObject body;
    AudioSource audioSource;
    [HideInInspector] public GameController gameController;
    [HideInInspector] public HighlightTilemap highlightTilemap;
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
    public void Awake()
    {
        // Initialize all local object references.
        shadow = transform.GetChild(0).gameObject;
        glow = transform.GetChild(1).gameObject;
        glowSpriteRenderer = glow.GetComponent<SpriteRenderer>();
        body = transform.GetChild(2).gameObject;
        audioSource = transform.GetChild(3).GetComponent<AudioSource>();

        // Initialize all world object references.
        gameController = GameObject.Find("/GameController").GetComponent<GameController>();
        highlightTilemap = GameObject.Find("/Grid/Highlight Tilemap").GetComponent<HighlightTilemap>();

        // Not glowing initially
        glow.SetActive(false);
    }


    // Called from GameController.
    public void Initialize()
    {

        // Initialize this chess piece's type.
        SetChessPieceType(chessPieceType);

        // Add this chess piece to the global array.
        gameController.pieces.Add(this);
    }


    // Called just before first frame.
    public void Start()
    {
        SetValidMoves();
        SnapToBoardPosition();
    }


    // Update is called once per frame
    public void Update()
    {
        // Move
        if (animatingMovementToAIPreferredMove)
        {
            AnimateMovement();
        }

        // Make glow glowy
        if (glow.activeInHierarchy)
        {
            float glowMagnitude = 0.5f + (Mathf.Abs(Mathf.Sin(Time.unscaledTime * glowPulseSpeed)) * 0.5f);
            glowSpriteRenderer.color = new Color(glowColor.r * glowMagnitude, glowColor.g * glowMagnitude, glowColor.b * glowMagnitude);
        }
    }

    
    public void OnMouseEnter()
    {
        Highlight();
    }

    
    public void OnMouseExit()
    {
        Unhighlight();
    }

    
    public void OnMouseUp()
    {
        if (dragging)
        {
            gameController.playerIsDraggingSomething = false;
            dragging = false;
            shadow.SetActive(true);
            
            // Move the mouse to the new tile if it's a valid move.
            Vector2Int _hoveredTile = GetMouseHoveredBoardPosition();
            if (validMoves.Contains(_hoveredTile))
            {
                MoveToBoardPosition(_hoveredTile);
                audioSource.PlayOneShot(moveSound);
                gameController.gamePhase = GameController.GamePhase.PLAYER_END_TURN;
            }
            else
            {
                SnapToBoardPosition();
            }
        }
    }

    
    public void OnMouseDown()
    {
        if (gameController.gamePhase == GameController.GamePhase.PLAYER_TO_MOVE &&
            !gameController.playerIsDraggingSomething &&
            playerColor == PlayerColor.White)
        {
            gameController.playerIsDraggingSomething = true;
            dragging = true;
            shadow.SetActive(false);
        }
    }
    

    public void OnMouseDrag()
    {
        if (dragging)
        {
            Vector3 mouseScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(mouseScreenPoint) + mouseDragOffset;
            mousePosition.z = -2f;
            transform.position = mousePosition;
        }
    }

    
    public void SetChessPieceType(ChessPieceType t)
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
                break;

            case PlayerColor.Black:
                glowColor = Color.red;
                break;

            default:
                Debug.Log("Error: Tried to set Piece " + name + " to an undefined playerColor.");
                break;
        }
    }


    public Vector3 BoardPositionToWorldSpace(Vector2Int _position)
    {
        Vector3 _v = boardOrigin + (_position.x * displacementPerX) + (_position.y * displacementPerY);
        return new Vector3(_v.x, _v.y, _v.y * zDepthPerY);
    }

    
    public void SnapToBoardPosition()
    {
        transform.position = BoardPositionToWorldSpace(boardPosition);
    }


    public void AnimateMovement()
    {
        if (chessPieceType == ChessPieceType.Knight)
        {
            animationPercentage += knightAnimationLerpSpeed * Time.deltaTime;
            if (animationPercentage < 1f)
            {
                float _knightJumpHeight = Mathf.Sin(animationPercentage * Mathf.PI) * knightJumpAnimationMagnitude;
                transform.position = Vector3.Lerp(BoardPositionToWorldSpace(boardPosition), BoardPositionToWorldSpace(aiPreferredMove), animationPercentage);
                transform.position += new Vector3(0f, _knightJumpHeight, 0f);
                shadow.transform.localPosition = new Vector3(0f, -_knightJumpHeight, 0f);
            }
            else
            {
                MoveToBoardPosition(aiPreferredMove);
                shadow.transform.localPosition = Vector3.zero;
                animatingMovementToAIPreferredMove = false;
                gameController.animating = false;
                audioSource.PlayOneShot(moveSound);
            }
        }
        else
        {
            Vector3 _movementVector = Vector3.Normalize(BoardPositionToWorldSpace(aiPreferredMove) - BoardPositionToWorldSpace(boardPosition)) * pieceMovementSpeed * Time.deltaTime;
            float _distanceToTarget = (BoardPositionToWorldSpace(aiPreferredMove) - transform.position).magnitude;
            if (_movementVector.magnitude < _distanceToTarget)
            {
                transform.position += _movementVector;
            }
            else
            {
                MoveToBoardPosition(aiPreferredMove);
                animatingMovementToAIPreferredMove = false;
                gameController.animating = false;
                audioSource.PlayOneShot(moveSound);
            }
        }
    }


    public List<Vector2Int> GetValidMoves(Vector2Int _boardPosition)
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

                // Castling
                if (!hasMoved)
                {
                    // Let us first assume that castling long (to queenside rook) is possible.
                    bool _canCastleLong = true;

                    // Queenside Rook must be in the correct position to castle long
                    Piece _queensideRook = null;
                    _queensideRook = gameController.pieces.Find(p =>
                        p.playerColor == playerColor &&
                        p.chessPieceType == ChessPieceType.Rook &&
                        !p.hasMoved &&
                        p.boardPosition == new Vector2Int(0, 8)
                    );

                    if (_queensideRook != null)
                    {
                        // There must be no intervening pieces.
                        foreach (Piece _piece in gameController.pieces)
                        {
                            if (_piece.boardPosition.y == boardPosition.y &&
                                _piece.boardPosition.x > _queensideRook.boardPosition.x &&
                                _piece.boardPosition.x < boardPosition.x)
                            {
                                _canCastleLong = false;
                                break;
                            }
                        }

                        // Can't castle out of, through, or into check.
                        foreach (Piece _piece in gameController.pieces)
                        {
                            if (_piece.playerColor != playerColor)
                            {
                                foreach (Vector2Int _move in _piece.validMoves)
                                {
                                    if (_move.y == boardPosition.y &&
                                        _move.x >= _queensideRook.boardPosition.x &&
                                        _move.x <= boardPosition.x)
                                    {
                                        _canCastleLong = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (_canCastleLong)
                        {
                            _baseValidMoves.Add(new Vector2Int(-2, 0));
                        }
                    }

                    // Let us first assume that castling short (to kingside rook) is possible.
                    bool _canCastleShort = true;

                    // Kingside Rook must be in the correct position to castle long
                    Piece _kingsideRook = null;
                    _kingsideRook = gameController.pieces.Find(p =>
                        p.playerColor == playerColor &&
                        p.chessPieceType == ChessPieceType.Rook &&
                        !p.hasMoved &&
                        p.boardPosition == new Vector2Int(7, 8)
                    );

                    if (_kingsideRook != null)
                    {
                        // There must be no intervening pieces.
                        foreach (Piece _piece in gameController.pieces)
                        {
                            if (_piece.boardPosition.y == boardPosition.y &&
                                _piece.boardPosition.x < _kingsideRook.boardPosition.x &&
                                _piece.boardPosition.x > boardPosition.x)
                            {
                                _canCastleShort = false;
                                break;
                            }
                        }

                        // Can't castle out of, through, or into check.
                        foreach (Piece _piece in gameController.pieces)
                        {
                            if (_piece.playerColor != playerColor)
                            {
                                foreach (Vector2Int _move in _piece.validMoves)
                                {
                                    if (_move.y == boardPosition.y &&
                                        _move.x <= _kingsideRook.boardPosition.x &&
                                        _move.x >= boardPosition.x)
                                    {
                                        _canCastleShort = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (_canCastleShort)
                        {
                            _baseValidMoves.Add(new Vector2Int(2, 0));
                        }
                    }
                }
                break;

            case ChessPieceType.Queen: // Queens can move up to 7 squares in any direction or 9 squares vertically, obstructed by pieces.
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 9; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 9; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                break;

            case ChessPieceType.Bishop: // Bishops can move up to 7 squares diagonally, obstructed by pieces.
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                break;

            case ChessPieceType.Rook: // Rooks can move up to 7 squares horizontally or 9 squares vertically, obstructed by pieces.
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 7; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(-i, 0);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 9; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
                }
                for (int i = 1; i <= 9; i++)
                {
                    Vector2Int _moveOnBoard = new Vector2Int(0, -i);
                    _baseValidMoves.Add(_moveOnBoard);
                    if (PotentialMoveIsObstructed(_boardPosition, _moveOnBoard)) { break; }
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
                    if (!PotentialMoveIsObstructed(_boardPosition, new Vector2Int(0, -1)))
                    {
                        _baseValidMoves.Add(new Vector2Int(0, -1));
                        if (!PotentialMoveIsObstructed(_boardPosition, new Vector2Int(0, -2)) && !hasMoved)
                        {
                            _baseValidMoves.Add(new Vector2Int(0, -2)); // Double move on first move.
                        }
                    }

                    // Add potential captures.
                    foreach (Piece _piece in gameController.pieces)
                    {
                        if (_piece.playerColor != playerColor)
                        {
                            if (
                                _piece.boardPosition == _boardPosition + new Vector2Int(-1, -1) || // Regular pawn capture
                                 (_piece.canBeCapturedEnPassant && _piece.boardPosition == _boardPosition + new Vector2Int(-1, 0))    // En Passant capture
                               )
                            {
                                _baseValidMoves.Add(new Vector2Int(-1, -1));
                            }
                            if (
                                _piece.boardPosition == _boardPosition + new Vector2Int(1, -1) || // Regular pawn capture
                                 (_piece.canBeCapturedEnPassant && _piece.boardPosition == _boardPosition + new Vector2Int(1, 0))    // En Passant capture
                               )
                            {
                                _baseValidMoves.Add(new Vector2Int(1, -1));
                            }
                        }
                    }
                }
                else // Pawn is black
                {
                    if (!PotentialMoveIsObstructed(_boardPosition, new Vector2Int(0, 1)))
                    {
                        _baseValidMoves.Add(new Vector2Int(0, 1));
                        if (!PotentialMoveIsObstructed(_boardPosition, new Vector2Int(0, 2)) && !hasMoved)
                        {
                            _baseValidMoves.Add(new Vector2Int(0, 2)); // Double move on first move.
                        }
                    }

                    // Add potential captures.
                    foreach (Piece _piece in gameController.pieces)
                    {
                        if (_piece.playerColor != playerColor)
                        {
                            if (
                                _piece.boardPosition == _boardPosition + new Vector2Int(-1, 1) || // Regular pawn capture
                                 (_piece.canBeCapturedEnPassant && _piece.boardPosition == _boardPosition + new Vector2Int(-1, 0))    // En Passant capture
                               )
                            {
                                _baseValidMoves.Add(new Vector2Int(-1, 1));
                            }
                            if (
                                _piece.boardPosition == _boardPosition + new Vector2Int(1, 1) || // Regular pawn capture
                                 (_piece.canBeCapturedEnPassant && _piece.boardPosition == _boardPosition + new Vector2Int(1, 0))    // En Passant capture
                               )
                            {
                                _baseValidMoves.Add(new Vector2Int(1, 1));
                            }
                        }
                    }
                }
                break;

            default:
                Debug.Log("Error: Tried to get valid moves for Piece " + name + ", but chessPieceType is undefined.");
                break;
        }

        List<Vector2Int> _validMoves = new List<Vector2Int>();

        // Now that we have the normal valid movement for the chess pieces, apply it to this piece's position and add it to valid moves.
        foreach(Vector2Int v in _baseValidMoves)
        {
            Vector2Int _moveOnBoard = v + boardPosition;


            // Cull if it's out of bounds.
            if (PotentialMoveIsOutOfBounds(_moveOnBoard))
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

            // Cull it if this is a King and the move would put King in check.
            if (chessPieceType == ChessPieceType.King)
            {
                foreach (Piece _piece in gameController.pieces)
                {
                    if (_piece.playerColor != playerColor && _piece.playerColor == PlayerColor.Black) // No plans to add black kings for now
                    {
                        if (_piece.chessPieceType == ChessPieceType.Pawn)
                        {
                            if (_moveOnBoard == _piece.boardPosition + new Vector2Int(1, 1) || _moveOnBoard == _piece.boardPosition + new Vector2Int(-1, 1))
                            {
                                _cullMe = true;
                                break;
                            }
                        }
                        else
                        {
                            foreach (Vector2Int _move in _piece.validMoves)
                            {
                                if (_move == _moveOnBoard)
                                {
                                    _cullMe = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (_cullMe)
            {
                continue;
            }

            _validMoves.Add(_moveOnBoard);
        }

        return _validMoves;
    }

    
    public void SetValidMoves()
    {
        validMoves = GetValidMoves(boardPosition);
    }


    public bool PotentialMoveIsObstructed(Vector2Int _position, Vector2Int _potentialMove)
    {
        foreach (Piece _piece in gameController.pieces)
        {
            if (_piece.boardPosition == _position + _potentialMove)
            {
                return true;
            }
        }
        return false;
    }


    public bool PotentialMoveIsOutOfBounds(Vector2Int _potentialMove)
    {
        return _potentialMove.x < BoardXMin || _potentialMove.x > BoardXMax || _potentialMove.y < BoardYMin || _potentialMove.y > BoardYMax;
    }

    
    public int GetPieceAICaptureScore(ChessPieceType _chessPieceType)
    {
        switch(_chessPieceType)
        {
            case ChessPieceType.King:
                return 10000;

            case ChessPieceType.Queen:
                return 1000;

            case ChessPieceType.Rook:
                return 500;

            case ChessPieceType.Bishop:
                return 300;

            case ChessPieceType.Knight:
                return 300;

            case ChessPieceType.Pawn:
                return 100;

            default:
                return 0;
        }
    }


    public int ScoreFromPotentialMoveCapturePiece(Vector2Int _potentialMove)
    {
        foreach (Piece _piece in gameController.pieces)
        {
            if (_piece.playerColor != playerColor)
            {
                if (_piece.boardPosition == _potentialMove)
                {
                    return GetPieceAICaptureScore(_piece.chessPieceType);
                }

                // En Passant
                if (_piece.canBeCapturedEnPassant && chessPieceType == ChessPieceType.Pawn)
                {
                    if (playerColor == PlayerColor.White && _piece.boardPosition == _potentialMove + new Vector2Int(0, 1))
                    {
                        return GetPieceAICaptureScore(_piece.chessPieceType);
                    }
                    if (playerColor == PlayerColor.Black && _piece.boardPosition == _potentialMove + new Vector2Int(0, -1))
                    {
                        return GetPieceAICaptureScore(_piece.chessPieceType);
                    }
                }
            }
        }
        return 0;
    }


    public bool PotentialMoveWillCaptureWhiteKing(Vector2Int _potentialMove)
    {
        Piece _whiteKing = null;
        _whiteKing = gameController.pieces.Find(p => p.chessPieceType == ChessPieceType.King && p.playerColor == PlayerColor.White);

        if (_whiteKing != null)
        {
            if (_whiteKing.boardPosition == _potentialMove)
            {
                return true;
            }
        }
        return false;
    }


    public bool PotentialMoveWillThreatenWhiteKing(Vector2Int _potentialMove)
    {
        Piece _whiteKing = null;
        _whiteKing = gameController.pieces.Find(p => p.chessPieceType == ChessPieceType.King && p.playerColor == PlayerColor.White);

        if (_whiteKing != null)
        {
            List<Vector2Int> _validMovesAfterPotentialMove = GetValidMoves(_potentialMove);

            foreach (Vector2Int _futureMove in _validMovesAfterPotentialMove)
            {
                if (_futureMove == _whiteKing.boardPosition)
                {
                    return true;
                }
            }
        }
        return false;
    }


    public void CalcAIPreferredMove()
    {
        // This function assigns a score to each valid move and then puts the best scoring move in aiPreferredMove.

        // First, return null if there are no valid moves.
        if (validMoves.Count < 1)
        {
            aiPreferredMove = new Vector2Int(-1, -1);
            return;
        }

        // There are valid moves? Great. First we create a list
        List<int> _moveScores = new List<int>();

        // And an int to keep track of the highest score.
        int _highestMoveScore = 0;

        // Now run through all valid moves.
        foreach (Vector2Int _move in validMoves)
        {
            // Base score: 0.
            int _evaluatedScore = 0;

            // +10000 Score if move will overrun player.
            if (_move.y >= 9)
            {
                _evaluatedScore += 10000;
            }

            // +1000 Score if white king will be put in check.
            if (PotentialMoveWillThreatenWhiteKing(_move))
            {
                _evaluatedScore += 1000;
            }

            // Add score based on any captured piece.
            _evaluatedScore += ScoreFromPotentialMoveCapturePiece(_move);

            // +10 Score per square ahead on the board it is.
            _evaluatedScore += _move.y * 10;

            // Finally, add the score to the list
            _moveScores.Add(_evaluatedScore);

            // Increase highest move score if this score is highest
            if (_evaluatedScore > _highestMoveScore)
            {
                _highestMoveScore = _evaluatedScore;
            }
        }

        // Now that we have our list of valid moves, let's make a new list only including moves that are the highest move score.
        List<Vector2Int> _movesWithHighestMoveScore = new List<Vector2Int>();
        for (int i = 0; i < validMoves.Count; i++)
        {
            if (_moveScores[i] == _highestMoveScore)
            {
                _movesWithHighestMoveScore.Add(validMoves[i]);
            }
        }

        // The final preferred move is the move with the highest score, or the one randomly determined from among moves tied for highest move score.
        int _chosenMoveIndex = Random.Range(0,_movesWithHighestMoveScore.Count - 1);
        aiPreferredMove = _movesWithHighestMoveScore[_chosenMoveIndex];
    }


    public void ExecuteAIMove()
    {
        if (aiPreferredMove != new Vector2Int(-1, -1))
        {
            animatingMovementToAIPreferredMove = true;
            animationPercentage = 0f;
            gameController.animating = true;
        }
    }


    public void MoveToBoardPosition(Vector2Int _move)
    {
        if (chessPieceType == ChessPieceType.Pawn && Mathf.Abs((_move - boardPosition).magnitude) == 2)
        {
            canBeCapturedEnPassant = true;
        }

        // Castling
        if (chessPieceType == ChessPieceType.King)
        {
            // Castling long
            if (_move - boardPosition == new Vector2Int(-2, 0))
            {
                Piece _queensideRook = null;
                _queensideRook = gameController.pieces.Find(p =>
                    p.playerColor == playerColor &&
                    p.chessPieceType == ChessPieceType.Rook &&
                    !p.hasMoved &&
                    p.boardPosition == new Vector2Int(0, 8)
                );

                if (_queensideRook != null)
                {
                    _queensideRook.aiPreferredMove = new Vector2Int(3, boardPosition.y);
                    _queensideRook.ExecuteAIMove();
                }
            }

            // Castling short
            if (_move - boardPosition == new Vector2Int(2, 0))
            {
                Piece _kingsideRook = null;
                _kingsideRook = gameController.pieces.Find(p =>
                    p.playerColor == playerColor &&
                    p.chessPieceType == ChessPieceType.Rook &&
                    !p.hasMoved &&
                    p.boardPosition == new Vector2Int(7, 8)
                );

                if (_kingsideRook != null)
                {
                    _kingsideRook.aiPreferredMove = new Vector2Int(5, boardPosition.y);
                    _kingsideRook.ExecuteAIMove();
                }
            }
        }

        boardPosition = _move;
        hasMoved = true;

        // Capture enemy pieces
        Piece _capturedPieceIfAny = null;
        foreach (Piece _piece in gameController.pieces)
        {
            if (_piece.playerColor != playerColor && _piece.boardPosition == boardPosition)
            {
                _capturedPieceIfAny = _piece;
                break;
            }

            // En Passant capture
            if (chessPieceType == ChessPieceType.Pawn && _piece.canBeCapturedEnPassant)
            {
                if (playerColor == PlayerColor.White && _piece.boardPosition == boardPosition + new Vector2Int(0, 1))
                {
                    _capturedPieceIfAny = _piece;
                    break;
                }
                if (playerColor == PlayerColor.Black && _piece.boardPosition == boardPosition + new Vector2Int(0, -1))
                {
                    _capturedPieceIfAny = _piece;
                    break;
                }
            }
        }

        if (_capturedPieceIfAny != null)
        {
            gameController.Capture(_capturedPieceIfAny);
        }

        // Now refresh valid moves of all pieces
        gameController.SetAllValidMoves();

        // And update UI
        if (highlighted)
        {
            Unhighlight();
            Highlight();
        }

        SnapToBoardPosition();
    }


    public void Highlight()
    {
        if (!highlighted && !gameController.playerIsDraggingSomething)
        {
            highlighted = true;
            glow.SetActive(true);

            // Highlight areas that black pawn can potentially capture
            if (playerColor == PlayerColor.Black && chessPieceType == ChessPieceType.Pawn)
            {
                Vector2Int _forwardAndLeft = new Vector2Int(boardPosition.x - 1, boardPosition.y + 1);
                if (!PotentialMoveIsOutOfBounds(_forwardAndLeft))
                {
                    highlightTilemap.HighlightDangerTile(_forwardAndLeft);
                }

                Vector2Int _forwardAndRight = new Vector2Int(boardPosition.x + 1, boardPosition.y + 1);
                if (!PotentialMoveIsOutOfBounds(_forwardAndRight))
                {
                    highlightTilemap.HighlightDangerTile(_forwardAndRight);
                }
            }

            foreach (Vector2Int v in validMoves)
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
    }

    
    public void Unhighlight()
    {
        if (highlighted && !alwaysHighlight && !gameController.playerIsDraggingSomething)
        {
            highlighted = false;
            glow.SetActive(false);
            highlightTilemap.LoadTilesFromCache();
        }
    }

    
    public Vector2Int GetMouseHoveredBoardPosition()
    {
        Vector3 _mouseScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 _mousePosition3D = Camera.main.ScreenToWorldPoint(_mouseScreenPoint);
        Vector2 _mousePosition = new Vector2(_mousePosition3D.x - boardOriginForMouse.x, boardOriginForMouse.y - _mousePosition3D.y);
        GridLayout _gridLayout = highlightTilemap.transform.parent.GetComponentInParent<GridLayout>();
        Vector3Int _cellPosition = _gridLayout.WorldToCell(_mousePosition);
        return new Vector2Int(_cellPosition.x, _cellPosition.y);
    }
}
