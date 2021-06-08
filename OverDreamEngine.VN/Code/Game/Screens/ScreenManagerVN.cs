using System;

namespace ODEngine.Game.Screens
{
    public class ScreenManagerVN : ScreenManager
    {
        public ScenarioScreen scenarioScreen;
        public TitleScreenPrototype titleScreen;

        public override void Start()
        {
            base.Start();
            scenarioScreen = (ScenarioScreen)AddScreenToGame(typeof(ScenarioScreen));
            AddScreenToGame(typeof(DevelopmentMenu), true);
        }

        public void GameStart(Type startScreenType, Type settingsScreenType, Type titleScreenType, Type miniMenuType, Type exitMenuType)
        {
            titleScreen = (TitleScreenPrototype)AddScreenToGame(titleScreenType);
            base.GameStart(startScreenType, settingsScreenType, miniMenuType, exitMenuType);
            scenarioScreen.GameStart();
        }

    }
}
