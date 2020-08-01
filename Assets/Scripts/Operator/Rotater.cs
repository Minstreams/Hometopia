using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/Rotater")]
        public class Rotater : MonoBehaviour
        {
            public Vector3 targetRotation;
            public AnimationCurve moveCurve;
            public float time;

            //Input
            [ContextMenu("RotateToLocally")]
            public void RotateToLocally()
            {
                transform.localRotation = Quaternion.Euler(targetRotation);
            }
            public void RotateToLocally(Vector3 targetRotation)
            {
                transform.localRotation = Quaternion.Euler(targetRotation);
            }

            [ContextMenu("RotateToInTime")]
            public void RotateToInTime()
            {
                RotateToInTime(time);
            }
            public void RotateToInTime(float time)
            {
                StopAllCoroutines();
                StartCoroutine(rotateToInTime(targetRotation, time));
            }
            public void RotateToInTime(Vector3 targetRotation)
            {
                StopAllCoroutines();
                StartCoroutine(rotateToInTime(targetRotation, time));
            }
            private IEnumerator rotateToInTime(Vector3 targetRotation, float time)
            {
                float timer = 0;
                Quaternion tRot = Quaternion.Euler(targetRotation);
                Quaternion oRot = transform.rotation;
                while (timer < 1)
                {
                    yield return 0;
                    transform.rotation = Quaternion.SlerpUnclamped(oRot, tRot, moveCurve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                transform.rotation = tRot;
            }

            [ContextMenu("RotateToInTimeLocally")]
            public void RotateToInTimeLocally()
            {
                RotateToInTimeLocally(time);
            }
            public void RotateToInTimeLocally(float time)
            {
                StopAllCoroutines();
                StartCoroutine(rotateToInTimeLocally(targetRotation, time));
            }
            public void RotateToInTimeLocally(Vector3 targetRotation)
            {
                StopAllCoroutines();
                StartCoroutine(rotateToInTimeLocally(targetRotation, time));
            }
            private IEnumerator rotateToInTimeLocally(Vector3 targetRotation, float time)
            {
                float timer = 0;
                Quaternion tRot = Quaternion.Euler(targetRotation);
                Quaternion oRot = transform.localRotation;
                while (timer < 1)
                {
                    yield return 0;
                    transform.localRotation = Quaternion.SlerpUnclamped(oRot, tRot, moveCurve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                transform.localRotation = tRot;
            }

            [ContextMenu("RotateToRelativeInTimeLocally")]
            public void RotateToRelativeInTimeLocally()
            {
                StopAllCoroutines();
                StartCoroutine(rotateToRelativeInTimeLocally(targetRotation, time));
            }
            public void RotateToRelativeInTimeLocally(float time)
            {
                StopAllCoroutines();
                StartCoroutine(rotateToRelativeInTimeLocally(targetRotation, time));
            }
            public void RotateToRelativeInTimeLocally(Vector3 targetRotation)
            {
                StopAllCoroutines();
                StartCoroutine(rotateToRelativeInTimeLocally(targetRotation, time));
            }
            private IEnumerator rotateToRelativeInTimeLocally(Vector3 targetRotation, float time)
            {
                float timer = 0;
                Quaternion tRot = transform.localRotation * Quaternion.Euler(targetRotation);
                Quaternion oRot = transform.localRotation;
                while (timer < 1)
                {
                    yield return 0;
                    transform.localRotation = Quaternion.SlerpUnclamped(oRot, tRot, moveCurve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                transform.localRotation = tRot;
            }
        }
    }
}