using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.Setting;


namespace GameSystem
{
    /// <summary>
    /// 封装输入的系统
    /// </summary>
    public class InputSystem : SubSystem<InputSystemSetting>
    {
        //所有输入按键种类
        public enum InputKey
        {
            slide
        }

        //API---------------------------------
        public static bool GetKey(InputKey input)
        {
            return Input.GetKey(Setting.Keys[input]);
        }
        public static bool GetKeyDown(InputKey input)
        {
            return Input.GetKeyDown(Setting.Keys[input]);
        }
        public static bool GetKeyUp(InputKey input)
        {
            return Input.GetKeyUp(Setting.Keys[input]);
        }
    }
}