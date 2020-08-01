using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/Transformer")]
        public class Transformer : MonoBehaviour
        {
            public Vector3 targetPosition;
            public AnimationCurve moveCurve;
            public float time;
            public bool sendMovementEvent = false;
            public FloatEvent movementOutput;

            [ContextMenu("MoveToInTime")]
            public void MoveToInTime()
            {
                MoveToInTime(time);
            }
            public void MoveToInTime(float time)
            {
                StopAllCoroutines();
                StartCoroutine(moveToInTime(targetPosition, time));
            }
            public void MoveToInTime(Vector3 targetPosition)
            {
                StopAllCoroutines();
                StartCoroutine(moveToInTime(targetPosition, time));
            }
            public void MoveToInTime(Vector3 targetPosition, float time)
            {
                StopAllCoroutines();
                StartCoroutine(moveToInTime(targetPosition, time));
            }
            private IEnumerator moveToInTime(Vector3 targetPosition, float time)
            {
                float timer = 0;
                Vector3 oPos = transform.position;
                while (timer < 1)
                {
                    yield return 0;
                    transform.position = Vector3.LerpUnclamped(oPos, targetPosition, moveCurve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                transform.position = targetPosition;
            }

            [ContextMenu("MoveToInTimeLocally")]
            public void MoveToInTimeLocally()
            {
                MoveToInTimeLocally(time);
            }
            public void MoveToInTimeLocally(float time)
            {
                StopAllCoroutines();
                StartCoroutine(moveToInTimeLocally(targetPosition, time));
            }
            public void MoveToInTimeLocally(Vector3 targetPosition)
            {
                StopAllCoroutines();
                StartCoroutine(moveToInTimeLocally(targetPosition, time));
            }
            public void MoveToInTimeLocally(Vector3 targetPosition, float time)
            {
                StopAllCoroutines();
                StartCoroutine(moveToInTimeLocally(targetPosition, time));
            }
            private IEnumerator moveToInTimeLocally(Vector3 targetPosition, float time)
            {
                float timer = 0;
                Vector3 oPos = transform.localPosition;
                while (timer < 1)
                {
                    yield return 0;
                    transform.localPosition = Vector3.LerpUnclamped(oPos, targetPosition, moveCurve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                transform.localPosition = targetPosition;
            }

            [ContextMenu("MoveToInTimeRelativeLocally")]
            public void MoveToInTimeRelativeLocally()
            {
                MoveToInTimeRelativeLocally(time);
            }
            public void MoveToInTimeRelativeLocally(float time)
            {
                StopAllCoroutines();
                if (sendMovementEvent) StartCoroutine(moveToInTimeRelativeLocallyAndOutput(targetPosition, time));
                else
                    StartCoroutine(moveToInTimeRelativeLocally(targetPosition, time));
            }
            public void MoveToInTimeRelativeLocally(Vector3 targetPosition)
            {
                StopAllCoroutines();
                if (sendMovementEvent) StartCoroutine(moveToInTimeRelativeLocallyAndOutput(targetPosition, time));
                else
                    StartCoroutine(moveToInTimeRelativeLocally(targetPosition, time));
            }
            public void MoveToInTimeRelativeLocally(Vector3 targetPosition, float time)
            {
                StopAllCoroutines();
                if (sendMovementEvent) StartCoroutine(moveToInTimeRelativeLocallyAndOutput(targetPosition, time));
                else
                    StartCoroutine(moveToInTimeRelativeLocally(targetPosition, time));
            }
            private IEnumerator moveToInTimeRelativeLocally(Vector3 targetPosition, float time)
            {
                float timer = 0;
                Vector3 oPos = transform.localPosition;
                Vector3 tPosition = oPos + targetPosition;
                while (timer < 1)
                {
                    yield return 0;
                    transform.localPosition = Vector3.LerpUnclamped(oPos, tPosition, moveCurve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                transform.localPosition = tPosition;
            }
            private IEnumerator moveToInTimeRelativeLocallyAndOutput(Vector3 targetPosition, float time)
            {
                float timer = 0;
                Vector3 oPos = transform.localPosition;
                Vector3 tPosition = oPos + targetPosition;
                float distance = Vector3.Distance(oPos, tPosition);
                float currentD = 0;
                while (timer < 1)
                {
                    yield return 0;
                    float t = moveCurve.Evaluate(timer) * distance;
                    movementOutput?.Invoke(t - currentD);
                    currentD = t;
                    transform.localPosition = Vector3.LerpUnclamped(oPos, tPosition, moveCurve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                movementOutput?.Invoke(distance - currentD);
                transform.localPosition = tPosition;
            }


            private void OnDrawGizmos()
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(transform.localToWorldMatrix * targetPosition));
                Gizmos.color = new Color(0.7f, 0.5f, 0.5f);
                Gizmos.DrawWireCube(transform.position + (Vector3)(transform.localToWorldMatrix * targetPosition), Vector3.one * 0.1f);
                Gizmos.color = Color.white;
            }
            private void OnDrawGizmosSelected()
            {
                Gizmos.color = new Color(0.65f, 0.5f, 0.5f);
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(transform.localToWorldMatrix * targetPosition));
                Gizmos.color = new Color(0.9f, 0.5f, 0.5f);
                Gizmos.DrawWireCube(transform.position + (Vector3)(transform.localToWorldMatrix * targetPosition), Vector3.one * 0.1f);
                Gizmos.color = Color.white;
            }
        }
    }
}
