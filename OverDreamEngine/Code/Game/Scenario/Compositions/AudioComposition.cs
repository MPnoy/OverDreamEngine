using System;

namespace ODEngine.Game
{
    public class AudioComposition : Composition
    {
        public readonly string filename;
        public readonly TimeSpan[] loopSplitters = null;
        public readonly float fadeTime = 0f;

        public AudioComposition(string name, string filename) : base(name)
        {
            this.filename = filename;
        }

        public AudioComposition(string name, string filename, TimeSpan[] loopSplitters, float fadeTime) : base(name)
        {
            this.filename = filename;
            this.loopSplitters = loopSplitters;
            this.fadeTime = fadeTime;
        }

    }

}
