using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

namespace GameSystem
{
    namespace Linker
    {
        /// <summary>
        /// 玩家输入器
        /// </summary>
        [AddComponentMenu("Linker/SuperInputSystem/SuperInputer")]
        public class SuperInputer : MonoBehaviour
        {
            [MinsHeader("玩家输入器", SummaryType.Title)]
            [Label("输入动作", true)]
            public SuperInputSystem.InputActions inputAction;

            private void Start()
            {
                switch (inputAction)
                {
                    case SuperInputSystem.InputActions.drag: SuperInputSystem.drag += vec2Input.Invoke; break;
                }
            }

            //Output
            [ConditionalShow4UEvent("inputAction",
                SuperInputSystem.InputActions.point,
                SuperInputSystem.InputActions.drag)]
            public Vec2Event vec2Input;
            [ConditionalShow4UEvent("inputAction", SuperInputSystem.InputActions.slide)]
            public FloatEvent floatInput;
        }
    }
}