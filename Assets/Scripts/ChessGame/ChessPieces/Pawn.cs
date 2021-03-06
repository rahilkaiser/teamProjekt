using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] chessPieceMap, int tileCountX, int tileCountY)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        //Go up if you are White otherwise go down
        int direction = (this.team == Team.BLACK) ? 1 : -1;


        Vector2Int currentPos = ChessGameUtil.floorToIntVector2Int(this.currentPosition);
        
        //1 forward
        if (chessPieceMap[currentPos.x, currentPos.y + direction] == null)
        {
            possibleMoves.Add(new Vector2Int(currentPos.x, currentPos.y + direction));
        }

        //2 forward
        if (chessPieceMap[currentPos.x, currentPos.y + direction] == null)
        {
            if( this.team == Team.WHITE 
                && currentPos.y == (tileCountY-2) 
                && chessPieceMap[currentPos.x, currentPos.y + direction *2] == null
                )
            {
                possibleMoves.Add(new Vector2Int(currentPos.x, currentPos.y + direction * 2));
            }
            
            if (this.team == Team.BLACK
                && currentPos.y == 1
                && chessPieceMap[currentPos.x, currentPos.y + direction * 2] == null
                )
            {
                possibleMoves.Add(new Vector2Int(currentPos.x, currentPos.y + direction * 2));
            }
        }

        //Move diagonaly to the right 
        if(currentPos.x != tileCountX - 1)
        {
            if( chessPieceMap[currentPos.x + 1, currentPos.y + direction] != null 
                && chessPieceMap[currentPos.x + 1, currentPos.y + direction].team != this.team)
            {
                possibleMoves.Add(new Vector2Int(currentPos.x + 1, currentPos.y + direction));
            }
        }

        //Move diagonaly to the left
        if (currentPos.x != 0)
        {
            if( chessPieceMap[currentPos.x - 1, currentPos.y + direction] != null 
                && chessPieceMap[currentPos.x - 1, currentPos.y + direction].team != this.team)
            {
                possibleMoves.Add(new Vector2Int(currentPos.x - 1, currentPos.y + direction));
            }
        }
        return possibleMoves; 
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] chessPieceMap, ref List<Vector2Int[]> moveList, ref List<Vector2Int> possibleMoves)
    {
        SpecialMove specialMove = SpecialMove.NONE;
        Vector2Int currentPos = ChessGameUtil.floorToIntVector2Int(this.currentPosition);

        int direction = (this.team == Team.BLACK) ? 1 : -1;

        if(this.team == Team.BLACK && currentPos.y == 6 || this.team == Team.WHITE && currentPos.y == 1)
        {
            return SpecialMove.PROMOTION;
        }

        return specialMove;
    }
}
