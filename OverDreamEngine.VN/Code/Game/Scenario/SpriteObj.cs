using System;
using System.Collections.Generic;

namespace ODEngine.Game
{
    [Serializable]
    public class SpriteObj
    {
        public string objectName, spriteName;
        public List<string> properties;

        public SpriteObj(string objectName, string spriteName, List<string> properties)
        {
            this.objectName = objectName;
            this.spriteName = spriteName;
            this.properties = properties;
        }

        public string GetProperties()
        {
            string ret = null;

            for (int i = 0; i < properties.Count; i++)
            {
                ret += "_" + properties[i];
            }

            return ret;
        }
    }
}