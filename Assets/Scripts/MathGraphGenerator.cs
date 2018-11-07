using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CustomMath
{
    public delegate Vector3 GraphMethod(float u, float v, float t);

    public enum MethodType
    {
        Sine,
        Sine2D,
        MultiSine,
        MultiSine2D,
        Ripple,
        Cylinder,
        Sphere,
        Torus
    }

    public class MathGraphGenerator : MonoBehaviour
    {

        [SerializeField]
        private GameObject pointPrefab = null;

        [Range(10, 100)]
        [SerializeField]
        private int resolution = 10;

        private float step = .2f;

        [SerializeField]
        private MethodType type = MethodType.Sine;

        private Transform[] points = null;

        private GraphMethod[] methods = null;

        private const float pi = Mathf.PI;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            methods = new GraphMethod[] { SineFunction, Sine2DFunction, MultiSineFunction, MultiSine2DFunction, Ripple, Cylinder, Sphere, Torus };
            step = 2f / resolution;
            var scale = Vector3.one * step;
            points = Enumerable.Range(0, resolution * resolution).Select((i, index) =>
            {
                var obj = Instantiate(pointPrefab, transform, false) as GameObject;
                obj.transform.localScale = scale;
                return obj.transform;
            }).ToArray();
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (points != null)
            {
                var method = methods[(int)type];
                float t = Time.time;
                step = 2f / resolution;
                int index = 0;
                foreach(var p in points)
                {
                    var u = (index % resolution + .5f) * step - 1f;
                    var v = (index / resolution + .5f) * step - 1f;
                    p.localPosition = method(u, v, t);
                    ++index;
                }
            }
        }

        Vector3 SineFunction(float u, float v, float t)
        {
            Vector3 pos = new Vector3(u, 0, v)
            {
                y = Mathf.Sin(pi * (u + t))
            };
            return pos;
        }

        Vector3 Sine2DFunction(float u, float v, float t)
        {
            Vector3 pos = new Vector3(u, 0, v);
            float y = Mathf.Sin(pi * (u + t));
            y += Mathf.Sin(pi * (v + t));
            y *= 0.5f;
            pos.y = y;
            return pos;
        }

        Vector3 MultiSineFunction(float u, float v, float t)
        {
            Vector3 pos = new Vector3(u, 0, v);
            float y = Mathf.Sin(pi * (u + t));
            y += Mathf.Sin(2f * pi * (u + 2f * t)) * .5f;
            y *= 2f / 3f;
            pos.y = y;
            return pos;
        }

        Vector3 MultiSine2DFunction(float u, float v, float t)
        {
            Vector3 pos = new Vector3(u, 0, v);
            float y = 4f * Mathf.Sin(pi * (u + v + t * 0.5f));
            y += Mathf.Sin(pi * (u + t));
            y += Mathf.Sin(2f * pi * (v + 2f * t)) * 0.5f;
            y *= 1f / 5.5f;
            pos.y = y;
            return pos;
        }

        Vector3 Ripple(float u, float v, float t)
        {
            Vector3 pos = new Vector3(u, 0, v);
            float d = Mathf.Sqrt(u * u + v * v);
            float y = Mathf.Sin(pi * (4f * d - t));
            y /= 1f + 10f * d;
            pos.y = y;
            return pos;
        }

        Vector3 Cylinder(float u, float v, float t)
        {
            Vector3 pos;
            float r = 0.8f + Mathf.Sin(pi * (6f * u + 2f * v + t)) * 0.2f;
            pos.x = r * Mathf.Sin(pi * u);
            pos.y = v;
            pos.z = r * Mathf.Cos(pi * u);
            return pos;
        }

        Vector3 Sphere(float u, float v, float t)
        {
            Vector3 pos;
            float r = 0.8f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
            r += Mathf.Sin(pi * (4f * v + t)) * 0.1f;
            float s = r * Mathf.Cos(pi * 0.5f * v);
            pos.x = s * Mathf.Sin(pi * u);
            pos.y = r * Mathf.Sin(pi * 0.5f * v);
            pos.z = s * Mathf.Cos(pi * u);
            return pos;
        }

        Vector3 Torus(float u, float v, float t)
        {
            Vector3 p;
            float r1 = 0.65f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
            float r2 = 0.2f + Mathf.Sin(pi * (4f * v + t)) * 0.05f;
            float s = r2 * Mathf.Cos(pi * v) + r1;
            p.x = s * Mathf.Sin(pi * u);
            p.y = r2 * Mathf.Sin(pi * v);
            p.z = s * Mathf.Cos(pi * u);
            return p;
        }
    }

}
