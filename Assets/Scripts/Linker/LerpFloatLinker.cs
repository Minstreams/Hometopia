using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/LerpFloatLinker")]
        public class LerpFloatLinker : MonoBehaviour
        {
            public AnimationCurve remapCurve;
            public bool clamp = false;

            //Output
            public FloatEvent output;

            //Input
            public void Invoke(float input)
            {
                output?.Invoke(remapCurve.Evaluate(clamp ? Mathf.Clamp01(input) : input));
            }
        }
    }
}