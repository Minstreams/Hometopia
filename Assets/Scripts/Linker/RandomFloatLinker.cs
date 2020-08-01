using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/RandomFloatLinker")]
        public class RandomFloatLinker : MonoBehaviour
        {
            public Vector2 range;

            //Output
            public FloatEvent output;

            //Input
            public void Invoke(float input)
            {
                output?.Invoke(Random.Range(range.x, range.y));
            }
            public void SetRange(Vector2 range)
            {
                this.range = range;
            }
        }
    }
}
