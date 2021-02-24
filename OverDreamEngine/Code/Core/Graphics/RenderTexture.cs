using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ODEngine.Core
{
    [Serializable]
    public class RenderTexture : ISerializable
    {
        [Serializable]
        public struct TextureParameters
        {
            public bool linearFiltering;
            public bool repeat;

            public TextureParameters(bool linearFiltering, bool repeat)
            {
                this.linearFiltering = linearFiltering;
                this.repeat = repeat;
            }

            public static TextureParameters GetStandard()
            {
                return new TextureParameters()
                {
                    linearFiltering = true,
                    repeat = false
                };
            }
        }

        private bool isInited = false;
        public bool IsInited { get => isInited; }

        private bool isAllocated = false;
        public bool IsAllocated { get => isAllocated; }

        private int textureID = -1;
        public int TextureID { get => isAllocated ? textureID : throw new Exception(); }

        private int framebufferID = -1;
        public int FramebufferID { get => isAllocated ? framebufferID : throw new Exception(); }

        private int width = -1;
        public int Width { get => width; }

        private int height = -1;
        public int Height { get => height; }

        public Vector2Int Size { get => new Vector2Int(width, height); }

        private TextureParameters parameters = TextureParameters.GetStandard();
        public TextureParameters Parameters { get => parameters; }

        private bool useMipMap = false;
        public bool UseMipMap
        {
            get => useMipMap;
            set
            {
                if (isAllocated)
                {
                    throw new Exception("Нельзя изменять использование мипмап, когда выделена память");
                }
                useMipMap = value;
            }
        }

        private int mipLevelCount;
        public int MipLevelCount
        {
            get => mipLevelCount;
        }

        private SizedInternalFormat format;

        private RenderTexture()
        {
            Init();
        }

        public RenderTexture(int width, int height, bool useMipMap = false, SizedInternalFormat format = SizedInternalFormat.Rgba8)
        {
            Init();
            AllocateMemory(width, height, useMipMap, format);
        }

        private RenderTexture(ImageLoader.RawImage rawImage, SizedInternalFormat format = SizedInternalFormat.Rgba8)
        {
            Init();
            AllocateMemory(rawImage.width, rawImage.height, false, format);
            LoadImage(rawImage);
        }

        public void Draw(int offsetX, int offsetY, int width, int height, byte[] data, PixelFormat pixelFormat = PixelFormat.Rgba)
        {
            if (this.width < offsetX + width || this.height < offsetY + height)
            {
                throw new Exception($"width: {this.width} {offsetX} {width}  height:  {this.height} {offsetY} {height}");
            }

            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, offsetX, offsetY, width, height, pixelFormat, PixelType.UnsignedByte, data);
            CheckError();
        }

        public void Draw(int offsetX, int offsetY, int width, int height, IntPtr data, PixelFormat pixelFormat = PixelFormat.Rgba)
        {
            if (this.width < offsetX + width || this.height < offsetY + height)
            {
                throw new Exception($"width: {this.width} {offsetX} {width}  height:  {this.height} {offsetY} {height}");
            }

            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, offsetX, offsetY, width, height, pixelFormat, PixelType.UnsignedByte, data);
            CheckError();
        }

        private void CheckError()
        {
#if DEBUG
            var errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new Exception(errorCode.ToString());
            }
#endif
        }

        public void LoadImage(ImageLoader.RawImage rawImage)
        {
            if (rawImage.width != width || rawImage.height != height)
            {
                throw new Exception("Размеры загружаемого изображения и текстуры не равны");
            }

            if (!isAllocated)
            {
                textureID = GL.GenTexture();
                isAllocated = true;
            }

            GL.BindTexture(TextureTarget.Texture2D, textureID);

            if (!useMipMap)
            {
                if (parameters.linearFiltering)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            if (parameters.repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

            GraphicsHelper.GLCheckErrorFast();
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, rawImage.data);
            GraphicsHelper.GLCheckErrorFast();

            if (useMipMap)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }

        public void LoadImage(byte[] data)
        {
            if (!isAllocated)
            {
                textureID = GL.GenTexture();
                isAllocated = true;
            }

            GL.BindTexture(TextureTarget.Texture2D, textureID);

            if (!useMipMap)
            {
                if (parameters.linearFiltering)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            if (parameters.repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

            GraphicsHelper.GLCheckErrorFast();
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            GraphicsHelper.GLCheckErrorFast();

            if (useMipMap)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }

        public void SetParameters(TextureParameters parameters)
        {
            bool flag = false;

            if (this.parameters.linearFiltering != parameters.linearFiltering)
            {
                this.parameters.linearFiltering = parameters.linearFiltering;

                if (!flag)
                {
                    flag = true;
                    GL.BindTexture(TextureTarget.Texture2D, textureID);
                }

                if (parameters.linearFiltering)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
            }

            if (this.parameters.repeat != parameters.repeat)
            {
                this.parameters.repeat = parameters.repeat;

                if (!flag)
                {
                    flag = true;
                    GL.BindTexture(TextureTarget.Texture2D, textureID);
                }

                if (parameters.repeat)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                }
            }
        }

        private void Init()
        {
            if (isInited)
            {
                throw new Exception("Текстура уже инициализирована");
            }

            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 64, 64, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, null as byte[]);

            CreateFramebuffer();
            isInited = true;
        }

        public void Destroy()
        {
            if (!isInited)
            {
                throw new Exception("Текстура уже удалена");
            }

            GL.DeleteTexture(textureID);
            GL.DeleteFramebuffer(framebufferID);
            isAllocated = false;
            textureID = -1;
            isInited = false;
        }

        private void CreateFramebuffer()
        {
            framebufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, textureID, 0);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer is not complete: " + status.ToString());
            }
        }

        public void FramebufferSetMipLevel(int mipLevel)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, textureID, mipLevel);
        }

        public void AllocateMemory(int width, int height, bool useMipMap = false, SizedInternalFormat format = SizedInternalFormat.Rgba8)
        {
            if (!isInited)
            {
                throw new Exception("Текстура удалена");
            }

            if (isAllocated)
            {
                throw new Exception("Текстура уже создана");
            }

            if (width <= 0 || height <= 0)
            {
                throw new Exception("Invalid texture size");
            }

            this.width = width;
            this.height = height;
            this.useMipMap = useMipMap;
            this.format = format;

            GL.BindTexture(TextureTarget.Texture2D, textureID);

            ApplyParameters(width, height, useMipMap, parameters, format);

            isAllocated = true;
        }

        private void ApplyParameters(int width, int height, bool useMipMap, TextureParameters parameters, SizedInternalFormat format = SizedInternalFormat.Rgba8)
        {
#if DEBUG
            ErrorCode errorCode;
            errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new Exception(errorCode.ToString());
            }
#endif
            if (!useMipMap)
            {
                mipLevelCount = 1;

                if (parameters.linearFiltering)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }

                GL.TexStorage2D(TextureTarget2d.Texture2D, 1, format, width, height);
            }
            else
            {
                mipLevelCount = Math.Max((int)Math.Log2(Math.Min(width, height) / 64), 1);

                GL.TexStorage2D(TextureTarget2d.Texture2D, mipLevelCount, format, width, height);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mipLevelCount - 1);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            if (parameters.repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

#if DEBUG
            errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new Exception("Ошибка выделения памяти: useMipMap " + useMipMap.ToString() + ", " + errorCode.ToString());
            }
#endif
        }

        private Image<Rgba32> BytesToImage(byte[] raw)
        {
            byte[] raw2 = new byte[width * height * 4];
            for (int i = 0; i < height; i++)
            {
                raw.AsSpan(i * width * 4, width * 4).CopyTo(raw2.AsSpan((height - i - 1) * width * 4, width * 4));
            }
            return Image.LoadPixelData<Rgba32>(raw2, width, height);
        }

        public long GetMemorySize()
        {
            return (long)width * height * 4;
        }

        private int lifeTime = -1;
        private static readonly List<RenderTexture> temporary = new List<RenderTexture>(1024);
        private static readonly List<int> temporaryToRemove = new List<int>(1024);
        private static int temporaryCounter = 0;

        public const int LIFE_TIME_DEFAULT = 10;
        public static long memoryMax = 1250 * 1024 * 1024;
        public static long memoryCurrent = 0;

        public static RenderTexture GetTemporary(int width, int height, bool useMipMap = false, bool linearFiltering = true, bool repeat = false, float accuracy = 0.75f)
        {
            int roundWidth = width;
            int roundHeight = height;
            TexturePool.RoundSize(ref roundWidth, ref roundHeight, accuracy);
            RenderTexture ret = null;
            int retIndex = -1;
            for (int i = 0; i < temporary.Count; i++)
            {
                var value = temporary[i];
                if (value.width >= width && value.height >= height && value.width <= roundWidth && value.height <= roundHeight && value.useMipMap == useMipMap)
                {
                    ret = value;
                    retIndex = i;
                    break;
                }
            }

            if (ret != null)
            {
                temporary.RemoveAt(retIndex);
            }
            else
            {
                if (temporaryCounter >= 500)
                {
                    throw new Exception("Too many textures, memory leak?");
                }

                ret = new RenderTexture(roundWidth, roundHeight, useMipMap);
                memoryCurrent += ret.GetMemorySize();
                temporaryCounter++;
            }

            ret.lifeTime = -1;
            ret.SetParameters(new TextureParameters(linearFiltering, repeat));
            return ret;
        }

        public static void ReleaseTemporary(RenderTexture renderTexture, int lifeTime = LIFE_TIME_DEFAULT)
        {
            renderTexture.lifeTime = lifeTime;
            temporary.Add(renderTexture);
        }

        public static void Update()
        {
            if (memoryCurrent > memoryMax)
            {
                for (int i = 0; i < temporary.Count; i++)
                {
                    var value = temporary[i];
                    value.lifeTime--;
                    if (value.lifeTime == 0 && memoryCurrent > memoryMax)
                    {
                        if (memoryCurrent > memoryMax)
                        {
                            temporaryToRemove.Add(i);
                            memoryCurrent -= value.GetMemorySize();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = temporaryToRemove.Count - 1; i >= 0; i--)
                {
                    var index = temporaryToRemove[i];
                    var value = temporary[index];
                    value.Destroy();
                    temporary.RemoveAt(index);
                }

                temporaryCounter -= temporaryToRemove.Count;
                temporaryToRemove.Clear();
            }
        }

        public void Save(string path)
        {
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            byte[] raw = new byte[width * height * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, raw);
            using var image = BytesToImage(raw);
            image.SaveAsPng(path);
        }

        public byte[] GetRaw()
        {
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            byte[] raw = new byte[width * height * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, raw);
            return raw;
        }

        public void SaveOpenSleep(int sleepTime = 100000)
        {
            Save("debug.png");
            var processInfo = new ProcessStartInfo(AppContext.BaseDirectory + "debug.png")
            {
                UseShellExecute = true
            };
            Process.Start(processInfo);
            Thread.Sleep(sleepTime * 1000);
        }

        public void SaveSleep(int sleepTime = 2)
        {
            Save("debug.png");
            Thread.Sleep(sleepTime * 1000);
        }

        protected RenderTexture(SerializationInfo info, StreamingContext context)
        {
            try
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }

                var width = (int)info.GetValue("width", typeof(int));
                var height = (int)info.GetValue("height", typeof(int));
                var useMipMap = (bool)info.GetValue("useMipMap", typeof(bool));
                var format = (SizedInternalFormat)info.GetValue("format", typeof(SizedInternalFormat));
                parameters = (TextureParameters)info.GetValue("parameters", typeof(TextureParameters));

                Init();
                AllocateMemory(width, height, useMipMap, format);

                var png = (byte[])info.GetValue("data", typeof(byte[]));
                var img = Image.Load<Rgba32>(png);
                var rawImage = ImageLoader.ImageToBytesStatic(img);
                LoadImage(rawImage);
            }
            catch (Exception ex)
            {
                Kernel.screenManager.Print(ex.Message + "\n" + ex.StackTrace);
                throw;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            try
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }

                info.AddValue("width", width);
                info.AddValue("height", height);
                info.AddValue("useMipMap", useMipMap);
                info.AddValue("format", format);
                info.AddValue("parameters", parameters);

                var casted = MemoryMarshal.Cast<byte, Rgba32>(GetRaw());
                var img = Image.LoadPixelData<Rgba32>(casted, width, height);
                var stream = new MemoryStream();
                img.SaveAsPng(stream);
                info.AddValue("data", stream.ToArray());
            }
            catch (Exception ex)
            {
                Kernel.screenManager.Print(ex.Message + "\n" + ex.StackTrace);
                throw;
            }
        }

    }
}
