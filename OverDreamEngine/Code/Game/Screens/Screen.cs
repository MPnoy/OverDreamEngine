using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Mathematics;
using ODEngine.Core;
using ODEngine.EC.Components;

namespace ODEngine.Game.Screens
{
    public abstract class Screen
    {
        public string name = "unknown screen";
        public bool isDominant = false;

        protected readonly ScreenManager screenManager;
        protected Renderer parentRenderer;

        public GUIElement screenContainer;

        public bool prevsDeactivated;
        public bool prevsDisabled;
        public List<Screen> prevScreens = new List<Screen>();
        public List<Screen> childs = new List<Screen>();

        private Random random = new Random();

        private bool isAlive = true;
        protected bool isEnable;

        public bool IsEnable
        {
            get => isEnable;
        }

        public bool IsAlive
        {
            get => isAlive;
        }

        public Screen(ScreenManager screenManager, Renderer parentRenderer)
        {
            this.screenManager = screenManager;
            this.parentRenderer = parentRenderer;

            screenContainer = GUIElement.CreateContainer(this.parentRenderer, Vector3.Zero, this.parentRenderer.size, "Game/Alpha");
            screenContainer.renderer.isVisible = false;
        }

        public void ChangeParent(Renderer parent)
        {
            parentRenderer = parent;
            screenContainer.renderer.SetParent(parent);
        }

        public float GetMinZ()
        {
            if (childs.Count == 0)
            {
                return 0;
            }
            float min = childs[0].screenContainer.renderer.position.Z;
            for (int i = 1; i < childs.Count; i++)
            {
                if (childs[i].screenContainer.renderer.position.Z < min)
                {
                    min = childs[i].screenContainer.renderer.position.Z;
                }
            }
            return min;
        }

        public void ChangeZ(float z)
        {
            screenContainer.renderer.position = new Vector3(0, 0, z);
        }

        public void Hide()
        {
            if (isEnable)
            {
                if (prevsDeactivated)
                {
                    screenManager.ActivateScreens(prevScreens);
                }

                if (prevsDisabled)
                {
                    screenManager.EnableScreens(prevScreens);
                }

                prevScreens.Clear();
                Disable();
            }
        }

        internal void Enable()
        {
            if (!isEnable)
            {
                isEnable = true;
                OnEnable();
            }
        }

        public void DisableWithKids()
        {
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].DisableWithKids();
            }
            Disable();
        }

        internal void Disable()
        {
            if (isEnable)
            {
                isEnable = false;
                OnDisable();
            }
        }

        public void Destroy()
        {
            if (isAlive)
            {
                isAlive = false;
                Disable();
                OnDestroy();
                screenManager.RemoveScreen(GetType().GUID);
            }
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        public virtual void Update() { }

        public virtual void OnDestroy() { }

        protected void CoroutineStep(ref IEnumerator enumerator)
        {
            if (enumerator != null)
            {
                if (!enumerator.MoveNext())
                {
                    enumerator = null;
                }
            }
        }

        protected void PlayDontWorkSound()
        {
            var sound = screenManager.audioCore.Play(screenManager.audioChannelUiSfx, PathBuilder.dataPath + $"Audio/UI/not_works_0{Math.Abs(random.Next()) % 4 + 1}.wav");
            sound.SetVolume(0.4f);
        }

    }
}
