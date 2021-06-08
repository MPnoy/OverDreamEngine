using System;
using System.Runtime.CompilerServices;

namespace ODEngine.EC
{
    public abstract class Component
    {
        public Entity entity = null;

        public bool isInited = false;
        public bool isAlive = true;
        public string name = null;

        public Entity Entity { get => entity; }
        public bool IsInited { get => isInited; }
        public bool IsAlive { get => isAlive; }

        public Component() { }

        public void Create(Entity entity)
        {
            this.entity = entity;
            OnCreate();
            isInited = true;
        }

        public void Destroy()
        {
            isInited = false;
            isAlive = false;
            OnDestroy();
        }

        protected virtual void OnCreate() { }

        protected virtual void OnDestroy() { }

        public virtual void HardUpdate() { }

        public virtual void Update() { }

        public virtual void LateUpdate() { }

        public override string ToString()
        {
            return isAlive ? name : name + " (dead)";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetComponent<T>() where T : Component
        {
            return entity.GetComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Component GetComponent(Type type)
        {
            return entity.GetComponent(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateComponent<T>(string name = null) where T : Component, new()
        {
            return entity.CreateComponent<T>(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>() where T : Component
        {
            return entity.HasComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T1, T2>() where T1 : Component where T2 : Component
        {
            return entity.HasComponent<T1, T2>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T1, T2, T3>() where T1 : Component where T2 : Component where T3 : Component
        {
            return entity.HasComponent<T1, T2, T3>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T1, T2, T3, T4>() where T1 : Component where T2 : Component where T3 : Component where T4 : Component
        {
            return entity.HasComponent<T1, T2, T3, T4>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent(params Type[] componentTypes)
        {
            return entity.HasComponent(componentTypes);
        }

    }
}
