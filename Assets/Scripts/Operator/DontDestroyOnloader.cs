using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/DontDestroyOnloader")]
        public class DontDestroyOnloader : MonoBehaviour
        {
            private void Start()
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
