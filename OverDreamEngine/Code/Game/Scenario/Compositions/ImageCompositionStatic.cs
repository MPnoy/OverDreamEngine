using System.Collections;
using ODEngine.Core;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Mathematics;

namespace ODEngine.Game
{
    public abstract class ImageCompositionStatic : ImageComposition
    {
        private enum State : byte
        {
            Unload,
            RamLoading,
            RamUnloading,
            RamLoaded,
            VRamLoading1,
            VRamLoading2,
            VRamLoaded
        }

        private State loadingState = State.Unload;
        private State ChangedState { get => vRamLoadCounter != 0 ? State.VRamLoaded : ramLoadCounter != 0 ? State.RamLoaded : State.Unload; }

        public override bool IsRamLoaded { get => loadingState >= State.RamLoaded; }
        public override bool IsVRamLoaded { get => loadingState >= State.VRamLoaded; }

        private IEnumerator coroutine = null;

        public Task<Image<Rgba32>> taskRamLoading = null;
        private Task taskRamUnloading = null;
        private Task<ImageLoader.RawImage> taskGetRawImage = null;
        private GPUTextureLoader.Ticket gpuTicket = null;

        private RenderTexture texture;

        public ImageCompositionStatic(string name) : base(name) { }

        protected void PostInit(ResourceCache resourceCache)
        {
            textureSize = CalcSize(resourceCache);
        }

        protected abstract Vector2Int CalcSize(ResourceCache resourceCache);

        protected abstract Task<Image<Rgba32>> RamLoadAsync();

        protected virtual void AfterRamLoad() { }

        protected abstract Task RamUnloadAsync();

        protected override void UpdateState()
        {
            if (coroutine != null)
            {
                return;
            }

            if (ChangedState != loadingState)
            {
                coroutine = StateMachine();
                coroutine.MoveNext();
                if (coroutine != null)
                {
                    CoroutineExecutor.Add(coroutine);
                }
            }
        }

        private IEnumerator StateMachine()
        {
            while (ChangedState != loadingState)
            {
                switch (loadingState)
                {
                    case State.Unload:
                        {
                            taskRamLoading = RamLoadAsync();
                            taskRamUnloading = null;
                            loadingState = State.RamLoading;
                            break;
                        }
                    case State.RamLoading:
                        {
                            if (taskRamLoading.IsCompletedSuccessfully)
                            {
                                AfterRamLoad();
                                loadingState = State.RamLoaded;
                            }
                            else if (taskRamLoading.IsFaulted)
                            {
                                throw taskRamLoading.Exception;
                            }
                            break;
                        }
                    case State.RamLoaded:
                        {
                            if (ChangedState == State.Unload)
                            {
                                taskRamUnloading = RamUnloadAsync();
                                loadingState = State.RamUnloading;
                            }
                            else if (ChangedState == State.VRamLoaded)
                            {
                                texture = RenderTexture.GetTemporary(textureSize.x, textureSize.y, true, accuracy: 1f);
                                taskGetRawImage = Task.Run(() => ImageLoader.ImageToBytesStatic(taskRamLoading.Result));
                                loadingState = State.VRamLoading1;
                            }
                            break;
                        }
                    case State.RamUnloading:
                        {
                            if (taskRamUnloading.IsCompletedSuccessfully)
                            {
                                taskRamUnloading = null;
                                loadingState = State.Unload;
                            }
                            else if (taskRamUnloading.IsFaulted)
                            {
                                throw taskRamUnloading.Exception;
                            }
                            break;
                        }
                    case State.VRamLoading1:
                        {
                            if (taskGetRawImage.IsCompletedSuccessfully)
                            {
                                if (ChangedState == State.VRamLoaded)
                                {
                                    gpuTicket = GPUTextureLoader.LoadAsync(texture, taskGetRawImage.Result);
                                    loadingState = State.VRamLoading2;
                                }
                                else
                                {
                                    RenderTexture.ReleaseTemporary(texture);
                                    texture = null;
                                    taskGetRawImage.Result.Free();
                                    taskGetRawImage = null;
                                    loadingState = State.RamLoaded;
                                }
                            }
                            else if (taskGetRawImage.IsFaulted)
                            {
                                throw taskGetRawImage.Exception;
                            }
                            break;
                        }
                    case State.VRamLoading2:
                        {
                            if (gpuTicket.isLoaded)
                            {
                                taskGetRawImage.Result.Free();
                                taskGetRawImage = null;
                                loadingState = State.VRamLoaded;
                            }
                            break;
                        }
                    case State.VRamLoaded:
                        {
                            RenderTexture.ReleaseTemporary(texture);
                            texture = null;
                            loadingState = State.RamLoaded;
                            break;
                        }
                }

                yield return null;
            }

            coroutine = null;
        }

        public override (RenderTexture texture, RenderAtlas.Texture atlasTexture) Render(Vector4 visibleRectangleNorm)
        {
            return (texture, default);
        }

    }
}
