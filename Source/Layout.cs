using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


namespace ChineseCheckers
{
    class Layout
    {
        public struct Section
        {
            public float X { get; set; }
            public float Y { get; set; }
        }
        public class Piece
        {
            public int IsSelected { get; set; }
            public int IsEmpty { get; set; }
            public int IsEdge { get; set; }
            public float[][] Coordinates { get; set; }
            public Tuple<String, Piece>[] Adjacent { get; set; }   //North, East, South, West
            public void InitializeSquare(float x, float y, int multiplier)
            {
                Adjacent = new Tuple<String, Piece>[4];
                Coordinates = new float[4][];
                Coordinates[0] = new float[] { x, y };
                Coordinates[1] = new float[] { x + multiplier, y };
                Coordinates[2] = new float[] { x + multiplier, y + multiplier };
                Coordinates[3] = new float[] { x, y + multiplier };
                IsSelected = 0;
                IsEmpty = Layout.rand.Next(0, 2);
                IsEdge = 0;
            }
        }
        public static Random rand = new Random();
        public List<Piece> Board { get; set; }
        public Piece SelectedPiece { get; set; }
        public float[][] Edges = new float[][]
            {
                new float[] {25.0f, 25.0f}, new float[] {34.0f, 25.0f},
                new float[] {34.0f, 34.0f}, new float[] {43.0f, 34.0f}, 
                new float[] {43.0f, 43.0f}, new float[] {34.0f, 43.0f},
                new float[] {34.0f, 52.0f}, new float[] {25.0f, 52.0f}, 
                new float[] {25.0f, 43.0f}, new float[] {16.0f, 43.0f}, 
                new float[] {16.0f, 34.0f}, new float[] {25.0f, 34.0f}
            };
        public void Initialize()
        {
            Section[] sectionList = new Section[5];
            InitializeCoordinates(ref sectionList);
            InitializeBoard(sectionList);
            InitializeAdjacency();
        }
        private void InitializeCoordinates(ref Section[] sectionList)
        {
            float x = 25.0f, y = 25.0f;
            for (int i = 0; i < 5; i++, y += 9)
            {
                sectionList[i].X = x;
                sectionList[i].Y = y;
                if (i == 1)
                {
                    sectionList[i + 1].X = x - 9;
                    sectionList[i + 1].Y = y;
                    sectionList[i + 2].X = x + 9;
                    sectionList[i + 2].Y = y;
                    i += 2;
                }
            }
        }
        private void InitializeBoard(Section[] coordinateList)
        {
            Board = new List<Piece>();
            Piece piece;
            for (int i = 0; i < coordinateList.Length; i++)
            {
                for (float column = coordinateList[i].X; column < coordinateList[i].X + 9; column += 3)
                {
                    for (float row = coordinateList[i].Y; row < coordinateList[i].Y + 9; row += 3)
                    {
                        piece = new Piece();
                        piece.InitializeSquare(column, row, 3);
                        Board.Add(piece);
                    }
                }
            }
        }
        private void InitializeAdjacency()
        {
            int adjacentCount;
            for (int i = 0; i < Board.Count; i++)
            {
                adjacentCount = 0;
                for (int x = 0; x < Board.Count; x++)
                {
                    if (Board[i].Equals(Board[x]))
                        continue;
                    if (Board[i].Coordinates[0].SequenceEqual(Board[x].Coordinates[3])
                            && Board[i].Coordinates[1].SequenceEqual(Board[x].Coordinates[2]))
                    {
                        Board[i].Adjacent[0] = new Tuple<string, Piece>("North", Board[x]);
                        adjacentCount++;
                    }
                    if (Board[i].Coordinates[1].SequenceEqual(Board[x].Coordinates[0])
                            && Board[i].Coordinates[2].SequenceEqual(Board[x].Coordinates[3]))
                    {
                        Board[i].Adjacent[1] = new Tuple<string, Piece>("East", Board[x]);
                        adjacentCount++;
                    }
                    if (Board[i].Coordinates[3].SequenceEqual(Board[x].Coordinates[0])
                            && Board[i].Coordinates[2].SequenceEqual(Board[x].Coordinates[1]))
                    {
                        Board[i].Adjacent[2] = new Tuple<string, Piece>("South", Board[x]);
                        adjacentCount++;
                    }
                    if (Board[i].Coordinates[0].SequenceEqual(Board[x].Coordinates[1])
                            && Board[i].Coordinates[3].SequenceEqual(Board[x].Coordinates[2]))
                    {
                        Board[i].Adjacent[3] = new Tuple<string, Piece>("West", Board[x]);
                        adjacentCount++;
                    }
                }
                if (adjacentCount < 4)
                    Board[i].IsEdge = 1;
            }
        }
        public void DrawBoard()
        {
            Piece[] validmoves = new Piece[4];
            DetermineValidMove(Board[13], ref validmoves);
            GL.Color3(new byte[] { 255, 0, 0 });
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(Board[13].Coordinates[0][0], Board[13].Coordinates[0][1], 0.0f);
            GL.Vertex3(Board[13].Coordinates[1][0], Board[13].Coordinates[1][1], 0.0f);
            GL.Vertex3(Board[13].Coordinates[2][0], Board[13].Coordinates[2][1], 0.0f);
            GL.Vertex3(Board[13].Coordinates[3][0], Board[13].Coordinates[3][1], 0.0f);
            GL.End();
            foreach (Piece adjacent in validmoves)
            {
                if (adjacent != null)
                {
                    GL.Color4(new byte[] { 200, 0, 0, 255 });
                    GL.Begin(PrimitiveType.Quads);
                    GL.Vertex3(adjacent.Coordinates[0][0], adjacent.Coordinates[0][1], 0.0f);
                    GL.Vertex3(adjacent.Coordinates[1][0], adjacent.Coordinates[1][1], 0.0f);
                    GL.Vertex3(adjacent.Coordinates[2][0], adjacent.Coordinates[2][1], 0.0f);
                    GL.Vertex3(adjacent.Coordinates[3][0], adjacent.Coordinates[3][1], 0.0f);
                    GL.End();
                }
            }
            for (int i = 0; i < Board.Count; i++)
            {
                if (i == 13)
                    Board[i].IsSelected = 1;
                foreach (Piece piece in Board)
                {
                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Color3(new byte[] { 255, 255, 255 });
                    GL.Vertex3(piece.Coordinates[0][0], piece.Coordinates[0][1], 0.0f);
                    GL.Vertex3(piece.Coordinates[1][0], piece.Coordinates[1][1], 0.0f);
                    GL.Vertex3(piece.Coordinates[2][0], piece.Coordinates[2][1], 0.0f);
                    GL.Vertex3(piece.Coordinates[3][0], piece.Coordinates[3][1], 0.0f);
                    GL.End();
                    if (piece.IsEmpty == 0) //Not Empty
                    {
                        GL.Begin(PrimitiveType.TriangleFan);
                        double x, y;
                        int t;
                        for (t = 0; t <= 360; t += 1)
                        {
                            x = ((piece.Coordinates[1][0] + piece.Coordinates[0][0]) / 2) + Math.Cos(t * Math.PI / 180);
                            y = ((piece.Coordinates[0][1] + piece.Coordinates[2][1]) / 2) + Math.Sin(t * Math.PI / 180);
                            GL.Vertex3(x, y, 0);
                        }
                        GL.End();
                    }
                }
            }
        }
            //Piece[] validmoves = new Piece[4];
            //DetermineValidMove(Board[25], ref validmoves);
        private void DetermineValidMove(Piece selected, ref Piece[] validMoves)
        {
            int element = 0;
            foreach (Tuple<string, Piece> adjacent in selected.Adjacent)
            {
                if (adjacent != null && adjacent.Item2.IsEmpty == 0)
                {
                    if (adjacent.Item2.Adjacent[element] != null && adjacent.Item2.Adjacent[element].Item2.IsEmpty == 1)
                    {
                        validMoves[element] = adjacent.Item2;
                    }
                }
                element++;
            }
        }
    }
}
