using ODEngine.EC.Components;
using ODEngine.Helpers;

namespace ODEngine.TextAnimations
{
    public class PropAnim : IPoolable
    {
        public GameImage gameImage;
        private TextAnimation.Var propName;

        public void Init(GameImage gameImage, TextAnimation.Var propName)
        {
            this.gameImage = gameImage;
            this.propName = propName;
        }

        public void ResetObject()
        {
            gameImage = null;
            propName = default;
        }

        public float GetValue()
        {
            if (gameImage == null)
            {
                return 0f;
            }

            return gameImage.GetAnimProp(propName);
        }

        public void SetValue(float value)
        {
            if (gameImage == null)
            {
                return;
            }

            gameImage.SetAnimProp(propName, value);
            return;
        }
    }
}