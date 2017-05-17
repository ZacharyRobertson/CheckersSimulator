using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("CheckerBoardData")]
public class CheckerBoardData
{
    [XmlArray("Pieces")]
    [XmlArrayItem("Piece")]
    public PieceData[] pieces;
    public void Save(string path)
    {
        var serialzer = new XmlSerializer(typeof(CheckerBoardData));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serialzer.Serialize(stream, this);
        }
    }
    public static CheckerBoardData Load(string path)
    {
        var serializer = new XmlSerializer(typeof(CheckerBoardData));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as CheckerBoardData;
        }
    }
}
public class CheckerBoard : MonoBehaviour
{
    public GameObject blackPiece;
    public GameObject whitePiece;
    public string fileName;

    public int boardX = 8, boardZ = 8;
    public float pieceRadius = 0.5f;

    public Piece[,] pieces;
    private int halfBoardX, halfBoardZ;
    private float pieceDiameter;
    private Vector3 bottomLeft;
    private CheckerBoardData data;

    void Start()
    {
        //Calculate some values
        halfBoardX = boardX / 2;
        halfBoardZ = boardZ / 2;
        pieceDiameter = pieceRadius * 2;
        bottomLeft = /*transform.position*/ -Vector3.right * halfBoardX - Vector3.forward * halfBoardZ;
        string path = Application.persistentDataPath + "/" + fileName;
        //data = CheckerBoardData.Load(path);
        CreateGrid();
        data = new CheckerBoardData();
        data.Save(path);
    }
    void CreateGrid()
    {
        //Initialise our 2D array
        pieces = new Piece[boardX, boardZ];

        #region Generate White Pieces
        //Loop through board columns and skip 2 each time
        for (int x = 0; x < boardX; x += 2)
        {
            // Loop through first 3 rows
            for (int z = 0; z < 3; z++)
            {
                //Check even row
                bool evenRow = z % 2 == 0;
                int gridX = evenRow ? x : x + 1;
                int gridZ = z;
                //Generate piece
                GeneratePiece(whitePiece, gridX, gridZ);
            }
        }
        #endregion

        #region Generate Black Pieces
        //Loop through board columns and skip 2 each time
        for (int x = 0; x < boardX; x += 2)
        {
            for (int z = boardZ - 3; z < boardZ; z++)
            {
                //Check even row
                bool evenRow = z % 2 == 0;
                int gridX = evenRow ? x : x + 1;
                int gridZ = z;
                //Generate piece
                GeneratePiece(blackPiece, gridX, gridZ);
            }
        }
        #endregion
    }
    void GeneratePiece(GameObject piecePrefab, int x, int z)
    {
        // Create instance of piece
        GameObject clone = Instantiate(piecePrefab);
        // Set the parent to be this transform
        clone.transform.SetParent(transform);
        // Get the piece component from clone
        Piece piece = clone.GetComponent<Piece>();
        // Place the piece
        PlacePiece(piece, x, z);
    }
    void PlacePiece(Piece piece, int x, int z)
    {
        // Calculate offset for piece based on coordinate
        float xOffset = x * pieceDiameter + pieceRadius;
        float zOffset = z * pieceDiameter + pieceRadius;
        //set piece's new grid coordinate
        piece.gridX = x;
        piece.gridZ = z;
        // Move piece to physically bound coordinate
        piece.transform.position = bottomLeft + Vector3.right * xOffset + Vector3.forward * zOffset;
        // Set pieces in array slot
        pieces[x, z] = piece;
    }

    public void PlacePiece(Piece piece, Vector3 position)
    {
        // Translate position to coordinate in array
        float percentX = (position.x + halfBoardX) / boardX;
        float percentZ = (position.z + halfBoardZ) / boardZ;

        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((boardX - 1) * percentX);
        int z = Mathf.RoundToInt((boardZ - 1) * percentZ);

        if (IsValid(x, z))
        {
            // Get oldPiece from that coordinate
            Piece oldPiece = pieces[x, z];
            // If there is an oldPiece in the slot currently
            if (oldPiece != null)
            {
                // Swap the Pieces
                SwapPieces(piece, oldPiece);
            }
            else
            {
                // Place the piece
                int oldX = piece.gridX;
                int oldZ = piece.gridZ;
                pieces[oldX, oldZ] = null;
                PlacePiece(piece, x, z);
            }
        }
        else
        {
            int oldX = piece.gridX;
            int oldZ = piece.gridZ;
            PlacePiece(piece, oldX, oldZ);
        }

    }

    void SwapPieces(Piece pieceA, Piece pieceB)
    {
        // Check if pieceA or pieceB is null
        if (pieceA == null || pieceB == null)
            return;
        // return (Exit the function)

        // pieceA Grid position
        int pAX = pieceA.gridX;
        int pAZ = pieceA.gridZ;

        // pieceB Grid position
        int pBX = pieceB.gridX;
        int pBZ = pieceB.gridZ;

        // Swap pieces
        PlacePiece(pieceA, pBX, pBZ);
        PlacePiece(pieceB, pAX, pAZ);
    }

    bool IsValid(int x, int z)
    {
        //Check even row and column
        bool evenRow = z % 2 == 0;
        bool evenColumn = x % 2 == 0;
        bool oddRow = z % 2 == 1;
        bool oddColumn = x % 2 == 1;
        return (evenRow && evenColumn) || (oddRow && oddColumn);
    }
    void Update()
    {

    }
}
