using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ODEngine.Core
{
    public class GraphicsHelper
    {
        public static RenderTexture textureNothing;

        public static void Init()
        {
            textureNothing = new RenderTexture(1, 1);
            Graphics.Clear(textureNothing);
        }

        public static Matrix4 GetProjectionMatrix(float unitsCountX, float unitsCountY)
        {
            return GetProjectionMatrix(Kernel.gameForm.Size.X, Kernel.gameForm.Size.Y, unitsCountX, unitsCountY);
        }

        public static Matrix4 GetProjectionMatrix(Vector2 unitsCount)
        {
            return GetProjectionMatrix(Kernel.gameForm.Size.X, Kernel.gameForm.Size.Y, unitsCount.X, unitsCount.Y);
        }

        public static Matrix4 GetProjectionMatrix(int viewPortWidth, int viewPortHeight, float unitsCountX, float unitsCountY)
        {
            var ret = Matrix4.Identity;
            float width = unitsCountX;
            float height = unitsCountY;
            if (Kernel.gameForm.Size.X / unitsCountX < viewPortHeight / unitsCountY)
            {
                height *= (unitsCountX / unitsCountY) / ((float)viewPortWidth / viewPortHeight);
            }
            else
            {
                width *= (unitsCountY / unitsCountX) / ((float)viewPortHeight / viewPortWidth);
            }

            ret[0, 0] = 1f / width;
            ret[1, 1] = 1f / height;
            return ret;
        }

        public static Matrix4 GetModelMatrix(EC.Components.Renderer renderer)
        {
            var ret = Matrix4.CreateScale(renderer.size.X, renderer.size.Y, 1f);
            ret = Matrix4.CreateScale(renderer.scale.X, renderer.scale.Y, 1f) * ret;
            ret = Matrix4.CreateRotationZ(renderer.rotation) * ret;
            ret = Matrix4.CreateTranslation(renderer.Position) * ret;
            return ret;
        }

        public static void GLCheckError()
        {
#if DEBUG
            string errors = null;
            ErrorCode code;
            while ((code = GL.GetError()) != ErrorCode.NoError)
            {
                errors += code.ToString() + " ";
            }

            if (errors != null)
            {
                throw new Exception($"OpenGL Error(s): " + errors);
            }
#endif
        }

        public static void GLCheckErrorFast()
        {
#if DEBUG
            var errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new Exception(errorCode.ToString());
            }
#endif
        }

        public static void GLCheckErrorForRelease()
        {
            var errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new Exception(errorCode.ToString());
            }
        }

        public static bool GLCheckErrorNoEx()
        {
            var errorCode = GL.GetError();
            return errorCode != ErrorCode.NoError;
        }

    }
}