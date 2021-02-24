using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ODEngine.Core
{
    [Serializable]
    public class Material : ISerializable
    {
        private static int nowBindID = -1;
        private static readonly Dictionary<(ShaderType type, string name), int> shaderIDs = new Dictionary<(ShaderType type, string name), int>();

        public string materialName;
        public string vertName;
        public string fragName;

        private readonly Dictionary<string, int> uniformIDs = new Dictionary<string, int>();
        private readonly HashSet<string> noLocationUniforms = new HashSet<string>();
        private readonly Dictionary<string, object> variables = new Dictionary<string, object>();

        public BlendingFactor blendingFactorSource = BlendingFactor.One;
        public BlendingFactor blendingFactorDestination = BlendingFactor.Zero;

        public int programID = -1;
        private int vertID = -1;
        private int fragID = -1;

        private static void LoadVertShader(string name, string filename)
        {
            shaderIDs.Add((ShaderType.VertexShader, name), CompileShader(filename, ShaderType.VertexShader));
        }

        private static void LoadFragShader(string name, string filename)
        {
            shaderIDs.Add((ShaderType.FragmentShader, name), CompileShader(filename, ShaderType.FragmentShader));
        }

        public override string ToString()
        {
            var ret = materialName;
            foreach (var item in variables)
            {
                ret += "\n" + "[" + (item.Key != null ? item.Key.ToString() : "null") + "] = " + (item.Value != null ? item.Value.ToString() : "null");
            }
            return ret;
        }

        private static void LoadShader(string rootFolder, string filename)
        {
            var relative = Path.GetRelativePath(rootFolder, filename);
            var name = relative.Substring(0, relative.LastIndexOf('.')).Replace('\\', '/');
            var extension = Path.GetExtension(filename);
            switch (extension)
            {
                case ".frag":
                    LoadFragShader(name, filename);
                    break;
                case ".vert":
                    LoadVertShader(name, filename);
                    break;
                default:
                    throw new Exception("Invalid file extension '" + extension + "'");
            }
        }

        private static int CompileShader(string filename, ShaderType shaderType)
        {
            string source = File.ReadAllText(filename);

            if (!Graphics.gl_shading_language_420 && source.IndexOf("#version 4") != -1)
            {
                for (int i = 0; i <= 6; i++)
                {
                    source = source.Replace($"#version 4{i}0", "#version 130");
                }

                source = source.Replace("layout(binding = 0) ", "");
                switch (shaderType)
                {
                    case ShaderType.VertexShader:
                        {
                            source = source.Replace("in ", "attribute ");
                            source = source.Replace("out ", "varying ");
                            break;
                        }
                    case ShaderType.FragmentShader:
                        {
                            source = source.Replace("out vec4 out_Color;", "");
                            source = source.Replace("in ", "varying ");
                            source = source.Replace("texture(", "texture2D(");
                            source = source.Replace("out_Color =", "gl_FragColor =");
                            break;
                        }
                }
            }

            int id = GL.CreateShader(shaderType);   // Создаем шейдер
            GL.ShaderSource(id, source);            // Передаем исходный код
            GL.CompileShader(id);                   // Компилируем шейдер 

            GL.GetShader(id, ShaderParameter.CompileStatus, out int link_ok); // Проверяем статус компиляции

            if (link_ok != 1)
            {
                if (Graphics.gl_shading_language_420)
                {
                    Graphics.gl_shading_language_420 = false;
                    return CompileShader(filename, shaderType);
                }
                else
                {
                    throw new Exception("Error " + shaderType.ToString() + "! Filename: " + filename + "\n" + GL.GetShaderInfoLog(id));
                }
            }

            return id;
        }

        public static void LoadShaders()
        {
            var root = PathBuilder.dataPath + "Shaders";
            var filenames = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
            for (int i = 0; i < filenames.Length; i++)
            {
                LoadShader(root, filenames[i]);
            }
        }

        public Material(string materialName, string vertName = null, string fragName = null)
        {
            if (vertName == null)
            {
                vertName = "Identity";
            }

            if (fragName == null)
            {
                fragName = "Identity";
            }

            this.vertName = vertName;
            this.fragName = fragName;

            int vertID = shaderIDs[(ShaderType.VertexShader, vertName)];
            int fragID = fragName == null ? -1 : shaderIDs[(ShaderType.FragmentShader, fragName)];
            Construct(vertID, fragID, materialName);
        }

        protected Material(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            materialName = (string)info.GetValue("materialName", typeof(string));
            vertName = (string)info.GetValue("vertName", typeof(string));
            fragName = (string)info.GetValue("fragName", typeof(string));
            uniformIDs = (Dictionary<string, int>)info.GetValue("uniformIDs", typeof(Dictionary<string, int>));
            noLocationUniforms = (HashSet<string>)info.GetValue("noLocationUniforms", typeof(HashSet<string>));
            variables = (Dictionary<string, object>)info.GetValue("variables", typeof(Dictionary<string, object>));
            blendingFactorSource = (BlendingFactor)info.GetValue("blendingFactorSource", typeof(BlendingFactor));
            blendingFactorDestination = (BlendingFactor)info.GetValue("blendingFactorDestination", typeof(BlendingFactor));

            int vertID = shaderIDs[(ShaderType.VertexShader, vertName)];
            int fragID = fragName == null ? -1 : shaderIDs[(ShaderType.FragmentShader, fragName)];
            Construct(vertID, fragID, materialName);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("materialName", materialName);
            info.AddValue("vertName", vertName);
            info.AddValue("fragName", fragName);
            info.AddValue("uniformIDs", uniformIDs);
            info.AddValue("noLocationUniforms", noLocationUniforms);
            info.AddValue("variables", variables);
            info.AddValue("blendingFactorSource", blendingFactorSource);
            info.AddValue("blendingFactorDestination", blendingFactorDestination);
        }

        public Material(Material material)
        {
            Construct(material.vertID, material.fragID, material.materialName);
        }

        private void Construct(int vertID, int fragID, string materialName)
        {
            this.materialName = materialName;
            this.vertID = vertID;
            this.fragID = fragID;

            programID = GL.CreateProgram(); // Создаем программу и прикрепляем шейдеры к ней

            GL.AttachShader(programID, vertID);
            if (fragID != -1)
            {
                GL.AttachShader(programID, fragID);
            }

            GL.LinkProgram(programID); // Линкуем шейдерную программу
#if DEBUG
            BindProgram(); // Проверяем, что залинковалось, если тут ошибка, проверь точки входа, должны быть main()
#endif
            GL.DetachShader(programID, vertID);
            if (fragID != -1)
            {
                GL.DetachShader(programID, fragID);
            }

        }

        public void Bind()
        {
            GL.BlendFunc(blendingFactorSource, blendingFactorDestination);
            BindProgram();
            int texCounter = 1; // 0 зарезервировано для blit input
            foreach (var item in variables)
            {
                if (item.Value is RenderTexture texture)
                {
                    var id = GetUniformID(item.Key);
                    if (id != -1)
                    {
                        GL.ActiveTexture((TextureUnit)(33984 + texCounter)); // 33984 = TextureUnit.Texture0
                        GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
                        GL.Uniform1(GetUniformID(item.Key), texCounter);
                        texCounter++;
                    }
                }
                if (item.Value is RenderTexture[] textureArray)
                {
                    var id = GetUniformID(item.Key);
                    if (id != -1)
                    {
                        var raw = new int[textureArray.Length];
                        for (int i = 0; i < textureArray.Length; i++)
                        {
                            GL.ActiveTexture((TextureUnit)(33984 + texCounter));
                            GL.BindTexture(TextureTarget.Texture2D, textureArray[i].TextureID);
                            raw[i] = texCounter;
                            texCounter++;
                        }
                        GL.Uniform1(GetUniformID(item.Key), textureArray.Length, raw);
                    }
                }
            }
        }

        private void BindProgram()
        {
            if (nowBindID != programID)
            {
                GL.UseProgram(programID);
#if DEBUG
                CheckIfBound();
#endif
                nowBindID = programID;
            }
        }

        public void Destroy()
        {
            if (programID != -1)
            {
                GL.DeleteProgram(programID);
                programID = -1;
            }
        }

        public int GetUniformID(string name)
        {
#if DEBUG
            GraphicsHelper.GLCheckError();
#endif

            if (uniformIDs.TryGetValue(name, out var ret))
            {
                return ret;
            }
            else if (noLocationUniforms.Contains(name))
            {
                return -1;
            }

            int id = GL.GetUniformLocation(programID, name);

            if (id == -1)
            {
#if DEBUG
                var errorCode = GL.GetError();
                if (errorCode != ErrorCode.NoError)
                {
                    throw new Exception("Ошибка переменной " + name + " в шейдере " + materialName + ": " + errorCode.ToString());
                }
                else
                {
                    noLocationUniforms.Add(name);
                    return -1;
                }
#else
                noLocationUniforms.Add(name);
                return -1;
#endif
            }

            uniformIDs.Add(name, id);
            return id;
        }

        private void Uniform<T>(string name, T value)
        {
            if (Graphics.gl_separate_shader_objects)
            {
                int id = GetUniformID(name);
                if (id != -1)
                {
                    switch (value)
                    {
                        case int val:
                            GL.ProgramUniform1(programID, id, val);
                            break;
                        case float val:
                            GL.ProgramUniform1(programID, id, val);
                            break;
                        case Vector2 val:
                            GL.ProgramUniform2(programID, id, val);
                            break;
                        case Vector4 val:
                            GL.ProgramUniform4(programID, id, val);
                            break;
                        case Color4 val:
                            GL.ProgramUniform4(programID, id, val);
                            break;
                        case Matrix4 val:
                            GL.ProgramUniformMatrix4(programID, id, false, ref val);
                            break;
                        case Matrix4[] val:
                            {
                                var raw = new float[val.Length * 16];
                                for (int i = 0; i < val.Length; i++)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        for (int k = 0; k < 4; k++)
                                        {
                                            raw[(i * 4 + j) * 4 + k] = val[i][j, k];
                                        }
                                    }
                                }
                                GL.ProgramUniformMatrix4(programID, id, val.Length, false, raw);
                                break;
                            }
                    }
                }
            }
            else
            {
                GL.UseProgram(programID);
                int id = GetUniformID(name);
                if (id != -1)
                {
                    switch (value)
                    {
                        case int val:
                            GL.Uniform1(id, val);
                            break;
                        case float val:
                            GL.Uniform1(id, val);
                            break;
                        case Vector2 val:
                            GL.Uniform2(id, val);
                            break;
                        case Vector4 val:
                            GL.Uniform4(id, val);
                            break;
                        case Color4 val:
                            GL.Uniform4(id, val);
                            break;
                        case Matrix4 val:
                            GL.UniformMatrix4(id, false, ref val);
                            break;
                        case Matrix4[] val:
                            {
                                var raw = new float[val.Length * 16];
                                for (int i = 0; i < val.Length; i++)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        for (int k = 0; k < 4; k++)
                                        {
                                            raw[(i * 4 + j) * 4 + k] = val[i][j, k];
                                        }
                                    }
                                }
                                GL.UniformMatrix4(id, val.Length, false, raw);
                                break;
                            }
                    }
                }
                GL.UseProgram(nowBindID);
            }
        }

        public void SetFloat(string name, float value)
        {
            variables[name] = value;
            Uniform(name, value);
        }

        public void SetInt(string name, int value)
        {
            variables[name] = value;
            Uniform(name, value);
        }

        public void SetTexture(string name, RenderTexture value)
        {
            variables[name] = value;
        }

        public void SetVector2(string name, Vector2 value)
        {
            variables[name] = value;
            Uniform(name, value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            variables[name] = value;
            Uniform(name, value);
        }

        public void SetColor(string name, Color4 value)
        {
            variables[name] = value;
            Uniform(name, value);
        }

        public void SetMatrix4(string name, Matrix4 value)
        {
            variables[name] = value;
            Uniform(name, value);
        }

        public void SetTextureArray(string name, RenderTexture[] value)
        {
            variables[name] = value;
        }

        public void SetMatrix4Array(string name, Matrix4[] value)
        {
            variables[name] = value;
            Uniform(name, value);
        }

        public T Get<T>(string name)
        {
            return variables.TryGetValue(name, out var ret) ? (T)ret : throw new Exception(materialName + ": значение " + name + " не инициализировано ");
        }

        public int GetInt(string name) => Get<int>(name);
        public float GetFloat(string name) => Get<float>(name);
        public Color4 GetColor(string name) => Get<Color4>(name);
        public Vector4 GetVector4(string name) => Get<Vector4>(name);
        public RenderTexture GetTexture(string name) => Get<RenderTexture>(name);

        public void CopyPropertiesFromMaterial(Material material)
        {
            foreach (var item in material.variables)
            {
                string varName = item.Key;
                switch (item.Value)
                {
                    case int value:
                        {
                            SetInt(varName, value);
                            break;
                        }
                    case float value:
                        {
                            SetFloat(varName, value);
                            break;
                        }
                    case Color4 value:
                        {
                            SetColor(varName, value);
                            break;
                        }
                    case Vector4 value:
                        {
                            SetVector4(varName, value);
                            break;
                        }
                    case Matrix4 value:
                        {
                            SetMatrix4(varName, value);
                            break;
                        }
                    case RenderTexture value:
                        {
                            SetTexture(varName, value);
                            break;
                        }
                }
            }
        }

        private void CheckIfBound()
        {
#if DEBUG
            int currentProgramID = GL.GetInteger(GetPName.CurrentProgram);
            if (currentProgramID != programID)
            {
                throw new Exception($"ShaderProgram {programID} is not bound");
            }
#endif
        }

    }
}
