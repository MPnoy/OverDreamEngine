//using System;
//using System.Collections.Generic;

//using ODEngine.Core;

//namespace ODEngine.Game.Images
//{
//    public class TextureManager
//    {
//        public abstract class Request
//        {
//            public RenderTexture texture = null;
//            public TexturePool.TextureRequest textureRequest = null;
//            public bool isDone = false;
//            public int usersCounter = 1;
//            public List<Request> nexts = new List<Request>();
//        }

//        public class ImageRequest : Request
//        {
//            public ImageRequestData imageRequestData;

//            public ImageLoader.Ticket[] imageTicket = null;
//            public GPUTextureLoader.Ticket[] gpuTicket = null;

//            public ImageRequest(ImageRequestData imageRequestData)
//            {
//                this.imageRequestData = imageRequestData;
//            }
//        }

//        public TexturePool texturePool;
//        private readonly Dictionary<ImageRequestData, ImageRequest> imageRequests = new Dictionary<ImageRequestData, ImageRequest>(1024);

//        //private Material buildMaterial;
//        //private Material copyMaterial;
//        //private Material colorMaterial;

//        public TextureManager()
//        {
//            //buildMaterial = new Material("Game/BuildTexture", null, "Game/BuildTexture")
//            //{
//            //    blendingFactorSource = OpenTK.Graphics.OpenGL4.BlendingFactor.One,
//            //    blendingFactorDestination = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha,
//            //};
//            //copyMaterial = new Material("Game/CopyTexture", null, "Game/CopyTexture");
//            //colorMaterial = new Material("Game/ColorTexture", null, "Game/ColorTexture");
//        }

//        public ImageRequest CaptureImage(ImageRequestData imageRequestData)
//        {
//            if (imageRequestData.composition == null)
//            {
//                throw new Exception("Не передана композиция");
//            }

//            if (imageRequests.TryGetValue(imageRequestData, out var request))
//            {
//                request.usersCounter++;
//                return request;
//            }

//            request = new ImageRequest(imageRequestData);

//            switch (imageRequestData.composition)
//            {
//                case ImageCompositionStatic imageComposition:
//                    request.textureRequest = texturePool.CaptureTextureRequest(imageComposition.TextureSize.x, imageComposition.TextureSize.y, true, true);
//                    string[] paths = imageComposition.items.ToArray();
//                    var ticket = ImageLoader.LoadRaw(paths, new Vector2Int(request.textureRequest.texture.Width, request.textureRequest.texture.Height), imageRequestData.colorMatrix);
//                    request.imageTicket = ticket;
//                    break;
//                case ImageCompositionFrameAnimation imageComposition:
//                    //TODO
//                    request.textureRequest = texturePool.CaptureTextureRequest(imageComposition.TextureSize.x * imageComposition.items.Count, imageComposition.TextureSize.y);
//                    break;
//            }

//            request.texture = request.textureRequest.texture;

//            Graphics.Clear(request.texture);
//            imageRequests.Add(imageRequestData, request);
//            RequestUpdate(request);
//            return request;
//        }

//        public void Free(Request request)
//        {
//            request.usersCounter--;
//            TestDestroy(request);
//        }

//        public void NextDestroyed(Request request, Request next)
//        {
//            request.nexts.Remove(next);
//            TestDestroy(request);
//        }

//        public void TestDestroy(Request request)
//        {
//            if (request.usersCounter == 0 && request.nexts.Count == 0)
//            {
//                RequestUpdate(request, true);
//            }
//        }

//        public void Update()
//        {
//            foreach (var request in imageRequests)
//            {
//                RequestUpdate(request.Value);
//            }
//        }

//        private void RequestUpdate(Request request, bool destroy = false)
//        {
//            switch (request)
//            {
//                case ImageRequest imageRequest:
//                    {
//                        if (destroy)
//                        {
//                            imageRequest.imageTicket.Unload();
//                            imageRequests.Remove(imageRequest.imageRequestData);
//                            texturePool.FreeTextureRequest(imageRequest.textureRequest);
//                            return;
//                        }

//                        if (!imageRequest.isDone)
//                        {
//                            if (imageRequest.gpuTicket == null)
//                            {
//                                //Ждём загрузки c диска и первичной обработки
//                                if (!imageRequest.imageTicket.isLoaded)
//                                {
//                                    return;
//                                }

//                                //Рендерим выходную текстуру
//                                var image = imageRequest.imageTicket.rawImage;
//                                imageRequest.gpuTicket = GPUTextureLoader.LoadAsync(imageRequest.texture, image);
//                            }
//                            else
//                            {
//                                //Ждём загрузки в видеопамять
//                                if (!imageRequest.gpuTicket.isLoaded)
//                                {
//                                    return;
//                                }

//                                imageRequest.imageTicket.Unload();
//                                imageRequest.isDone = true;
//                            }
//                        }
//                    }
//                    break;
//            }
//        }

//    }
//}