using System;
using ODEngine.Helpers;

namespace ODEngine.Game.Images
{
    public struct ImageRequestData
    {
        [Serializable]
        public struct SerializableData
        {
            public int compositionID;
            public ColorMatrix colorMatrix;

            public ImageRequestData Deserialize()
            {
                return new ImageRequestData
                {
                    colorMatrix = colorMatrix,
                    composition = (ImageComposition)Composition.compositions[compositionID]
                };
            }
        }

        public ImageComposition composition;
        public ColorMatrix colorMatrix;

        public ImageRequestData(ImageComposition composition, ColorMatrix colorMatrix)
        {
            this.composition = composition ?? throw new Exception("Composition is null");
            this.colorMatrix = colorMatrix;
        }

        public static bool operator ==(ImageRequestData value1, ImageRequestData value2)
        {
            return value1.composition == value2.composition && value1.colorMatrix == value2.colorMatrix;
        }

        public static bool operator !=(ImageRequestData value1, ImageRequestData value2)
        {
            return !(value1 == value2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public SerializableData Serialize()
        {
            return new SerializableData
            {
                colorMatrix = this.colorMatrix,
                compositionID = this.composition.id
            };
        }

    }
}