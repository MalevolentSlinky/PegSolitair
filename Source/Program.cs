using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

//TO-DO: Implement the AI (MiniMax / AB-Pruning), Add Player Transitions, Add Win States

namespace PegSolitair
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (Renderable render = new Renderable())
            {
                render.Run(30.0);
            }
        }
    }
    class Renderable : GameWindow
    {
        private LogicBase logicBase;
        private SolitairBase solitairBase;
        private int currentX;
        private int currentY;
        private double[] projectionMatrix = new double[16];
        private double[] modelViewMatrix = new double[16];
        private int[] viewport = new int[4];
        private Thread logicThread;

        private int currentTurn;

        public int CurrentTurn { get { return currentTurn; }
            set
            {
                if (currentTurn != value && value == 0)
                {
                    logicBase.HandleAIMove(solitairBase.Board);
                }
                currentTurn = value;
            }
        } // 0 = AI, 1 = Player

        protected override void OnLoad(EventArgs eventargs)
        {
            currentX = 0;
            currentY = 0;
            base.OnLoad(eventargs);
            base.Title = "Peg Solitair";
            base.VSync = VSyncMode.On;
            this.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(OnMouseDownHandler);
            logicBase = new LogicBase();
            logicBase.InitializeLogic(LogicBase.Mode.ArtificalIntelligence, LogicBase.Difficulty.Dumb, 1);
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Enable(EnableCap.ColorMaterial);
            currentTurn = 1;
            solitairBase = new SolitairBase();
            solitairBase.InitializeGameBoard();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();         
            GL.Ortho(0, 100.0, 100.0, 0.0, 0.0, 4.0);
            GL.GetDouble(GetPName.ProjectionMatrix, projectionMatrix);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.GetDouble(GetPName.ModelviewMatrix, modelViewMatrix);
            GL.GetInteger(GetPName.Viewport, viewport);
            solitairBase.DrawBoard();
            SwapBuffers();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);  
        }
        protected void OnMouseDownHandler(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (e.IsPressed && (currentX != e.X && currentY != e.Y))
                UpdateMouse(e.X, e.Y);       
        }
        private void UpdateMouse(int x, int y)
        {
            if (currentX != x && currentY != y && CurrentTurn == 1)
            {
                WindowToClient(x, ref currentX, y, ref currentY);
                CurrentTurn = solitairBase.SelectPiece(currentX, currentY) ? 0 : 1;
            }
        }
        private void WindowToClient(int x, ref int currentX, int y, ref int currentY)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(projectionMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(modelViewMatrix);

            var box = (float)0.0;
            GL.ReadPixels(x, viewport[3] - y, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, ref box);
            Vector3 results;
            OpenTK.Graphics.Glu.UnProject(new Vector3(x, viewport[3] - y, 0.0f), modelViewMatrix, projectionMatrix, viewport, out results);
            currentX = (int)results.X;
            currentY = (int)results.Y;
        }
    }
}
    