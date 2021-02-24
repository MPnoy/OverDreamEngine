using System.Collections.Generic;

namespace ODEngine.Game
{
    public abstract class Composition
    {
        public string name;
        public int id;

        public static List<Composition> compositions = new List<Composition>();

        public Composition(string name)
        {
            this.name = name;
            id = compositions.Count;
            compositions.Add(this);
        }

        public override string ToString()
        {
            return "CompositionType: " + GetType().Name + "\nName: " + name;
        }

    }
}
