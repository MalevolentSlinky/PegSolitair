﻿using System;
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

        private int totalMoves;
        //private Stack<GameBucket<int, int, SolitairBase.Piece>[][]> SearchTree { get; set; }

        private Thread SearchThread;

        public void InitializeLogic(Mode gameMode, Difficulty difficulty, int playerStart)
        {
            SearchTree = new Stack<int[][]>();
            Depth = 2;
            totalMoves = 0;
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
                    SearchThread = new Thread(() => Dumb(Depth, true, Int32.MaxValue, Int32.MinValue, rootNode, ref bestMove));
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
            Console.WriteLine("totalMoves = " + totalMoves);
            TimeSpan end = DateTime.Now.TimeOfDay;
            Console.WriteLine("{0} {1}",first.ToString(), end.ToString());
            Console.WriteLine("Best Move = " + bestMove[0] + " to " +bestMove[2]);
        }
        public int Dumb(int depth, bool isMax, int min, int max, int[] node, ref int[] bestMove)
        {
            totalMoves++;
            Console.WriteLine("\nDepth: " + depth);
            String s1 = isMax ? "max, currently: " + max : "min, currently " + min;
            Console.WriteLine("current Player = " + s1);
            if (depth == 0)//if at leaf node (root = maxDepth)
            {
                int j = DumbEvaluation(node);
                Console.WriteLine("Node score: "+j);
                PrintNode(node);
                return j;
            }

            PrintNode(node);
            int[] bestNode = new int[81];
            foreach (int[] possibleMoves in GetChildren(node, depth)) 
            {//calculates and receives one move at a time for every possible move in 'node'
                int currentScore = Dumb(depth - 1, !isMax, min, max, TranslateNode(possibleMoves, node), ref bestMove);
                //recursive call of Dumb() will travel from current node to each child node until all moves are evaluated
                //this is where all backtracking occurs

                if (depth == 4)
                {
                    int k = 5;
                }

                if (isMax) //MaxPlayer
                {
                    if (currentScore > max)
                    {//we have found a better max
                        max = currentScore;  //setting aplha value
                        //Console.WriteLine("depth = " + depth +" new max = "+ max);
                        bestNode = possibleMoves;
                        bestMove = possibleMoves;
                    }
                    if (max >= min) //THIS 'IF' PROVIDES PRUNING FUNCTIONALITY FOR MAX NODES
                    {//if alpha > beta
                        Console.WriteLine(max+ ">="+ min +"Pruning the rest of this branch and backtracking...");
                        return max; //return alpha (cut off rest of search for this branch)
                    }
                }
                if (!isMax) //MinPlayer
                {
                    if (currentScore < min)
                    {//we have found a lower min
                        //Console.WriteLine("Pruning the rest of this branch and backtracking...");
                        min = currentScore;//setting beta value
                        bestNode = possibleMoves;
                        bestMove = possibleMoves;
                    }
                    if (min <= max) //THIS 'IF' PROVIDES PRUNING FUNCTIONALITY FOR MIN NODES
                    {//if beta < alpha
                        Console.WriteLine(min + "<=" +max+", Pruning the rest of this branch and backtracking...");
                        return min; //return beta (cut off rest of search for this branch)
                    }
                }
            }
            //has exausted all moves for a node
            return isMax ? max : min;
        }
        public void Somewhat()
        {

        }
        public void Very()
        {

        }
        private IEnumerable<int[]> GetChildren(int[] node, int depth)
        {
            //computes and returns a single possible move
            //into an enumarable array of ints(a node).
            //each yield return adds one move to 

            //this is where the logic for only picking 16 pieces to update WOULD come in
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    int index = y * 9 + x;
                    //(if (node[index] == 1) should go here?)//
                    int adjacentNorth = (y - 1) * 9 + x, moveNorth = (y - 2) * 9 + x;
                    int adjacentEast = y * 9 + (x + 1), moveEast = y * 9 + (x + 2);
                    int adjacentSouth = (y + 1) * 9 + x, moveSouth = (y + 2) * 9 + x;
                    int adjacentWest = y * 9 + (x - 1), moveWest = y * 9 + (x - 2);
                    if (node[index] == 1)//shouldn't this if statement go before all of the move and adjacency calculations?
                    {//if there is a piece at the current coordinate
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

        private void PrintNode(int[] nodeToPrint)
        {
            Console.Write("+012345678");
            for (int y = 0; y < 9; y++)
            {
                Console.Write("\n" + y);
                for (int x = 0; x < 9; x++)
                {
                    int index = x * 9 + y;
                    if(nodeToPrint[index] == -1)
                         Console.Write("-");
                    else
                        Console.Write(nodeToPrint[index]);
                }

            }
            Console.Write("\n");
        }

        private int Evaluation(int[] node)
        {
            for (int i = 0; i < 81; i++)
            {

            }
            return new Random().Next(0, 1000);
        }
        private int DumbEvaluation(int[] node)
        {// high value = more weighted 
            //Dumb version only looks for win/loss states
            int moveCount = 0;
            foreach (int[] validMove in GetChildren(node,0)) //0 parameter is arbitrary
            {
                moveCount++;
            }
            if (moveCount == 0)
                return 1000;
            else
                return 1;
        }
        private int SomewhatEvaluation(int[] node)
        {// high value = more weighted 
            //Somewhat intelligent version looks for win/loss states, 
            //if none are found it looks for 
            int moveCount = 0;
            foreach (int[] validMove in GetChildren(node, 0)) //0 parameter is arbitrary
            {
                moveCount++;
            }
            if (moveCount == 0)
                return 1000; //endstate
            if(moveCount == 1) 
            {
                if (checkForSweep(node))//does this node lead to an unavoidable string of one moves til endgame?
                    return 1000;
            }
            return 1;
        }


        private int SmartEvaluation(int[] node)
        {
            // high value = more weighted 
            //smart evaluation looks for win/loss states, 
            //if none are found it looks for sweeps
            //if none are found it weighs the node by number of moves
            //  -since we aimed to develop a very very efficient AI, 
            //      we will be hoping that is our edge over the competition
            //      , so we will give nodes with more complexity(more moves) a higher precedence,
            //      as we will preserve our edge due to search speed
            //I'm considering adding a function that looks for endgame 'packages' 
            //  and purging strategies in a similar manner as sweeps...
            //  'packages' are described on page 6 of this pdf:
            //  http://www.link.cs.cmu.edu/15859-s11/notes/peg-solitaire.pdf 
            //A huge problem for many complex evaluation functions is that it will have 
            //  negative impacts on our performance, and since our main focus from the 
            //  beginning has been speed, we need to strike a balance here...
            int moveCount = 0;
            foreach (int[] validMove in GetChildren(node, 0)) //0 parameter is arbitrary
            {
                moveCount++;
            }
            if (moveCount == 0)
                return 1000; //endstate
            if (moveCount == 1)
            {
                if (checkForSweep(node))//does this node lead to an unavoidable string of one moves til endgame?
                    return 1000;
            }
            return 1;
        }

        private Boolean checkForSweep(int[] node)
        {//this method will go beyond depth bounds to search for a possible endgame sweep 
            int moveCount = 0;
            int[] move = { 0, 0, 0 };
            foreach (int[] validMove in GetChildren(node, 0)) //0 parameter is arbitrary
            {
                move = validMove;
                moveCount++;
                if (moveCount > 1)
                    return false;//the sweep devolves into a more complex branching pattern
            }
            if (moveCount == 0)
            {//this examined sweep continues until endgame
                return true;
            }
            if (moveCount == 1)
            {
                TranslateNode(move, node);
                checkForSweep(node);
            }
            return false;//program should never reach here 
        }
    }
}