/**
 * Solution adapted from https://answers.unity.com/questions/583316/fluent-animation-from-orthographic-to-perspective.html
 */

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera))]
public class CamProjectionTransition : MonoBehaviour
{
    public float transitionLength = 1;

    private bool transitionInProgress = false;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void StartTransition(float t)
    {
        if (!transitionInProgress)
        {
            StartCoroutine(TransitionCoroutine(t));
        }
    }

    public void StartTransition()
    {
        StartTransition(transitionLength);
    }


    private IEnumerator TransitionCoroutine(float transitionLength)
    {
        transitionInProgress = true;
        bool currentlyOrtho = cam.orthographic;

        if (transitionLength > 0)
        {
            Matrix4x4 fromMatrix, toMatrix;
            float t = 0;
            float d = 0;

            // set start and destination matrix
            fromMatrix = cam.projectionMatrix;
            cam.orthographic = !currentlyOrtho;
            cam.ResetProjectionMatrix();
            toMatrix = cam.projectionMatrix;
            cam.orthographic = !currentlyOrtho;

            // transition itself
            while (t < 1.0f)
            {
                t += (Time.deltaTime / transitionLength);

                if (currentlyOrtho) d = t * t;
                else d = Mathf.Sqrt(t);

                cam.projectionMatrix = MatrixLerp(fromMatrix, toMatrix, d);

                yield return null;
            }
        }

        // change to final projection
        cam.orthographic = !currentlyOrtho;
        cam.ResetProjectionMatrix();

        transitionInProgress = false;
    }

    private Matrix4x4 MatrixLerp(Matrix4x4 a, Matrix4x4 b, float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        var newMatrix = new Matrix4x4();
        newMatrix.SetRow(0, Vector4.Lerp(a.GetRow(0), b.GetRow(0), t));
        newMatrix.SetRow(1, Vector4.Lerp(a.GetRow(1), b.GetRow(1), t));
        newMatrix.SetRow(2, Vector4.Lerp(a.GetRow(2), b.GetRow(2), t));
        newMatrix.SetRow(3, Vector4.Lerp(a.GetRow(3), b.GetRow(3), t));
        return newMatrix;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CamProjectionTransition))]
public class TileSetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CamProjectionTransition self = (CamProjectionTransition)target;

        DrawDefaultInspector();

        if (self.GetComponent<Camera>().orthographic)
        {
            if (GUILayout.Button("Orthographic -> Perspective"))
            {
                self.StartTransition();
            }
        }
        else
        {
            if (GUILayout.Button("Perspective -> Orthographic"))
            {
                self.StartTransition();
            }
        }
    }
}
#endif