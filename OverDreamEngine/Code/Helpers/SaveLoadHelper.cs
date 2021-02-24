using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using ODEngine.Core;
using ODEngine.Game;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ODEngine.Helpers
{
    public static class SaveLoadHelper
    {
        [Serializable]
        public struct Save
        {
            public string labelNow;
            public string userDescription;
            public DateTime date;
            public byte[] texture;
            public int lineOnStream;
            public int dataIndex;
            public int microIndex;
            public ScenarioStep.DataText prevText;
            public ScenarioStep.DataText nowText;
            public List<ScenarioStep.DataAddImage> listI;
            public List<ScenarioStep.DataAddSound> listA;
        }

        public static void DeleteSave(int saveIndex)
        {
            if (saveIndex < 0)
            {
                throw new Exception();
            }

            LinkedList<Save> totalSave;
            BinaryFormatter Formatter = new BinaryFormatter();
            if (File.Exists("save.dat"))
            {
                using (FileStream fs = new FileStream("save.dat", FileMode.Open))
                {
                    totalSave = (LinkedList<Save>)Formatter.Deserialize(fs);
                }
            }
            else
            {
                return;
            }

            var link = totalSave.First;
            for (int i = 0; i < saveIndex; i++)
            {
                link = link.Next;
            }
            totalSave.Remove(link);

            using (FileStream fs = new FileStream("save.dat", FileMode.Create))
            {
                Formatter.Serialize(fs, totalSave);
            }
        }

        public static void SaveGame(string userDescription)
        {
            Save save = default;
            save.userDescription = userDescription;
            save.labelNow = Kernel.screenManager.scenarioScreen.labelNow;
            save.lineOnStream = Kernel.screenManager.scenarioScreen.lineOnStream;
            save.dataIndex = Kernel.screenManager.scenarioScreen.dataIndex;
            save.microIndex = Kernel.screenManager.scenarioScreen.microIndex;
            save.prevText = Kernel.screenManager.scenarioScreen.prevText;
            save.nowText = Kernel.screenManager.scenarioScreen.nowText;
            save.date = DateTime.Now;
            var tex = RenderTexture.GetTemporary((int)Graphics.cameraWidth, (int)Graphics.cameraHeight);
            Kernel.screenManager.scenarioScreen.screenRenderer.Render(null, tex);

            var casted = MemoryMarshal.Cast<byte, Rgba32>(tex.GetRaw());

            var img = Image.LoadPixelData<Rgba32>(casted, tex.Width, tex.Height);
            img.Mutate(x => x.Flip(FlipMode.Vertical));
            var stream = new MemoryStream();
            img.SaveAsPng(stream);
            save.texture = stream.ToArray();

            save.listI = Kernel.screenManager.scenarioScreen.imageManager.GetAllObjectsData();
            save.listA = Kernel.screenManager.scenarioScreen.audioManager.GetAllObjectsData();

            List<Save> totalSave;
            BinaryFormatter formatter = new BinaryFormatter();

            if (File.Exists("save.dat"))
            {
                using (FileStream fs = new FileStream("save.dat", FileMode.Open))
                {
                    totalSave = (List<Save>)formatter.Deserialize(fs);
                }
            }
            else
            {
                totalSave = new List<Save>();
            }

            totalSave.Add(save);

            using (FileStream fs = new FileStream("save.dat", FileMode.Create))
            {
                formatter.Serialize(fs, totalSave);
            }
        }

        public static List<Save> GetSaves()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            if (!File.Exists("save.dat"))
            {
                return new List<Save>();
            }

            using (FileStream fs = new FileStream("save.dat", FileMode.Open))
            {
                return (List<Save>)formatter.Deserialize(fs);
            }
        }

        public static void LoadGame(int saveIndex)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            if (!File.Exists("save.dat"))
            {
                return;
            }

            using (FileStream fs = new FileStream("save.dat", FileMode.Open))
            {
                List<Save> totalSave = (List<Save>)formatter.Deserialize(fs);

                if (saveIndex < 0)
                {
                    saveIndex = totalSave.Count + saveIndex;
                }

                var save = totalSave[saveIndex];
                Kernel.screenManager.scenarioScreen.labelNow = save.labelNow;
                Kernel.screenManager.scenarioScreen.history = new LinkedList<ScenarioStep>();
                Kernel.screenManager.scenarioScreen.lineOnStream = save.lineOnStream;
                Kernel.screenManager.scenarioScreen.dataIndex = save.dataIndex;
                Kernel.screenManager.scenarioScreen.microIndex = save.microIndex;
                Kernel.screenManager.scenarioScreen.prevText = save.prevText;
                Kernel.screenManager.scenarioScreen.nowText = save.nowText;

                var img = Image.Load<Rgba32>(save.texture);
                img.Save("Anus.png");

                //var tex = RenderTexture.GetTemporary((int)Graphics.cameraWidth, (int)Graphics.cameraHeight);

                Kernel.screenManager.scenarioScreen.imageManager.RemoveGroup(SpeedMode.normal, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false);
                Kernel.screenManager.scenarioScreen.audioManager.RemoveGroup(SpeedMode.normal, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false);

                for (int i = 0; i < save.listI.Count; i++)
                {
                    Kernel.screenManager.scenarioScreen.imageManager.AddImage(SpeedMode.normal, save.listI[i], false);
                }

                for (int i = 0; i < save.listA.Count; i++)
                {
                    Kernel.screenManager.scenarioScreen.audioManager.AddSound(SpeedMode.normal, save.listA[i], false);
                }

                string s = "";
                if (save.microIndex == -1)
                {
                    save.nowText.microTexts.ForEach(Item => s += Item);
                }
                else
                {
                    for (int i = 0; i < save.microIndex; i++)
                    {
                        s += save.nowText.microTexts[i];
                    }
                }

                Kernel.screenManager.scenarioScreen.textManager.NextStep(s, save.nowText.characterID);
            }
        }

    }

}
