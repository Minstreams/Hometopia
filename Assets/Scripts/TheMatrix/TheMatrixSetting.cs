using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.Savable;

namespace GameSystem
{
    namespace Setting
    {
        [CreateAssetMenu(fileName = "TheMatrixSetting", menuName = "系统配置文件/TheMatrixSetting")]
        public class TheMatrixSetting : ScriptableObject
        {
            [Header("游戏场景表列")]
            public GameSceneMap gameSceneMap;
            [Header("所有要自动保存的数据")]
            public SavableObject[] dataAutoSave;
        }
    }
}
