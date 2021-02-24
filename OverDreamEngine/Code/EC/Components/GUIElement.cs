using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODEngine.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using static ODEngine.Core.ImageLoader;

namespace ODEngine.EC.Components
{
    public class GUIElement : Component
    {
        public Renderer renderer;
        public Material material = null;

        public bool isEnable = true;
        public bool mouseOnElement = false;

        public CommitSet<MouseButton> pressedButtons = new CommitSet<MouseButton>(); // What buttons is used

        public float threshold = -1f; // Alpha color threshold
        public bool childsProcessing = true;
        public RawImage mask = default;

        public Func<bool> isLoaded = null;
        public bool IsLoaded { get => isLoaded?.Invoke() ?? true; }

        protected override void OnCreate()
        {
            renderer = entity.CreateComponent<Renderer>();
        }

        protected override void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }

        private event Action OnDestroyEvent = null;

        public event EventHandler<(Vector2 mousePosition, MouseButton mouseButton)> MouseClick;
        public event EventHandler<(Vector2 mousePosition, MouseButton mouseButton)> MouseDown;
        public event EventHandler<(Vector2 mousePosition, MouseButton mouseButton)> MouseUp;
        public event EventHandler<Vector2> MouseEnter;
        public event EventHandler<Vector2> MouseLeave;
        public event EventHandler<Vector2> MouseMove;

        public void MouseUpdate(Vector2 mousePosition, bool mouseOnElement)
        {
            if (mouseOnElement)
            {
                mouseOnElement = CheckMask(mousePosition / 2f + new Vector2(0.5f));
            }

            // Нажатие кнопок
            pressedButtons.SlateForRemoval(Input.mouseUps);
            if (mouseOnElement)
            {
                foreach (var item in Input.mouseUps)
                {
                    MouseUp?.Invoke(this, (mousePosition, item));
                }
                foreach (var item in pressedButtons.listToRemove)
                {
                    MouseClick?.Invoke(this, (mousePosition, item));
                }
                pressedButtons.SlateForAdding(Input.mouseDowns);
                foreach (var item in Input.mouseDowns)
                {
                    //Kernel.screenManager.consoleScreen.Print("MouseDown: " + renderer.position.ToString() + " " + name + "; " + renderer.name);
                    MouseDown?.Invoke(this, (mousePosition, item));
                }
            }
            pressedButtons.Commit();

            // Наведение на кнопку
            if (this.mouseOnElement != mouseOnElement)
            {
                if (mouseOnElement)
                {
                    MouseEnter?.Invoke(this, mousePosition);
                }
                else
                {
                    MouseLeave?.Invoke(this, mousePosition);
                }
                this.mouseOnElement = mouseOnElement;
            }
            if (mouseOnElement)
            {
                MouseMove?.Invoke(this, mousePosition);
            }
        }

        public Task CreateMask(RawImage rawImage)
        {
            return Task.Run(() =>
            {
                mask = rawImage.Clone();
            });
        }

        public bool CheckMask(Vector2 position)
        {
            if (mask.data == IntPtr.Zero)
            {
                return true;
            }

            int x = Math.Clamp(MathHelper.RoundToInt(position.X * (mask.width - 1)), 0, mask.width - 1);
            int y = Math.Clamp(MathHelper.RoundToInt(position.Y * (mask.height - 1)), 0, mask.height - 1);

            unsafe
            {
                return *(byte*)(mask.data + (x + y * mask.width) * 4 + 3) > threshold * 255f;
            }
        }

        public static GUIElement CreateEmpty(Renderer parent, Vector3 position, Vector2 size)
        {
            var guiElement = new Entity().CreateComponent<GUIElement>();
            guiElement.renderer.SetParent(parent);
            guiElement.renderer.position = position;
            guiElement.renderer.size = size;
            return guiElement;
        }

        public static GUIElement CreateImage(Renderer parent, Vector3 position, Vector2 size, string imageFile)
        {
            return CreateImage(parent, position, size, imageFile, ColorMatrix.Identity, null);
        }

        public static GUIElement CreateImage(Renderer parent, Vector3 position, Vector2 size, string imageFile, Material material)
        {
            return CreateImage(parent, position, size, imageFile, ColorMatrix.Identity, material);
        }

        public static GUIElement CreateImage(Renderer parent, Vector3 position, Vector2 size, string imageFile, float alpha = 1f, Material material = null)
        {
            var colorMatrix = ColorMatrix.Identity;
            colorMatrix.M44 = alpha;
            return CreateImage(parent, position, size, imageFile, colorMatrix, material);
        }

        public static GUIElement CreateImage(Renderer parent, Vector3 position, Vector2 size, string imageFile, ColorMatrix colorMatrix, Material material = null)
        {
            imageFile = PathBuilder.dataPath + "Images/" + imageFile + ".png";
            var guiElement = CreateEmpty(parent, position, size);
            var texTicket = GPUTextureLoader.LoadAsync(imageFile, colorMatrix);
            guiElement.material = material;
            guiElement.renderer.onRender = (input, output) =>
            {
                if (texTicket.texture != null)
                {
                    Graphics.Blit(texTicket.texture, output, material);
                }
            };
            guiElement.isLoaded = () => texTicket.isLoaded;
            guiElement.OnDestroyEvent += () => { texTicket.isCanceled = true; texTicket.texture?.Destroy(); };
            return guiElement;
        }

        public static GUIElement CreateImage(Renderer parent, Vector3 position, Vector2 size, RenderTexture texture, Material material = null)
        {
            var guiElement = CreateEmpty(parent, position, size);
            guiElement.renderer.onRender = (input, output) =>
            {
                if (texture != null)
                {
                    Graphics.Blit(texture, output, material);
                }
            };
            return guiElement;
        }

        public static GUIElement CreateImage(Renderer parent, Vector3 position, Vector2 size, SColor color)
        {
            var guiElement = CreateEmpty(parent, position, size);
            guiElement.renderer.onRender = (input, output) =>
            {
                Graphics.Clear(output, color);
            };
            return guiElement;
        }

        public static GUIElement CreateTransparent(Renderer parent, Vector3 position, Vector2 size)
        {
            var guiElement = CreateEmpty(parent, position, size);
            guiElement.renderer.onRender = (input, output) =>
            {
                Graphics.Clear(output);
            };
            return guiElement;
        }

        public static GUIElement CreateContainer(Renderer parent, Vector3 position, Vector2 size, string fragShader)
        {
            return CreateContainer(parent, position, size, new Material(fragShader, null, fragShader), true);
        }

        public static GUIElement CreateContainer(Renderer parent, Vector3 position, Vector2 size, Material material, bool destroyMaterialOnDestroy)
        {
            var guiElement = CreateEmpty(parent, position, size);
            guiElement.material = material;
            guiElement.renderer.onRender = (input, output) =>
            {
                Graphics.Blit(input, output, material);
            };
            if (destroyMaterialOnDestroy)
            {
                guiElement.OnDestroyEvent += () => { material.Destroy(); };
            }
            guiElement.childsProcessing = true;
            return guiElement;
        }

        public static GUIElement CreateFrameAnimation(Renderer parent, Vector3 position, Vector2 size, params (string imageFile, ColorMatrix colorMatrix, float time)[] images)
        {
            var tickets = new GPUTextureLoader.Ticket[images.Length];
            var timePoints = new float[images.Length + 1];
            timePoints[0] = 0f;

            var dict = new Dictionary<(string imageFile, ColorMatrix colorMatrix), int>(images.Length);
            for (int i = 0; i < images.Length; i++)
            {
                var image = images[i];
                if (!dict.TryGetValue((image.imageFile, image.colorMatrix), out var index))
                {
                    var imageFile = PathBuilder.dataPath + "Images/" + image.imageFile + ".png";
                    tickets[i] = GPUTextureLoader.LoadAsync(imageFile, image.colorMatrix);
                    timePoints[i + 1] = timePoints[i] + image.time;
                    dict.Add((image.imageFile, image.colorMatrix), i);
                }
                else
                {
                    tickets[i] = tickets[index];
                    timePoints[i + 1] = timePoints[i] + image.time;
                }
            }

            for (int i = 0; i < images.Length; i++)
            {
                var image = images[i];
                var imageFile = PathBuilder.dataPath + "Images/" + image.imageFile + ".png";
                tickets[i] = GPUTextureLoader.LoadAsync(imageFile, image.colorMatrix);
                timePoints[i + 1] = timePoints[i] + image.time;
            }

            var timeStart = DateTime.Now;
            var guiElement = CreateEmpty(parent, position, size);

            guiElement.renderer.onRender = (input, output) =>
            {
                for (int i = 1; i < timePoints.Length; i++)
                {
                    if ((DateTime.Now - timeStart).TotalSeconds % timePoints[^1] < timePoints[i])
                    {
                        if (tickets[i - 1].texture != null)
                        {
                            Graphics.Blit(tickets[i - 1].texture, output);
                        }
                        break;
                    }
                }
            };

            guiElement.isLoaded = () =>
            {
                for (int i = 0; i < tickets.Length; i++)
                {
                    if (!tickets[i].isLoaded)
                    {
                        return false;
                    }
                }
                return true;
            };

            guiElement.OnDestroyEvent += () =>
            {
                for (int i = 0; i < tickets.Length; i++)
                {
                    var ticket = tickets[i];
                    ticket.isCanceled = true;
                    ticket.texture?.Destroy();
                    ticket.texture = null;
                }
            };

            return guiElement;
        }

    }
}
