using System.Collections;

namespace ODEngine.EC.Components
{
    public abstract class GameObject : Component
    {
        public string objectName; //Имя объекта из сценария
        public bool isDeath = false;

        internal abstract void StopStep();

        protected void CoroutineStep(ref IEnumerator enumerator)
        {
            if (enumerator != null)
            {
                if (!enumerator.MoveNext())
                {
                    enumerator = null;
                }
            }
        }

    }
}