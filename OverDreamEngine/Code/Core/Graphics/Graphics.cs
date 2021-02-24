using System;
using System.Runtime.CompilerServices;
using ODEngine.EC.Components;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace ODEngine.Core
{
    public static class Graphics
    {
        private static class StaticData
        {
            public static readonly float[] quad =
            {
                0, 0, -1f, -1f, 0,
                0, 1, -1f, 1f, 0,
                1, 1, 1f, 1f, 0,
                0, 0, -1f, -1f, 0,
                1, 1, 1f, 1f, 0,
                1, 0, 1f, -1f, 0
            };
        }

        public static float aspectRatio = 16f / 9f;
        public static float cameraWidth = 1920f;
        public static float cameraHeight = 1080f;

        //Buffers
        public static int quadBuffer;

        public static Material identityMaterial = null;
        public static Material matrixMaterial = null;
        public static Material alphaDivMaterial = null;

        public static Renderer mainRenderer = null;
        public static RenderAtlas temporaryAtlas = null;    // Для временного использования в коде

        public static Text.TextRenderer textRenderer;
        public static bool drawTemporalyAtlas = false;

        public static bool gl_direct_state_access = false;
        public static bool gl_sync = false;
        public static bool gl_separate_shader_objects = false;
        public static bool gl_shading_language_420 = false;

        public static void Init()
        {
#if DEBUG
            //GL.Enable(EnableCap.DebugOutput);
            //GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
            GraphicsHelper.GLCheckError();
#endif
            var extCount = GL.GetInteger(GetPName.NumExtensions);

            for (int i = 0; i < extCount; i++)
            {
                var extName = GL.GetString(StringNameIndexed.Extensions, i).ToLower();

                if (extName.IndexOf("direct_state_access") >= 0)
                {
                    gl_direct_state_access = true;
                }

                if (extName.IndexOf("arb_sync") >= 0)
                {
                    gl_sync = true;
                }

                if (extName.IndexOf("separate_shader_objects") >= 0)
                {
                    gl_separate_shader_objects = true;
                }

                if (extName.IndexOf("shading_language_420") >= 0)
                {
                    gl_shading_language_420 = true;
                }
            }

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend); // Поддержка прозрачности
            GL.Disable(EnableCap.StencilTest);

            Material.LoadShaders();

            identityMaterial = new Material("Identity");

            matrixMaterial = new Material("Matrix", "Matrix", "Matrix")
            {
                blendingFactorSource = BlendingFactor.One,
                blendingFactorDestination = BlendingFactor.OneMinusSrcAlpha
            };

            alphaDivMaterial = new Material("AlphaDiv", null, "AlphaDiv");

            matrixMaterial.Bind();

            quadBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, quadBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, StaticData.quad.Length * sizeof(float), StaticData.quad, BufferUsageHint.StaticDraw);

            RenderAtlas.Init();

            temporaryAtlas = new RenderAtlas(8192 / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv, 8192 / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv);

            textRenderer = new Text.TextRenderer();

            GraphicsHelper.GLCheckError();
        }

        public static void GLDeInit()
        {
            GL.DeleteBuffer(quadBuffer);
            identityMaterial.Destroy();
            matrixMaterial.Destroy();
            GraphicsHelper.GLCheckError();
        }

        public static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string source_str;

            switch (source)
            {
                case DebugSource.DebugSourceApi: source_str = "API"; break;
                case DebugSource.DebugSourceWindowSystem: source_str = "WINDOW SYSTEM"; break;
                case DebugSource.DebugSourceShaderCompiler: source_str = "SHADER COMPILER"; break;
                case DebugSource.DebugSourceThirdParty: source_str = "THIRD PARTY"; break;
                case DebugSource.DebugSourceApplication: source_str = "APPLICATION"; break;
                case DebugSource.DebugSourceOther: source_str = "OTHER"; break;
                default: source_str = "UNKNOWN"; break;
            }

            string type_str;

            switch (type)
            {
                case DebugType.DebugTypeError: type_str = "ERROR"; break;
                case DebugType.DebugTypeDeprecatedBehavior: type_str = "DEPRECATED_BEHAVIOR"; break;
                case DebugType.DebugTypeUndefinedBehavior: type_str = "UNDEFINED_BEHAVIOR"; break;
                case DebugType.DebugTypePortability: type_str = "PORTABILITY"; break;
                case DebugType.DebugTypePerformance: type_str = "PERFORMANCE"; break;
                case DebugType.DebugTypeMarker: type_str = "MARKER"; break;
                case DebugType.DebugTypeOther: type_str = "OTHER"; break;
                default: type_str = "UNKNOWN"; break;
            }

            string severity_str;

            switch (severity)
            {
                case DebugSeverity.DebugSeverityNotification: severity_str = "NOTIFICATION"; break;
                case DebugSeverity.DebugSeverityLow: severity_str = "LOW"; break;
                case DebugSeverity.DebugSeverityMedium: severity_str = "MEDIUM"; break;
                case DebugSeverity.DebugSeverityHigh: severity_str = "HIGH"; break;
                default: severity_str = "UNKNOWN"; break;
            }

            StringBuilder message_str = new StringBuilder();
            if (message != IntPtr.Zero)
            {
                unsafe
                {
                    byte* ptr = (byte*)message;
                    while (*ptr != 0)
                    {
                        message_str.Append((char)*ptr);
                        ptr++;
                    }
                }
            }

            Debug.Print(
                source_str + ", " +
                type_str + ", " +
                severity_str + ", " +
                id + ": " +
                message_str.ToString());
        }

        public static int frameBlitCounter = 0;

        public static void RenderFrame()
        {
            frameBlitCounter = 0;

            RenderTexture RenderObject(Renderer renderer, Vector2 textureSizeMult)
            {
                if (renderer.size.X <= 0f || renderer.size.Y <= 0f || renderer.scale.X <= 0f || renderer.scale.Y <= 0f)
                {
                    return null;
                }

                var textureSize = textureSizeMult * renderer.size * renderer.scale;

                if (textureSize.X <= 4f || textureSize.Y <= 4f)
                {
                    textureSize = new Vector2(4f, 4f);
                }

                var textureTemp1 = RenderTexture.GetTemporary((int)textureSize.X, (int)textureSize.Y);
                Clear(textureTemp1);
                bool textureTemp1IsEmpty = true;

                DepthFirstSearch(renderer, Matrix4.Identity, textureSizeMult);

                void DepthFirstSearch(Renderer depthRenderer, Matrix4 nowMatrix, Vector2 depthTextureSizeMult)
                {
                    if (!depthRenderer.isVisible)
                    {
                        return;
                    }

                    depthRenderer.childs.Sort((x, y) => Math.Sign(y.position.Z - x.position.Z));

                    for (int i = 0; i < depthRenderer.childs.Count; i++)
                    {
                        var child = depthRenderer.childs[i];

                        if (!child.isVisible)
                        {
                            continue;
                        }

                        if (child.size == Vector2.Zero)
                        {
                            // Применяем матрицу для потомков этого потомка без промежуточного рендеринга
                            var nextMatrix =
                                Matrix4.CreateScale(new Vector3(child.scale.X, child.scale.Y, 1f)) *
                                Matrix4.CreateRotationZ(child.rotation) *
                                Matrix4.CreateTranslation(new Vector3(child.position.X, child.position.Y, 0f) * 2f) *
                                nowMatrix;

                            DepthFirstSearch(child, nextMatrix, textureSizeMult * depthRenderer.scale);
                        }
                        else
                        {
                            // Рендерим потомка
                            var tex1 = RenderObject(child, depthTextureSizeMult * depthRenderer.scale);
                            if (tex1 != null)
                            {
                                var mtrx =
                                    Matrix4.CreateScale(new Vector3(child.size.X, child.size.Y, 1f)) *
                                    Matrix4.CreateScale(new Vector3(child.scale.X, child.scale.Y, 1f)) *
                                    Matrix4.CreateRotationZ(child.rotation) *
                                    Matrix4.CreateTranslation(new Vector3(
                                        MathF.Round(child.position.X * textureSize.X) / textureSize.X,
                                        MathF.Round(child.position.Y * textureSize.Y) / textureSize.Y,
                                        0f) * 2f) *
                                    nowMatrix *
                                    Matrix4.CreateScale(new Vector3(1f / renderer.size.X, 1f / renderer.size.Y, 1f));
                                matrixMaterial.SetMatrix4("matrix", mtrx);
                                Blit(tex1, textureTemp1, matrixMaterial);
                                textureTemp1IsEmpty = false;
                                RenderTexture.ReleaseTemporary(tex1);
                            }
                        }
                    }
                }

                var textureTemp2 = RenderTexture.GetTemporary((int)textureSize.X, (int)textureSize.Y);

                if (!textureTemp1IsEmpty)
                {
                    Blit(textureTemp1, textureTemp2, alphaDivMaterial);
                }
                else
                {
                    Clear(textureTemp2);
                }

                renderer.Render(textureTemp2, textureTemp1, textureTemp1IsEmpty, false);
                RenderTexture.ReleaseTemporary(textureTemp2);
                return textureTemp1;
            }

            if (mainRenderer == null || !mainRenderer.isVisible)
            {
                return;
            }

            GraphicsHelper.GLCheckError();
            var tex = RenderObject(mainRenderer, new Vector2(1f / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv));

            matrixMaterial.Bind();

            GL.Viewport(0, 0, Kernel.gameForm.Size.X, Kernel.gameForm.Size.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            {
                var mtrx = Matrix4.CreateScale(cameraWidth, cameraHeight, 1f);
                mtrx = GraphicsHelper.GetProjectionMatrix(cameraWidth, cameraHeight) * mtrx;
                matrixMaterial.SetMatrix4("matrix", mtrx);
            }

            // Set texture
            GL.ActiveTexture(TextureUnit.Texture0);

            if (drawTemporalyAtlas)
            {
                GL.BindTexture(TextureTarget.Texture2D, temporaryAtlas.atlasTexture.TextureID);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, tex.TextureID);
            }

            DrawQuad();

            RenderTexture.ReleaseTemporary(tex);
            TextureMemoryBarrier();
        }

        public static void Blit(RenderTexture input, RenderTexture output, Material material = null)
        {
            frameBlitCounter++;

            if (input != null && !input.IsAllocated)
            {
                throw new Exception("Вызван Blit с неинициализированным input");
            }

            if (!output.IsAllocated)
            {
                throw new Exception("Вызван Blit с неинициализированным output");
            }

            GL.Viewport(0, 0, output.Width, output.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, output.FramebufferID);

            if (material == null)
            {
                identityMaterial.Bind();
            }
            else
            {
                material.Bind();
            }

            // Set texture
            if (input != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, input.TextureID);
            }

            DrawQuad();

            if (output.UseMipMap)
            {
                GL.BindTexture(TextureTarget.Texture2D, output.TextureID);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                //throw new Exception("Mipmap generation is slow, change the code without this");
            }

            TextureMemoryBarrier();
        }

        public static void Blit(RenderTexture input, RenderTexture output, int mipLevel, Material material = null)
        {
            frameBlitCounter++;

            if (input != null && !input.IsAllocated)
            {
                throw new Exception("Вызван Blit с неинициализированным input");
            }

            if (!output.IsAllocated)
            {
                throw new Exception("Вызван Blit с неинициализированным output");
            }

            output.FramebufferSetMipLevel(mipLevel);

            GL.Viewport(0, 0, output.Width / (int)Math.Pow(2, mipLevel), output.Height / (int)Math.Pow(2, mipLevel));
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, output.FramebufferID);

            if (material == null)
            {
                identityMaterial.Bind();
            }
            else
            {
                material.Bind();
            }

            // Set texture
            if (input != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, input.TextureID);
            }

            DrawQuad();

            output.FramebufferSetMipLevel(0);
        }

        public static void Blit(RenderAtlas.Texture input, RenderAtlas.Texture output, Material material = null)
        {
            if (material == null)
            {
                material = RenderAtlas.defaultMaterial;
            }

            input.UniformReadThis(material, "Blit");
            output.UniformWriteThis(material);

            Blit(null, output.renderAtlas.atlasTexture, material);
        }

        public static void Blit(RenderTexture input, RenderAtlas.Texture output, Material material = null)
        {
            output.BlitFrom(input, material);
        }

        public static void Blit((RenderTexture texture, RenderAtlas.Texture atlasTexture) input, RenderAtlas.Texture output, Material material = null)
        {
            if (input.texture != null)
            {
                Blit(input.texture, output, material);
            }
            else if (input.atlasTexture != default)
            {
                Blit(input.atlasTexture, output, material);
            }
        }

        private static readonly Color4 blackTransparent = new Color4(0, 0, 0, 0);

        public static void Clear(RenderTexture texture)
        {
            Clear(texture, blackTransparent);
        }

        public static void Clear(RenderTexture texture, Color4 color)
        {
            if (!texture.IsAllocated)
            {
                throw new Exception("Вызван Clear с неинициализированной текстурой");
            }

            GL.Viewport(0, 0, texture.Width, texture.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, texture.FramebufferID);
            GL.ClearColor(color);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TextureMemoryBarrier()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
        }

        private static void DrawQuad()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadBuffer);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
        }

    }
}
