using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/TheMatrix/SavableObjectOperator")]
        public class SavableObjectOperator : MonoBehaviour
        {
            public Savable.SavableObject target;
            public bool saveOnDestroy;
            public bool loadOnStart;

            private void OnDestroy()
            {
                if (saveOnDestroy) Save();
            }
            private void Start()
            {
                if (loadOnStart) Load();
            }

            //Input
            [ContextMenu("Save")]
            public void Save()
            {
                TheMatrix.Save(target);
            }
            [ContextMenu("Load")]
            public void Load()
            {
                TheMatrix.Load(target);
            }
        }
    }
}
