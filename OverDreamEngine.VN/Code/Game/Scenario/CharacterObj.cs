using System;

namespace ODEngine.Game
{
    [Serializable]
    public class CharacterObj
    {
        public string id, nameCharacter;
        public SColor color;
        public bool outline, shadow;

        public CharacterObj(string id, string nameCharacter, SColor color, bool outline, bool shadow)
        {
            this.id = id;
            this.nameCharacter = nameCharacter;
            this.color = color;
            this.outline = outline;
            this.shadow = shadow;
        }
    }
}