using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using ODEngine.Core;
using ODEngine.Game;
using ODEngine.Game.Text;

namespace ODEngine.Helpers
{
    public static class SaveLoadHelper
    {
        [Serializable]
        public struct Save
        {
            public string LabelNow { get; set; }
            public string UserDescription { get; set; }
            public DateTime Date { get; set; }
            public TextManager.TextMode TextMode { get; set; }
            public int LineOnStream { get; set; }
            public int DataIndex { get; set; }
            public int MicroIndex { get; set; }
            public ScenarioStep.DataText PrevText { get; set; }
            public ScenarioStep.DataText NowText { get; set; }
            public List<ScenarioStep.DataAddImage> ListI { get; set; }
            public List<ScenarioStep.DataAddSound> ListA { get; set; }
            public byte[] Texture { get; set; }
        }

        public static void DeleteSave(int saveIndex)
        {
            if (saveIndex < 0)
            {
                throw new Exception();
            }

            LinkedList<Save> totalSave;
            BinaryFormatter Formatter = new BinaryFormatter();

            if (FileManager.SystemExists("save.dat"))
            {
                using (var fs = FileManager.SystemGetReadStream("save.dat"))
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

            using (var fs = FileManager.GetWriteStream("save.dat", System.IO.FileMode.Create))
            {
                Formatter.Serialize(fs, totalSave);
            }
        }

        public static void SaveGame(string userDescription)
        {
            Save save = default;
            save.UserDescription = userDescription;
            save.LabelNow = GameKernel.screenManager.scenarioScreen.labelNow;
            save.TextMode = GameKernel.screenManager.scenarioScreen.textManager.textAnimator.ActiveMode;
            save.LineOnStream = GameKernel.screenManager.scenarioScreen.lineOnStream;
            save.DataIndex = GameKernel.screenManager.scenarioScreen.dataIndex;
            save.MicroIndex = GameKernel.screenManager.scenarioScreen.microIndex;
            save.PrevText = GameKernel.screenManager.scenarioScreen.prevText;
            save.NowText = GameKernel.screenManager.scenarioScreen.nowText;
            save.Date = DateTime.Now;

            //var tex = RenderTexture.GetTemporary((int)Graphics.cameraWidth, (int)Graphics.cameraHeight);
            //Kernel.screenManager.scenarioScreen.screenRenderer.Render(null, tex);
            //var casted = MemoryMarshal.Cast<byte, Rgba32>(tex.GetRaw());
            //var img = Image.LoadPixelData<Rgba32>(casted, tex.Width, tex.Height);
            //img.Mutate(x => x.Flip(FlipMode.Vertical));
            //var stream = new MemoryStream();
            //img.SaveAsPng(stream);
            //save.Texture = stream.ToArray();

            save.ListI = GameKernel.screenManager.scenarioScreen.imageManager.GetAllObjectsData();
            save.ListA = GameKernel.screenManager.scenarioScreen.audioManager.GetAllObjectsData();

            List<Save> totalSave = GetSaves();
            totalSave.Add(save);
            FileManager.WriteAllText("save.dat", JsonSerializer.Serialize(totalSave, new JsonSerializerOptions() { WriteIndented = true }), System.Text.Encoding.Unicode);
        }

        public static List<Save> GetSaves()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            if (!FileManager.SystemExists("save.dat"))
            {
                return new List<Save>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<Save>>(FileManager.SystemReadAllText("save.dat", System.Text.Encoding.Unicode));
            }
            catch (Exception ex)
            {
                throw new Exception("Save loading error", ex);
            }
        }

        public static bool LoadGame(int saveIndex)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            if (!FileManager.SystemExists("save.dat"))
            {
                return false;
            }

            List<Save> totalSave;

            try
            {
                totalSave = JsonSerializer.Deserialize<List<Save>>(FileManager.SystemReadAllText("save.dat", System.Text.Encoding.Unicode));
            }
            catch (Exception ex)
            {
                throw new Exception("Save loading error", ex);
            }

            if (saveIndex < 0)
            {
                saveIndex = totalSave.Count + saveIndex;
            }

            GameKernel.screenManager.scenarioScreen.Show(null, true);
            GameKernel.screenManager.scenarioScreen.textManager.NvlClear();

            var save = totalSave[saveIndex];
            GameKernel.screenManager.scenarioScreen.labelNow = save.LabelNow;
            GameKernel.screenManager.scenarioScreen.history = new LinkedList<ScenarioStep>();
            GameKernel.screenManager.scenarioScreen.textManager.textAnimator.ActiveMode = save.TextMode;
            GameKernel.screenManager.scenarioScreen.lineOnStream = save.LineOnStream;
            GameKernel.screenManager.scenarioScreen.dataIndex = save.DataIndex;
            GameKernel.screenManager.scenarioScreen.microIndex = save.MicroIndex;
            GameKernel.screenManager.scenarioScreen.prevText = save.PrevText;
            GameKernel.screenManager.scenarioScreen.nowText = save.NowText;

            GameKernel.screenManager.scenarioScreen.imageManager.RemoveGroup(GameKernel.screenManager.scenarioScreen.imageManager.oldScene, SpeedMode.Normal, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
            GameKernel.screenManager.scenarioScreen.imageManager.RemoveGroup(GameKernel.screenManager.scenarioScreen.imageManager.newScene, SpeedMode.Normal, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
            GameKernel.screenManager.scenarioScreen.audioManager.RemoveGroup(SpeedMode.Normal, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false);

            for (int i = 0; i < save.ListI.Count; i++)
            {
                GameKernel.screenManager.scenarioScreen.imageManager.AddImage(GameKernel.screenManager.scenarioScreen.imageManager.newScene, SpeedMode.Normal, save.ListI[i], false);
            }

            for (int i = 0; i < save.ListA.Count; i++)
            {
                GameKernel.screenManager.scenarioScreen.audioManager.AddSound(SpeedMode.Normal, save.ListA[i], false);
            }

            string s = "";

            if (save.MicroIndex == -1)
            {
                save.NowText.microTexts.ForEach(Item => s += Item);
            }
            else
            {
                for (int i = 0; i < save.MicroIndex; i++)
                {
                    s += save.NowText.microTexts[i];
                }
            }

            GameKernel.screenManager.scenarioScreen.textManager.NextStep(s, save.NowText.characterID);

            return true;
        }

    }

}
