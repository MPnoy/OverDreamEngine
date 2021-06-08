using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ODEngine.Core
{
    public class Material
    {
        public struct SerializableData
        {
            public struct Var
            {
                public string Name { get; set; }
                public string Type { get; set; }
                public object Value { get; set; }

                public Var(string name, string type, object value)
                {
                    Name = name;
                    Type = type;
                    Value = value;
                }
            }

            public string VertName { get; set; }
            public string FragName { get; set; }
            public Dictionary<string, int> UniformIDs { get; set; }
            public HashSet<string> NoLocationUniforms { get; set; }
            public List<Var> Variables { get; set; }
            public BlendingFactor BlendingFactorSource { get; set; }
            public BlendingFactor BlendingFactorDestination { get; set; }

            public Material Deserialize()
            {
                var ret = new Material(UniformIDs, NoLocationUniforms, Variables)
                {
                    vertName = VertName,
                    fragName = FragName,
                    blendingFactorSource = BlendingFactorSource,
                    blendingFactorDestination = BlendingFactorDestination,
                };

                var vert = shaders[(ShaderType.VertexShader, VertName)];
                var frag = FragName == null ? default : shaders[(ShaderType.FragmentShader, FragName)];
                ret.Construct(vert, frag);
                return ret;
            }
        }

        private struct Shader
        {
            public static Shader Empty { get; } = default;

            public ShaderType shaderType;
            public int id;
            public List<string> ins;
        }

        private static int nowBindID = -1;
        private static readonly Dictionary<(ShaderType type, string name), Shader> shaders = new Dictionary<(ShaderType type, string name), Shader>();

        public string vertName;
        public string fragName;

        private readonly Dictionary<string, int> uniformIDs = new Dictionary<string, int>();
        private readonly HashSet<string> noLocationUniforms = new HashSet<string>();
        private readonly Dictionary<string, object> variables = new Dictionary<string, object>();

        public BlendingFactor blendingFactorSource = BlendingFactor.One;
        public BlendingFactor blendingFactorDestination = BlendingFactor.Zero;

        public int programID = -1;
        private Shader vert = default;
        private Shader frag = default;

        public SerializableData Serialize()
        {
            var serVars = new List<SerializableData.Var>(variables.Count);

            foreach (var variable in variables)
            {
                switch (variable.Value)
                {
                    case int val:
                        serVars.Add(new SerializableData.Var(variable.Key, "int", val));
                        break;
                    case float val:
                        serVars.Add(new SerializableData.Var(variable.Key, "float", val));
                        break;
                    case Vector2 val:
                        serVars.Add(new SerializableData.Var(variable.Key, "Vector2", new[] { val.X, val.Y }));
                        break;
                    case Vector4 val:
                        serVars.Add(new SerializableData.Var(variable.Key, "Vector4", new[] { val.X, val.Y, val.Z, val.W }));
                        break;
                    case Color4 val:
                        serVars.Add(new SerializableData.Var(variable.Key, "Color4", new[] { val.R, val.G, val.B, val.A }));
                        break;
                    case Matrix4 val:
                        {
                            var raw = new float[4, 4];

                            for (int i = 0; i < 4; i++)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    raw[i, j] = val[i, j];
                                }
                            }

                            serVars.Add(new SerializableData.Var(variable.Key, "Matrix4", raw));
                            break;
                        }
                    case Matrix4[] val:
                        {
                            var raw = new float[val.Length, 4, 4];

                            for (int i = 0; i < val.Length; i++)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    for (int k = 0; k < 4; k++)
                                    {
                                        raw[i, j, k] = val[i][j, k];
                                    }
                                }
                            }

                            serVars.Add(new SerializableData.Var(variable.Key, "Matrix4Array", raw));
                            break;
                        }
                }
            }

            return new SerializableData
            {
                VertName = vertName,
                FragName = fragName,
                UniformIDs = uniformIDs,
                NoLocationUniforms = noLocationUniforms,
                Variables = serVars,
                BlendingFactorSource = blendingFactorSource,
                BlendingFactorDestination = blendingFactorDestination,
            };
        }

        private static void LoadVertShader(string name, string filename)
        {
            shaders.Add((ShaderType.VertexShader, name), CompileShader(filename, ShaderType.VertexShader));
        }

        private static void LoadFragShader(string name, string filename)
        {
            shaders.Add((ShaderType.FragmentShader, name), CompileShader(filename, ShaderType.FragmentShader));
        }

        public override string ToString()
        {
            var ret = vertName + " : " + fragName + " ";

            foreach (var item in variables)
            {
                ret += "\n" + "[" + (item.Key != null ? item.Key.ToString() : "null") + "] = " + (item.Value != null ? item.Value.ToString() : "null");
            }

            return ret;
        }

        private static void LoadShader(string rootFolder, string filename)
        {
            var relative = FileManager.GetRelativePath(rootFolder, filename);
            var name = relative.Substring(0, relative.LastIndexOf('.')).Replace('\\', '/');
            var extension = FileManager.GetExtension(filename);

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

        private static Shader CompileShader(string filename, ShaderType shaderType, Version glVersion = null)
        {
            if (glVersion == null)
            {
                glVersion = Graphics.glVersion;
            }

            var glVersionString = glVersion.ToString();

            string glslVersion = glVersionString switch
            {
                "2.0" => "130",
                "2.1" => "130",
                "3.0" => "130",
                "3.1" => "140",
                "3.2" => "150",
                _ => glVersionString.Replace(".", null) + "0"
            };

            string source;
            string filenameCompatible = filename.Substring(0, Math.Max(filename.LastIndexOf(@"\"), filename.LastIndexOf(@"/"))) +
                "/" + FileManager.GetFileNameWithoutExtension(filename) + "Compatible" + FileManager.GetExtension(filename);

            source = Graphics.gl_shading_language_420
                ? FileManager.DataReadAllText(filename)
                : glVersion.Major < 3 || (glVersion.Major == 3 && glVersion.Minor < 2)
                    ? FileManager.DataExists(filenameCompatible)
                        ? FileManager.DataReadAllText(filenameCompatible)
                        : FileManager.DataReadAllText(filename)
                    : FileManager.DataReadAllText(filename);

            var shader = new Shader()
            {
                shaderType = shaderType
            };

            source = Preprocess(source, ref shader, glslVersion);

            if (!Graphics.gl_shading_language_420)
            {
                Logger.Log($"shader {filename} change to GLSL {glslVersion}");

                for (int i = 1; i <= 4; i++)
                {
                    for (int j = 0; j <= 9; j++)
                    {
                        source = source.Replace($"#version {i}{j}0", $"#version {glslVersion}");
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
                    Logger.Log("Error " + shaderType.ToString() + "! Filename: " + filename + "\n" + GL.GetShaderInfoLog(id));
                    Graphics.gl_shading_language_420 = false;
                    return CompileShader(filename, shaderType);
                }
                else if (glVersion.Major != 3 || glVersion.Minor != 0)
                {
                    Logger.Log("Error " + shaderType.ToString() + "! Filename: " + filename + "\n" + GL.GetShaderInfoLog(id));
                    return CompileShader(filename, shaderType, new Version(3, 0));
                }
                else
                {
                    throw new Exception("Error " + shaderType.ToString() + "! Filename: " + filename + "\n" + GL.GetShaderInfoLog(id));
                }
            }

            shader.id = id;
            return shader;
        }

        private static string Preprocess(string shaderText, ref Shader shader, string glslVersion)
        {
            shaderText = shaderText.Replace("\r", null);
            int start = -1;

            if (shader.shaderType == ShaderType.VertexShader)
            {
                shader.ins = new List<string>();

                while (true)
                {
                    start = shaderText.IndexOf("\nin ", start + 1) + 1;

                    if (start < 1)
                    {
                        break;
                    }

                    var variableStr = shaderText.Substring(start);
                    variableStr = variableStr.Substring(0, variableStr.IndexOf("\n"));
                    variableStr = variableStr.Trim(' ');
                    var variableSplit = variableStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var variableName = variableSplit[2].TrimEnd(';');

                    shader.ins.Add(variableName);
                }
            }

            while (true)
            {
                start = shaderText.IndexOf(@"\\\for");

                if (start < 0)
                {
                    break;
                }

                var end = shaderText.IndexOf("\n", start);
                var variableStr = shaderText.Substring(start);
                variableStr = variableStr.Substring(0, variableStr.IndexOf("\n"));
                variableStr = variableStr.Trim(' ');
                var variableSplit = variableStr.Split('_');

                var variableName = variableSplit[1];
                var variableStart = int.Parse(variableSplit[2]);
                var variableEnd = int.Parse(variableSplit[3]);

                var body = shaderText.Substring(end + 1);
                body = body.Substring(0, body.IndexOf(@"\\\next_" + variableName));

                shaderText = shaderText.Remove(start, end + 1 - start + body.Length + (@"\\\next_" + variableName).Length);

                for (int i = variableStart; i < variableEnd; i++)
                {
                    var bodyReplaced = body.Replace(@"\\\" + variableName, $"{i}") + "\n";
                    shaderText = shaderText.Insert(start, bodyReplaced);
                    start += bodyReplaced.Length;
                }
            }

            shaderText = shaderText.Replace(@"\\\version", glslVersion);

            return shaderText;
        }

        public static void LoadShaders()
        {
            var root = "Shaders";
            var filenames = FileManager.DataGetFiles(root, "*", System.IO.SearchOption.AllDirectories);

            for (int i = 0; i < filenames.Length; i++)
            {
                if (!filenames[i].Contains("Compatible."))
                {
                    LoadShader(root, filenames[i]);
                }
            }
        }

        public Material(string vertName = null, string fragName = null)
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

            var vert = shaders[(ShaderType.VertexShader, vertName)];
            var frag = fragName == null ? default : shaders[(ShaderType.FragmentShader, fragName)];
            Construct(vert, frag);
        }

        public Material(Material material)
        {
            Construct(material.vert, material.frag);
        }

        private Material(Dictionary<string, int> uniformIDs, HashSet<string> noLocationUniforms, List<SerializableData.Var> variables)
        {
            this.uniformIDs = uniformIDs;
            this.noLocationUniforms = noLocationUniforms;
            foreach (var variable in variables)
            {
                switch (variable.Type)
                {
                    case "int":
                        {
                            var val = ((JsonElement)variable.Value).GetInt32();
                            this.variables.Add(variable.Name, val);
                            break;
                        }
                    case "float":
                        {
                            var val = ((JsonElement)variable.Value).GetSingle();
                            this.variables.Add(variable.Name, val);
                            break;
                        }
                    case "Vector2":
                        {
                            var val = (JsonElement)variable.Value;
                            this.variables.Add(variable.Name, new Vector2(val[0].GetSingle(), val[1].GetSingle()));
                            break;
                        }
                    case "Vector4":
                        {
                            var val = (JsonElement)variable.Value;
                            this.variables.Add(variable.Name, new Vector4(val[0].GetSingle(), val[1].GetSingle(), val[2].GetSingle(), val[3].GetSingle()));
                            break;
                        }
                    case "Color4":
                        {
                            var val = (JsonElement)variable.Value;
                            this.variables.Add(variable.Name, new Color4(val[0].GetSingle(), val[1].GetSingle(), val[2].GetSingle(), val[3].GetSingle()));
                            break;
                        }
                    case "Matrix4":
                        {
                            var val = (JsonElement)variable.Value;
                            var matrix = new Matrix4();

                            for (int i = 0; i < 4; i++)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    matrix[i, j] = val[i][j].GetSingle();
                                }
                            }

                            this.variables.Add(variable.Name, matrix);
                            break;
                        }
                    case "Matrix4Array":
                        {
                            var val = (JsonElement)variable.Value;
                            var len = val.GetArrayLength();
                            var matrixes = new Matrix4[len];

                            for (int i = 0; i < len; i++)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    for (int k = 0; k < 4; k++)
                                    {
                                        matrixes[i][j, k] = val[i][j][k].GetSingle();
                                    }
                                }
                            }

                            this.variables.Add(variable.Name, matrixes);
                            break;
                        }
                }
            }
        }

        private void Construct(Shader vert, Shader frag)
        {
            this.vert = vert;
            this.frag = frag;

            programID = GL.CreateProgram(); // Создаем программу и прикрепляем шейдеры к ней

            for (int i = 0; i < vert.ins.Count; i++)
            {
                GL.BindAttribLocation(programID, i, vert.ins[i]);
            }

            GL.AttachShader(programID, vert.id);

            if (!frag.Equals(Shader.Empty))
            {
                GL.AttachShader(programID, frag.id);
            }

            GL.LinkProgram(programID); // Линкуем шейдерную программу
#if DEBUG
            BindProgram(); // Проверяем, что залинковалось, если тут ошибка, проверь точки входа, должны быть main()
#endif
            GL.DetachShader(programID, vert.id);

            if (frag.id != -1)
            {
                GL.DetachShader(programID, frag.id);
            }

            GL.BindFragDataLocation(programID, 0, "out_Color");
        }

        public void Bind()
        {
            GL.BlendFunc(blendingFactorSource, blendingFactorDestination);
            BindProgram();
            int texCounter = 0;

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
                    throw new Exception("Ошибка переменной " + name + " в шейдере " + vertName + " : " + fragName + ": " + errorCode.ToString());
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
            return variables.TryGetValue(name, out var ret) ? (T)ret : throw new Exception(vertName + " : " + fragName + ": значение " + name + " не инициализировано ");
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
