namespace ODEngine.Game
{
    public struct ScenarioRequest
    {
        public enum Type
        {
            NextStep,
            PreviousStep
        }

        public Type type;
    }
}
