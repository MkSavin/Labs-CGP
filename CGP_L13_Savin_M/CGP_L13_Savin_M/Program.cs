using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace CGP_L13_Savin_M
{
    class Program
    {
        private static Texture currentTexture;

        static void Main(string[] args)
        {
            using (var baseWindow = new BaseWindow())
            {
                int width = baseWindow.Width, height = baseWindow.Height;

                Vector3 rotation = Vector3.Zero;
                Vector3 position = new Vector3(0, 0, -100);
                Vector3 halfSize = new Vector3(10, 10, 10);

                Matrix4 rotationMatrix;

                bool solidView = true, defaultTexture = true;

                baseWindow.KeyDown += (sender, e) => {
                    if (e.Key == Key.T)
                    {
                        if (solidView)
                        {
                            if (defaultTexture)
                            {
                                currentTexture = new Texture("textures/default.bmp");
                            }
                            else
                            {
                                currentTexture = new Texture("textures/fish.bmp");
                            }
                            
                            defaultTexture = !defaultTexture;

                            AssetLoader.LoadTexture(currentTexture);
                        }
                        else
                        {
                            GL.BindTexture(TextureTarget.Texture2D, 0);
                        }

                        solidView = !solidView;
                    }
                };

                baseWindow.Render((deltaTick) => {
                    GL.Light(LightName.Light0, LightParameter.Position, new float[] { 100, 100, 100 });
                    GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.6f, 0.6f, 0.6f, 1.0f });

                    rotation.X += (float)deltaTick / 2;
                    rotation.Y += (float)deltaTick / 4;
                    rotation.Z += (float)deltaTick / 3;

                    rotationMatrix = Matrix4.CreateRotationX(rotation.X)
                        * Matrix4.CreateRotationY(rotation.Y)
                        * Matrix4.CreateRotationZ(rotation.Z)
                        * Matrix4.CreateTranslation(position);

                    GL.LoadMatrix(ref rotationMatrix);

                    GL.Begin(PrimitiveType.Quads);
                    // FRONT
                    GL.Normal3(Vector3.UnitZ);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(halfSize.X, halfSize.Y, halfSize.Z);
                    GL.TexCoord2(0, 1);
                    GL.Vertex3(halfSize.X, -halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 1);
                    GL.Vertex3(-halfSize.X, -halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 0);
                    GL.Vertex3(-halfSize.X, halfSize.Y, halfSize.Z);

                    // BACK
                    GL.Normal3(-Vector3.UnitZ);
                    GL.TexCoord2(1, 0);
                    GL.Vertex3(halfSize.X, halfSize.Y, -halfSize.Z);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(halfSize.X, -halfSize.Y, -halfSize.Z);
                    GL.TexCoord2(0, 1);
                    GL.Vertex3(-halfSize.X, -halfSize.Y, -halfSize.Z);
                    GL.TexCoord2(1, 1);
                    GL.Vertex3(-halfSize.X, halfSize.Y, -halfSize.Z);

                    // TOP
                    GL.Normal3(Vector3.UnitY);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(-halfSize.X, halfSize.Y, -halfSize.Z);
                    GL.TexCoord2(1, 0);
                    GL.Vertex3(-halfSize.X, halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 1);
                    GL.Vertex3(halfSize.X, halfSize.Y, halfSize.Z);
                    GL.TexCoord2(0, 1);
                    GL.Vertex3(halfSize.X, halfSize.Y, -halfSize.Z);

                    // BOTTOM
                    GL.Normal3(-Vector3.UnitY);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(-halfSize.X, -halfSize.Y, -halfSize.Z);
                    GL.TexCoord2(0, 1);
                    GL.Vertex3(-halfSize.X, -halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 1);
                    GL.Vertex3(halfSize.X, -halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 0);
                    GL.Vertex3(halfSize.X, -halfSize.Y, -halfSize.Z);

                    // LEFT
                    GL.Normal3(-Vector3.UnitX);
                    GL.TexCoord2(1, 0);
                    GL.Vertex3(-halfSize.X, halfSize.Y, -halfSize.Z);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(-halfSize.X, halfSize.Y, halfSize.Z);
                    GL.TexCoord2(0, 1);
                    GL.Vertex3(-halfSize.X, -halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 1);
                    GL.Vertex3(-halfSize.X, -halfSize.Y, -halfSize.Z);

                    // RIGHT
                    GL.Normal3(Vector3.UnitX);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(halfSize.X, halfSize.Y, -halfSize.Z);
                    GL.TexCoord2(0, 1);
                    GL.Vertex3(halfSize.X, halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 1);
                    GL.Vertex3(halfSize.X, -halfSize.Y, halfSize.Z);
                    GL.TexCoord2(1, 0);
                    GL.Vertex3(halfSize.X, -halfSize.Y, -halfSize.Z);

                    GL.End();
                    // Console.SetCursorPosition(0, 0);
                    // Console.WriteLine("Position: " + position + ", rotation: " + rotation + ", scale: " + scale + "       ");
                    // Console.WriteLine("WASDQE - position, TFGHRY - rotation, Numpad 845679 - scale");
                });
            }
        }

    }

    public class Asset
    {
        public string Location { get; private set; } = "";
        public string RelativeLocation { get; private set; } = "";

        public Asset(string location)
        {
            RelativeLocation = location;
            Location = GetLocation(location);
        }

        public bool Exists()
        {
            return File.Exists(Location);
        }

        public static string GetLocation(string asset)
        {
            return AppDomain.CurrentDomain.BaseDirectory + "/assets/" + asset.Trim('/');
        }

        public string ReadAllText()
        {
            if (!Exists())
            {
                return null;
            }

            return File.ReadAllText(Location);
        }

    }

    public class AssetLoader
    {
        public static Texture BadTexture;

        public static Dictionary<string, Texture> LoadedTextures = new Dictionary<string, Texture>();

        public static void LoadTexture(Texture texture)
        {
            if (!LoadedTextures.ContainsKey(texture.Asset.Location))
            {
                Console.WriteLine("Loading new texture");

                BadTexture = new Texture("textures/notex.bmp");

                BitmapData bitmapData;

                int textureId = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, textureId);

                LoadedTextures.Add(texture.Asset.Location, texture);

                texture.ID = textureId;
                bitmapData = texture.BitmapData();

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, bitmapData.Scan0);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
            else
            {
                Console.WriteLine("Loading already initialized texture");
                GL.BindTexture(TextureTarget.Texture2D, LoadedTextures[texture.Asset.Location].ID);
            }
        }
    }

    public class Texture
    {
        public Asset Asset { get; private set; }

        public Texture(string location) : this(new Asset(location)) { }

        public int ID = 0;

        public Texture(Asset asset)
        {
            Asset = asset;
        }

        public BitmapData BitmapData()
        {
            var bmp = new Bitmap(Asset.Location);
            var bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmp.UnlockBits(bmpdata);

            return bmpdata;
        }

    }

    public class BaseWindow : GameWindow
    {
        Stopwatch watch;

        public BaseWindow() : base(800, 600)
        {
            Title = "Savin M. PRI-117. CGP_L13";

            Load += (sender, e) => {
                VSync = VSyncMode.On;
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
                GL.Enable(EnableCap.DepthTest);
            };

            Resize += (sender, e) => {
                GL.Viewport(0, 0, Width, Height);
                GL.MatrixMode(MatrixMode.Projection);

                //Ortho();
                Perspective();
            };

            UpdateFrame += (sender, e) =>
            {
                if (Keyboard.GetState().IsKeyDown(Key.Escape))
                {
                    Exit();
                }
            };

            watch = Stopwatch.StartNew();
        }

        public void Ortho()
        {
            GL.LoadIdentity();
            Matrix4 p = Matrix4.CreateOrthographic(Width, Height, 1.0f, 1000.0f);
            GL.LoadMatrix(ref p);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void Perspective()
        {
            GL.LoadIdentity();
            Matrix4 p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(70), (float)Width / Height, 1.0f, 10000.0f);
            GL.LoadMatrix(ref p);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void Identity()
        {
            GL.LoadIdentity();
            GL.Translate(0, 0, -100);
            // GL.Translate(-Width / 2, -Height / 2, -100);
        }

        public void Render(Action<double> callback)
        {
            RenderFrame += (sender, e) =>
            {
                Identity();

                GL.ClearColor(Color.Black);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                watch.Stop();
                callback.Invoke(watch.ElapsedMilliseconds / 1000F);
                watch.Restart();

                SwapBuffers();
            };

            Run();
        }
    }
}
