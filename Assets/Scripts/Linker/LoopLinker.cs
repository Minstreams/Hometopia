using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/LoopLinker")]
        public class LoopLinker : MonoBehaviour
        {
            private int index = 0;

            //Output
            public SimpleEvent[] outputs;

            //Input
            [ContextMenu("Invoke")]
            public void Invoke()
            {
                outputs[index++]?.Invoke();
                if (index > outputs.Length) index = 0;
            }
            public void ResetIndex()
            {
                index = 0;
            }

        }
    }
}