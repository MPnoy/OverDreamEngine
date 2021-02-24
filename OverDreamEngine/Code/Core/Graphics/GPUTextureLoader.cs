using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using static ODEngine.Core.ImageLoader;

namespace ODEngine.Core
{
    public static class GPUTextureLoader
    {
        private static readonly List<Ticket> tickets = new List<Ticket>();

        public class Ticket
        {
            public bool isCanceled = false;
            public bool isLoaded = false;
            public IEnumerator coroutine = null;
            public RenderTexture texture = null;
            public string path = null;

            public Ticket() { }
        }

        public static Ticket LoadAsync(string filename)
        {
            return LoadAsync(filename, SixLabors.ImageSharp.ColorMatrix.Identity);
        }

        public static Ticket LoadAsync(string filename, SixLabors.ImageSharp.ColorMatrix colorMatrix)
        {
            var ticket = new Ticket();
            var coroutine = routine();
            ticket.coroutine = coroutine;
            ticket.path = filename;
            coroutine.MoveNext();
            tickets.Add(ticket);

            IEnumerator routine()
            {
                var imageTicket = LoadRaw(filename, colorMatrix, 1f / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv);
                while (!imageTicket.isLoaded)
                {
                    if (ticket.isCanceled)
                    {
                        imageTicket.Unload();
                        yield break;
                    }
                    yield return null;
                }
                var rawImage = imageTicket.rawImage;

                var texture = RenderTexture.GetTemporary(rawImage.width, rawImage.height, false, true, false, 1.0f);
                var loadCoroutine = PBOLoad(texture, rawImage);
                while (loadCoroutine.MoveNext())
                {
                    yield return null;
                }

                imageTicket.Unload();
                ticket.texture = texture;
                ticket.isLoaded = true;
                yield break;
            }

            return ticket;
        }

        public static Ticket LoadAsync(string filename, int width, int height)
        {
            var ticket = new Ticket();
            var coroutine = routine();
            ticket.coroutine = coroutine;
            coroutine.MoveNext();
            tickets.Add(ticket);

            IEnumerator routine()
            {
                var texture = RenderTexture.GetTemporary(width / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv, height / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv);

                var imageTicket = LoadRaw(new[] { filename }, new Vector2Int(texture.Width / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv, texture.Height / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv), SixLabors.ImageSharp.ColorMatrix.Identity);
                while (!imageTicket.isLoaded)
                {
                    if (ticket.isCanceled)
                    {
                        imageTicket.Unload();
                        yield break;
                    }
                    yield return null;
                }
                var rawImage = imageTicket.rawImage;

                var loadCoroutine = PBOLoad(texture, rawImage);
                while (loadCoroutine.MoveNext())
                {
                    yield return null;
                }

                imageTicket.Unload();
                ticket.texture = texture;
                ticket.isLoaded = true;
                yield break;
            }

            return ticket;
        }

        public static Ticket LoadAsync(RenderTexture texture, string filename)
        {
            var ticket = new Ticket()
            {
                texture = texture
            };
            var coroutine = routine();
            ticket.coroutine = coroutine;
            coroutine.MoveNext();
            tickets.Add(ticket);

            IEnumerator routine()
            {
                var imageTicket = LoadRaw(new[] { filename }, new Vector2Int(texture.Width, texture.Height), SixLabors.ImageSharp.ColorMatrix.Identity);
                while (!imageTicket.isLoaded)
                {
                    if (ticket.isCanceled)
                    {
                        imageTicket.Unload();
                        yield break;
                    }
                    yield return null;
                }
                var rawImage = imageTicket.rawImage;

                var loadCoroutine = PBOLoad(texture, rawImage);
                while (loadCoroutine.MoveNext())
                {
                    yield return null;
                }

                imageTicket.Unload();
                ticket.texture = texture;
                ticket.isLoaded = true;
                yield break;
            }

            return ticket;
        }

        public static Ticket LoadAsync(RenderTexture texture, RawImage rawImage)
        {
            var ticket = new Ticket()
            {
                texture = texture
            };
            var coroutine = routine();
            ticket.coroutine = coroutine;
            coroutine.MoveNext();
            tickets.Add(ticket);

            IEnumerator routine()
            {
                var loadCoroutine = PBOLoad(texture, rawImage);
                while (loadCoroutine.MoveNext())
                {
                    yield return null;
                }

                ticket.texture = texture;
                ticket.isLoaded = true;
                yield break;
            }

            return ticket;
        }

        private static int[] pbo;
        private static int nowPbo = 0;
        private static int maxTextureSize;
        private static Task copyTask = null;
        private static IntPtr sync = IntPtr.Zero;

        public static void Init()
        {
            if (Graphics.gl_sync)
            {
                pbo = new int[1];
            }
            else
            {
                pbo = new int[2];
            }

            maxTextureSize = 8192 / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv * 4096 / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv * 4;

            //Создание буферов и их отражений в оперативной памяти
            for (int i = 0; i < pbo.Length; i++)
            {
                pbo[i] = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pbo[i]);
                GL.BufferData(BufferTarget.PixelUnpackBuffer, maxTextureSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            GraphicsHelper.GLCheckErrorFast();
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        }

        public static void Deinit()
        {
            for (int i = 0; i < pbo.Length; i++)
            {
                GL.UnmapNamedBuffer(pbo[i]);
                GL.DeleteBuffer(pbo[i]);
            }
        }

        public static void Update()
        {
            for (int i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                if (!ticket.coroutine.MoveNext())
                {
                    ticket.coroutine = null;
                    tickets.RemoveAt(i);
                    i--;
                }
            }
        }

        private static IEnumerator PBOLoad(RenderTexture texture, RawImage rawImage)
        {
            if (texture.Width != rawImage.width || texture.Height != rawImage.height)
            {
                throw new Exception("texture size != rawImage size");
            }

            if (rawImage.length > maxTextureSize)
            {
                throw new Exception("Too big image");
            }

            //Ждём окончания другой загрузки
            while (copyTask != null)
            {
                yield return null;
            }

            // Ждём передачи из буфера в текстуру
            if (Graphics.gl_sync && sync != IntPtr.Zero)
            {
                while (true)
                {
                    var syncStatus = GL.ClientWaitSync(sync, ClientWaitSyncFlags.None, 0L);
                    if (syncStatus == WaitSyncStatus.TimeoutExpired || copyTask != null)
                    {
                        yield return null;
                    }
                    else if (syncStatus == WaitSyncStatus.WaitFailed)
                    {
                        GraphicsHelper.GLCheckErrorForRelease();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            IntPtr pboPtr;

            // Если синхронизация Graphics.gl_sync не доступна, при достаточно малом количестве буферов здесь будет лаг
            if (Graphics.gl_direct_state_access)
            {
                pboPtr = GL.MapNamedBuffer(pbo[nowPbo], BufferAccess.WriteOnly);
            }
            else
            {
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pbo[nowPbo]);
                pboPtr = GL.MapBuffer(BufferTarget.PixelUnpackBuffer, BufferAccess.WriteOnly);
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
            }

            // Копирование изображения в GPU буфер в другом потоке
            copyTask = Task.Run(() =>
            {
                MemoryCopy(rawImage, pboPtr);
            });

            while (!copyTask.IsCompleted)
            {
                yield return null;
            }

            // copy pixels from PBO to texture object
            // Use offset instead of ponter.
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pbo[nowPbo]);
            GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);
            GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, rawImage.width, rawImage.height, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);

            if (Graphics.gl_sync)
            {
                sync = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
            }

            if (texture.UseMipMap)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); // GL3.0
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GraphicsHelper.GLCheckErrorFast();
            nowPbo = (nowPbo + 1) % pbo.Length;
            copyTask = null;
        }

        private static void MemoryCopy(RawImage rawImage, IntPtr pboPtr) // C# не смог в unsafe в итераторах
        {
            unsafe
            {
                System.Buffer.MemoryCopy((void*)rawImage.data, (void*)pboPtr, (int)rawImage.length, (int)rawImage.length);
            }
        }
    }
}
