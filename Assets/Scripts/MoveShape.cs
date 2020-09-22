using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveShape : MonoBehaviour
{
    private Vector3 defaultPosition;

    [Space(10)]
    [Header("Circle Settings")]
    public float circleDuration;
    public float circleRadius;
    public AnimationCurve circleZ;
    public AnimationCurve circleY;

    [Space(10)]
    [Header("Eight Settings")]
    public float eightDuration;
    public float eightRadius;
    public AnimationCurve eightZ;
    public AnimationCurve eightY;

    private void Start ()
    {
        defaultPosition = transform.position;
    }

    private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) StartCoroutine(MoveToDefaultPosition(0.5f));
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(MoveInShapeOnce(circleDuration, circleRadius, circleY, circleZ));
        if (Input.GetKeyDown(KeyCode.Alpha3)) StartCoroutine(MoveInShapeOnce(eightDuration, eightRadius, eightY, eightZ));
    }

    public IEnumerator MoveToDefaultPosition(float duration)
    {
        Vector3 startPosition = transform.position;
        float timePassed = 0.0f;
        while(timePassed < duration)
        {
            timePassed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, defaultPosition, timePassed / duration);
            yield return null;
        }
    }

    //based on Lissajous curve
    public IEnumerator MoveInShapeOnce(float duration, float shapeRadius, AnimationCurve curveY, AnimationCurve curveZ)
    {
        float timePassed = 0.0f;
        float scale = 1.0f / duration;
        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            transform.position = defaultPosition + new Vector3(0.0f, curveY.Evaluate(timePassed * scale), curveZ.Evaluate(timePassed * scale)) * shapeRadius;
            yield return null;
        }
    }
}
