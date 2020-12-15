using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGP_L8_Savin_M
{
    class Program
    {
        private static List<Point> points = new List<Point>();
        public static int segments = 20;

        [STAThread]
        static void Main(string[] args)
        {
            using (var baseWindow = new BaseWindow())
            {
                int width = baseWindow.Width, height = baseWindow.Height, index = 0;
                bool pointFound, drawing, inWindowBounds;
                float pointX, pointY;

                points.Add(new Point(width / 2, height / 2));

                MouseState mouseState;

                baseWindow.Render((deltaTick) =>
                {
                    width = baseWindow.Width;
                    height = baseWindow.Height;

                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    mouseState = Mouse.GetCursorState();

                    drawing = pointFound = false;

                    if (mouseState.LeftButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed)
                    {
                        pointX = mouseState.X - baseWindow.Location.X - 8;
                        pointY = baseWindow.Height - (mouseState.Y - baseWindow.Y - 32);

                        inWindowBounds = pointX >= 0 && pointY >= 0 && pointX < baseWindow.Width && pointY < baseWindow.Height;

                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            drawing = true;

                            points.ForEach((point) => {
                                if (point.InBounds(pointX, pointY))
                                {
                                    pointFound = true;

                                    if (!point.MovingState && inWindowBounds)
                                    {
                                        point.SetMovingState(true);
                                        point.SetMovingDelta(pointX, pointY);
                                    }
                                }
                            });

                            points.ForEach((point) => {
                                if (point.MovingState)
                                {
                                    pointFound = true;

                                    point.Move(pointX, pointY, baseWindow);
                                }
                            });

                            if (!pointFound && inWindowBounds)
                            {
                                points.Add(new Point(pointX, pointY));
                            }
                        } 
                        else if (mouseState.RightButton == ButtonState.Pressed)
                        {
                            points.RemoveAll((point) => point.InBounds(pointX, pointY));
                        }
                    }

                    if (!drawing)
                    {
                        points.ForEach((point) => {
                            point.SetMovingState(false);
                        });
                    }

                    TextUtility.textRenderer.Clear(Color.Transparent);

                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Color3(Color.LightGray);
                    points.ForEach((point) => {
                        GL.Vertex2(point.position);
                    });
                    GL.End();

                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Color3(Color.DarkRed);
                    if (points.Count >= 4)
                    {
                        (new BSpline(segments, points)).Draw(baseWindow);
                    }
                    GL.End();

                    points.ForEach((point) => {
                        baseWindow.Identity();
                        point.Draw(baseWindow);
                    });

                    index = 0;
                    points.ForEach((point) => {
                        baseWindow.Identity();
                        point.DrawInfo(baseWindow, index++);
                    });

                    TextUtility.Render(width, height);
                });
            }
        }
    }


    class BSpline : Spline
    {
        public int segments = Program.segments;

        public BSpline(int _segments, List<Point> _points) : base (_points)
        {
            segments = _segments;
        }

        public float[] Poses(int index, bool xAxis = true, int length = 4)
        {
            var result = new float[length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = xAxis ? points[index + i].position.X : points[index + i].position.Y;
            }

            return result;
        }

        public float[] Coeffs(float[] x)
        {
            return new float[] {
                (x[0] + 4 * x[1] + x[2]) / 6,
                (x[2] - x[0]) / 2,
                (x[0] - 2 * x[1] + x[2]) / 2,
                (-x[0] + 3 * (x[1] - x[2]) + x[3]) / 6
            };
        }

        public float Coord(float[] a, float t)
        {
            return ((a[3] * t + a[2]) * t + a[1]) * t + a[0];
        }

        public override void Draw(BaseWindow window)
        {
            float[] a, b;
            float t;

            for (int i = 1; i < points.Count - 2; i++)
            {
                a = Coeffs(Poses(i - 1));
                b = Coeffs(Poses(i - 1, false));

                for (int j = 0; j <= segments; j++)
                {
                    t = (float) j / segments;

                    GL.Vertex2(
                        Coord(a, t),
                        Coord(b, t)
                    );
                }
            }
        }
    }

    abstract class Spline : Drawable
    {
        public List<Point> points;
        
        public Spline(List<Point> _points)
        {
            points = _points;
        }
    }

    public class Point : Drawable
    {
        public static float width = 20;
        public static float height = 20;
        public static float padding = 3;

        public int index = 0;
        public bool MovingState { get; set; }
        public Vector2 MovingDelta { get; set; }

        public Point(Vector2 _position)
        {
            position = _position;
        }

        public Point(float x, float y)
        {
            position = new Vector2(x, y);
        }

        public Point DrawInfo(BaseWindow window, int index = 0)
        {
            TextUtility.textRenderer.DrawString("[" + index + "]: " + position.X + ", " + position.Y, TextUtility.serif, new SolidBrush(Color.Black), new PointF(position.X + width / 2 + padding, window.Height - position.Y + height / 2 + padding));
            return this;
        }

        public Point Move(Vector2 point, BaseWindow window = null)
        {
            position = point - MovingDelta;

            if (window != null)
            {
                if (position.X < width / 2)
                {
                    position.X = width / 2;
                }
                if (position.Y < height / 2)
                {
                    position.Y = height / 2;
                }
                if (position.X > window.Width - width / 2)
                {
                    position.X = window.Width - width / 2;
                }
                if (position.Y > window.Height - height / 2)
                {
                    position.Y = window.Height - height / 2;
                }
            }

            return this;
        }

        public Point Move(float x, float y, BaseWindow window = null)
        {
            Move(new Vector2(x, y), window);
            return this;
        }

        public Point SetMovingState(bool state)
        {
            MovingState = state;
            return this;
        }

        public Point SetMovingDelta(Vector2 point)
        {
            MovingDelta = new Vector2(point.X - position.X, point.Y - position.Y);
            return this;
        }

        public Point SetMovingDelta(float x, float y)
        {
            SetMovingDelta(new Vector2(x, y));
            return this;
        }

        public bool InBounds(Vector2 point)
        {
            var x = point.X - position.X;
            var y = point.Y - position.Y;

            return x >= -width / 2 && x <= width / 2 && y >= -height / 2 && y <= height / 2;
        }

        public bool InBounds(float x, float y)
        {
            return InBounds(new Vector2(x, y));
        }

        public override void Draw(BaseWindow window)
        {
            GL.Color3(Color.Black);

            GL.Translate(new Vector3(position));

            DrawingUtility.Quad(new Vector2(width, height));

            if (MovingState)
            {
                DrawingUtility.Quad(new Vector2(width / 2, height / 2));
            }
        }
    }

    public abstract class Drawable
    {
        public Vector2 position;

        public abstract void Draw(BaseWindow window);
    }

    public class DrawingUtility
    {
        public static void Quad(Vector2 size, bool filled = false, Vector2? origin = null)
        {
            if (!origin.HasValue)
            {
                origin = new Vector2(0.5F, 0.5F);
            }

            Vector2 _origin = origin.Value;

            GL.Begin(filled ? PrimitiveType.Quads : PrimitiveType.LineLoop);

            GL.Vertex2(-size.X * (1 - _origin.X), -size.Y * (1 - _origin.Y));
            GL.Vertex2(size.X * _origin.X, -size.Y * (1 - _origin.Y));
            GL.Vertex2(size.X * _origin.X, size.Y * _origin.Y);
            GL.Vertex2(-size.X * (1 - _origin.X), size.Y * _origin.Y);

            GL.End();
        }

        public static void Quad(float size = 1, bool filled = false, Vector2? origin = null)
        {
            Quad(new Vector2(size, size), filled, origin);
        }

        public static void Quad(Vector2 position, Vector2 size, bool filled = false, Vector2? origin = null)
        {
            GL.Translate(new Vector3(position));
            Quad(size, filled, origin);
        }

        public static void Quad(Vector2 position, float size, bool filled = false, Vector2? origin = null)
        {
            GL.Translate(new Vector3(position));
            Quad(size, filled, origin);
        }

        public static void Quad(float x, float y, float size = 1, bool filled = false, Vector2? origin = null)
        {
            GL.Translate(new Vector3(x, y, 0));
            Quad(size, filled, origin);
        }
    }

    public class TextUtility
    {
        public static TextRenderer textRenderer;

        public static Font serif = new Font(FontFamily.GenericSerif, 14);
        public static Font sans = new Font(FontFamily.GenericSansSerif, 14);
        public static Font mono = new Font(FontFamily.GenericMonospace, 14);

        public static void Render(int width, int height)
        {
            // GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            GL.MatrixMode(MatrixMode.Modelview);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BindTexture(TextureTarget.Texture2D, textRenderer.Texture);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(0, 0);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(width, 0);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(width, height);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(0, height);

            GL.End();

            // GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
        }
    }

    public class BaseWindow : GameWindow
    {
        Stopwatch watch;

        public BaseWindow() : base(800, 600)
        {
            Title = "B-Spline Editor © Savin M. PRI-117. CGP_L8";

            Load += (sender, e) => {
                VSync = VSyncMode.On;
                GL.Enable(EnableCap.Texture2D);
            };

            Resize += (sender, e) => {
                GL.Viewport(0, 0, Width, Height);
                GL.MatrixMode(MatrixMode.Projection);

                Ortho();

                TextUtility.textRenderer = new TextRenderer(Width, Height);
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

        public void Identity()
        {
            GL.LoadIdentity();
            GL.Translate(-Width / 2, -Height / 2, -100);
        }

        public void Render(Action<double> callback)
        {
            RenderFrame += (sender, e) =>
            {
                Identity();

                GL.ClearColor(Color.White);
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
