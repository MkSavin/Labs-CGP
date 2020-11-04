using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CGP_L6_Savin_M
{
    class Program
    {
        public static LayerManager layerManager;
        public static BrushManager brushManager;

        public static Random rnd = new Random();
        public static DataOption SetDefaultSizeOption;
        public static DataOption SetDefaultColorOption;

        [STAThread]
        static void Main(string[] args)
        {
            using (var baseWindow = new BaseWindow())
            {
                int width = baseWindow.Width, height = baseWindow.Height;

                int pointX, pointY;
                MouseState mouseState;

                layerManager = new LayerManager();
                brushManager = new BrushManager();

                layerManager
                    .Add(new Layer(width, height))
                    .Add(new Layer(width, height))
                    .Add(new Layer(width, height));

                brushManager
                    .Add(new Brush("assets/brushes/circle.png"))
                    .Add(new Brush("assets/brushes/circle.png").SetIsErase(true))
                    .Add(new Brush("assets/brushes/curve.png"))
                    .Add(new Brush("assets/brushes/name.png"));

                var brushSizeIndex = 1;
                var brushSizes = new float[] { .1f, .2f, .3f, .5f, .7f, 1f, 1.2f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 6f, 8f, 10f, 12f, 15f, 20f };

                DataOption sizeOption;
                DataOption colorOption;

                OptionsBar optionsBar = new OptionsBar()
                    .AddOptionsList(
                        new OptionsList("Layers", LayerOption.LayersToOptions(layerManager))
                            .SetWidth(120)
                            .SetNextKey(Key.L)
                        )
                    .AddOptionsList(
                        new OptionsList("Brushes", BrushOption.BrushesToOptions(brushManager))
                            .SetWidth(130)
                            .SetNextKey(Key.B)
                    );
                optionsBar
                    .AddOptionsList(
                        new OptionsList("Options", new List<ActionOption>() {
                                (ActionOption)new ActionOption()
                                    .SetTitle("Save")
                                    .SetShortCutText("ctrl+s")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.S);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        SaveFileDialog dialog = new SaveFileDialog();
                                        dialog.Filter = "png files (*.png)|*.png"  ;

                                        if (dialog.ShowDialog() == DialogResult.OK && dialog.FileName != "")
                                        {
                                            Bitmap bmp = layerManager.ToBitmap();
                                            bmp.Save(dialog.OpenFile(), System.Drawing.Imaging.ImageFormat.Png);
                                        }
                                    }),
                                (ActionOption)new ActionOption()
                                    .SetTitle("Open")
                                    .SetShortCutText("ctrl+o")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.O);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        OpenFileDialog dialog = new OpenFileDialog();
                                        
                                        if (dialog.ShowDialog() == DialogResult.OK && dialog.FileName != "")
                                        {
                                            Bitmap bmp = new Bitmap(dialog.OpenFile());

                                            baseWindow.Width = bmp.Width;
                                            baseWindow.Height = bmp.Height;

                                            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                                            OptionsList list;
                                            if ((list = optionsBar.GetOptionsListByTitle("Layers")) != null)
                                            {
                                                layerManager
                                                    .Set(new List<Layer>() { new Layer(bmp) })
                                                    .Reindex();

                                                list.SetOptions(LayerOption.LayersToOptions(layerManager));
                                            }
                                        }
                                    }),
                                (ActionOption)new ActionOption()
                                    .SetTitle("New Layer")
                                    .SetShortCutText("ctrl+h+n")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.H) && keyboard.IsKeyDown(Key.N);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        OptionsList list;
                                        if ((list = optionsBar.GetOptionsListByTitle("Layers")) != null)
                                        {
                                            layerManager
                                                .Add(new Layer(width, height))
                                                .Reindex();
                                            list.SetOptions(LayerOption.LayersToOptions(layerManager));
                                        }
                                    }),
                                (ActionOption)new ActionOption()
                                    .SetTitle("Delete Layer")
                                    .SetShortCutText("ctrl+h+d")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.H) && keyboard.IsKeyDown(Key.D);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        OptionsList list;
                                        if ((list = optionsBar.GetOptionsListByTitle("Layers")) != null)
                                        {
                                            if (layerManager.Elements.Count > 1)
                                            {
                                                layerManager
                                                    .Delete()
                                                    .Reindex();
                                                list.SetOptions(LayerOption.LayersToOptions(layerManager));
                                            }
                                        }
                                    })
                            })
                            .SetShowPointer(false)
                            .SetWidth(210)
                        )
                    .AddOptionsList(
                        new OptionsList("Brush", new List<DataOption>() {
                                (sizeOption = (DataOption)new DataOption()
                                    .SetTitle("Size")
                                    .SetShortCutText("ctrl+r+t")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.R) && keyboard.IsKeyDown(Key.T);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        brushSizeIndex++;
                                        brushSizeIndex = brushSizeIndex % brushSizes.Length;
                                        brushManager.Current.Size = brushSizes[brushSizeIndex];
                                        option.Value.Text = brushManager.Current.Size + "";
                                    })
                                    .SetValue(new OptionValue().SetText("0,2"))),
                                (SetDefaultSizeOption = (DataOption)new DataOption()
                                    .SetTitle("Set Default Size")
                                    .SetShortCutText("ctrl+r+y")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.R) && keyboard.IsKeyDown(Key.Y);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        brushSizeIndex = 1;
                                        brushManager.Current.Size = brushSizes[brushSizeIndex];
                                        sizeOption.Value.Text = brushManager.Current.Size + "";
                                    })),
                                (colorOption = (DataOption)new DataOption()
                                    .SetTitle("Color")
                                    .SetShortCutText("ctrl+c+a")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.C) && keyboard.IsKeyDown(Key.A);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        brushManager.Current.Color = Color.FromArgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                                        option.Value.Color = brushManager.Current.Color;
                                    })
                                    .SetValue(new OptionValue().SetColor(Color.Red))),
                                (SetDefaultColorOption = (DataOption)new DataOption()
                                    .SetTitle("Set Default Color")
                                    .SetShortCutText("ctrl+c+d")
                                    .SetWillShortcutWork((keyboard, _) => {
                                        return keyboard.IsKeyDown(Key.ControlLeft) && keyboard.IsKeyDown(Key.C) && keyboard.IsKeyDown(Key.D);
                                    })
                                    .SetOnSet((lastIndex, newIndex, option) => {
                                        brushManager.Current.Color = Color.Red;
                                        colorOption.Value.Color = brushManager.Current.Color;
                                    }))
                            })
                            .SetShowPointer(false)
                            .SetWidth(150)
                    );

                baseWindow.KeyDown += (sender, e) =>
                {
                    optionsBar.Controls();
                };

                baseWindow.Resize += (sender, e) =>
                {
                    if (baseWindow.Width != 0 && baseWindow.Height != 0)
                    {
                        TextUtility.textRenderer = new TextRenderer(baseWindow.Width, baseWindow.Height);
                    }
                };

                var isDrawing = false;
                Vector2 lastDrawingPosition = new Vector2();
                int drawingXMin = 0, drawingXMax = 0, drawingYMin = 0, drawingYMax = 0;

                baseWindow.Render((deltaTick) =>
                {
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    mouseState = Mouse.GetCursorState();

                    if (mouseState.LeftButton == OpenTK.Input.ButtonState.Pressed)
                    {
                        pointX = mouseState.X - baseWindow.Location.X - 8;
                        pointY = baseWindow.Height - (mouseState.Y - baseWindow.Y - 32);

                        if (pointX >= 0 && pointY >= 0 && pointX < baseWindow.Width && pointY < baseWindow.Height)
                        {
                            if (!isDrawing)
                            {
                                isDrawing = true;

                                layerManager.Current.DrawBrush(brushManager.Current, pointX, pointY);

                                lastDrawingPosition = new Vector2(pointX, pointY);
                            }
                            else
                            {
                                drawingXMin = (int)Math.Min(pointX, lastDrawingPosition.X);
                                drawingXMax = (int)Math.Max(pointX, lastDrawingPosition.X);
                                drawingYMin = (int)Math.Min(pointY, lastDrawingPosition.Y);
                                drawingYMax = (int)Math.Max(pointY, lastDrawingPosition.Y);

                                if (pointX != (int)lastDrawingPosition.X)
                                {
                                    for (int x = drawingXMin; x < drawingXMax; x++)
                                    {
                                        layerManager.Current.DrawBrush(brushManager.Current, x, (int)Math.Round((pointY - (double)lastDrawingPosition.Y) / (pointX - (double)lastDrawingPosition.X) * (x - (double)lastDrawingPosition.X) + (double)lastDrawingPosition.Y));
                                    }
                                } 
                                else
                                {
                                    for (int y = drawingYMin; y < drawingYMax; y++)
                                    {
                                        layerManager.Current.DrawBrush(brushManager.Current, pointX, y);
                                    }
                                }

                                layerManager.Current.DrawBrush(brushManager.Current, pointX, pointY);

                                lastDrawingPosition = new Vector2(pointX, pointY);
                            }
                        }
                    } 
                    else
                    {
                        isDrawing = false;
                    }

                    layerManager.Render();
                    optionsBar.Render(baseWindow, deltaTick);
                    TextUtility.Render(width, height);
                });
            }
        }
    }

    public class BrushManager : Manager<Brush>
    {

    }

    public class Brush : Indexed
    {
        public Color Color { get; set; } = Color.Red;
        public float Size { get; set; } = .2f;
        public string Name { get; set; } = "";
        public Bitmap Bitmap { get; set; }

        public Vector2 Origin { get; set; } = new Vector2(.5f, .5f);

        public bool IsErase { get; set; } = false;

        public Brush(string path)
        {
            LoadBrush(path);
            Name = Path.GetFileNameWithoutExtension(path);
        }

        public void LoadBrush(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            Bitmap = (Bitmap) Bitmap.FromFile(path);
        }

        public Brush SetIsErase(bool value)
        {
            IsErase = value;

            return this;
        }

        public BrushOption GetOption(BrushManager manager = null)
        {
            BrushOption bo = (BrushOption)new BrushOption().SetTitle(IsErase ? "erase" : Name != "" ? Name : "Brush " + Index);
            if (manager != null)
            {
                bo.SetOnSet((last, current, option) => {
                    manager.CurrentElement = current;
                    Program.SetDefaultSizeOption.OnSet(last, current);
                    Program.SetDefaultColorOption.OnSet(last, current);
                });
            }
            return bo;
        }
    }

    public class LayerManager : Manager<Layer>
    {
        public Bitmap ToBitmap()
        {
            if (Elements.Count == 0)
            {
                return null;
            }

            var width = Elements[0].width;
            var height = Elements[0].height;

            var bmp = new Bitmap(width, height);
            Color lastColor, color;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmp.SetPixel(x, y, Color.White);
                }
            }

            foreach (var layer in Elements)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        lastColor = bmp.GetPixel(x, height - 1 - y);
                        color = layer.Bitmap.GetPixel(x, y);

                        if (color.A != 0)
                        {
                            bmp.SetPixel(x, height - 1 - y, Color.FromArgb(Math.Max(color.A, lastColor.A), color.R, color.G, color.B));
                        }
                    }
                }
            }
            
            return bmp;
        }

        public void Render()
        {
            var index = 0;
            foreach (var layer in Elements)
            {
                layer.Render(index++);
            }
        }
    }

    public class Manager<T> where T : Indexed
    {
        public int CurrentElement;

        public List<T> Elements { get; set; } = new List<T>();

        public Manager<T> Set(List<T> value)
        {
            Elements = value;
            return this;
        }
        public Manager<T> Add(T value)
        {
            value.Index = Elements.Count;
            Elements.Add(value);
            return this;
        }
        public Manager<T> Delete(int element)
        {
            Elements.RemoveAt(CurrentElement);

            if (CurrentElement == element)
            {
                CurrentElement = 0;
            }

            return this;
        }
        public Manager<T> Delete()
        {
            return Delete(CurrentElement);
        }
        public Manager<T> Reindex()
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Index = i;
            }
            return this;
        }
        public Manager<T> Next()
        {
            CurrentElement++;
            CurrentElement %= Elements.Count;
            return this;
        }
        public Manager<T> Last()
        {
            CurrentElement--;
            while (CurrentElement > 0)
            {
                CurrentElement += Elements.Count;
            }
            return this;
        }

        public T Current
        {
            get
            {
                if (CurrentElement >= Elements.Count || CurrentElement < 0)
                {
                    return null;
                }

                return Elements[CurrentElement];
            }
        }
    }

    public class Layer : Indexed
    {
        public Bitmap Bitmap { get; set; }
        public int Texture { get; set; }

        public int width, height;

        System.Drawing.Imaging.BitmapData data;

        public Layer(int width, int height)
        {
            Bitmap = new Bitmap(width, height);
            Texture = GL.GenTexture();

            this.width = width;
            this.height = height;
        }

        public Layer(Bitmap bitmap)
        {
            Bitmap = bitmap;
            Texture = GL.GenTexture();

            this.width = bitmap.Width;
            this.height = bitmap.Height;
        }


        public LayerOption GetOption(LayerManager manager = null)
        {
            LayerOption bo = (LayerOption)new LayerOption().SetTitle(Index == 0 ? "Main" : "Layer " + Index);
            if (manager != null)
            {
                bo.SetOnSet((last, current, option) => {
                    manager.CurrentElement = current;
                    Program.SetDefaultSizeOption.OnSet(last, current);
                    Program.SetDefaultColorOption.OnSet(last, current);
                });
            }
            return bo;
        }

        public void DrawBrush(Brush brush, int pointX, int pointY)
        {
            var bmp = brush.Bitmap;
            Color color, lastColor;
            Vector2 position;

            for (int x = 0; x < bmp.Width * brush.Size; x++)
            {
                for (int y = 0; y < bmp.Height * brush.Size; y++)
                {
                    color = bmp.GetPixel((int)(x / brush.Size), bmp.Height - 1 - (int)(y / brush.Size));

                    position = new Vector2((int)(pointX + x - bmp.Width * brush.Size * brush.Origin.X), (int)(pointY + y - bmp.Height * brush.Size * brush.Origin.Y));

                    if (position.X >= 0 && position.X < Bitmap.Width && position.Y >= 0 && position.Y < Bitmap.Height && color.A > 0)
                    {
                        lastColor = Bitmap.GetPixel((int)position.X, (int)position.Y);
                        if (!brush.IsErase)
                        {
                            Bitmap.SetPixel((int)position.X, (int)position.Y, Color.FromArgb(Math.Max(color.A, lastColor.A), brush.Color.R, brush.Color.G, brush.Color.B));
                        }
                        else
                        {
                            Bitmap.SetPixel((int)position.X, (int)position.Y, Color.FromArgb(255 - Math.Max(color.A, lastColor.A), 255, 255, 255));
                        }
                    }
                }
            }
        }


        public void Render(int index = 0)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.BindTexture(TextureTarget.Texture2D, Texture);

            data = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            Bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, index);
            GL.TexCoord2(1, 0);
            GL.Vertex3(width, 0, index);
            GL.TexCoord2(1, 1);
            GL.Vertex3(width, height, index);
            GL.TexCoord2(0, 1);
            GL.Vertex3(0, height, index);
            GL.End();

            GL.Disable(EnableCap.Blend);
        }
    }

    public class Indexed
    {
        public int Index = 0;
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

    public class OptionsBar
    {
        public List<OptionsList> OptionsLists { get; set; } = new List<OptionsList>();

        public Font font = TextUtility.serif;

        public OptionsBar() { }

        public OptionsBar(List<OptionsList> value) { 
            OptionsLists = value;
        }

        public OptionsBar SetOptionsLists(List<OptionsList> value)
        {
            OptionsLists = value;
            return this;
        }
        public OptionsBar AddOptionsList(OptionsList value)
        {
            OptionsLists.Add(value);
            return this;
        }
        public OptionsList GetOptionsListByTitle(string title)
        {
            return OptionsLists.Find((optionsList) => optionsList.Title == title);
        }

        public OptionsBar AddOptionsList(Func<OptionsList> action)
        {
            OptionsLists.Add(action.Invoke());
            return this;
        }

        public Color textColor = Color.FromArgb(38, 42, 43);

        public void Render(BaseWindow window, double deltaTick)
        {
            TextUtility.textRenderer.Clear(Color.Transparent);

            GL.Color3(1f, 1f, 1f);

            PointF position = PointF.Empty;
            string optionText;
            int optionIndex;

            foreach (var optionsList in OptionsLists)
            {

                position.Y = 0;

                if (optionsList.Title != "")
                {
                    TextUtility.textRenderer.DrawString(optionsList.Title + (optionsList.NextKey != null ? " [" + optionsList.NextKey.Value + " >]" : "") + ":", font, new SolidBrush(textColor), position);
                    position.Y += font.Height;
                }
                
                optionIndex = 0;

                foreach (var option in optionsList.Options)
                {
                    optionText = "";

                    if (optionsList.ShowPointer)
                    {
                        if (optionsList.CurrentOption == optionIndex)
                        {
                            optionText += "[x] ";
                        }
                        else
                        {
                            optionText += "[  ] ";
                        }
                    }

                    if (option.ShortCutText != "")
                    {
                        optionText += "[" + option.ShortCutText + "] ";
                    }

                    optionText += option.Title;

                    if (option.Value != null)
                    {
                        if (option.Value.Text != "")
                        {
                            optionText += ": " + option.Value.Text;
                        } 
                        else if (option.Value.Color.HasValue)
                        {
                            optionText += ":";
                        }
                    }

                    if (optionText != "")
                    {
                        var textSize = TextUtility.textRenderer.DrawString(optionText, font, new SolidBrush(textColor), position);

                        if (option.Value != null && option.Value.Color.HasValue)
                        {
                            optionText = "█";
                            
                            TextUtility.textRenderer.DrawString(optionText, font, new SolidBrush(option.Value.Color.Value), new PointF(position.X + textSize.Width, position.Y));
                        }

                        position.Y += font.Height;
                    }

                    optionIndex++;
                }

                position.X += optionsList.Width;
            }
        }

        public void Controls()
        {
            var keyboard = Keyboard.GetState();

            if (!keyboard.IsAnyKeyDown)
            {
                return;
            }

            foreach (var optionsList in OptionsLists)
            {
                if (optionsList.Controls(keyboard))
                {
                    continue;
                }

                if (optionsList.NextKey.HasValue && keyboard.IsKeyDown(optionsList.NextKey.Value))
                {
                    optionsList.Next();
                }
                if (optionsList.LastKey.HasValue && keyboard.IsKeyDown(optionsList.LastKey.Value))
                {
                    optionsList.Last();
                }
            }
        }
    }

    public class OptionsList
    {
        public string Title { get; set; } = "";
        public Vector2 Position { get; set; } = Vector2.Zero;
        public List<ParametrizedOption> Options { get; set; } = new List<ParametrizedOption>();
        public Key? NextKey { get; set; } = null;
        public Key? LastKey { get; set; } = null;
        public int CurrentOption { get; set; } = 0;
        public bool ShowPointer { get; set; } = true;

        public int Width { get; set; } = 100;

        public OptionsList(string title, List<ParametrizedOption> options)
        {
            Title = title;
            Options = options;
        }

        public OptionsList(string title, List<LayerOption> options) : this(title, options.ToList<ParametrizedOption>()) { }
        public OptionsList(string title, List<BrushOption> options) : this(title, options.ToList<ParametrizedOption>()) { }
        public OptionsList(string title, List<ActionOption> options) : this(title, options.ToList<ParametrizedOption>()) { }
        public OptionsList(string title, List<DataOption> options) : this(title, options.ToList<ParametrizedOption>()) { }

        public OptionsList SetTitle(string value)
        {
            Title = value;
            return this;
        }
        public OptionsList SetPosition(Vector2 value)
        {
            Position = value;
            return this;
        }
        public OptionsList SetOptions(List<ParametrizedOption> value)
        {
            Options = value;
            return this;
        }
        public OptionsList SetOptions(List<LayerOption> value)
        {
            return SetOptions(value.ToList<ParametrizedOption>());
        }
        public OptionsList AddOption(ParametrizedOption value)
        {
            Options.Add(value);
            return this;
        }
        public OptionsList SetNextKey(Key? value)
        {
            NextKey = value;
            return this;
        }
        public OptionsList SetLastKey(Key? value)
        {
            LastKey = value;
            return this;
        }
        public OptionsList SetCurrentOption(int value)
        {
            CurrentOption = value;
            return this;
        }
        public OptionsList SetShowPointer(bool value)
        {
            ShowPointer = value;
            return this;
        }
        public OptionsList SetWidth(int value)
        {
            Width = value;
            return this;
        }

        public void Next()
        {
            if (Options.Count == 0)
            {
                return;
            }

            var lastOption = CurrentOption;
            CurrentOption++;
            CurrentOption = CurrentOption % Options.Count;

            Options[CurrentOption].OnSet(lastOption, CurrentOption);
        }

        public void Last()
        {
            if (Options.Count == 0)
            {
                return;
            }

            var lastOption = CurrentOption;
            CurrentOption--;

            while (CurrentOption < 0)
            {
                CurrentOption += Options.Count;
            }

            Options[CurrentOption].OnSet(lastOption, CurrentOption);
        }

        public bool Controls(KeyboardState keyboard)
        {
            int index = 0;
            foreach (var option in Options)
            {
                if (option.WillShortcutWork(keyboard))
                {
                    option.OnSet(CurrentOption, index);
                    CurrentOption = index;
                    return true;
                }

                index++;
            }

            return false;
        }
    }

    public class DataOption : ParametrizedOption {
    }

    public class ActionOption : ParametrizedOption {
    }

    public class BrushOption : ParametrizedOption
    {
        public static List<BrushOption> BrushesToOptions(List<Brush> brushes) => brushes.Select((brush) => brush.GetOption()).ToList();
        public static List<BrushOption> BrushesToOptions(BrushManager brushManager) => brushManager.Elements.Select((brush) => brush.GetOption(brushManager)).ToList();
    }

    public class LayerOption : ParametrizedOption
    {
        public static List<LayerOption> LayersToOptions(List<Layer> layers) => layers.Select((layer) => layer.GetOption()).ToList();
        public static List<LayerOption> LayersToOptions(LayerManager layerManager) => layerManager.Elements.Select((layer) => layer.GetOption(layerManager)).ToList();
    }

    public class ParametrizedOption : Option<ParametrizedOption>
    {

        Action<int, int, ParametrizedOption> onSet;
        Func<KeyboardState, ParametrizedOption, bool> willShortcutWork;

        public ParametrizedOption SetOnSet(Action<int, int, ParametrizedOption> onSet)
        {
            this.onSet = onSet;
            return this;
        }

        public ParametrizedOption SetWillShortcutWork(Func<KeyboardState, ParametrizedOption, bool> willShortcutWork)
        {
            this.willShortcutWork = willShortcutWork;
            return this;
        }

        public override void OnSet(int lastIndex, int currentIndex)
        {
            if (onSet == null)
            {
                return;
            }

            onSet.Invoke(lastIndex, currentIndex, this);
        }

        public override bool WillShortcutWork(KeyboardState keyboard)
        {
            if (willShortcutWork == null)
            {
                return false;
            }


            return willShortcutWork.Invoke(keyboard, this);
        }
    }

    public abstract class Option<TOption> where TOption : Option<TOption>
    {
        public string Title { get; set; } = "";
        public OptionValue Value { get; set; }

        public string ShortCutText { get; set; } = "";

        public TOption SetTitle(string value)
        {
            Title = value;
            return (TOption)this;
        }
        public TOption SetValue(OptionValue value)
        {
            Value = value;
            return (TOption)this;
        }
        public TOption SetShortCutText(string value)
        {
            ShortCutText = value;
            return (TOption)this;
        }

        public abstract void OnSet(int lastIndex, int currentIndex);
        public abstract bool WillShortcutWork(KeyboardState keyboard);
    }

    public class OptionValue
    {
        public string Text { get; set; } = "";
        public Color? Color { get; set; }

        public OptionValue SetText(string value)
        {
            Text = value;
            return this;
        }
        public OptionValue SetColor(Color? value)
        {
            Color = value;
            return this;
        }
    }

    public class BaseWindow : GameWindow
    {
        Stopwatch watch;

        public BaseWindow() : base(800, 600)
        {
            Title = "Micro-Editor";

            Load += (sender, e) => {
                VSync = VSyncMode.On;
                // GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Texture2D);
            };

            Resize += (sender, e) => {
                GL.Viewport(0, 0, Width, Height);
                GL.MatrixMode(MatrixMode.Projection);

                Identity();
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

        public void Identity()
        {
            GL.LoadIdentity();
            Matrix4 p = Matrix4.CreateOrthographic(Width, Height, 1.0f, 1000.0f);
            GL.LoadMatrix(ref p);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void Render(Action<double> callback)
        {
            RenderFrame += (sender, e) =>
            {
                GL.LoadIdentity();
                GL.ClearColor(Color.White);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                watch.Stop();
                GL.Translate(-Width / 2, -Height / 2, -100);
                callback.Invoke(watch.ElapsedMilliseconds / 1000F);
                watch.Restart();

                SwapBuffers();
            };

            Run();
        }
    }
}
