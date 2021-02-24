using ODEngine.EC.Components;

namespace ODEngine.TextAnimations
{
    public class None : ConcreteAnimation // Пустая анимация
    {
        public None(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }
    }
}