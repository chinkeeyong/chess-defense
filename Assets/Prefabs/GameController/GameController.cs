using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public GameObject piecePrefab;
    public GameObject turnCounter;
    public GameObject scoreCounter;
    public GameObject dinarCounter;
    public GameObject lossCounter;
    public GameObject whiteToMoveBanner;
    public GameObject blackToMoveBanner;
    public GameObject checkBanner;
    public GameObject bewareBanner;
    public GameObject checkmateBanner;
    public GameObject overrunBanner;

    public AudioClip bannerSound;
    public AudioClip dangerSound;
    public AudioClip dinarSound;

    public List<BuyablePiece> buyablePieces;

    List<Piece> piecesThatCanOverrunPlayer = new List<Piece>();
    List<Piece> piecesThreateningWhiteKing = new List<Piece>();

    AudioSource audioSource;
    HighlightTilemap highlightTilemap;

    [HideInInspector] public List<Piece> pieces = new List<Piece>();

    public enum GamePhase {
        AI_START_TURN,
        AI_EXECUTION,
        AI_SPAWNING,
        AI_CHECK_FOR_PLAYER_LOSS,
        AI_CHECK_FOR_CHECK,
        AI_CHECK_FOR_BEWARE,
        AI_END_TURN,
        PLAYER_START_TURN,
        PLAYER_TO_MOVE,
        PLAYER_JUST_BOUGHT_PIECE,
        PLAYER_PIECE_SPAWNING,
        PLAYER_END_TURN,
        GAME_OVER
    }

    [HideInInspector] public GamePhase gamePhase = GamePhase.AI_END_TURN;

    [HideInInspector] public bool playerIsInCheck = false;
    [HideInInspector] public bool animating = false;
    [HideInInspector] public bool playerIsDraggingSomething = false;
    [HideInInspector] public int turns = 0;
    [HideInInspector] public int score = 0;
    [HideInInspector] public int dinars = 0;
    public int startingDinars = 0;
    [HideInInspector] public int losses = 0;

    [HideInInspector] public float animationTimer = 0f;
    [HideInInspector] public float bannerAnimationTime = 0.75f;
    [HideInInspector] public float endBannerAnimationTime = 1f;
    [HideInInspector] public float pieceSpawnAnimationTime = 0.65f;
    [HideInInspector] public float delayAfterEndTurn = 0.2f;

    [HideInInspector] public int pawnScoreValue = 1;
    [HideInInspector] public int knightScoreValue = 2;
    [HideInInspector] public int bishopScoreValue = 3;
    [HideInInspector] public int rookScoreValue = 4;
    [HideInInspector] public int queenScoreValue = 5;
    


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        highlightTilemap = GameObject.Find("/Grid/Highlight Tilemap").GetComponent<HighlightTilemap>();
    }

    private void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

        whiteToMoveBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        whiteToMoveBanner.SetActive(true);

        blackToMoveBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        blackToMoveBanner.SetActive(true);

        checkBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        checkBanner.SetActive(true);

        bewareBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        bewareBanner.SetActive(true);

        checkmateBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        checkmateBanner.SetActive(true);

        overrunBanner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        overrunBanner.SetActive(true);

        gamePhase = GamePhase.AI_SPAWNING;
        animating = false;

        turns = 0;
        score = 0;
        dinars = startingDinars;
        losses = 0;

        InstantiateAllWhitePieces();

        UpdateCounters();
        UpdatePieceShopBuyability();
        UpdatePieceShopPossibleSpawnPositions();
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
            case GamePhase.AI_START_TURN:
                PlayBannerAnimation(blackToMoveBanner, bannerSound);
                gamePhase = GamePhase.AI_EXECUTION;
                break;

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
                    animationTimer = delayAfterEndTurn;
                    gamePhase = GamePhase.AI_SPAWNING;
                }
                break;

            case GamePhase.AI_SPAWNING:
                SpawnAllEnemyPieces();
                gamePhase = GamePhase.AI_CHECK_FOR_PLAYER_LOSS;
                break;

            case GamePhase.AI_CHECK_FOR_PLAYER_LOSS:
                if (IsWhiteKingDead())
                {
                    PlayEndBannerAnimation(checkmateBanner, dangerSound);
                    gamePhase = GamePhase.GAME_OVER;
                    break;
                }
                if (IsPlayerOverrun())
                {
                    PlayEndBannerAnimation(overrunBanner, dangerSound);
                    gamePhase = GamePhase.GAME_OVER;
                    break;
                }
                highlightTilemap.ClearAllTiles();
                gamePhase = GamePhase.AI_CHECK_FOR_CHECK;
                break;

            case GamePhase.AI_CHECK_FOR_CHECK:
                piecesThreateningWhiteKing = GetPiecesThreateningWhiteKing();
                if (piecesThreateningWhiteKing.Count > 0)
                {
                    playerIsInCheck = true;
                    foreach(Piece _piece in piecesThreateningWhiteKing)
                    {
                        _piece.Highlight();
                        _piece.alwaysHighlight = true;
                    }
                    highlightTilemap.SaveTilesToCache();
                    PlayBannerAnimation(checkBanner, dangerSound);
                }
                gamePhase = GamePhase.AI_CHECK_FOR_BEWARE;
                break;

            case GamePhase.AI_CHECK_FOR_BEWARE:
                piecesThatCanOverrunPlayer = GetPiecesThatCanOverrunPlayer();
                if (piecesThatCanOverrunPlayer.Count > 0)
                {
                    foreach (Piece _piece in piecesThatCanOverrunPlayer)
                    {
                        _piece.Highlight();
                        _piece.alwaysHighlight = true;
                    }
                    highlightTilemap.SaveTilesToCache();
                    PlayBannerAnimation(bewareBanner, dangerSound);
                }
                gamePhase = GamePhase.AI_END_TURN;
                break;

            case GamePhase.AI_END_TURN:
                turns++;
                UpdateTurnCounter();
                PlayBannerAnimation(whiteToMoveBanner, bannerSound);
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
                UpdatePieceShopPossibleSpawnPositions();
                gamePhase = GamePhase.PLAYER_TO_MOVE;
                break;

            case GamePhase.PLAYER_JUST_BOUGHT_PIECE:
                SetAllValidMoves();
                UpdateDinarCounter();
                UpdatePieceShopBuyability();
                UpdatePieceShopPossibleSpawnPositions();
                foreach (Piece _piece in pieces)
                {
                    _piece.alwaysHighlight = false;
                    _piece.Unhighlight();
                }
                highlightTilemap.ClearAllTiles();
                audioSource.PlayOneShot(dinarSound);
                piecesThreateningWhiteKing = GetPiecesThreateningWhiteKing();
                if (piecesThreateningWhiteKing.Count > 0)
                {
                    playerIsInCheck = true;
                    foreach (Piece _piece in piecesThreateningWhiteKing)
                    {
                        _piece.Highlight();
                        _piece.alwaysHighlight = true;
                    }
                    highlightTilemap.SaveTilesToCache();
                }
                piecesThatCanOverrunPlayer = GetPiecesThatCanOverrunPlayer();
                if (piecesThatCanOverrunPlayer.Count > 0)
                {
                    foreach (Piece _piece in piecesThatCanOverrunPlayer)
                    {
                        _piece.Highlight();
                        _piece.alwaysHighlight = true;
                    }
                    highlightTilemap.SaveTilesToCache();
                }
                animationTimer = pieceSpawnAnimationTime;
                gamePhase = GamePhase.PLAYER_PIECE_SPAWNING;
                break;

            case GamePhase.PLAYER_PIECE_SPAWNING:
                gamePhase = GamePhase.PLAYER_TO_MOVE;
                break;

            case GamePhase.PLAYER_END_TURN:
                foreach (Piece _piece in pieces)
                {
                    _piece.alwaysHighlight = false;
                    _piece.Unhighlight();
                    if (_piece.playerColor == Piece.PlayerColor.Black)
                    {
                        _piece.canBeCapturedEnPassant = false;
                    }
                }
                highlightTilemap.ClearAllTiles();
                playerIsInCheck = false;
                UpdatePieceShopBuyability();
                animationTimer = delayAfterEndTurn;
                gamePhase = GamePhase.AI_START_TURN;
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
            UpdateLossCounter();
        }
        else
        {
            switch(_capturedPiece.chessPieceType)
            {
                case Piece.ChessPieceType.Pawn:
                    score += pawnScoreValue;
                    dinars += pawnScoreValue;
                    break;

                case Piece.ChessPieceType.Knight:
                    score += knightScoreValue;
                    dinars += knightScoreValue;
                    break;

                case Piece.ChessPieceType.Bishop:
                    score += bishopScoreValue;
                    dinars += bishopScoreValue;
                    break;

                case Piece.ChessPieceType.Rook:
                    score += rookScoreValue;
                    dinars += rookScoreValue;
                    break;

                case Piece.ChessPieceType.Queen:
                    score += queenScoreValue;
                    dinars += queenScoreValue;
                    break;

                default:
                    break;
            }
            UpdateScoreCounter();
            UpdateDinarCounter();
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
            animationTimer = pieceSpawnAnimationTime;
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


    private bool IsWhiteKingDead()
    {
        Piece _whiteKing = null;
        _whiteKing = pieces.Find(p => p.chessPieceType == Piece.ChessPieceType.King && p.playerColor == Piece.PlayerColor.White);
        if (_whiteKing == null)
        {
            return true;
        }
        return false;
    }


    private List<Piece> GetPiecesThreateningWhiteKing()
    {
        Piece _whiteKing = pieces.Find(p => p.chessPieceType == Piece.ChessPieceType.King && p.playerColor == Piece.PlayerColor.White);

        List<Piece> _listOfPieces = new List<Piece>();
        
        foreach (Piece _piece in pieces)
        {
            if (_piece.playerColor == Piece.PlayerColor.Black)
            {
                foreach (Vector2Int _move in _piece.validMoves)
                {
                    if (_move == _whiteKing.boardPosition)
                    {
                        _listOfPieces.Add(_piece);
                        break;
                    }
                }
            }
        }
        return _listOfPieces;
    }


    private bool IsPlayerOverrun()
    {

        foreach (Piece _piece in pieces)
        {
            if (_piece.boardPosition.y >= 9)
            {
                if (_piece.playerColor == Piece.PlayerColor.Black)
                {
                    return true;
                }
            }
        }
        return false;
    }


    private List<Piece> GetPiecesThatCanOverrunPlayer()
    {
        List<Piece> _listOfPieces = new List<Piece>();

        foreach (Piece _piece in pieces)
        {
            if (_piece.playerColor == Piece.PlayerColor.Black)
            {
                foreach (Vector2Int _move in _piece.validMoves)
                {
                    if (_move.y >= 9)
                    {
                        _listOfPieces.Add(_piece);
                        break;
                    }
                }
            }
        }
        return _listOfPieces;
    }


    private void PlayBannerAnimation(GameObject _banner, AudioClip _sound)
    {
        _banner.GetComponent<Animation>().Play();
        audioSource.PlayOneShot(_sound, 0.6f);
        animationTimer += bannerAnimationTime;
    }


    private void PlayEndBannerAnimation(GameObject _banner, AudioClip _sound)
    {
        _banner.GetComponent<Animation>().Play();
        audioSource.PlayOneShot(_sound, 0.6f);
        animationTimer += endBannerAnimationTime;
    }


    private void UpdateCounters()
    {
        UpdateTurnCounter();
        UpdateScoreCounter();
        UpdateDinarCounter();
        UpdateLossCounter();
    }

    private void UpdateTurnCounter()
    {
        turnCounter.GetComponentInChildren<Text>().text = "Turn: " + turns;
    }

    private void UpdateScoreCounter()
    {
        scoreCounter.GetComponentInChildren<Text>().text = "Score: " + score;
    }

    private void UpdateDinarCounter()
    {
        dinarCounter.GetComponentInChildren<Text>().text = "Dinars: " + dinars;
    }

    private void UpdateLossCounter()
    {
        lossCounter.GetComponentInChildren<Text>().text = "Pieces Lost: " + losses;
    }

    public void UpdatePieceShopBuyability()
    {
        foreach (BuyablePiece _piece in buyablePieces)
        {
            _piece.UpdateBuyability();
        }
    }

    public void UpdatePieceShopPossibleSpawnPositions()
    {
        foreach (BuyablePiece _piece in buyablePieces)
        {
            _piece.UpdatePossibleSpawnPositions();
        }
    }
}
