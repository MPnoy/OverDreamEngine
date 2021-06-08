using System;
using System.Collections.Generic;
using ODEngine.Core;
using System.Text.Json;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

public static class MaterialReader
{
    public static MaterialsInfo materialsInfo;
    public static Dictionary<string, Material> materials;

    [Serializable]
    public class MaterialsInfo
    {
        [Serializable]
        public class MaterialInfo
        {
            [Serializable]
            public class VariableInfo
            {
                public string Type { get; set; }
                public string Name { get; set; }
                public string Value { get; set; }
            }

            public string Name { get; set; }
            public string Shader { get; set; }
            public VariableInfo[] Variables { get; set; }
        }

        public MaterialInfo[] Materials { get; set; }
    }

    public static void Read()
    {
        string text = FileManager.DataReadAllText("Text/Materials.json");
        materialsInfo = JsonSerializer.Deserialize<MaterialsInfo>(text);
        materials = new Dictionary<string, Material>();

        foreach (var matInfo in materialsInfo.Materials)
        {
            var mat = new Material("Atlas/Identity", "Game/" + matInfo.Shader);

            foreach (var variable in matInfo.Variables)
            {
                switch (variable.Type)
                {
                    case "Texture":
                        {
                            using var image = Image.Load<Rgba32>(FileManager.DataReadAllBytes(variable.Value + ".png"));
                            var rawImage = ImageLoader.ImageToBytesStatic(image);

                            var texture = RenderTexture.GetTemporary(rawImage.width, rawImage.height, false, true, false, 1f);

                            texture.LoadImage(rawImage);
#if DEBUG
                            GraphicsHelper.GLCheckErrorFast();
#endif
                            mat.SetTexture(variable.Name, texture);
                            break;
                        }

                    case "Float":
                        mat.SetFloat(variable.Name, float.Parse(variable.Value));
                        break;

                    case "Color":
                        mat.SetColor(variable.Name, SColor.FromHTMLString(variable.Value));
                        break;

                    default:
                        throw new Exception("Invalid type, expected Texture, Float or Color");
                }
            }

            materials.Add(matInfo.Name, mat);
        }
    }

}