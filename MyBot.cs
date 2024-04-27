using ChessChallenge.API;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    float[] pieceValues = { 0, 1, 3, 3, 5, 9 };
    float moveToPointScore = .03f;
    int maxDepth = 3;

    public Move Think(Board board, Timer timer)
    {
        bool isWhite = board.IsWhiteToMove;
        return Branch(board, isWhite).GetBestMove(isWhite).currentMove; //search + evaluation are combined to save space
    }

    public MoveNode Branch(Board board, bool isWhite, int depth = 0)
    {
        Move[] moves = board.GetLegalMoves();
        MoveNode moveNode = new();

        if (depth < maxDepth)
        {
            foreach (var move in moves)//check all moves
            {
                board.MakeMove(move);
                MoveNode posibleNextMove = Branch(board, !isWhite, depth + 1);
                posibleNextMove.currentMove = move;
                moveNode.moves.Add(posibleNextMove);
                board.UndoMove(move);
            }
        }

        if (moveNode.moves.Count == 0)
        {
            moveNode.adv = Evaluate(board);
            return moveNode;
        }

        //minimax algorithm
        float adv = 99999 * (isWhite ? -1 : 1); //assume the worst
        foreach (MoveNode m in moveNode.moves)if (m.adv > adv == isWhite) adv = m.adv;

        moveNode.adv = adv;

        return moveNode;
    }

    public float Evaluate(Board board)
    {
        float adv = 0;
        bool isWhite = board.IsWhiteToMove;
        if (board.IsInCheckmate()) return 9999 * (isWhite ? -1 : 1);
        if (board.IsDraw()) return 0;

        for (int iteration = 0; iteration < 2; iteration++)
        {
            bool iterationWhite = (iteration == 0);

            for (int i = 0; i < 6; i++) adv += BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard((PieceType)i, iterationWhite))
                    * pieceValues[i] * (iterationWhite ? 1 : -1);
        }

        if (board.TrySkipTurn())
        {
            adv += board.GetLegalMoves().Length * (board.IsWhiteToMove ? 1 : -1) * moveToPointScore;
            board.UndoSkipTurn();

            adv += board.GetLegalMoves().Length * (board.IsWhiteToMove ? 1 : -1) * moveToPointScore;
        }
        return adv;
    }

    public class MoveNode
    {
        public List<MoveNode> moves = new();
        public float adv;//cascading using minimax
        public Move currentMove;
        public MoveNode GetBestMove(bool isWhite)
        {
            MoveNode best = moves.First();
            foreach (MoveNode m in moves)if (m.adv > best.adv == isWhite) best=m;
            return best;
        }
        public MoveNode() { }
    }
}