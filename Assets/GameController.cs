using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public GameObject piecePrefab;
    public GameObject turnCounter;
    public GameObject scoreCounter;
    public GameObject lossCounter;
    public GameObject whiteToMoveBanner;
    public GameObject blackToMoveBanner;

    public List<Piece> pieces = new List<Piece>();

    public enum GamePhase { AI_EXECUTION, AI_SPAWNING, AI_END_TURN, PLAYER_START_TURN, PLAYER_TO_MOVE, PLAYER_END_TURN }
    public GamePhase gamePhase = GamePhase.AI_END_TURN;
    
    public bool animating = false;
    public int turns = 0;
    public int score = 0;
    public int losses = 0;

    public float animationTimer = 0f;
    static float bannerAnimationTime = 0.75f;
    static float pieceSpawnAnimationTime = 0.65f;


    static int pawnScoreValue = 1;
    static int knightScoreValue = 2;
    static int bishopScoreValue = 3;


    private void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

        whiteToMoveBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        whiteToMoveBanner.SetActive(true);

        blackToMoveBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        blackToMoveBanner.SetActive(true);
        
        gamePhase = GamePhase.AI_SPAWNING;
        animating = false;

        turns = 0;
        score = 0;
        losses = 0;

        InstantiateAllWhitePieces();

        updateCounters();
    }

    private void Update()
    {
        if (animating)
        {
            return;
        }
        if (animationTimer > 0f)
        {
            animationTimer -= Time.deltaTime;
            return;
        }

        switch (gamePhase)
        {
            case GamePhase.AI_EXECUTION:
                if (pieces.Find(p => !p.hasBeenMovedThisTurn && p.playerColor == Piece.PlayerColor.Black) != null) // If there is a piece that hasn't moved
                {
                    Piece _piece = pieces.Find(p => !p.hasBeenMovedThisTurn && p.playerColor == Piece.PlayerColor.Black);
                    _piece.CalcAIPreferredMove();
                    _piece.ExecuteAIMove();
                    _piece.hasBeenMovedThisTurn = true;
                }
                else
                {
                    foreach (Piece _piece in pieces)
                    {
                        _piece.hasBeenMovedThisTurn = false;
                    }
                    gamePhase = GamePhase.AI_SPAWNING;
                }
                break;

            case GamePhase.AI_SPAWNING:
                SpawnAllEnemyPieces();
                gamePhase = GamePhase.AI_END_TURN;
                break;

            case GamePhase.AI_END_TURN:
                turns++;
                updateTurnCounter();
                whiteToMoveBanner.GetComponent<Animation>().Play();
                animationTimer += bannerAnimationTime;
                gamePhase = GamePhase.PLAYER_START_TURN;
                break;

            case GamePhase.PLAYER_START_TURN:
                foreach (Piece _piece in pieces)
                {
                    if (_piece.playerColor == Piece.PlayerColor.White)
                    {
                        _piece.canBeCapturedEnPassant = false;
                    }
                }
                gamePhase = GamePhase.PLAYER_TO_MOVE;
                break;

            case GamePhase.PLAYER_END_TURN:
                blackToMoveBanner.GetComponent<Animation>().Play();
                animationTimer += bannerAnimationTime;
                foreach (Piece _piece in pieces)
                {
                    if (_piece.playerColor == Piece.PlayerColor.Black)
                    {
                        _piece.canBeCapturedEnPassant = false;
                    }
                }
                gamePhase = GamePhase.AI_EXECUTION;
                break;

            default:
                break;
        }
    }


    // Instantiate a piece of the given type and color at the given location
    public Piece InstantiatePiece(Piece.PlayerColor _playerColor, Piece.ChessPieceType _chessPieceType, int _x, int _y)
    {
        GameObject _newGameObject = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity);
        Piece _newPiece = _newGameObject.GetComponent<Piece>();
        _newPiece.playerColor = _playerColor;
        _newPiece.chessPieceType = _chessPieceType;
        _newPiece.boardPosition = new Vector2Int(_x, _y);
        _newPiece.Initialize();
        return _newPiece;
    }


    // Destroy a piece and remove it from the piece array.
    public void Capture(Piece _capturedPiece)
    {
        if (_capturedPiece.playerColor == Piece.PlayerColor.White)
        {
            losses++;
            updateLossCounter();
        }
        else
        {
            switch(_capturedPiece.chessPieceType)
            {
                case Piece.ChessPieceType.Pawn:
                    score += pawnScoreValue;
                    break;

                case Piece.ChessPieceType.Knight:
                    score += knightScoreValue;
                    break;

                case Piece.ChessPieceType.Bishop:
                    score += bishopScoreValue;
                    break;

                default:
                    break;
            }
            updateScoreCounter();
        }
        pieces.Remove(_capturedPiece);
        Destroy(_capturedPiece.gameObject);

    }

    // Instantiate all white pieces
    private void InstantiateAllWhitePieces()
    {
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Rook, 0, 8);
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Knight, 1, 8);
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Bishop, 2, 8);
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Queen, 3, 8);
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.King, 4, 8);
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Bishop, 5, 8);
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Knight, 6, 8);
        InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Rook, 7, 8);
        for (int i = 0; i < 8; i++)
        {
            InstantiatePiece(Piece.PlayerColor.White, Piece.ChessPieceType.Pawn, i, 7);
        }
    }


    // Enemy Spawner, spawns black pieces based on turn count
    private void SpawnAllEnemyPieces()
    {
        if (turns < 8)
        {
            if (turns % 2 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Pawn);
            }
        }
        else if (turns < 18)
        {
            if (turns % 4 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Knight);
            }
            if (turns % 2 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Pawn);
            }
        }
        else if (turns < 30)
        {
            if (turns % 6 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Bishop);
            }
            else if (turns % 3 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Knight);
            }
            if (turns % 2 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Pawn);
            }
        }
        else if (turns < 40)
        {
            if (turns % 5 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Rook);
            }
            else if (turns % 6 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Bishop);
            }
            else if (turns % 3 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Knight);
            }
            if (turns % 2 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Pawn);
            }
        }
        else
        {
            if (turns % 10 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Queen);
            }
            if (turns % 5 == 1)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Rook);
            }
            if (turns % 5 == 3)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Bishop);
            }
            if (turns % 3 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Knight);
            }
            if (turns % 2 == 0)
            {
                SpawnEnemyPiece(Piece.ChessPieceType.Pawn);
            }
        }
    }

    private void SpawnEnemyPiece(Piece.ChessPieceType _chessPieceType)
    {
        // First make sure there are valid spaces to spawn in.
        int[] _input = { 0, 1, 2, 3, 4, 5, 6, 7 };
        List<int> _validXCoordinates = new List<int>(_input);
        foreach (Piece _piece in pieces)
        {
            if (_piece.boardPosition.y == 0)
            {
                _validXCoordinates.Remove(_piece.boardPosition.x);
            }
        }

        if (_validXCoordinates.Count < 1)
        {
            Debug.Log("Tried to spawn a new enemy piece but there was no space.");
            return;
        }

        int _spawnedX = _validXCoordinates[Random.Range(0, _validXCoordinates.Count - 1)];

        Piece _newPiece = null;

        _newPiece = InstantiatePiece(Piece.PlayerColor.Black, _chessPieceType, _spawnedX, 0);

        if (_newPiece != null)
        {
            _newPiece.GetComponent<Animation>().Play("SpawnAnimation");
            animationTimer += pieceSpawnAnimationTime;
        }
    }


    public void SetAllValidMoves()
    {
        foreach (Piece _piece in pieces)
        {
            if (_piece.chessPieceType != Piece.ChessPieceType.King)
            {
                _piece.SetValidMoves();
            }
        }

        // Kings get set after all other pieces because we need to know whether we're moving into check or through check for castling
        foreach (Piece _piece in pieces)
        {
            if (_piece.chessPieceType == Piece.ChessPieceType.King)
            {
                _piece.SetValidMoves();
            }
        }
    }
    

    private void updateCounters()
    {
        updateTurnCounter();
        updateScoreCounter();
        updateLossCounter();
    }

    private void updateTurnCounter()
    {
        turnCounter.GetComponentInChildren<Text>().text = "Turn: " + turns;
    }

    private void updateScoreCounter()
    {
        scoreCounter.GetComponentInChildren<Text>().text = "Score: " + score;
    }

    private void updateLossCounter()
    {
        lossCounter.GetComponentInChildren<Text>().text = "Pieces Lost: " + losses;
    }
}
