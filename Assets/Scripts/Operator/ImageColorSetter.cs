using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/ImageColorSetter")]
        public class ImageColorSetter : MonoBehaviour
        {
            public Image target;

            //Input
            public void Set(Color color)
            {
                target.color = color;
            }
        }
    }
}
