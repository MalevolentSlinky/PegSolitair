using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

//TO-DO: Implement VBO's, Optimize Piece Selection, Optimize ValidMoves

namespace ChineseCheckers
{
    class PegSolitaire
    {
        public class Board<T1, T2, T3>
        {
            public T1 Item1 { get; set; }   
            public T2 Item2 { get; set; }
            public T3 Item3 { get; set; }

            public Board(T1 item1, T2 item2, T3 item3)
            {
                Item1 = item1;
                Item2 = item2;
                Item3 = item3;
            }
        }
        public class Piece
        {
            private int X               { get; set; }
            private int Y               { get; set; }
            public int[][] ValidMoves   { get; private set; }
            public int[][] Coordinates  { get; private set; }

            public Piece(int initialX, int column, int initialY, int row)
            {
                X = column;
                Y = row;
                Coordinates = new int[4][];
                Coordinates[0] = new int[] { initialX + (Displacement * column), initialY + (Displacement * row) };
                Coordinates[1] = new int[] { initialX + (Displacement * (column + 1)), initialY + (Displacement * row) };
                Coordinates[2] = new int[] { initialX + (Displacement * (column + 1)), initialY + (Displacement * (row + 1)) };
                Coordinates[3] = new int[] { initialX + (Displacement * column), initialY + (Displacement * (row + 1)) };
                ValidMoves = new int[4][];
            }
            public void CalculateValidMoves()
            {
                ValidMoves = new int[4][];
                if ((Y - 2 > 0) && GridCell[X][Y - 2] != null && GridCell[X][Y - 2].Item1 == 0)         //North
                {
                    if (GridCell[X][Y - 1].Item1 == 1)
                    {
                        ValidMoves[0] = new int[] { X, Y - 2 };
                    }
                }
                if ((X + 2 < Cols) && GridCell[X + 2][Y] != null && GridCell[X + 2][Y].Item1 == 0)      //East
                {
                    if (GridCell[X + 1][Y].Item1 == 1)
                    {
                        ValidMoves[1] = new int[] { X + 2, Y };
                    }
                }
                if ((X - 2 > 0) && GridCell[X - 2][Y] != null && GridCell[X - 2][Y].Item1 == 0)         //West
                {
                    if (GridCell[X - 1][Y].Item1 == 1)
                    {
                        ValidMoves[2] = new int[] { X - 2, Y };
                    }
                }
                if ((Y + 2 < Rows) && GridCell[X][Y + 2] != null && GridCell[X][Y + 2].Item1 == 0)      //South
                {
                    if (GridCell[X][Y + 1].Item1 == 1)
                    {
                        ValidMoves[3] = new int[] { X, Y + 2 };
                    }
                }
            }
        };
        public const int Cols = 9;
        public const int Rows = 9;
        public const int Displacement = 8;
        public const int InitialX = 10;
        public const int InitialY = 10;
        public static List<List<Board<int, int, Piece>>> GridCell = new List<List<Board<int, int, Piece>>>(Cols * Rows);
        public static List<Tuple<int[], int[][]>> Coordinates = new List<Tuple<int[], int[][]>>();
        public static int[] CurrentlySelected;
        private static List<int[][]> ValidLocations = new List<int[][]>();

        public enum MouseState
        {
            Picking,
            Translating
        };

        private static MouseState mouseState;

        public static void InitializeGameBoard()
        {
            for (int x = 0; x < Cols; x++)
            {
                GridCell.Add(new List<Board<int, int, Piece>>());  
                for (int y = 0; y < Rows; y++)
                {
                    if (((x >= 0 && x <= 2) && (y >= 0 && y <= 2)) || ((x >= 0 && x <= 2) && (y >= 6 && y <= 8))
                        || ((x >= 6 && x <= 8) && (y >= 0 && y <= 2)) || ((x >= 6 && x <= 8) && (y >= 6 && y <= 8)))
                    {
                        GridCell[x].Add(null);
                        continue;
                    }
                    else if (x == 4 && y == 4)
                        GridCell[x].Add(new Board<int, int, Piece>(0, 0, new Piece(InitialX, x, InitialY, y)));

                    else
                        GridCell[x].Add(new Board<int, int, Piece>(1, 0, new Piece(InitialX, x, InitialY, y)));
                }
            }
            mouseState = MouseState.Picking;
        }
        public static void DrawBoard()
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (GridCell[x][y] == null)
                        continue;
                    if (GridCell[x][y].Item2 == 1)
                    {
                        GL.Color3(new byte[] { 255, 0, 0 });
                        GL.Begin(PrimitiveType.Quads);
                        GL.Vertex3(GridCell[x][y].Item3.Coordinates[0][0], GridCell[x][y].Item3.Coordinates[0][1], 0);
                        GL.Vertex3(GridCell[x][y].Item3.Coordinates[1][0], GridCell[x][y].Item3.Coordinates[1][1], 0);
                        GL.Vertex3(GridCell[x][y].Item3.Coordinates[2][0], GridCell[x][y].Item3.Coordinates[2][1], 0);
                        GL.Vertex3(GridCell[x][y].Item3.Coordinates[3][0], GridCell[x][y].Item3.Coordinates[3][1], 0);
                        GL.End();

                        foreach (int[] validCoordinates in GridCell[x][y].Item3.ValidMoves)
                        {
                            if (validCoordinates != null)
                            {
                                GL.Begin(PrimitiveType.Quads);
                                GL.Vertex3(GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[0][0], GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[0][1], 0);
                                GL.Vertex3(GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[1][0], GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[1][1], 0);
                                GL.Vertex3(GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[2][0], GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[2][1], 0);
                                GL.Vertex3(GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[3][0], GridCell[validCoordinates[0]][validCoordinates[1]].Item3.Coordinates[3][1], 0);
                                GL.End();
                            }
                        }                       
                    }
                    if (GridCell[x][y].Item1 == 1)
                    {
                        GL.Color3(new byte[] { 0, 255, 255 });
                        GL.Begin(PrimitiveType.TriangleFan);
                        for (int t = 0; t <= 360; t++)
                        {
                            GL.Vertex3((((GridCell[x][y].Item3.Coordinates[1][0] + GridCell[x][y].Item3.Coordinates[0][0]) / 2) + Math.Cos(t * Math.PI / 180)),
                                (((GridCell[x][y].Item3.Coordinates[0][1] + GridCell[x][y].Item3.Coordinates[2][1]) / 2) + Math.Sin(t * Math.PI / 180)), 0.0);
                        }
                        GL.End();
                    }
                    GL.Color3(new byte[] { 255, 255, 255 });
                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(GridCell[x][y].Item3.Coordinates[0][0], GridCell[x][y].Item3.Coordinates[0][1], 0);
                    GL.Vertex3(GridCell[x][y].Item3.Coordinates[1][0], GridCell[x][y].Item3.Coordinates[1][1], 0);
                    GL.Vertex3(GridCell[x][y].Item3.Coordinates[2][0], GridCell[x][y].Item3.Coordinates[2][1], 0);
                    GL.Vertex3(GridCell[x][y].Item3.Coordinates[3][0], GridCell[x][y].Item3.Coordinates[3][1], 0);
                    GL.End();
                }
            }
        }
        public static void SelectPiece(int currentX, int currentY)
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (GridCell[x][y] != null)
                    {
                        if ((currentX > GridCell[x][y].Item3.Coordinates[0][0] && currentX < GridCell[x][y].Item3.Coordinates[1][0])
                            && (currentY > GridCell[x][y].Item3.Coordinates[0][1] && currentY < GridCell[x][y].Item3.Coordinates[2][1]))
                        {
                            switch (mouseState)
                            {
                                case MouseState.Picking:
                                    {
                                        if (GridCell[x][y].Item1 == 1)
                                        {
                                            if (CurrentlySelected == null)
                                            {
                                                CurrentlySelected = new int[] { x, y };
                                            }
                                            else
                                            {
                                                CurrentlySelected[0] = x;
                                                CurrentlySelected[1] = y;
                                            }
                                            GridCell[x][y].Item2 = 1;
                                            GridCell[x][y].Item3.CalculateValidMoves();
                                            mouseState = MouseState.Translating;
                                        }
                                        break;
                                    }
                                case MouseState.Translating:
                                    {
                                        if (x == CurrentlySelected[0] && y == CurrentlySelected[1])
                                        {
                                            GridCell[CurrentlySelected[0]][CurrentlySelected[1]].Item2 = 0;
                                            CurrentlySelected = null;
                                            mouseState = MouseState.Picking;
                                            break;
                                        }
                                        if (GridCell[x][y].Item1 == 1)
                                        {
                                            GridCell[CurrentlySelected[0]][CurrentlySelected[1]].Item2 = 0;
                                            GridCell[x][y].Item2 = 1;
                                            GridCell[x][y].Item3.CalculateValidMoves();
                                            CurrentlySelected[0] = x;
                                            CurrentlySelected[1] = y;
                                        }
                                        else
                                        {
                                            foreach (int[] validMoves in GridCell[CurrentlySelected[0]][CurrentlySelected[1]].Item3.ValidMoves)
                                            {
                                                if (validMoves != null)
                                                {
                                                    if (x == validMoves[0] && y == validMoves[1])
                                                    {
                                                        TranslatePiece(CurrentlySelected, 
                                                            new int[]{(CurrentlySelected[0] < validMoves[0]) ? validMoves[0] - 1 : (CurrentlySelected[0] > validMoves[0]) ? validMoves[0] + 1 : validMoves[0],
                                                                (CurrentlySelected[1] < validMoves[1]) ? validMoves[1] - 1 : (CurrentlySelected[1] > validMoves[1]) ? validMoves[1] + 1 : validMoves[1]},
                                                                new int[] { x, y });
                                                        mouseState = MouseState.Picking;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                        }

                    }
                }
            }
        }
        private static void TranslatePiece(int[] source, int[] translation, int[] destination)
        {
            GridCell[source[0]][source[1]].Item1 = 0;
            GridCell[source[0]][source[1]].Item2 = 0;
            GridCell[translation[0]][translation[1]].Item1 = 0;
            GridCell[destination[0]][destination[1]].Item1 = 1;
            CurrentlySelected = null;
        }
    }
}
