using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/LerpVec3Linker")]
        public class LerpVec3Linker : MonoBehaviour
        {
            public AnimationCurve remapXCurve;
            public AnimationCurve remapYCurve;
            public AnimationCurve remapZCurve;
            public bool clamp = false;

            //Output
            public Vec3Event output;

            //Input
            public void Invoke(float input)
            {
                float t = clamp ? Mathf.Clamp01(input) : input;
                output?.Invoke(new Vector3(remapXCurve.Evaluate(t), remapYCurve.Evaluate(t), remapZCurve.Evaluate(t)));
            }
        }
    }
}
