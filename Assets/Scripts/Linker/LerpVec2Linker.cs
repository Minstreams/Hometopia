using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/LerpVec2Linker")]
        public class LerpVec2Linker : MonoBehaviour
        {
            public AnimationCurve remapXCurve;
            public AnimationCurve remapYCurve;
            public bool clamp = false;

            //Output
            public Vec2Event output;

            //Input
            public void Invoke(float input)
            {
                float t = clamp ? Mathf.Clamp01(input) : input;
                output?.Invoke(new Vector2(remapXCurve.Evaluate(t), remapYCurve.Evaluate(t)));
            }
        }
    }
}