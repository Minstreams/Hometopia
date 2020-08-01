using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/DebugPrinter")]
        public class DebugPrinter : MonoBehaviour
        {
            //Input
            public void Print(float val) { Debug.Log(val); }
            public void Print(string val) { Debug.Log(val); }
            public void Print(Vector2 val) { Debug.Log(val); }
            public void Print(Vector3 val) { Debug.Log(val); }
            public void Print(Color val) { Debug.Log(val); }
        }
    }
}