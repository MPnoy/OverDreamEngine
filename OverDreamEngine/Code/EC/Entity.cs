using System;
using System.Collections.Generic;

namespace ODEngine.EC
{
    public class Entity
    {
        public static List<Entity> entities = new List<Entity>();
        public Dictionary<Type, Component> components = new Dictionary<Type, Component>();

        public event Action OnDestroy;

        public Entity()
        {
            entities.Add(this);
        }

        public void Destroy()
        {
            foreach (var item in components)
            {
                item.Value.Destroy();
            }
            entities.Remove(this);

            OnDestroy?.Invoke();
        }

        public T GetComponent<T>() where T : Component
        {
            return components.TryGetValue(typeof(T), out var component) ? (T)component : null;
        }

        public Component GetComponent(Type type)
        {
            return components.TryGetValue(type, out var component) ? component : null;
        }

        public T CreateComponent<T>(string name = null) where T : Component, new()
        {
            if (components.TryGetValue(typeof(T), out var component))
            {
                if (name != null)
                {
                    component.name = name;
                }
                return (T)component;
            }
            else
            {
                var created = new T
                {
                    name = name
                };
                created.Create(this);
                components.Add(typeof(T), created);
                return created;
            }
        }

        public bool HasComponent<T>() where T : Component
        {
            return components.ContainsKey(typeof(T));
        }

        public bool HasComponent<T1, T2>() where T1 : Component where T2 : Component
        {
            return components.ContainsKey(typeof(T1)) && components.ContainsKey(typeof(T2));
        }

        public bool HasComponent<T1, T2, T3>() where T1 : Component where T2 : Component where T3 : Component
        {
            return components.ContainsKey(typeof(T1)) && components.ContainsKey(typeof(T2)) && components.ContainsKey(typeof(T3));
        }

        public bool HasComponent<T1, T2, T3, T4>() where T1 : Component where T2 : Component where T3 : Component where T4 : Component
        {
            return components.ContainsKey(typeof(T1)) && components.ContainsKey(typeof(T2)) && components.ContainsKey(typeof(T3)) && components.ContainsKey(typeof(T4));
        }

        public bool HasComponent(params Type[] componentTypes)
        {
            foreach (var type in componentTypes)
            {
                if (!components.ContainsKey(type))
                {
                    return false;
                }
            }

            return true;
        }

        internal void HardUpdate()
        {
            foreach (var component in components)
            {
                component.Value.HardUpdate();
            }
        }

        internal void Update()
        {
            foreach (var component in components)
            {
                component.Value.Update();
            }
        }

        internal void LateUpdate()
        {
            foreach (var component in components)
            {
                component.Value.LateUpdate();
            }
        }
    }
}