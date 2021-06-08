using System;
using System.Collections.Generic;
using ODEngine.EC.Components;
using ODEngine.Helpers;

namespace ODEngine.TextAnimations
{
    public class TextAnimation : IDisposable, IPoolable
    {
        public enum AnimEventType
        {
            None,
            Show,
            Replace,
            Hide
        }

        public struct TestStartValues
        {
            public float scale;
            public float x;
            public float y;

            public TestStartValues(bool _)
            {
                scale = 1f;
                x = 0f;
                y = 0f;
            }
        }

        public struct TestValues
        {
            public bool minScaleChanged;
            public bool minXChanged;
            public bool minYChanged;
            public bool maxScaleChanged;
            public bool maxXChanged;
            public bool maxYChanged;

            public float minScale;
            public float minX;
            public float minY;
            public float maxScale;
            public float maxX;
            public float maxY;

            public TestValues(bool _)
            {
                minScaleChanged = false;
                minXChanged = false;
                minYChanged = false;
                maxScaleChanged = false;
                maxXChanged = false;
                maxYChanged = false;

                minScale = 1f;
                minX = 0f;
                minY = 0f;
                maxScale = 1f;
                maxX = 0f;
                maxY = 0f;
            }
        }

        public TextAnimationController controller;
        public GameImage gameImage;
        public ConcreteAnimation concreteAnim = null;
        public bool isInited = false;

        private readonly Dictionary<Var, PropAnim> propAnims = new Dictionary<Var, PropAnim>(64);
        private readonly List<(AtomicAnimation, Var)> atomicAnimations = new List<(AtomicAnimation, Var)>(64);

        private readonly Dictionary<Var, float> initValues = new Dictionary<Var, float>(64); //Значения в рантайме (для Show, Replace...)
        public readonly Dictionary<string, object> scenarioValues = new Dictionary<string, object>(64); //Значения из сценария

        // Храним текущие значения во время создания анимации, для правильного присвоения стартовых значений команд Anim
        private readonly Dictionary<Var, float> tmpValues = new Dictionary<Var, float>();

        private float? repeat = null; //Включён ли повтор и его время начала
        private float timeEnd = 0f; //Время, на которое установит команда ToEnd, это либо конец анимации, либо время начала повтора
        private float timeRealEnd = 0f; //Время конца анимации
        private float timeAdd = 0f; //Время начала новой добавленной операции
        private float time = 0f; //Текущее время
        private DateTime lastDateTime = DateTime.Now;
        public AnimEventType animEventType = AnimEventType.None;

        public TextAnimation() { }

        public void Init(TextAnimationController controller, GameImage gameImage, ConcreteAnimation concreteAnim)
        {
            this.controller = controller;
            this.gameImage = gameImage;
            this.concreteAnim = concreteAnim;
            isInited = true;
        }

        public void ChangeGameImage(GameImage gameImage)
        {
            this.gameImage = gameImage;

            foreach (var item in propAnims)
            {
                item.Value.gameImage = gameImage;
            }
        }

        public void ResetObject()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isInited)
            {
                controller = null;
                gameImage = null;
                concreteAnim = null;
                foreach (var item in propAnims)
                {
                    TextAnimationController.poolPropAnim.Return(item.Value);
                }
                propAnims.Clear();
                atomicAnimations.ForEach((item) => TextAnimationController.poolAtomicAnimation.Return(item.Item1));
                atomicAnimations.Clear();
                initValues.Clear();
                scenarioValues.Clear();
                repeat = null;
                timeEnd = 0f;
                timeRealEnd = 0f;
                timeAdd = 0f;
                time = 0f;
                lastDateTime = DateTime.Now;
                animEventType = AnimEventType.None;
                tmpValues.Clear();
                OnShow = null;
                OnReplace = null;
                OnHide = null;
                isInited = false;
            }
        }

        public event Action OnShow;
        public event Action OnReplace;
        public event Action OnHide;

        public void Show()
        {
            Reset();
            animEventType = AnimEventType.Show;
            OnShow?.Invoke();
            AfterBody();
        }

        public void Replace()
        {
            Reset();
            animEventType = AnimEventType.Replace;
            OnReplace?.Invoke();
            AfterBody();
        }

        public void Hide()
        {
            Reset();
            animEventType = AnimEventType.Hide;
            OnHide?.Invoke();
            AfterBody();
        }

        public void ApplyInitValues()
        {
            foreach (var item in initValues)
            {
                GetPropAnim(item.Key).SetValue(item.Value);
            }
        }

        public void ApplyScenarioValues()
        {
            foreach (var item in scenarioValues)
            {
                concreteAnim.GetType().GetProperty(item.Key,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance
                    ).SetValue(concreteAnim, item.Value);
            }
        }

        public void SaveInitValues()
        {
            for (int i = 0; i < GameImage.animPropArray.Length; i++)
            {
                initValues[GameImage.animPropArray[i]] = GetPropAnim(GameImage.animPropArray[i]).GetValue();
            }
        }

        public void ToEnd()
        {
            time = Math.Max(time, timeEnd); //Если повтор включён, это не будет его сбивать и отматывать на начало
            Tick();
        }

        private void Reset()
        {
            atomicAnimations.ForEach((item) => TextAnimationController.poolAtomicAnimation.Return(item.Item1));
            atomicAnimations.Clear();
            repeat = null;
            timeEnd = 0f;
            timeRealEnd = 0f;
            timeAdd = 0f;
            time = 0f;
            lastDateTime = DateTime.Now;
            for (int i = 0; i < GameImage.animPropArray.Length; i++)
            {
                tmpValues[GameImage.animPropArray[i]] = GetPropAnim(GameImage.animPropArray[i]).GetValue();
            }
        }

        private void AfterBody()
        {
            if (repeat == null)
            {
                timeEnd = timeRealEnd;
            }
        }

        public void Tick()
        {
            time += (float)(DateTime.Now - lastDateTime).TotalSeconds;
            lastDateTime = DateTime.Now;
            if (time > timeRealEnd)
            {
                if (repeat == null)
                {
                    time = timeRealEnd;
                }
                else
                {
                    time -= (timeRealEnd - repeat.Value);
                }
            }
            ChangeProps();
        }

        private void ChangeProps()
        {
            foreach (var item in atomicAnimations)
            {
                var (HasValue, Value) = item.Item1.GetValue(time);
                if (HasValue)
                {
                    GetPropAnim(item.Item2).SetValue(Value);
                }
            }
        }

        private PropAnim GetPropAnim(Var varName)
        {
            if (!propAnims.TryGetValue(varName, out PropAnim ret))
            {
                ret = TextAnimationController.poolPropAnim.Get();
                ret.Init(gameImage, varName);
                propAnims[varName] = ret;
            }
            return ret;
        }

        public void SetValue(Var varName, float value)
        {
            Anim(varName, value, value, 0f, InterpolationType.Off);
        }

        public float GetStartValue((float, Var) varName)
        {
            return GetPropAnim(varName.Item2).GetValue();
        }

        public float GetStartValue(Var varName)
        {
            return GetPropAnim(varName).GetValue();
        }

        public enum Var : int
        {
            Scale,
            PosX,
            PosY,
            PosZ,
            Alpha,
            RectangleX,
            RectangleY,
            RectangleWidth,
            RectangleHeight,
            CompositionSpeed,
            CompositionVar0,
            CompositionVar1,
            CompositionVar2,
            CompositionVar3,
            CompositionVar4,
            CompositionVar5,
            CompositionVar6,
            CompositionVar7
        }

        public void Anim(Var varType, float end, float time, InterpolationType interpolation)
        {
            float start = tmpValues[varType];
            Anim(varType, start, end, time, interpolation);
        }

        public void Anim(Var varType, float start, float end, float time, InterpolationType interpolation)
        {
            tmpValues[varType] = end;
            var atomicAnimation = TextAnimationController.poolAtomicAnimation.Get();
            atomicAnimation.Init(timeAdd, time, start, end, interpolation);
            atomicAnimations.Add((atomicAnimation, varType));
            timeRealEnd = Math.Max(timeRealEnd, timeAdd + time);
        }

        public void StartRepeat()
        {
            WaitForAll();
            timeEnd = timeRealEnd;
            repeat = timeRealEnd;
        }

        public void WaitForAll()
        {
            timeAdd = timeRealEnd;
        }

        public void WaitForTime(float seconds)
        {
            timeAdd += seconds;
        }

        public override string ToString()
        {
            return "isInited = " + isInited.ToString() + ", concrete: " + (concreteAnim != null ? concreteAnim.ToString() : "null");
        }
    }
}