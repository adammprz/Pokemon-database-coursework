using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealProject
{
    public static class CoroutineManager
    {
        public class CoroutineInstance
        {
            public IEnumerator<object> enumerator;
            public float timer;

            public CoroutineInstance(IEnumerator<object> enume)
            {
                this.enumerator = enume;
                timer = 0f;
            }
        }

        static List<CoroutineInstance> coroutines = new List<CoroutineInstance>();
        static List<CoroutineInstance> toRemove = new List<CoroutineInstance>();

        public static CoroutineInstance Start(IEnumerator<object> coroutine)
        {
            var instance = new CoroutineInstance(coroutine);
            coroutines.Add(instance);
            return instance;
        }

        public static void Stop(CoroutineInstance coroutine)
        {
            toRemove.Add(coroutine);
        }

        public static void Update()
        {
            for (int i = coroutines.Count - 1; i >= 0; i--)
            {
                var instance = coroutines[i];

                // check if the current enumerator is yielding a nested coroutine
                if (instance.enumerator.Current is CoroutineInstance nested && coroutines.Contains(nested))
                {
                    // nested coroutine still running, skip everything else
                    continue;
                }

                // only now decrement timer and advance enumerator
                instance.timer -= Global.deltaTime;

                if (toRemove.Contains(instance))
                {
                    coroutines.RemoveAt(i);
                    toRemove.Remove(instance);
                    continue;
                }

                if (instance.timer <= 0f)
                {
                    if (!instance.enumerator.MoveNext())
                    {
                        toRemove.Add(instance);
                        continue;
                    }

                    object current = instance.enumerator.Current;

                    if (current is float waitTime)
                        instance.timer = waitTime;
                    else if (current == null)
                    {
                        bool doneIterating = false;
                        while (!doneIterating)
                        {
                            doneIterating = true;

                            if (!instance.enumerator.MoveNext())
                            {
                                toRemove.Add(instance);
                            }
                            else
                            {

                                object c = instance.enumerator.Current;

                                if (c == null)
                                {
                                    doneIterating = false;
                                }
                                else if (c is float w)
                                    instance.timer = w;
                                else
                                    instance.timer = 0f;
                            }
                        }
                    }
                    else if (current is CoroutineInstance)
                        instance.timer = 0f; // nested will be checked next frame
                    else
                        throw new Exception($"Unsupported yield type: {current.GetType()}");
                }
            }

        }
    }
}
