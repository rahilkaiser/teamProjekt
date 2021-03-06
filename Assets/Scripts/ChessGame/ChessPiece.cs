using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    NONE,
    PAWN,
    ROOK,
    KNIGHT,
    BISHOP,
    QUEEN,
    KING
}

public enum Team
{
    NONE,
    BLACK,
    WHITE
}

public class ChessPiece : MonoBehaviour
{
    public Team team;
    public ChessPieceType type;

    public Vector3 originalPosition;
    public Vector3 currentPosition;
    public Vector3 currentLocalScale;

    private Vector3 desiredPosition;
    private Vector3 desiredScale;

    #region Unity Builtin Methods
    void Awake()
    {
        
        this.originalPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        this.currentPosition = this.originalPosition;
        this.desiredPosition = this.currentPosition;

        this.currentLocalScale = this.transform.localScale;
        this.desiredScale = this.currentLocalScale;
    }

    public void Update()
    {

        //Debug.Log("Type: " + type.ToString());
        //Debug.Log("CurrentPos: " + th);
        //Debug.Log("DesiredPos: " + desiredPosition);
        transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(this.transform.localScale, this.desiredScale, Time.deltaTime * 10);
    }
    #endregion

    /** Sets this chessPieces desired Position
     * 
     */ 
    public virtual void SetPosition(Vector3 pos)
    {
        this.desiredPosition = pos;
        //--- Black King is poorly imported has to be centered ---
        if (this.type == ChessPieceType.KING && this.team == Team.BLACK)
        {
            this.desiredPosition.x += 0.5f;
            this.desiredPosition.z += 0.4f;
        }
        else
        {
            this.desiredPosition.x += 0.5f;
            this.desiredPosition.z += 0.5f;
        }
    }

    /** Sets this chessPieces desired localScale
     * 
     */
    public virtual void SetScale(float scale)
    {
        Vector3 newScale = Vector3.one * scale;
        this.desiredScale = newScale;
    }

    public virtual List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] chessPieceMap, int tileCountX, int tileCountY)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        possibleMoves.Add(new Vector2Int(3, 3));
        possibleMoves.Add(new Vector2Int(3, 4));
        possibleMoves.Add(new Vector2Int(4, 3));
        possibleMoves.Add(new Vector2Int(4, 4));

        return possibleMoves;
    }
    
    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] chessPieceMap, ref List<Vector2Int[]> moveList, ref List<Vector2Int> possibleMoves)
    {
        return SpecialMove.NONE;
    }
}
