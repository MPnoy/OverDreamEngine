using System;
using System.Collections.Generic;

namespace ODEngine.Core
{
    public class TexturePool
    {
        public class TextureRequest
        {
            public RenderTexture texture = null;
            public int id;

            public override string ToString()
            {
                return "size: " + texture.Width + "x" + texture.Height;
            }
        }

        private readonly List<TextureRequest> pullFree = new List<TextureRequest>(10);
        private readonly List<TextureRequest> pullUse = new List<TextureRequest>(10);
        private int idCounter = 0;

        public TexturePool()
        {
            ////При старте игры создаются текстуры, которые покрывают все или большинство использований
            //var array = new List<TextureRequest>(10);
            //array.Add(CaptureTextureRequest(3100, 8000, true, true));
            //array.Add(CaptureTextureRequest(2400, 5000, true, true));
            //array.Add(CaptureTextureRequest(1920, 1080, true, true));
            //array.Add(CaptureTextureRequest(1920, 1080, true, true));
            //for (int i = 0; i < array.Count; i++)
            //{
            //    FreeTextureRequest(array[i]);
            //}
        }

        public TextureRequest CaptureTextureRequest(int width, int height, bool useMipMap = false, bool hardAspect = false)
        {
            if (hardAspect)
            {
                RoundSize(ref width, ref height);
            }
            TextureRequest ret = null;
            foreach (var item in pullFree)
            {
                bool flag;
                if (hardAspect)
                {
                    flag = item.texture.Width >= width && item.texture.Height >= height &&
                           EquelsAspects((float)item.texture.Width / item.texture.Height, (float)width / height);
                }
                else
                {
                    flag = item.texture.Width >= width && item.texture.Height >= height;
                }
                if (flag && item.texture.UseMipMap == useMipMap)
                {
                    if (ret == null || item.texture.Width * item.texture.Height < ret.texture.Width * ret.texture.Height)
                    {
                        ret = item;
                    }
                }
            }
            if (ret != null)
            {
                pullFree.Remove(ret);
            }
            else
            {
                ret = new TextureRequest
                {
                    texture = RenderTexture.GetTemporary(width, height, useMipMap),
                    id = idCounter
                };
                idCounter++;
                foreach (var item in pullFree)
                {
                    var flagReplace = item.texture.Width <= width && item.texture.Height <= height &&
                                      EquelsAspects((float)item.texture.Width / item.texture.Height, (float)width / height);
                    if (flagReplace)
                    {
                        RenderTexture.ReleaseTemporary(item.texture);
                        pullFree.Remove(item);
                        break;
                    }
                }
                //Debug.Log("RenderTexture created: " + width + "x" + height + ", useMipMaps = " + useMipMaps + ", ID: " + ret.ID + ", pullFree count: " + pullFree.Count + ", pullUse count: " + (pullUse.Count + 1));
            }
            pullUse.Add(ret);
            return ret;
        }

        public static void RoundSize(ref int width, ref int height, float accuracy = 0.75f)
        {
            //Приводим окончание чисел к степеням двойки для сокращения количества текстур
            //чем больше точность - тем больше текстур, но точнее мипмапы
            //Насколько точно должны совпадать размеры созданных текстур,
            //чем меньше параметр - тем более сильно размер приводится к степени двойки,
            //чем больше (максимум 1) - тем более точные размеры, но потребление памяти может быть выше
            //из-за создания множества текстур, мало отличающихся по размеру
            accuracy = 1f / (1f - accuracy);
            int widthDegree2 = (int)MathF.Pow(2f, MathF.Floor(MathF.Log(width / accuracy) / MathF.Log(2f)));
            int heightDegree2 = (int)MathF.Pow(2f, MathF.Floor(MathF.Log(height / accuracy) / MathF.Log(2f)));
            if (widthDegree2 == 0)
            {
                widthDegree2 = 1;
            }
            if (heightDegree2 == 0)
            {
                heightDegree2 = 1;
            }
            width = ((width - 1) / widthDegree2 + 1) * widthDegree2;
            height = ((height - 1) / heightDegree2 + 1) * heightDegree2;
        }

        public void FreeTextureRequest(TextureRequest textureRequest)
        {
            if (!pullUse.Remove(textureRequest))
            {
                throw new Exception();
            }
            //textureRequest.texture.FreeMemory();
            //RenderTexture.ReleaseTemporary(textureRequest.texture);
            //textureRequest.texture = null;
            pullFree.Add(textureRequest);
        }

        private bool EquelsAspects(float aspect1, float aspect2)
        {
            return aspect1 / aspect2 < 1.1f &&
                   aspect1 / aspect2 > (1f / 1.1f);
        }

        public void OnDestroy()
        {
            foreach (var item in pullUse)
            {
                RenderTexture.ReleaseTemporary(item.texture);
            }
            foreach (var item in pullFree)
            {
                RenderTexture.ReleaseTemporary(item.texture);
            }
        }

    }
}