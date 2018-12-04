using UnityEngine;

namespace CustomMesh
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshGrid : MonoBehaviour
    {
        public int xSize, ySize;

        Vector3[] vertices;

        Mesh mesh;

        private void Awake()
        {
            Generate();
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void Generate()
        {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Procedural Mesh";
            vertices = new Vector3[(xSize + 1) * (ySize + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            Vector4[] tangents = new Vector4[vertices.Length];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
            for (int i = 0, y = 0; y <= ySize; ++y)
            {
                for (int x = 0; x <= xSize; ++x, ++i)
                {
                    vertices[i] = new Vector3(x, y);
                    uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                    tangents[i] = tangent;
                }
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.tangents = tangents;

            int[] triangles = new int[xSize * ySize * 6];
            for(int i = 0, y = 0; y < ySize; ++y)
            {
                for(int x = 0;x < xSize; ++x, i += 6)
                {
                    int diagonalLP = i + 1, diagonalRP = i + 2;
                    triangles[i] = x + y * (xSize + 1);
                    triangles[diagonalRP + 3] = triangles[diagonalLP] = triangles[i] + xSize + 1;
                    triangles[diagonalLP + 3] = triangles[diagonalRP] = triangles[i] + 1;
                    triangles[i + 3] = triangles[diagonalLP] + 1;
                }
            }
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }

        private void OnDrawGizmos()
        {
            if(vertices == null)
            {
                return;
            }
            Gizmos.color = Color.black;
            for(int i = 0;i < vertices.Length; ++i)
            {
                Gizmos.DrawSphere(vertices[i], .1f);
            }
        }
    }
}
