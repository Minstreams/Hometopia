using System.Collections;
using System.Collections.Generic;
using GameSystem.Setting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace GameSystem
{
    /// <summary>
    /// SuperInputSystem，顶层玩家操作封装
    /// 通过Operator作为Controller控制场景
    /// </summary>
    public class SuperInputSystem : SubSystem<SuperInputSystemSetting>
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod]
        private static void RuntimeInit()
        {
            //用于控制Action初始化
            TheMatrix.onGameAwake += OnGameAwake;
            TheMatrix.onGameStart += OnGameStart;
        }
        private static void OnGameAwake()
        {
            //在进入游戏第一个场景时调用
            EnterState(TestState());
        }
        private static void OnGameStart()
        {
            //在主场景游戏开始时和游戏重新开始时调用
        }

        //输入行为枚举--------------------------
        public enum InputActions
        {
            point,
            slide,
            drag
        }

        //输入行为委托--------------------------
        public static event System.Action<UnityEngine.Vector2> drag;


        //状态机----------------------------
        private static IEnumerator TestState()
        {
            while (true)
            {
                //
                if (Mouse.current.leftButton.isPressed)
                {
                    drag?.Invoke(Mouse.current.delta.ReadValue());
                }
                yield return 0;
            }
        }


        //API---------------------------------
        //public static void SomeFunction(){}
    }
}
