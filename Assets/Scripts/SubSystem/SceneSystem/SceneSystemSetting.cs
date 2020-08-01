using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    namespace Setting
    {
        [CreateAssetMenu(fileName = "SceneSystemSetting", menuName = "系统配置文件/SceneSystemSetting")]
        public class SceneSystemSetting : ScriptableObject
        {
            [Header("加载过程场景")]
            public string loadingScene;
        }
    }
}
