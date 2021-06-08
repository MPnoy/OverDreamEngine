using System;
using System.Runtime.CompilerServices;
using ODEngine.EC.Components;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Text;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace ODEngine.Core
{
    public static class Graphics
    {
        private static readonly float[] quad =
        {
            -1f, -1f, 0f,  0f, 0f,
            -1f, 1f, 0f,   0f, 1f,
            1f, 1f, 0f,    1f, 1f,
            -1f, -1f, 0f,  0f, 0f,
            1f, 1f, 0f,    1f, 1f,
            1f, -1f, 0f,   1f, 0f
        };

        public static float aspectRatio = 16f / 9f;
        public static float cameraMultiplier;
        public static float cameraWidth;
        public static float cameraHeight;

        //Buffers
        public static int vbo, vao;

        public static Material identityMaterial = null;
        public static Material matrixMaterial = null;
        public static Material alphaDivMaterial = null;
        public static Material atlasToTexture = null;

        public static Renderer mainRenderer = null;
        public static RenderAtlas temporaryAtlas1 = null;   // Для временного использования в коде (ping-pong rendering)
        public static RenderAtlas temporaryAtlas2 = null;
        public static RenderAtlas resultAtlas = null;       // Атлас с результатами для вывода на экран

        public static Text.TextRenderer textRenderer;
        public static bool drawTemporalyAtlas = false;

        public static bool gl_direct_state_access = false;
        public static bool gl_sync = false;
        public static bool gl_separate_shader_objects = false;
        public static bool gl_shading_language_420 = false;
        public static bool gl_khr_debug = false;
        public static bool isCoreProfile = false;
        public static Version glVersion;

        public static void Init(Version glVersion, bool forceCompatible = false)
        {
            Graphics.glVersion = glVersion;
            isCoreProfile = !forceCompatible;
#if DEBUG
            //GL.Enable(EnableCap.DebugOutput);
            //GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
            GraphicsHelper.GLCheckError();
#endif
            if (!forceCompatible)
            {
                string log = null;
                var extCount = GL.GetInteger(GetPName.NumExtensions);
                gl_shading_language_420 = true;

                for (int i = 0; i < extCount; i++)
                {
                    var extName = GL.GetString(StringNameIndexed.Extensions, i).ToLower();
                    log += extName + Environment.NewLine;

                    if (extName.IndexOf("direct_state_access") >= 0)
                    {
                        log += "    gl_direct_state_access = true" + Environment.NewLine;
                        gl_direct_state_access = true;
                    }

                    if (extName.IndexOf("arb_sync") >= 0)
                    {
                        log += "    gl_sync = true" + Environment.NewLine;
                        gl_sync = true;
                    }

                    if (extName.IndexOf("separate_shader_objects") >= 0)
                    {
                        log += "    gl_separate_shader_objects = true" + Environment.NewLine;
                        gl_separate_shader_objects = true;
                    }

                    if (extName.IndexOf("khr_debug") >= 0)
                    {
                        log += "    gl_khr_debug = true" + Environment.NewLine;
                        gl_khr_debug = true;
                    }
                }

                Logger.Log(log);
            }
            else
            {
                Console.WriteLine("Graphics forcibly initialized in compatibility mode with OpenGL 3.0");
            }

            if (glVersion.Major == 4 && glVersion.Minor == 2)
            {
                gl_shading_language_420 = true;
            }

            cameraMultiplier = 100f / Kernel.settings.TextureSizeDiv;
            cameraWidth = 19.2f * cameraMultiplier;
            cameraHeight = 10.8f * cameraMultiplier;

            //gl_khr_debug = false;
            //if (gl_khr_debug)
            //{
            //    GL.Enable(EnableCap.DebugOutput);
            //    GL.DebugMessageCallback(OnGLError, IntPtr.Zero);
            //}

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend); // Поддержка прозрачности
            GL.Disable(EnableCap.StencilTest);
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Kernel.gameForm.SwapBuffers();
            GraphicsHelper.GLCheckError();
        }

        public static void OnGLError(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (type == DebugType.DebugTypeError)
            {
                var text = $"GL Callback: {source}, {type}, {id}, {severity}, {length}, {message}, {userParam}, {Marshal.PtrToStringAnsi(message)}";
                Debug.Print(text);
                throw new Exception(text);
            }
            else
            {
                Debug.Print($"GL Callback: {source}, {type}, {id}, {severity}, {length}, {message}, {userParam}, {Marshal.PtrToStringAnsi(message)}");
            }
        }

        public static void PostInit()
        {
            Material.LoadShaders();

            identityMaterial = new Material();
            alphaDivMaterial = new Material(null, "AlphaDiv");
            atlasToTexture = new Material("Identity", "Atlas/BlitToTexture");
            matrixMaterial = new Material("Matrix", "Matrix")
            {
                blendingFactorSource = BlendingFactor.One,
                blendingFactorDestination = BlendingFactor.OneMinusSrcAlpha
            };

            matrixMaterial.Bind();

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);

            if (isCoreProfile)
            {
                vao = GL.GenVertexArray();
                GL.BindVertexArray(vao);

                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
            }

            RenderAtlas.Init();

            temporaryAtlas1 = new RenderAtlas(4096 / Kernel.settings.TextureSizeDiv, 4096 / Kernel.settings.TextureSizeDiv);
            temporaryAtlas2 = new RenderAtlas(4096 / Kernel.settings.TextureSizeDiv, 4096 / Kernel.settings.TextureSizeDiv);
            resultAtlas = new RenderAtlas(8192 / Kernel.settings.TextureSizeDiv, 8192 / Kernel.settings.TextureSizeDiv);

            textRenderer = new Text.TextRenderer();

            GraphicsHelper.GLCheckError();
        }

        public static void GLDeInit()
        {
            GL.DeleteBuffer(vbo);
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

            if (mainRenderer == null || !mainRenderer.isVisible)
            {
                return;
            }

            GraphicsHelper.GLCheckError();
            RenderTexture tex;

            if (drawTemporalyAtlas)
            {
                tex = RenderTexture.GetTemporary(Kernel.gameForm.ClientSize.X, Kernel.gameForm.ClientSize.Y, accuracy: 1f);
                Clear(tex);

                var mtrx = Matrix4.CreateScale(0.25f, 0.5f, 1f) *
                    Matrix4.CreateTranslation(-0.75f, 0.5f, 0f);
                matrixMaterial.SetMatrix4("matrix", mtrx);
                Blit(temporaryAtlas1.atlasTexture, tex, matrixMaterial);

                mtrx = Matrix4.CreateScale(0.25f, 0.5f, 1f) *
                    Matrix4.CreateTranslation(-0.75f, -0.5f, 0f);
                matrixMaterial.SetMatrix4("matrix", mtrx);
                Blit(temporaryAtlas2.atlasTexture, tex, matrixMaterial);

                mtrx = Matrix4.CreateScale(0.75f, 1f, 1f) *
                    Matrix4.CreateTranslation(0.25f, 0f, 0f);
                matrixMaterial.SetMatrix4("matrix", mtrx);
                Blit(resultAtlas.atlasTexture, tex, matrixMaterial);
            }
            else
            {
                tex = RenderObject(mainRenderer);
            }

            matrixMaterial.Bind();

            GL.Viewport(0, 0, Kernel.gameForm.ClientSize.X, Kernel.gameForm.ClientSize.Y);
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

            GL.BindTexture(TextureTarget.Texture2D, tex.TextureID);

            DrawQuad();
            RenderTexture.ReleaseTemporary(tex);

            TextureMemoryBarrier();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture RenderObject(Renderer renderer)
        {
            return RenderObject(renderer, Vector2.One);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture RenderObject(Renderer renderer, RenderTexture output)
        {
            if (renderer.size.X <= 0f || renderer.size.Y <= 0f || renderer.scale.X <= 0f || renderer.scale.Y <= 0f)
            {
                return null;
            }

            var textureSize = output.Size;
            RenderObject(renderer, output, textureSize, Helpers.MathHelper.Vec2DivVec2(textureSize, renderer.size * renderer.scale));
            return output;
        }

        public static RenderTexture RenderObject(Renderer renderer, Vector2 textureSizeMult)
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

            var output = RenderTexture.GetTemporary((int)textureSize.X, (int)textureSize.Y);
            RenderObject(renderer, output, textureSize, textureSizeMult);
            return output;
        }

        private static void RenderObject(Renderer renderer, RenderTexture output, Vector2 textureSize, Vector2 textureSizeMult)
        {
            Clear(output);
            bool outputIsEmpty = true;

            DepthFirstSearch(renderer, Matrix4.Identity, textureSizeMult);

            void DepthFirstSearch(Renderer depthRenderer, Matrix4 nowMatrix, Vector2 depthTextureSizeMult)
            {
                if (!depthRenderer.isVisible)
                {
                    return;
                }

                depthRenderer.SortChilds();

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
                            Matrix4.CreateTranslation(new Vector3(child.Position.X, child.Position.Y, 0f) * 2f) *
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
                                    MathF.Round(child.Position.X * textureSize.X) / textureSize.X,
                                    MathF.Round(child.Position.Y * textureSize.Y) / textureSize.Y,
                                    0f) * 2f) *
                                nowMatrix *
                                Matrix4.CreateScale(new Vector3(1f / renderer.size.X, 1f / renderer.size.Y, 1f));

                            matrixMaterial.SetMatrix4("matrix", mtrx);
                            Blit(tex1, output, matrixMaterial);
                            outputIsEmpty = false;
                            RenderTexture.ReleaseTemporary(tex1);
                        }
                    }
                }
            }

            var textureTemp2 = RenderTexture.GetTemporary((int)textureSize.X, (int)textureSize.Y);

            if (!outputIsEmpty)
            {
                Blit(output, textureTemp2, alphaDivMaterial);
            }
            else
            {
                Clear(textureTemp2);
            }

            renderer.Render(textureTemp2, output, outputIsEmpty, outputIsEmpty);
            RenderTexture.ReleaseTemporary(textureTemp2);
        }

        public static void Blit(RenderTexture output, Material material)
        {
            frameBlitCounter++;

            if (!output.IsAllocated)
            {
                throw new Exception("Вызван Blit с неинициализированным output");
            }

            GL.Viewport(0, 0, output.Width, output.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, output.FramebufferID);

            var mat = material ?? identityMaterial;

            mat.Bind();
            DrawQuad();

            if (output.UseMipMap)
            {
                GL.BindTexture(TextureTarget.Texture2D, output.TextureID);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                //throw new Exception("Mipmap generation is slow, change the code without this");
            }

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

            var mat = material ?? identityMaterial;

            // Set texture
            if (input != null)
            {
                mat.SetTexture("blitInput", input);
            }

            mat.Bind();
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

            var mat = material ?? identityMaterial;

            // Set texture
            if (input != null)
            {
                mat.SetTexture("blitInput", input);
            }

            DrawQuad();
            output.FramebufferSetMipLevel(0);
            TextureMemoryBarrier();
        }

        public static void Blit(RenderAtlas.Texture input, RenderAtlas.Texture output, Material material = null)
        {
            if (material == null)
            {
                material = RenderAtlas.defaultMaterial;
            }

            input.UniformReadThis(material, "Blit");
            output.UniformWriteThis(material);

            Blit(output.renderAtlas.atlasTexture, material);
        }

        public static void Blit(RenderTexture input, RenderAtlas.Texture output, Material material = null)
        {
            output.BlitFrom(input, material);
        }

        public static void Blit(RenderAtlas.Texture input, RenderTexture output, Material material = null)
        {
            if (material == null)
            {
                material = atlasToTexture;
            }

            input.UniformReadThis(material, "Prev");
            material.SetVector4("WriteRect", Helpers.MathHelper.oneRect);
            material.SetVector2("WriteMultiplier", Vector2.One);
            Blit(output, material);
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
            GL.Disable(EnableCap.DepthTest);

            if (isCoreProfile)
            {
                GL.BindVertexArray(vao);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
            }
        }

    }
}
