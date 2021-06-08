using System;
using System.Collections;
using System.Collections.Generic;
using ODEngine.Core;

namespace ODEngine.Game
{
    public abstract class ImageCompositionDynamic : ImageComposition
    {
        private enum State : byte
        {
            Unload,
            RamLoading,
            RamLoaded,
            VRamLoading,
            VRamLoaded
        }

        public class Item
        {
            public ImageComposition composition;

            public Item(ImageComposition composition)
            {
                this.composition = composition;
            }
        }

        public List<Item> items;

        private State loadingState = State.Unload;
        private State ChangedState { get => vRamLoadCounter != 0 ? State.VRamLoaded : ramLoadCounter != 0 ? State.RamLoaded : State.Unload; }

        public override bool IsRamLoaded { get => loadingState >= State.RamLoaded; }
        public override bool IsVRamLoaded { get => loadingState >= State.VRamLoaded; }

        private IEnumerator coroutine = null;

        public readonly DateTime startTime = DateTime.Now;
        private DateTime realTime = DateTime.Now;
        private DateTime animTime = DateTime.Now;
        public float speed = 1f;

        public ImageCompositionDynamic(string name, Vector2Int textureSize) : base(name)
        {
            this.textureSize = textureSize;
        }

        public TimeSpan GetAnimTime()
        {
            var nowTime = DateTime.Now;
            animTime += (nowTime - realTime) * speed;
            realTime = nowTime;
            return animTime - startTime;
        }

        public TimeSpan GetRealTime()
        {
            return DateTime.Now - startTime;
        }

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
                            for (int i = 0; i < items.Count; i++)
                            {
                                items[i].composition.RamLoad();
                            }
                            loadingState = State.RamLoading;
                            break;
                        }
                    case State.RamLoading:
                        {
                            bool loaded = true;
                            for (int i = 0; i < items.Count; i++)
                            {
                                if (!items[i].composition.IsRamLoaded)
                                {
                                    loaded = false;
                                }
                            }
                            if(loaded)
                            {
                                loadingState = State.RamLoaded;
                            }
                            break;
                        }
                    case State.RamLoaded:
                        {
                            if (ChangedState == State.Unload)
                            {
                                for (int i = 0; i < items.Count; i++)
                                {
                                    items[i].composition.RamUnload();
                                }
                                loadingState = State.Unload;
                            }
                            else if (ChangedState == State.VRamLoaded)
                            {
                                for (int i = 0; i < items.Count; i++)
                                {
                                    items[i].composition.VRamLoad();
                                }
                                loadingState = State.VRamLoading;
                            }
                            break;
                        }
                    case State.VRamLoading:
                        {
                            if (ChangedState == State.VRamLoaded)
                            {
                                bool loaded = true;
                                for (int i = 0; i < items.Count; i++)
                                {
                                    if (!items[i].composition.IsVRamLoaded)
                                    {
                                        loaded = false;
                                    }
                                }
                                if (loaded)
                                {
                                    loadingState = State.VRamLoaded;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < items.Count; i++)
                                {
                                    items[i].composition.VRamUnload();
                                }
                                loadingState = State.RamLoaded;
                            }
                            break;
                        }
                    case State.VRamLoaded:
                        {
                            for (int i = 0; i < items.Count; i++)
                            {
                                items[i].composition.VRamUnload();
                            }
                            loadingState = State.RamLoaded;
                            break;
                        }
                }

                yield return null;
            }

            coroutine = null;
        }

    }
}
