using System;

namespace ODEngine.Game
{
    [Serializable]
    public class SpriteObj
    {
        public string objectName, spriteName, properties;

        public SpriteObj(string objectName, string spriteName, string properties)
        {
            this.objectName = objectName;
            this.spriteName = spriteName;
            this.properties = properties;
        }
    }
}