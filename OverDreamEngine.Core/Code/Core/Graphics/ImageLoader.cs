using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace ODEngine.Core
{
    public class ImageLoader
    {
        public class Processor
        {
            public BackgroundWorker worker;
            private int id;

            private byte[] rawImageBuffer;

            public Processor(int id, long memorySize = 8192 * 4096 * 4) // 128 MB
            {
                this.id = id;
                rawImageBuffer = new byte[memorySize];

                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };

                worker.DoWork += (_, args) =>
                {
                    if (Thread.CurrentThread.Name == null)
                    {
                        Thread.CurrentThread.Name = "Request Processor " + id;
                    }

                    if (args.Cancel)
                    {
                        worker.Dispose();
                        return;
                    }

                    MainLoop();
                };

                worker.RunWorkerAsync();
            }

            public void MainLoop()
            {
                while (true)
                {
                    var request = requests.Take();
                    ProcessRequest(request);
                }
            }

            private void ProcessRequest(Request request)
            {
                var ct = request.cancellationToken;

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                // Загрузка
                var loadingTasks = new Task<Image<Rgba32>>[request.paths.Length];

                for (int i = 0; i < loadingTasks.Length; i++)
                {
#if DEBUG
                    if (!FileManager.DataExists(request.paths[i]))
                    {
                        Debug.Print("Invalid path: " + request.paths[i]);
                        return;
                    }
#endif
                    var path = request.paths[i];
                    loadingTasks[i] = Task.Run(() => Image.Load<Rgba32>(FileManager.DataReadAllBytes(path)), ct);
                }

                if (request.scaledSize == Vector2Int.Zero)
                {
                    if (loadingTasks.Length == 0)
                    {
                        request.scaledSize.x = 1;
                        request.scaledSize.y = 1;
                    }
                    else
                    {
                        loadingTasks[0].Wait();
                        request.scaledSize.x = (int)(loadingTasks[0].Result.Width * request.scale);
                        request.scaledSize.y = (int)(loadingTasks[0].Result.Height * request.scale);
                    }
                }

                Image<Rgba32> image = new Image<Rgba32>(request.scaledSize.x, request.scaledSize.y, new Rgba32(255, 255, 0, 0));

                bool imagesDisposed = false;

                void DisposeImages()
                {
                    if (!imagesDisposed)
                    {
                        image.Dispose();
                        for (int i = 0; i < loadingTasks.Length; i++)
                        {
                            loadingTasks[i].Wait();
                            loadingTasks[i].Result.Dispose();
                        }
                        imagesDisposed = true;
                    }
                }

                try
                {
                    lock (request.ticket.cancelToken)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }

                        //Склейка
                        for (int i = 0; i < loadingTasks.Length; i++)
                        {
                            loadingTasks[i].Wait();
                            //CropImage(loadingTasks[i].Result, request.cropRectangle);
                            ResizeImage(loadingTasks[i].Result, request.scaledSize);
                            image.Mutate(x => x.DrawImage(loadingTasks[i].Result, PixelColorBlendingMode.Normal, 1f));

                            if (ct.IsCancellationRequested)
                            {
                                return;
                            }
                        }

                        if (!request.colorMatrix.IsIdentity)
                        {
                            var colorFilter = new SixLabors.ImageSharp.Processing.Processors.Filters.FilterProcessor(request.colorMatrix);
                            image.Mutate(x => x.ApplyProcessor(colorFilter));
                        }

                        var (raw, length) = ImageToBytes(image);

                        var rawImage = new RawImage()
                        {
                            width = image.Width,
                            height = image.Height,
                            data = raw,
                            length = length,
                        };

                        DisposeImages();

                        request.ticket.rawImage = rawImage;
                        request.ticket.isLoaded = true;
                        request.ticket.onLoad?.Invoke(request.ticket);

                        //Ждём до тех пор, пока ресурс нужен (не прочитан в Main процессе)
                        Monitor.Wait(request.ticket.cancelToken);
                    }
                }
                finally
                {
                    DisposeImages();
                }
            }

            private void CropImage(Image image, Vector4Int rectangle)
            {
                image.Mutate(x => x.Crop(new Rectangle(rectangle.x, rectangle.y, rectangle.z - rectangle.x, rectangle.w - rectangle.y)));
            }

            private void ResizeImage(Image image, Vector2Int size)
            {
                image.Mutate(x => x.Resize(size.x, size.y));
            }

            public (IntPtr raw, int length) ImageToBytes(Image<Rgba32> image)
            {
                int imageMemorySize = image.Width * image.Height * 4;

                if (rawImageBuffer.Length < image.Width * image.Height * 4)
                {
                    rawImageBuffer = new byte[image.Width * image.Height * 4];
                }

                for (int i = 0; i < image.Height; i++)
                {
                    var imageSpan = image.GetPixelRowSpan(i);
                    var imageSpanRaw = MemoryMarshal.AsBytes(imageSpan);
                    var bufferSpan = rawImageBuffer.AsSpan((image.Height - 1 - i) * image.Width * 4, image.Width * 4);
                    imageSpanRaw.CopyTo(bufferSpan);
                }
                unsafe
                {
                    return ((IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference<byte>(rawImageBuffer)), imageMemorySize);
                }
            }
        }

        public struct Request
        {
            public Ticket ticket;
            public CancellationToken cancellationToken;

            public string[] paths;
            public Vector2Int scaledSize;
            public float scale;
            public ColorMatrix colorMatrix;
        }

        public class Ticket
        {
            public CancellationTokenSource cancelToken; // Don't use it, a to otorvu ruki nahooy
            public bool isLoaded = false;
            public RawImage rawImage = default;

            public Action<Ticket> onLoad = null; // Is not thread safe

            public Ticket(CancellationTokenSource token)
            {
                this.cancelToken = token;
            }

            public void Unload() // Use it
            {
                isLoaded = false;
                rawImage = default;

                Task.Run(() =>
                {
                    cancelToken.Cancel();
                    lock (cancelToken)
                    {
                        Monitor.PulseAll(cancelToken);
                    }
                });
            }

        }

        public struct RawImage
        {
            public int width;
            public int height;
            public IntPtr data;
            public int length;

            public RawImage(int width, int height, IntPtr data, int length)
            {
                this.width = width;
                this.height = height;
                this.data = data;
                this.length = length;
            }

            public RawImage Clone()
            {
                var ptr = Marshal.AllocHGlobal(length);

                unsafe
                {
                    Buffer.MemoryCopy((void*)data, (void*)ptr, length, length);
                }

                var rawImage = new RawImage()
                {
                    width = width,
                    height = height,
                    data = ptr,
                    length = length,
                };

                return rawImage;
            }

            public byte[] ToByteArray()
            {
                var ret = new byte[length];
                Marshal.Copy(data, ret, 0, length);
                return ret;
            }

            public void Free()
            {
                Marshal.FreeHGlobal(data);
                width = 0;
                height = 0;
                data = IntPtr.Zero;
                length = 0;
            }
        }

        public static BlockingCollection<Request> requests = new BlockingCollection<Request>();
        private static Processor[] processors;

        private const int PROCESSOR_COUNT = 4;

        public static void Init()
        {
            processors = new Processor[PROCESSOR_COUNT];

            for (int i = 0; i < PROCESSOR_COUNT; i++)
            {
                processors[i] = new Processor(i);
            }
        }

        public static void Deinit()
        {
            for (int i = 0; i < processors.Length; i++)
            {
                processors[i].worker.CancelAsync();
            }
        }

        public static Ticket LoadRaw(string path, Action<Ticket> onImageLoad = null)
        {
            return LoadRaw(path, ColorMatrix.Identity, onImageLoad);
        }

        public static Ticket LoadRaw(string path, ColorMatrix colorMatrix, Action<Ticket> onImageLoad = null)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var ticket = new Ticket(cancellationTokenSource)
            {
                onLoad = onImageLoad
            };

            var request = new Request()
            {
                ticket = ticket,
                cancellationToken = cancellationTokenSource.Token,
                paths = new[] { path },
                scaledSize = Vector2Int.Zero,
                scale = 1f,
                colorMatrix = colorMatrix
            };

            requests.Add(request);

            return ticket;
        }

        public static Ticket LoadRaw(string path, ColorMatrix colorMatrix, float scale, Action<Ticket> onImageLoad = null)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var ticket = new Ticket(cancellationTokenSource)
            {
                onLoad = onImageLoad
            };

            var request = new Request()
            {
                ticket = ticket,
                cancellationToken = cancellationTokenSource.Token,
                paths = new[] { path },
                scaledSize = Vector2Int.Zero,
                scale = scale,
                colorMatrix = colorMatrix
            };

            requests.Add(request);

            return ticket;
        }

        public static Ticket LoadRaw(string[] paths, Vector2Int scaledSize, ColorMatrix colorMatrix, Action<Ticket> onImageLoad = null)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var ticket = new Ticket(cancellationTokenSource)
            {
                onLoad = onImageLoad
            };

            var request = new Request()
            {
                ticket = ticket,
                cancellationToken = cancellationTokenSource.Token,
                paths = paths,
                scaledSize = scaledSize,
                scale = 1f,
                colorMatrix = colorMatrix
            };

            requests.Add(request);

            return ticket;
        }

        public static RawImage LoadRawSync(string path)
        {
            return ImageToBytesStatic(Image.Load<Rgba32>(FileManager.DataReadAllBytes(path)));
        }

        public static RawImage ImageToBytesStatic(Image<Rgba32> image, bool flip = true)
        {
            int imageMemorySize = image.Width * image.Height * 4;
            var ptr = Marshal.AllocHGlobal(imageMemorySize);

            if (flip)
            {
                for (int i = 0; i < image.Height; i++)
                {
                    var imageSpan = image.GetPixelRowSpan(i);

                    unsafe
                    {
                        void* imagePtr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(imageSpan)));
                        Buffer.MemoryCopy(imagePtr, (byte*)ptr + (image.Height - 1 - i) * image.Width * 4, image.Width * 4, image.Width * 4);
                    }
                }
            }
            else
            {
                for (int i = 0; i < image.Height; i++)
                {
                    var imageSpan = image.GetPixelRowSpan(i);

                    unsafe
                    {
                        void* imagePtr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(imageSpan)));
                        Buffer.MemoryCopy(imagePtr, (byte*)ptr + i * image.Width * 4, image.Width * 4, image.Width * 4);
                    }
                }
            }

            var rawImage = new RawImage()
            {
                width = image.Width,
                height = image.Height,
                data = ptr,
                length = imageMemorySize,
            };

            return rawImage;
        }

    }
}
