using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Setting
    {
        [CreateAssetMenu(fileName = "CashSystemSetting", menuName = "系统配置文件/CashSystemSetting")]
        public class CashSystemSetting : ScriptableObject
        {
            //data definition here
            public string cashName = "$";
            public GUIStyle headerStyle;
            [ContextMenu("Test")]
            public void test()
            {
                headerStyle = cashName;
            }
        }
    }
}