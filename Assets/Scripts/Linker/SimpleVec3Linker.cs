using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/SimpleVec3Linker")]
        public class SimpleVec3Linker : MonoBehaviour
        {
            public Vector3 data;
            public bool invokeOnStart;

            private void Start()
            {
                if (invokeOnStart) Invoke();
            }

            //Output
            public Vec3Event output;

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
            public void SetZ(float z)
            {
                data.z = z;
            }
        }
    }
}
