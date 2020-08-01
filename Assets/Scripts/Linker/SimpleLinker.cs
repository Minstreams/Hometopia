using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/SimpleLinker")]
        public class SimpleLinker : MonoBehaviour
        {
            public bool invokeOnStart;

            private void Start()
            {
                if (invokeOnStart) Invoke();
            }

            //Output
            public SimpleEvent output;

            //Input
            [ContextMenu("Invoke")]
            public void Invoke()
            {
                output?.Invoke();
            }
        }
    }
}