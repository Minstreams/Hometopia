using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/DelayLinker")]
        public class DelayLinker : MonoBehaviour
        {
            public float delay = 0.5f;

            //Output
            public SimpleEvent output;

            //Input
            [ContextMenu("Invoke")]
            public void Invoke()
            {
                Invoke(delay);
            }
            public void Invoke(float delay)
            {
                Invoke("DoInvoke", delay);
            }
            private void DoInvoke()
            {
                output?.Invoke();
            }
        }
    }
}
