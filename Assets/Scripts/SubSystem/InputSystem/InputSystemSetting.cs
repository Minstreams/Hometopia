using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace Setting
    {
        [CreateAssetMenu(fileName = "InputSystemSetting", menuName = "系统配置文件/InputSystemSetting")]
        public class InputSystemSetting : ScriptableObject
        {
            [Header("所有输入按键种类")]
            public InputKeyMap Keys;
        }
    }
}
