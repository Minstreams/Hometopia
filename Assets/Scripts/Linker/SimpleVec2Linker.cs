using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/SimpleVec2Linker")]
        public class SimpleVec2Linker : MonoBehaviour
        {
            public Vector2 data;
            public bool invokeOnStart;

            private void Start()
            {
                if (invokeOnStart) Invoke();
            }

            //Output
            public Vec2Event output;

            //Input
            [ContextMenu("Invoke")]
            public void Invoke()
            {
                output?.Invoke(data);
            }
            public void SetX(float x)
            {
                data.x = x;
            }
            public void SetY(float y)
            {
                data.y = y;
            }
        }
    }
}