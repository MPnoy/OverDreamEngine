using System;
using System.Collections.Generic;
using ODEngine.Core;
using OpenTK.Mathematics;

namespace ODEngine.EC.Components
{
    public class Renderer : Component
    {
        private Vector3 position = new Vector3(0f, 0f, 0f);
        public Vector2 scale = new Vector2(1f, 1f);
        public float rotation = 0f;
        public Vector2 size;
        public bool isVisible = true;

        private Renderer parent = null;
        public Renderer Parent { get => parent; }

        public List<Renderer> childs = new List<Renderer>();

        public Action<RenderTexture, RenderTexture> onRender = null;
        private bool childsSorted = false;

        public Vector3 Position { get => position; set { position = value; if (parent != null) { parent.childsSorted = false; } } }
        public float PositionX { get => position.X; set => position.X = value; }
        public float PositionY { get => position.Y; set => position.Y = value; }
        public float PositionZ { get => position.Z; set { position.Z = value; if (parent != null) { parent.childsSorted = false; } } }

        public void SetParent(Renderer parent = null)
        {
            if (this.parent != null)
            {
                this.parent.childs.Remove(this);
            }

            if (parent != null)
            {
                parent.childs.Add(this);
            }

            this.parent = parent;
        }

        protected override void OnDestroy()
        {
            if (parent != null)
            {
                parent.childs.Remove(this);
            }
        }

        public void Render(RenderTexture input, RenderTexture output, bool inputIsEmpty = false, bool outputIsEmpty = false)
        {
            // input - отрендеренная текстура с дочерними объектами
            if (onRender != null)
            {
                onRender(input, output);
            }
            else if (!inputIsEmpty)
            {
                Graphics.Blit(input, output);
            }
            else if (!outputIsEmpty)
            {
                Graphics.Clear(output);
            }
            // Если input и output пустые ничего не делаем
        }

        public void SortChilds()
        {
            if (!childsSorted)
            {
                childs.Sort((x, y) => Math.Sign(y.Position.Z - x.Position.Z));
                childsSorted = true;
            }
        }

    }
}
