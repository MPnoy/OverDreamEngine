using System;
using ODEngine.EC;
using ODEngine.EC.Components;
using ODEngine.Game.Screens;
using ODEngine.Game.Text;
using ODEngine.Core;
using Screens;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

static class Program
{
    private static ScenarioScreen scenarioScreen;

    [STAThread]
    static void Main()
    {
        PathBuilder.dataPath = "Data/";
        var icon = ImageLoader.ImageToBytesStatic(Image.Load<Rgba32>("Data/Images/Misc/ICON_small.png"), false);
        Kernel.icon = new OpenTK.Windowing.Common.Input.Image(icon.width, icon.height, icon.ToByteArray());
        Kernel.title = "Demo";
        ODEngine.Core.Program.Init();
        Kernel.GameInit += () =>
        {
            Kernel.screenManager.scenarioScreen.textManager.InterfaceCreating += TextManager_InterfaceCreating;
            Kernel.screenManager.scenarioScreen.textManager.InterfaceDestroying += TextManager_InterfaceDestroying;
            Kernel.screenManager.scenarioScreen.GameStarting += ScenarioScreen_GameStarting;
            Kernel.screenManager.GameStart(typeof(MainMenu), typeof(SettingsScreen), typeof(MiniMenu), typeof(ExitMenu));
            Kernel.screenManager.AddScreenToGame(typeof(LoadMenu));
        };
        ODEngine.Core.Program.StartGame();
    }

    private static void ScenarioScreen_GameStarting(object sender, EventArgs e)
    {
        scenarioScreen = (ScenarioScreen)sender;
    }

    private static void TextManager_InterfaceCreating(object sender, EventArgs e)
    {
        TextManager textManager = (TextManager)sender;
        textManager.guiRoot = new Entity().CreateComponent<Renderer>("GUI Root");
        textManager.gameTextADV = new Entity().CreateComponent<GameText>("ADV");
        textManager.gameTextNVL = new Entity().CreateComponent<GameText>("NVL");
        textManager.gameTextName = new Entity().CreateComponent<GameText>("Name");
        Renderer renderer;
        { // GUI Root
            textManager.guiRoot.SetParent(Kernel.screenManager.scenarioScreen.screenRenderer);
            textManager.guiRoot.position = new Vector3(0f, 0f, -6f);
        }
        { // ADV
            var size = new Vector2(19.2f, 3.5f);
            textManager.containerADV = GUIElement.CreateContainer(textManager.guiRoot, new Vector3(0f, -3.7f, -1f), size + new Vector2(6f, 6f), "Game/Alpha");
            textManager.containerADV.name = "containerADV";
            renderer = textManager.gameTextADV.Entity.GetComponent<Renderer>();
            renderer.SetParent(textManager.containerADV.renderer);
            //var background = GUIElement.CreateImage(textManager.containerADV.renderer, new Vector3(0f, 0f, 1f), new Vector2(19.2f, 3.5f), "GUI/newdialogue", 0.8f);
            var background = GUIElement.CreateImage(textManager.containerADV.renderer, new Vector3(0f, -0.45f, 1f), new Vector2(15.56f, 1.71f), "GUI/adv_box");
            background.Entity.GetComponent<Renderer>().name = "ADV Background";
            var clickArea = GUIElement.CreateEmpty(textManager.containerADV.renderer, new Vector3(0f, 0f, 0f), textManager.containerADV.renderer.size);
            var buttonLeft = GUIElement.CreateImage(textManager.containerADV.renderer, new Vector3(-size.X / 2f + 1.4f, -0.5f, -1f), new Vector2(0.5f, 0.5f / 66f * 128f), "GUI/button_left", new Material("Game/Color", null, "Game/Color"));
            var buttonRight = GUIElement.CreateImage(textManager.containerADV.renderer, new Vector3(size.X / 2f - 1.4f, -0.5f, -1f), new Vector2(0.5f, 0.5f / 66f * 128f), "GUI/button_right", new Material("Game/Color", null, "Game/Color"));
            buttonLeft.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            buttonRight.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            clickArea.name = "clickAreaADV";
            clickArea.renderer.name = "clickAreaADV";
            clickArea.MouseDown += scenarioScreen.ClickArea_MouseDown;
            buttonLeft.MouseDown += (_, _) => scenarioScreen.PressBackStep();
            buttonRight.MouseDown += (_, _) => scenarioScreen.RollForward();
            buttonLeft.MouseEnter += (_, _) =>
            {
                buttonLeft.material.SetColor("color", new Color4(1.5f, 1.5f, 1.5f, 1f));
            };
            buttonRight.MouseEnter += (_, _) =>
            {
                buttonRight.material.SetColor("color", new Color4(1.5f, 1.5f, 1.5f, 1f));
            };
            buttonLeft.MouseLeave += (_, _) =>
            {
                buttonLeft.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            };
            buttonRight.MouseLeave += (_, _) =>
            {
                buttonRight.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            };
            ADVNormalTransform(textManager);
        }
        { // NVL
            var size = new Vector2(14.93f, 9.5187f);
            textManager.containerNVL = GUIElement.CreateContainer(textManager.guiRoot, new Vector3(0f, -0.192f, -1f), size + new Vector2(4f, 4f), "Game/Alpha");
            textManager.containerNVL.name = "containerNVL";
            renderer = textManager.gameTextNVL.Entity.GetComponent<Renderer>();
            textManager.gameTextNVL.UpdateSize(size.X, size.Y);
            textManager.gameTextNVL.FontName = "BloggerSans";
            textManager.gameTextNVL.Refresh();
            renderer.SetParent(textManager.containerNVL.renderer);
            var background = GUIElement.CreateImage(textManager.containerNVL.renderer, new Vector3(0f, 0f, 1f), size + new Vector2(0.5f, 0.5f), "GUI/NVL_New", 0.8f);
            background.Entity.GetComponent<Renderer>().name = "NVL Background";
            var clickArea = GUIElement.CreateEmpty(textManager.containerNVL.renderer, new Vector3(0f, 0f, 0f), textManager.containerNVL.renderer.size);
            var buttonLeft = GUIElement.CreateImage(textManager.containerNVL.renderer, new Vector3(-size.X / 2f - 0.8f, -size.Y / 2f + 0.3f, -1f), new Vector2(0.5f, 0.5f / 66f * 128f), "GUI/button_left", new Material("Game/Color", null, "Game/Color"));
            var buttonRight = GUIElement.CreateImage(textManager.containerNVL.renderer, new Vector3(size.X / 2f + 0.8f, -size.Y / 2f + 0.3f, -1f), new Vector2(0.5f, 0.5f / 66f * 128f), "GUI/button_right", new Material("Game/Color", null, "Game/Color"));
            buttonLeft.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            buttonRight.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            clickArea.name = "clickAreaNVL";
            clickArea.renderer.name = "clickAreaNVL";
            clickArea.MouseDown += scenarioScreen.ClickArea_MouseDown;
            buttonLeft.MouseDown += (_, _) => scenarioScreen.PressBackStep();
            buttonRight.MouseDown += (_, _) => scenarioScreen.RollForward();
            buttonLeft.MouseEnter += (_, _) =>
            {
                buttonLeft.material.SetColor("color", new Color4(1.5f, 1.5f, 1.5f, 1f));
            };
            buttonRight.MouseEnter += (_, _) =>
            {
                buttonRight.material.SetColor("color", new Color4(1.5f, 1.5f, 1.5f, 1f));
            };
            buttonLeft.MouseLeave += (_, _) =>
            {
                buttonLeft.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            };
            buttonRight.MouseLeave += (_, _) =>
            {
                buttonRight.material.SetColor("color", new Color4(1f, 1f, 1f, 1f));
            };
        }
        { // Name
            textManager.containerName = GUIElement.CreateContainer(textManager.guiRoot, new Vector3(-5.624f, -3.097f, 0f), new Vector2(4.14f, 0.49f), "Game/Alpha");

            renderer = textManager.gameTextName.Entity.GetComponent<Renderer>();
            textManager.gameTextName.UpdateSize(4.14f, 0.49f);
            textManager.gameTextName.outline = true;
            textManager.gameTextName.FontName = "BloggerSans";
            textManager.gameTextName.Refresh();
            renderer.SetParent(textManager.containerName.renderer);
        }
    }

    private static void TextManager_InterfaceDestroying(object sender, EventArgs e)
    {
        TextManager textManager = (TextManager)sender;
        textManager.gameTextADV.Entity.Destroy();
        textManager.gameTextNVL.Entity.Destroy();
        textManager.gameTextName.Entity.Destroy();
    }

    public static void ADVNormalTransform(TextManager textManager)
    {
        textManager.containerADV.renderer.position = new Vector3(0f, -3.7f, 0f);
        textManager.containerADV.renderer.size = new Vector2(19.2f, 3.5f);
        var renderer = textManager.gameTextADV.Entity.GetComponent<Renderer>();
        renderer.position = new Vector3(0f, -0.454f, 0f);
        //textManager.gameTextADV.UpdateSize(17f, 1.55f);
        textManager.gameTextADV.UpdateSize(14.93f, 1.55f);
        textManager.gameTextADV.FontName = "BloggerSans";
        textManager.gameTextADV.Refresh();
    }

    public static void ADVLargeTransform(TextManager textManager)
    {
        textManager.containerADV.renderer.position = new Vector3(0f, -3.947f, 0f);
        textManager.containerADV.renderer.size = new Vector2(14.93f, 1.774f);
        var renderer = textManager.gameTextADV.Entity.GetComponent<Renderer>();
        renderer.position = new Vector3(0f, 0f, 0f);
        textManager.gameTextADV.UpdateSize(14.93f, 1.774f);
        textManager.gameTextADV.FontName = "BloggerSans";
        textManager.gameTextADV.Refresh();
    }

}