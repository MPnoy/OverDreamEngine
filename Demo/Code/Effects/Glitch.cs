//using System;
//using ODEngine.Core;
//using ODEngine.Game.Images;
//using OpenTK;

//namespace Effects
//{
//    public class Glitch : BaseEffect
//    {
//        public enum GlitchingMode
//        {
//            Interferences,
//            Tearing,
//            Complete
//        }

//        [Serializable]
//        public class InterferenceSettings
//        {
//            public float speed = 10f;
//            public float density = 8f;
//            public float maxDisplacement = 2f;
//        }

//        [Serializable]
//        public class TearingSettings
//        {
//            public float speed = 1f;

//            public float intensity = 0.25f;

//            public float maxDisplacement = 0.05f;

//            public bool allowFlipping = false;
//            public bool yuvColorBleeding = true;

//            public float yuvOffset = 0.5f;
//        }

//        public bool randomActivation = false;

//        public Vector2 randomEvery = new Vector2(1f, 2f);
//        public Vector2 randomDuration = new Vector2(1f, 2f);

//        public GlitchingMode mode = GlitchingMode.Interferences;

//        public InterferenceSettings settingsInterferences = new InterferenceSettings();
//        public TearingSettings settingsTearing = new TearingSettings();

//        protected bool activated = true;
//        protected float everyTimer = 0f;
//        protected float everyTimerEnd = 0f;
//        protected float durationTimer = 0f;
//        protected float durationTimerEnd = 0f;

//        public Glitch()
//        {
//            material = new Material("Custom/Glitch", "Atlas/Identity", "Custom/Glitch");
//            durationTimerEnd = MathHelper.Lerp(randomDuration.X, randomDuration.Y, (float)random.NextDouble());
//            PostInit();
//        }

//        protected override void Update()
//        {
//            if (!randomActivation)
//            {
//                return;
//            }

//            if (activated)
//            {
//                durationTimer += Kernel.deltaTimeUpdate;

//                if (durationTimer >= durationTimerEnd)
//                {
//                    durationTimer = 0f;
//                    activated = false;
//                    everyTimerEnd = MathHelper.Lerp(randomEvery.X, randomEvery.Y, (float)random.NextDouble());
//                }
//            }
//            else
//            {
//                everyTimer += Kernel.deltaTimeUpdate;

//                if (everyTimer >= everyTimerEnd)
//                {
//                    everyTimer = 0f;
//                    activated = true;
//                    durationTimerEnd = MathHelper.Lerp(randomDuration.X, randomDuration.Y, (float)random.NextDouble());
//                }
//            }
//        }

//        public override void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination)
//        {
//            if (!activated)
//            {
//                Graphics.Blit(source, destination);
//                return;
//            }

//            if (mode == GlitchingMode.Interferences)
//            {
//                DoInterferences(source, destination, settingsInterferences);
//            }
//            //else if (mode == GlitchingMode.Tearing)
//            //{
//            //    //DoTearing(source, destination, SettingsTearing);
//            //}
//            //else // Complete
//            //{
//            //    RenderTexture temp = RenderTexture.GetTemporary(source.Width, source.Height);
//            //    //DoTearing(source, temp, SettingsTearing);
//            //    DoInterferences(temp, destination, settingsInterferences);
//            //    RenderTexture.ReleaseTemporary(temp);
//            //}
//        }

//        protected virtual void DoInterferences(RenderAtlas.Texture source, RenderAtlas.Texture destination, InterferenceSettings settings)
//        {
//            material.SetVector4("_Params", new Vector4(settings.speed, settings.density, settings.maxDisplacement, 0f));
//            Graphics.Blit(source, destination, material);
//        }

//        //protected virtual void DoTearing(RenderTexture source, RenderTexture destination, TearingSettings settings)
//        //{
//        //    material.SetVector4("_Params", new Vector4(settings.Speed, settings.Intensity, settings.MaxDisplacement, settings.YuvOffset));

//        //    int pass = 1;
//        //    if (settings.AllowFlipping && settings.YuvColorBleeding) pass = 4;
//        //    else if (settings.AllowFlipping) pass = 2;
//        //    else if (settings.YuvColorBleeding) pass = 3;

//        //    Graphics.Blit(source, destination, material, pass);
//        //}
//    }

//}
