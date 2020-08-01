using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace GameSystem
{
    namespace Operator
    {
        /// <summary>
        /// simply keep rotating around an axis
        /// </summary>
        [AddComponentMenu("Operator/AutoRotater")]
        public class AutoRotater : MonoBehaviour
        {
            public Vector3 axis = Vector3.up;
            public float speed = 1;
            [Range(0, 1)]
            public float factor = 1;
            public Space relatedSpace;
            private void Update()
            {
                transform.Rotate(axis, speed * factor * Time.deltaTime, relatedSpace);
            }
            public void SetFactor(float factor)
            {
                this.factor = factor;
            }
        }
    }
}
