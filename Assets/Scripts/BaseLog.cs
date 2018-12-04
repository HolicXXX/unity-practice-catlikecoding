using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CunstomUtils
{
    public class BaseLog : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            StartCoroutine(WaitThen(4f, () =>
            {
                transform.localPosition = Vector3.zero;
                Debug.Log($"{transform.parent.position}, {transform.position}");
            }));
        }

        IEnumerator WaitThen(float seconds, System.Action callback)
        {
            WaitForSeconds wait = new WaitForSeconds(seconds);
            yield return wait;
            callback.Invoke();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
