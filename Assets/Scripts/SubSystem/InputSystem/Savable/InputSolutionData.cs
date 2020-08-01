using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace Savable
    {
        [CreateAssetMenu(fileName = "InputSolutionData", menuName = "Savable/InputSolutionData")]
        public class InputSolutionData : SavableObject
        {
            [Header("所有输入按键种类")]
            public InputKeyMap Keys;

            public override void ApplyData()
            {
                InputSystem.Setting.Keys = Keys;
            }

            public override void UpdateData()
            {
                Keys = InputSystem.Setting.Keys;
            }
        }

    }
}
