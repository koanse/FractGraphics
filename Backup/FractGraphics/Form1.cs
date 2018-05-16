using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;

namespace FractGraphics
{
    public partial class Form1 : Form
    {
        Microsoft.DirectX.Direct3D.Device direct3dDevice;
        PresentParameters presentParameters;
        Mesh meshLetters;
        Texture[] textures;
        Material material;

        Microsoft.DirectX.DirectInput.Device directInputDevice;

        float angleX, angleY, distance, xCoord, zCoord;
        int order;

        public Form1()
        {
            InitializeComponent();
            Location = new Point(50, 20);
            ClientSize = new Size(800, 600);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            presentParameters = new PresentParameters();
            presentParameters.Windowed = true;
            presentParameters.SwapEffect = SwapEffect.Discard;
            presentParameters.EnableAutoDepthStencil = true;
            presentParameters.AutoDepthStencilFormat = DepthFormat.D16;
            direct3dDevice = new Microsoft.DirectX.Direct3D.Device(0,
                Microsoft.DirectX.Direct3D.DeviceType.Hardware, this,
                CreateFlags.HardwareVertexProcessing, presentParameters);
            direct3dDevice.Transform.Projection =
                Matrix.PerspectiveFovLH((float)Math.PI / 4, 4 / 3, 1, 100);
            
            direct3dDevice.Lights[0].Diffuse = Color.White;
            direct3dDevice.Lights[0].Type = LightType.Directional;
            direct3dDevice.Lights[0].Direction = new Vector3(0, -1, -2);
            direct3dDevice.Lights[0].Enabled = true;

            directInputDevice = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Mouse);
            directInputDevice.SetCooperativeLevel(this, CooperativeLevelFlags.Exclusive |
                CooperativeLevelFlags.Foreground);

            try
            {
                meshLetters = Mesh.FromFile("letters.x", MeshFlags.Managed, direct3dDevice);
            }
            catch
            {
                MessageBox.Show("Отсуствует файл letters.x", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

            }

            material = new Material();
            material.Diffuse = Color.White;
            direct3dDevice.Material = material;

            ArrayList arrListTextures = new ArrayList();
            for (int i = 0; i < 5; i++)
                arrListTextures.Add(GenerateFractTexture(i));
            textures = (Texture[])arrListTextures.ToArray(typeof(Texture));            
            
            angleX = angleY = 0;
            distance = 15;
            order = 0;
            xCoord = zCoord = 0;
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            directInputDevice.Acquire();
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
            if (e.KeyCode == Keys.Add && order + 1 <= 4) order++;
            if (e.KeyCode == Keys.Subtract && order - 1 >= 0) order--;
            if (e.KeyCode == Keys.Left) xCoord += 0.1f;
            if (e.KeyCode == Keys.Right) xCoord -= 0.1f;
            if (e.KeyCode == Keys.Down) zCoord += 0.1f;
            if (e.KeyCode == Keys.Up) zCoord -= 0.1f;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            MouseState mouseState = new MouseState();
            while (Created)
            {
                try
                {
                    mouseState = directInputDevice.CurrentMouseState;
                }
                catch { }

                angleX += mouseState.Y / 300.0f;
                angleY -= mouseState.X / 300.0f;
                distance -= mouseState.Z / 300.0f;

                if (distance < 7) distance = 7;
                if (distance > 30) distance = 30;

                Matrix mRot1, mRot2, mTrans, mWorld;
                mRot1 = Matrix.RotationX(angleX);
                mRot2 = Matrix.RotationY(angleY);
                mTrans = Matrix.Translation(xCoord, 0, zCoord);
                mWorld = mRot1 * mRot2 * mTrans;

                direct3dDevice.Transform.World = mWorld;
                direct3dDevice.BeginScene();
                direct3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.DarkBlue, 1, 0);
                direct3dDevice.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, distance),
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                direct3dDevice.SetTexture(0, textures[order]);

                meshLetters.DrawSubset(0);
                direct3dDevice.EndScene();
                direct3dDevice.Present();
                Application.DoEvents();
            }
        }
        private Texture GenerateFractTexture(int order)
        {
            int len = 400;
            float angle = 60;
            Bitmap bmp = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(bmp);
            Pen pen = new Pen(Color.Black, 2);
            Brush brush = Brushes.White;
            
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            m.Translate(0, 200);
            
            StackElement stackElem = new StackElement(m, 0, len);
            Stack stack = new Stack();
            stack.Push(stackElem);
            g.FillRectangle(brush, 0, 0, 400, 400);
            ArrayList points = new ArrayList();
            points.Add(new Point(len, 200));
            while (stack.Count > 0)
            {
                stackElem = stack.Peek() as StackElement;
                float x = stackElem.len / 6;
                float y = x * (float)Math.Sqrt(3) / 12;
                if (stackElem.order < order)
                {
                    stack.Pop();
                    m = stackElem.matrix.Clone();
                    stack.Push(new StackElement(m, stackElem.order + 1, x));

                    m = stackElem.matrix.Clone();
                    m.Translate(x, 0);
                    m.Rotate(angle);
                    stack.Push(new StackElement(m, stackElem.order + 1, x));

                    m = stackElem.matrix.Clone();
                    m.Translate(x, 0);
                    m.Rotate(angle);
                    m.Translate(x, 0);
                    m.Rotate(-angle);
                    stack.Push(new StackElement(m, stackElem.order + 1, x));

                    m = stackElem.matrix.Clone();
                    m.Translate(x, 0);
                    m.Rotate(angle);
                    m.Translate(x, 0);
                    m.Rotate(-angle);
                    m.Translate(x, 0);
                    stack.Push(new StackElement(m, stackElem.order + 1, x));

                    m = stackElem.matrix.Clone();
                    m.Translate(x, 0);
                    m.Rotate(angle);
                    m.Translate(x, 0);
                    m.Rotate(-angle);
                    m.Translate(x, 0);
                    m.Translate(x, 0);
                    stack.Push(new StackElement(m, stackElem.order + 1, x));

                    m = stackElem.matrix.Clone();
                    m.Translate(x, 0);
                    m.Rotate(angle);
                    m.Translate(x, 0);
                    m.Rotate(-angle);
                    m.Translate(x, 0);
                    m.Translate(x, 0);
                    m.Translate(x, 0);
                    m.Rotate(-angle);
                    stack.Push(new StackElement(m, stackElem.order + 1, x));

                    m = stackElem.matrix.Clone();
                    m.Translate(x, 0);
                    m.Rotate(angle);
                    m.Translate(x, 0);
                    m.Rotate(-angle);
                    m.Translate(x, 0);
                    m.Translate(x, 0);
                    m.Translate(x, 0);
                    m.Rotate(-angle);
                    m.Translate(x, 0);
                    m.Rotate(angle);
                    stack.Push(new StackElement(m, stackElem.order + 1, x));
                }
                else
                {
                    points.Add(new Point((int)stackElem.matrix.OffsetX,
                        (int)stackElem.matrix.OffsetY));
                    stack.Pop();
                }
            }
            g.DrawLines(pen, (Point[])points.ToArray(typeof(Point)));
            return Texture.FromBitmap(direct3dDevice, bmp, Usage.Dynamic, Pool.Default);
        }
        private class StackElement
        {
            public System.Drawing.Drawing2D.Matrix matrix;
            public int order;
            public float len;
            public StackElement(System.Drawing.Drawing2D.Matrix m, int ord, float l)
            {
                matrix = m;
                order = ord;
                len = l;
            }
        }
    }
}