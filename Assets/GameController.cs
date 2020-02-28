using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public GameObject piecePrefab;

    public List<Piece> pieces = new List<Piece>();

    private void Start()
    {
        InstantiateAllWhitePieces();

        for (int i = 0; i < 8; i++)
        {
            InstantiatePiece(Piece.PlayerColor.Black, Piece.ChessPieceType.Pawn, i, 2);
        }
    }

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

    public void Capture(Piece _capturedPiece)
    {
        pieces.Remove(_capturedPiece);
        Destroy(_capturedPiece.gameObject);
    }

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
}
