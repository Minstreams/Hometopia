using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/Bezier")]
        public class Bezier : MonoBehaviour
        {
            public Transform[] controlPoints;
            public LineRenderer lineRenderer;

            public int segmentCount = 50;

            private void Start()
            {
                lineRenderer.positionCount = controlPoints.Length / 3 * segmentCount;
            }
            void Update()
            {
                DrawCurve();
            }

            void DrawCurve()
            {
                for (int j = 0; j < controlPoints.Length / 3; j++)
                {
                    for (int i = 0; i < segmentCount; i++)
                    {
                        float t = i / (float)(segmentCount - 1);
                        int nodeIndex = j * 3;
                        Vector3 pixel = CalculateCubicBezierPoint(t, controlPoints[nodeIndex].position, controlPoints[nodeIndex + 1].position, controlPoints[nodeIndex + 2].position, controlPoints[nodeIndex + 3].position);
                        lineRenderer.SetPosition((j * segmentCount) + i, pixel);

                    }

                }
            }

            Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            {
                float u = 1 - t;
                float tt = t * t;
                float uu = u * u;
                float uuu = uu * u;
                float ttt = tt * t;

                Vector3 p = uuu * p0;
                p += 3 * uu * t * p1;
                p += 3 * u * tt * p2;
                p += ttt * p3;

                return p;
            }

            //Input
            [ContextMenu("DrawCurveManually")]
            public void DrawCurveManually()
            {
                lineRenderer.positionCount = controlPoints.Length / 3 * segmentCount;
                DrawCurve();
            }
        }
    }
}
