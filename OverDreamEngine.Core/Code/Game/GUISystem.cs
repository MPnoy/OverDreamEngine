using ODEngine.Core;
using ODEngine.EC.Components;
using OpenTK.Mathematics;

namespace ODEngine.Game
{
    public static class GUISystem
    {

        public static void Update()
        {
            if (Graphics.mainRenderer == null || !Graphics.mainRenderer.isVisible)
            {
                return;
            }

            Vector4 mousePosNorm = new Vector4(
                (Input.mousePos.X / Kernel.gameForm.Size.X - 0.5f),
                -(Input.mousePos.Y / Kernel.gameForm.Size.Y - 0.5f),
                0f, 1f);

            var mtrx = Matrix4.CreateScale(Graphics.cameraWidth, Graphics.cameraHeight, 1f);
            var proj = GraphicsHelper.GetProjectionMatrix(Graphics.mainRenderer.size * Graphics.mainRenderer.scale);
            proj.M11 = 1f / proj.M11;
            proj.M22 = 1f / proj.M22;
            mousePosNorm *= proj;

            //if (Input.mouseDowns.Contains(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
            //{
            //    Kernel.screenManager.consoleScreen.Print("Left Mouse Down");
            //}

            DepthFirstSearch(Graphics.mainRenderer, Matrix4.CreateScale(new Vector3(Graphics.mainRenderer.scale.X, Graphics.mainRenderer.scale.Y, 1f)), true, out _);

            void DepthFirstSearch(Renderer depthRenderer, Matrix4 nowMatrix, bool mouseOnParent, out bool mouseOnThis)
            {

                if (!depthRenderer.isVisible)
                {
                    mouseOnThis = false;
                    return;
                }

                var guiElement = depthRenderer.entity.GetComponent<GUIElement>();

                bool mouseOnElement = mouseOnParent;

                if (guiElement != null && guiElement.isEnable && depthRenderer.size != Vector2.Zero)
                {
                    var mousePos = mousePosNorm * nowMatrix.Inverted() * Matrix4.CreateScale(new Vector3(2f / depthRenderer.size.X, 2f / depthRenderer.size.Y, 1f));
                    var mousePos2D = new Vector2(mousePos.X / mousePos.W, mousePos.Y / mousePos.W);

                    mouseOnElement = mouseOnElement && (mousePos2D.X > -1f && mousePos2D.X < 1f && mousePos2D.Y > -1f && mousePos2D.Y < 1f);

                    guiElement.MouseUpdate(new Vector2(mousePos.X / mousePos.W, mousePos.Y / mousePos.W), mouseOnElement);

                    mouseOnThis = mouseOnElement;
                }
                else
                {
                    mouseOnThis = false;
                }

                if (guiElement != null && !guiElement.childsProcessing)
                {
                    return;
                }

                depthRenderer.SortChilds();

                //Порядок сортировки обратный рендерингу
                for (int i = depthRenderer.childs.Count - 1; i >= 0; i--)
                {
                    var child = depthRenderer.childs[i];

                    if (!child.isVisible || !child.isAlive)
                    {
                        continue;
                    }

                    var nextMatrix =
                        Matrix4.CreateScale(new Vector3(child.scale.X, child.scale.Y, 1f)) *
                        Matrix4.CreateRotationZ(child.rotation) *
                        Matrix4.CreateTranslation(new Vector3(child.Position.X, child.Position.Y, 0f)) *
                        nowMatrix;

                    DepthFirstSearch(child, nextMatrix, mouseOnElement, out var mouseOnChild);

                    if (guiElement == null || depthRenderer.size == Vector2.Zero)
                    {
                        if (mouseOnChild)
                        {
                            mouseOnThis = true;
                        }
                    }

                    // Только один дочерний объект может быть с активной мышью
                    if (mouseOnChild)
                    {
                        mouseOnElement = false;
                    }
                }

            }

        }

    }
}
