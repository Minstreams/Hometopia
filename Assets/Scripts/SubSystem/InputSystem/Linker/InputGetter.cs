using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/InputSystem/InputGetter")]
        public class InputGetter : MonoBehaviour
        {
            public InputSystem.InputKey key;
            public bool anyKey;

            private void Update()
            {
                if (anyKey ? Input.anyKey : InputSystem.GetKey(key)) keyOutput?.Invoke();
                if (anyKey ? Input.anyKeyDown : InputSystem.GetKeyDown(key)) keyDownOutput?.Invoke();
                if (InputSystem.GetKeyUp(key)) keyUpOutput?.Invoke();
            }

            //Output
            public SimpleEvent keyOutput;
            public SimpleEvent keyDownOutput;
            public SimpleEvent keyUpOutput;
        }
    }
}
