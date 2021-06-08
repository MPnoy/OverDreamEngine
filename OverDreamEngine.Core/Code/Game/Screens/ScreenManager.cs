using System;
using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.Core.Audio;
using ODEngine.EC;
using ODEngine.EC.Components;
using OpenTK.Mathematics;

namespace ODEngine.Game.Screens
{
    public class ScreenManager : IUpdatable
    {
        public AudioChannel audioChannelUiSfx;
        public AudioChannel audioChannelUiMusic;

        public ConsoleScreen consoleScreen;
        public SettingsScreenPrototype settingsScreen;
        public Screen startScreen;
        public Screen miniMenu;
        public Screen exitMenu;

        public Dictionary<Guid, Screen> dictScreens = new Dictionary<Guid, Screen>();
        public Dictionary<Guid, Screen> dominantScreens = new Dictionary<Guid, Screen>();

        public GUIElement domScreensContainer;
        public GUIElement screensContainer;

        public virtual void Start()
        {
            audioChannelUiSfx = new AudioChannel(Kernel.audioCore, 8, "UiSfx");
            audioChannelUiMusic = new AudioChannel(Kernel.audioCore, 8, "UiMusic");

            Graphics.mainRenderer = new Entity().CreateComponent<Renderer>("Main Renderer");
            Graphics.mainRenderer.size = new Vector2(19.2f, 10.8f);
            Graphics.mainRenderer.scale = new Vector2(Graphics.cameraMultiplier);

            screensContainer = GUIElement.CreateContainer(Graphics.mainRenderer, Vector3.Zero, Graphics.mainRenderer.size, "Game/Alpha");
            domScreensContainer = GUIElement.CreateContainer(Graphics.mainRenderer, new Vector3(0, 0, -1), Graphics.mainRenderer.size, "Game/Alpha");
            domScreensContainer.isEnable = false;

            AddScreenToGame(typeof(FPSScreen), true);
            consoleScreen = (ConsoleScreen)AddScreenToGame(typeof(ConsoleScreen), true);
        }

        public virtual void GameStart(Type startScreenType, Type settingsScreenType, Type miniMenuType, Type exitMenuType)
        {
            settingsScreen = (SettingsScreenPrototype)AddScreenToGame(settingsScreenType);
            startScreen = AddScreenToGame(startScreenType);
            miniMenu = AddScreenToGame(miniMenuType);
            exitMenu = AddScreenToGame(exitMenuType);

            dictScreens[startScreenType.GUID].Enable();
            dictScreens[startScreenType.GUID].ChangeZ(-1);
        }

        public Screen AddScreenToGame<T>(bool isDominant = false)
        {
            return AddScreenToGame(typeof(T), isDominant);
        }

        public Screen AddScreenToGame(Type screenType, bool isDominant = false)
        {
            Screen tmpScr;

            if (!isDominant)
            {
                tmpScr = (Screen)screenType.GetConstructors()[0].Invoke(new object[] { this, screensContainer.renderer });
                dictScreens.Add(screenType.GUID, tmpScr);
            }
            else
            {
                tmpScr = (Screen)screenType.GetConstructors()[0].Invoke(new object[] { this, domScreensContainer.renderer });
                dominantScreens.Add(screenType.GUID, tmpScr);
                // Если доминантные скрины будут перекрывать друг друга, то надо делать разный z
                tmpScr.isDominant = true;
                tmpScr.ChangeZ(0);
            }

            tmpScr.name = screenType.Name;
            return tmpScr;
        }

        public void ShowScreen(Screen screen, Screen parent = null, bool disableScreens = true, bool deactivateScreens = false)
        {
            ShowScreen(screen.GetType().GUID, parent, disableScreens, deactivateScreens);
        }

        public void ShowScreen<T>(Screen parent = null, bool disableScreens = true, bool deactivateScreens = false)
        {
            ShowScreen(typeof(T).GUID, parent, disableScreens, deactivateScreens);
        }

        public void ShowScreen(Guid screenGUID, Screen parent = null, bool disableScreens = true, bool deactivateScreens = false)
        {
            var screen = GetScreen(screenGUID);

            if (!screen.IsEnable)
            {
                var enscr = GetEnabledScreens();
                screen.Enable();
                var parentRenderer = screen.screenContainer.renderer.Parent;
                parentRenderer.SortChilds();

                for (int i = parentRenderer.childs.Count - 1; i >= 0; i--)
                {
                    var brother = parentRenderer.childs[i];

                    if (brother == screen.screenContainer.renderer)
                    {
                        continue;
                    }

                    if (brother.isVisible)
                    {
                        screen.ChangeZ(brother.PositionZ - 1f);
                        break;
                    }
                }

                if (parent != null)
                {
                    parent.childs.Add(screen);
                }

                screen.prevScreens = enscr;

                if (disableScreens)
                {
                    DisableScreens(enscr);
                    screen.prevsDisabled = true;
                }

                if (deactivateScreens)
                {
                    DeactivateScreens(enscr);
                    screen.prevsDeactivated = true;
                }
            }
        }

        public void RemoveScreen<T>()
        {
            RemoveScreen(typeof(T).GUID);
        }

        public void RemoveScreen(Guid screenGUID)
        {
            if (dictScreens.ContainsKey(screenGUID))
            {
                dictScreens.Remove(screenGUID);
            }
            else if (dominantScreens.ContainsKey(screenGUID))
            {
                dominantScreens.Remove(screenGUID);
            }
        }

        public Screen GetScreen<T>()
        {
            return GetScreen(typeof(T).GUID);
        }

        public Screen GetScreen(Guid screenGUID)
        {
            if (dictScreens.ContainsKey(screenGUID))
            {
                return dictScreens[screenGUID];
            }
            else if (dominantScreens.ContainsKey(screenGUID))
            {
                return dominantScreens[screenGUID];
            }
            else
            {
                return null;
            }
        }

        public List<Screen> GetEnabledScreens()
        {
            var tmp = new List<Screen>();
            foreach (var i in dictScreens.Values)
            {
                if (i.IsEnable)
                {
                    tmp.Add(i);
                }
            }
            return tmp;
        }

        public void DisableAllScreens()
        {
            foreach (var i in dictScreens.Values)
            {
                i.Disable();
            }
        }

        public void DisableScreens(List<Screen> list)
        {
            foreach (var i in list)
            {
                i.Disable();
            }
        }

        public void ActivateScreens(List<Screen> list)
        {
            foreach (var i in list)
            {
                i.screenContainer.isEnable = true;
                i.screenContainer.childsProcessing = true;
            }
        }

        public void DeactivateScreens(List<Screen> list)
        {
            foreach (var i in list)
            {
                i.screenContainer.isEnable = false;
                i.screenContainer.childsProcessing = false;
            }
        }

        public void EnableScreens(List<Screen> list)
        {
            foreach (var i in list)
            {
                i.Enable();
            }
        }

        public void Print(string s)
        {
            ((ConsoleScreen)GetScreen(typeof(ConsoleScreen).GUID)).Print(s);
        }

        public void Update()
        {
            foreach (var i in dominantScreens.Values)
            {
                i.Update();
            }

            foreach (var i in dictScreens.Values)
            {
                i.Update();
            }
        }

    }
}
