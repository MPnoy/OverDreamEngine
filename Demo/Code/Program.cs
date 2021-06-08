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
    static void Main(string[] args)
    {
        var icon = ImageLoader.ImageToBytesStatic(Image.Load<Rgba32>("Data/Images/Misc/ICON_small.png"), false);
        var cursor = ImageLoader.ImageToBytesStatic(Image.Load<Rgba32>("Data/Images/GUI/Pulse_Glass.png"), false);

        GameKernel.Init(
            "Depth of Cold",
            new OpenTK.Windowing.Common.Input.Image(icon.width, icon.height, icon.ToByteArray()),
            new OpenTK.Windowing.Common.Input.MouseCursor(0, 0, cursor.width, cursor.height, cursor.ToByteArray()),
            args);

        Kernel.GameInit += () =>
        {
            GameKernel.screenManager.scenarioScreen.textManager.InterfaceCreating += TextManager_InterfaceCreating;
            GameKernel.screenManager.scenarioScreen.textManager.InterfaceDestroying += TextManager_InterfaceDestroying;
            GameKernel.screenManager.scenarioScreen.GameStarting += ScenarioScreen_GameStarting;
            GameKernel.screenManager.GameStart(typeof(MainMenu), typeof(SettingsScreen), typeof(TitleScreen), typeof(MiniMenu), typeof(ExitMenu));
            GameKernel.screenManager.AddScreenToGame(typeof(LoadMenu));
        };

        GameKernel.StartGame();
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
            textManager.guiRoot.SetParent(GameKernel.screenManager.scenarioScreen.screenRenderer);
            textManager.guiRoot.Position = new Vector3(0f, 0f, -6f);
        }

        { // ADV
            var size = new Vector2(19.2f, 3.5f);
            textManager.containerADV = GUIElement.CreateContainer(textManager.guiRoot, new Vector3(0f, -3.7f, -1f), size + new Vector2(6f, 6f), "Game/Alpha");
            textManager.containerADV.name = "containerADV";
            renderer = textManager.gameTextADV.Entity.GetComponent<Renderer>();
            renderer.SetParent(textManager.containerADV.renderer);
            //var background = GUIElement.CreateImage(textManager.containerADV.renderer, new Vector3(0f, 0f, 1f), new Vector2(19.2f, 3.5f), "GUI/newdialogue", 0.8f);
            var background = GUIElement.CreateImage(textManager.containerADV.renderer, new Vector3(0f, -0.45f, 1f), new Vector2(15.56f, 1.71f), "GUI/adv_box");
            background.renderer.name = "ADV Background";
            var clickArea = GUIElement.CreateEmpty(textManager.containerADV.renderer, new Vector3(0f, 0f, 0f), textManager.containerADV.renderer.size);
            clickArea.name = "clickAreaADV";
            clickArea.renderer.name = "clickAreaADV";
            clickArea.MouseDown += scenarioScreen.ClickArea_MouseDown;

            ADVNormalTransform(textManager);
        }

        { // NVL
            var size = new Vector2(14.93f, 9.5187f);
            textManager.containerNVL = GUIElement.CreateContainer(textManager.guiRoot, new Vector3(0f, -0.192f, -1f), size + new Vector2(4f, 4f), "Game/Alpha");
            textManager.containerNVL.name = "containerNVL";
            renderer = textManager.gameTextNVL.Entity.GetComponent<Renderer>();
            textManager.gameTextNVL.UpdateSize(size);
            textManager.gameTextNVL.FontName = "BloggerSans";
            textManager.gameTextNVL.Refresh();
            renderer.SetParent(textManager.containerNVL.renderer);
            var background = GUIElement.CreateImage(textManager.containerNVL.renderer, new Vector3(0f, 0f, 1f), size + new Vector2(0.5f, 0.5f), "GUI/NVL_New", 0.8f);
            background.renderer.name = "NVL Background";
            var clickArea = GUIElement.CreateEmpty(textManager.containerNVL.renderer, new Vector3(0f, 0f, 0f), textManager.containerNVL.renderer.size);
            clickArea.name = "clickAreaNVL";
            clickArea.renderer.name = "clickAreaNVL";

            textManager.nvlSetPosition += e => TextManager_NvlSetPosition(textManager, background, e);
            TextManager_NvlSetPosition(textManager, background, TextManager.NVLPosition.Center);

            clickArea.MouseDown += scenarioScreen.ClickArea_MouseDown;
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

    private static void TextManager_NvlSetPosition(TextManager textManager, GUIElement background, TextManager.NVLPosition position)
    {
        textManager.nvlPosition = position;
        Vector2 size = default;

        switch (position)
        {
            case TextManager.NVLPosition.Center:
                {
                    size = new Vector2(14.93f, 9.52f);
                    textManager.containerNVL.renderer.PositionX = 0f;
                    break;
                }
            case TextManager.NVLPosition.Left:
                {
                    size = new Vector2(9f, 9.52f);
                    textManager.containerNVL.renderer.PositionX = -3.2f;
                    break;
                }
            case TextManager.NVLPosition.Right:
                {
                    size = new Vector2(9f, 9.52f);
                    textManager.containerNVL.renderer.PositionX = 3.2f;
                    break;
                }
        }

        textManager.containerNVL.renderer.size = size + new Vector2(4f);
        textManager.gameTextNVL.UpdateSize(size);
        background.renderer.size = size + new Vector2(0.5f);
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
        textManager.containerADV.renderer.Position = new Vector3(0f, -3.7f, -1f);
        textManager.containerADV.renderer.size = new Vector2(19.2f, 3.5f);
        var renderer = textManager.gameTextADV.Entity.GetComponent<Renderer>();
        renderer.Position = new Vector3(0f, -0.454f, 0f);
        //textManager.gameTextADV.UpdateSize(17f, 1.55f);
        textManager.gameTextADV.UpdateSize(14.93f, 1.55f);
        textManager.gameTextADV.FontName = "BloggerSans";
        textManager.gameTextADV.Refresh();
    }

    public static void ADVLargeTransform(TextManager textManager)
    {
        textManager.containerADV.renderer.Position = new Vector3(0f, -3.947f, -1f);
        textManager.containerADV.renderer.size = new Vector2(14.93f, 1.774f);
        var renderer = textManager.gameTextADV.Entity.GetComponent<Renderer>();
        renderer.Position = new Vector3(0f, 0f, 0f);
        textManager.gameTextADV.UpdateSize(14.93f, 1.774f);
        textManager.gameTextADV.FontName = "BloggerSans";
        textManager.gameTextADV.Refresh();
    }

}
