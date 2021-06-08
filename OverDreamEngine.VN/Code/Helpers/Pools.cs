namespace ODEngine.Helpers.Pool
{
    public static class Pools
    {
        public static ObjectPool<TextAnimations.TextAnimation> textAnimations;

        public static void Init()
        {
            textAnimations = new ObjectPool<TextAnimations.TextAnimation>(() => new TextAnimations.TextAnimation(), 1024 * 16);
        }

        public static void Dispose()
        {
            textAnimations?.Dispose();
            textAnimations = null;
        }
    }
}
