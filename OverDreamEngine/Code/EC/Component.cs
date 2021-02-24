namespace ODEngine.EC
{
    public abstract class Component
    {
        internal Entity entity = null;

        internal bool isInited = false;
        internal bool isAlive = true;
        public string name = null;

        public Entity Entity { get => entity; }
        public bool IsInited { get => isInited; }
        public bool IsAlive { get => isAlive; }

        internal Component() { }

        internal void Create(Entity entity)
        {
            this.entity = entity;
            OnCreate();
            isInited = true;
        }

        internal void Destroy()
        {
            isInited = false;
            isAlive = false;
            OnDestroy();
        }

        protected virtual void OnCreate() { }

        protected virtual void OnDestroy() { }

        internal virtual void HardUpdate() { }

        internal virtual void Update() { }

        internal virtual void LateUpdate() { }

        public override string ToString()
        {
            return name;
        }

    }
}
