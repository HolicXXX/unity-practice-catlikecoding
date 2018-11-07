using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConstructionFractal
{
    public class Fractal : MonoBehaviour {

        [SerializeField]
        private Mesh[] meshes;

        [SerializeField]
        private Material material;

        [SerializeField]
        private int maxDepth = 0;

        private int depth = 0;

        [SerializeField]
        private float childScale = 1f;

        [Range(0f, 1f)]
        [SerializeField]
        private float spawnProbability = .5f;

        [SerializeField]
        private float maxRotationSpeed = 0f;

        private float rotationSpeed = 0f;

        [SerializeField]
        private float maxTwist = 0f;

        private static readonly Vector3[] childDiretions = {
            Vector3.up,
            Vector3.right,
            Vector3.left,
            Vector3.forward,
            Vector3.back
        };


        private static readonly Quaternion[] childOrientations = {
            Quaternion.identity,
            Quaternion.Euler(0f, 0f, -90f),
            Quaternion.Euler(0f, 0f, 90f),
            Quaternion.Euler(90f, 0f, 0f),
            Quaternion.Euler(-90f, 0f, 0f)
        };

        private Material[,] materials = null;

        // Use this for initialization
        void Start () {
            if(materials == null)
            {
                InitialMaterials();
            }
            rotationSpeed = Random.Range(-maxRotationSpeed, maxRotationSpeed);
            transform.Rotate(Random.Range(-maxTwist, maxTwist), 0f, 0f);
            gameObject.AddComponent<MeshFilter>().mesh = meshes[Random.Range(0, meshes.Length)];
            gameObject.AddComponent<MeshRenderer>().material = materials[depth, Random.Range(0, 2)];
            if(depth < maxDepth)
            {
                StartCoroutine(createChildren());
            }
	    }

        private void InitialMaterials()
        {
            materials = new Material[maxDepth + 1, 2];
            for(int i = 0; i <= maxDepth; ++i)
            {
                float t = i / (maxDepth - 1f);
                t *= t;
                materials[i, 0] = new Material(material);
                materials[i, 0].color = Color.Lerp(Color.white, Color.yellow, t);
                materials[i, 1] = new Material(material);
                materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, t);
            }
            materials[maxDepth, 0].color = Color.magenta;
            materials[maxDepth, 1].color = Color.red;
        }

        private IEnumerator createChildren()
        {
            for(int i = 0; i < childDiretions.Length; ++i)
            {
                if(Random.value < spawnProbability)
                {
                    yield return new WaitForSeconds(Random.Range(.1f, .5f));
                    new GameObject($"Fractal Children {depth}").AddComponent<Fractal>().Initialize(this, i);
                }
            }
        }

        private void Initialize(Fractal parent, int childIndex)
        {
            meshes = parent.meshes;
            materials = parent.materials;
            maxDepth = parent.maxDepth;
            depth = parent.depth + 1;
            childScale = parent.childScale;
            spawnProbability = parent.spawnProbability;
            maxRotationSpeed = parent.maxRotationSpeed;
            maxTwist = parent.maxTwist;
            var tran = transform;
            tran.parent = parent.transform;
            tran.localScale = Vector3.one * childScale;
            var direction = childDiretions[childIndex];
            var orientation = childOrientations[childIndex];
            tran.localPosition = direction * (0.5f + 0.5f * childScale);
            tran.localRotation = orientation;
        }

	    // Update is called once per frame
	    void Update () {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
	    }
    }
}
