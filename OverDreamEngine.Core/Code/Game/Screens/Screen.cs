using System.Collections;
using System.Collections.Generic;
using OpenTK.Mathematics;
using ODEngine.EC.Components;

namespace ODEngine.Game.Screens
{
    public abstract class Screen
    {
        public string name = null;
        public bool isDominant = false;

        protected readonly ScreenManager screenManager;
        protected Renderer parentRenderer;

        public GUIElement screenContainer;

        public bool prevsDeactivated;
        public bool prevsDisabled;
        public List<Screen> prevScreens = new List<Screen>();
        public List<Screen> childs = new List<Screen>();

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

            screenContainer = GUIElement.CreateContainer(parentRenderer, Vector3.Zero, parentRenderer.size, "Game/Alpha");
            screenContainer.renderer.isVisible = false;
        }

        public void Show(Screen parent = null, bool disableScreens = false, bool deactivateScreens = false)
        {
            screenManager.ShowScreen(this, parent, disableScreens, deactivateScreens);
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

            float min = childs[0].screenContainer.renderer.Position.Z;

            for (int i = 1; i < childs.Count; i++)
            {
                if (childs[i].screenContainer.renderer.Position.Z < min)
                {
                    min = childs[i].screenContainer.renderer.Position.Z;
                }
            }

            return min;
        }

        public void ChangeZ(float z)
        {
            screenContainer.renderer.PositionZ = z;
        }

        public void Hide(bool enablePrevs = true, bool activatePrevs = true)
        {
            if (isEnable)
            {
                if (prevsDeactivated && activatePrevs)
                {
                    screenManager.ActivateScreens(prevScreens);
                }

                if (prevsDisabled && enablePrevs)
                {
                    screenManager.EnableScreens(prevScreens);
                }

                prevScreens.Clear();
                Disable();
            }
        }

        public void Enable()
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

        public void Disable()
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
                var e = enumerator;

                if (!e.MoveNext() && e == enumerator)
                {
                    enumerator = null;
                }
            }
        }

    }
}
