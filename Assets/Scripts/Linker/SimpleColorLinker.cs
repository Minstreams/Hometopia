using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/SimpleColorLinker")]
        public class SimpleColorLinker : MonoBehaviour
        {
            public Color data;
            public bool invokeOnStart;

            private void Start()
            {
                if (invokeOnStart) Invoke();
            }

            //Output
            public ColorEvent output;

            //Input
            [ContextMenu("Invoke")]
            public void Invoke()
            {
                output?.Invoke(data);
            }
            public void SetR(float r)
            {
                data.r = r;
            }
            public void SetG(float g)
            {
                data.g = g;
            }
            public void SetB(float b)
            {
                data.b = b;
            }
            public void SetA(float a)
            {
                data.a = a;
            }
        }
    }
}
