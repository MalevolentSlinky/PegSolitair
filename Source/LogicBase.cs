using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using PegSolitair.HelperBase;

namespace PegSolitair
{
    class LogicBase
    {
        public enum Mode
        {
            Debug,
            SinglePlayer,
            ArtificalIntelligence
        };
        public enum Difficulty
        {
            Dumb,
            Somewhat,
            Very
        };
        public TimeSpan startTime { get; private set; }
        public TimeSpan endTime { get; private set; }
        public Mode GameMode { get; set; }
        public Difficulty GameDifficulty { get; set; }
        public int Depth { get; set; }
        private Stack<int[][]> SearchTree { get; set; }
        private int count { get; set; }
        private int[] bestMove;
        private int[] Corners = { 3, 5, 27, 35, 45, 53, 75, 77 };
        private int[] Ones = { 4, 13, 36, 37, 43, 44, 67, 76 };
        private int[] Twos = { 12, 14, 22, 28, 34, 38, 42, 46, 52, 58, 66, 68 };
        private int[] Threes = { 21, 23, 29, 33, 47, 51, 57, 59 };
        private int[] Fours = { 30, 31, 32, 39, 40, 41, 48, 49, 50 };
        
        //private Stack<GameBucket<int, int, SolitairBase.Piece>[][]> SearchTree { get; set; }

        private Thread SearchThread;

        public void InitializeLogic(Mode gameMode, Difficulty difficulty, int playerStart)
        {
            SearchTree = new Stack<int[][]>();
            Depth = 4;
            GameMode = gameMode;
            GameDifficulty = difficulty;
            startTime = new TimeSpan();
            endTime = new TimeSpan();
            //SearchTree = new Stack<GameBucket<int, int, SolitairBase.Piece>[][]>(9);
        }
        public void HandleAIMove(GameBucket<int, int, SolitairBase.Piece>[][] currentBoard)
        {
            int[] rootNode = new int[currentBoard.Length * currentBoard.Length];
            int elem = 0;
            for (int x = 0; x < currentBoard.Length; x++)
            {
                for (int y = 0; y < currentBoard.Length; y++)
                {
                    if (currentBoard[x][y] == null)
                        rootNode[elem++] = -1;
                    else
                        rootNode[elem++] = currentBoard[x][y].Item1;
                }
            }
            switch (GameDifficulty)
            {
                case Difficulty.Dumb:
                    SearchThread = new Thread(() => Dumb(Depth, true, rootNode, ref bestMove));
                    break;
                case Difficulty.Somewhat:
                    SearchThread = new Thread(new ThreadStart(Somewhat));
                    break;
                case Difficulty.Very:
                    SearchThread = new Thread(new ThreadStart(Very));
                    break;
            }
            TimeSpan first = DateTime.Now.TimeOfDay;
            SearchThread.Start();
            SearchThread.Join();
            TimeSpan end = DateTime.Now.TimeOfDay;
            Console.WriteLine("{0} {1}",first.ToString(), end.ToString());
        }
        public int Dumb(int depth, bool isMax, int[] node, ref int[] bestMove)
        {
            int[] bestNode = new int[81];
            int bestScore = isMax ? Int32.MinValue : Int32.MaxValue;
            int currentScore;
            if (depth == 0)
            {
                return Evaluation();
            }
            foreach (int[] possibleMoves in GetChildren(node, depth))
            {
                int[] currentMove;
                currentScore = Dumb(depth - 1, !isMax, TranslateNode(possibleMoves, node), ref bestMove);
                if (isMax) //MaxPlayer
                {
                    if (currentScore > bestScore)
                    {
                        bestScore = currentScore;
                        bestNode = possibleMoves;
                        bestMove = possibleMoves;
                    }
                }
                if (!isMax) //MinPlayer
                {
                    if (currentScore < bestScore)
                    {
                        bestScore = currentScore;
                        bestNode = possibleMoves;
                        bestMove = possibleMoves;
                    }
                }
            }
            return bestScore;
        }
        public void Somewhat()
        {

        }
        public void Very()
        {

        }
        private IEnumerable<int[]> GetChildren(int[] node, int depth)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    int index = y * 9 + x;
                    int adjacentNorth = (y - 1) * 9 + x, moveNorth = (y - 2) * 9 + x;
                    int adjacentEast = y * 9 + (x + 1), moveEast = y * 9 + (x + 2);
                    int adjacentSouth = (y + 1) * 9 + x, moveSouth = (y + 2) * 9 + x;
                    int adjacentWest = y * 9 + (x - 1), moveWest = y * 9 + (x - 2);
                    if (node[index] == 1)
                    {
                        if ((y - 2 > 0) && (node[adjacentNorth] == 1) && (node[moveNorth] == 0))
                        {
                            yield return new int[] { index, adjacentNorth, moveNorth };
                        }
                        if ((x + 2 < 9) && (node[adjacentEast] == 1) && (node[moveEast] == 0))
                        {
                            yield return new int[] { index, adjacentEast, moveEast };
                        }
                        if ((y + 2 < 9) && (node[adjacentSouth] == 1) && (node[moveSouth] == 0))
                        {
                            yield return new int[] { index, adjacentSouth, moveSouth };
                        }
                        if ((x - 2 > 0) && (node[adjacentWest] == 1) && (node[moveWest] == 0))
                        {
                            yield return new int[] { index, adjacentWest, moveWest };
                        }
                    }
                }
            }
        }
        private int[] TranslateNode(int[] possibleMove, int[] currentNode)
        {
            int[] newNode = new int[currentNode.Length];
            Array.Copy(currentNode, newNode, currentNode.Length);
            newNode[possibleMove[0]] = 0;
            newNode[possibleMove[1]] = 0;
            newNode[possibleMove[2]] = 1;
            return newNode;
        }
        private int Evaluation()
        {
            int evalScore = 0;
            return 0;
        }
    }
}