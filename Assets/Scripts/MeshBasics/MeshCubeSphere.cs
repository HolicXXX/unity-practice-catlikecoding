using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomMesh
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshCubeSphere : MonoBehaviour
    {
        public int gridSize;

        public float radius = 1f;

        Mesh mesh;

        private Vector3[] vertices;

        private Vector3[] normals;

        private Color32[] cubeUV;

        private void Awake()
        {
            Generate();
        }

        void Generate()
        {
            mesh = new Mesh
            {
                name = "Procedural Sphere"
            };
            GetComponent<MeshFilter>().mesh = mesh;
            CreateVertices();
            CreateTriangles();
            CreateColliders();
        }

        void CreateVertices()
        {
            int cornerVertices = 8;
            int edgeVertices = 4 * (gridSize + gridSize + gridSize - 3);
            int faceVertices = ((gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1)) * 2;
            vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            normals = new Vector3[vertices.Length];
            cubeUV = new Color32[vertices.Length];

            int x, y, z, v = 0;
            for (y = 0; y <= gridSize; ++y)
            {
                for (x = 0; x <= gridSize; ++x)
                {
                    SetVertex(v++, x, y, 0);
                }

                for (z = 1; z <= gridSize; ++z)
                {
                    SetVertex(v++, gridSize, y, z);
                }

                for (x = gridSize - 1; x >= 0; --x)
                {
                    SetVertex(v++, x, y, gridSize);
                }

                for (z = gridSize - 1; z > 0; --z)
                {
                    SetVertex(v++, 0, y, z);
                }
            }

            for (z = 1; z < gridSize; ++z)
            {
                for (x = 1; x < gridSize; ++x)
                {
                    SetVertex(v++, x, gridSize, z);
                }
            }

            for (z = 1; z < gridSize; ++z)
            {
                for (x = 1; x < gridSize; ++x)
                {
                    SetVertex(v++, x, 0, z);
                }
            }
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors32 = cubeUV;
        }

        void SetVertex(int i, int x, int y, int z)
        {
            Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
            float x2 = v.x * v.x;
            float y2 = v.y * v.y;
            float z2 = v.z * v.z;
            Vector3 s;
            s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
            normals[i] = s;
            vertices[i] = normals[i] * radius;
            cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
        }

        void CreateTriangles()
        {
            int quads = (gridSize * gridSize + gridSize * gridSize + gridSize * gridSize) * 2;
            int[] trianglesZ = new int[gridSize * gridSize * 12];
            int[] trianglesX = new int[gridSize * gridSize * 12];
            int[] trianglesY = new int[gridSize * gridSize * 12];
            int[] triangles = new int[quads * 6];
            int ring = (gridSize + gridSize) * 2;

            int tZ = 0, tX = 0, tY = 0, v = 0;
            for(int y = 0; y < gridSize; ++y, ++v)
            {
                //for (int q = 0; q < ring - 1; ++q, ++v)
                //{
                //    startIndex = SetQuad(triangles, startIndex, v, v + 1, v + ring, v + ring + 1);
                //}
                //startIndex = SetQuad(triangles, startIndex, v, v - ring + 1, v + ring, v + 1);
                for (int q = 0; q < gridSize; q++, v++)
                {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }
                for (int q = 0; q < gridSize; q++, v++)
                {
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                }
                for (int q = 0; q < gridSize; q++, v++)
                {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }
                for (int q = 0; q < gridSize - 1; q++, v++)
                {
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                }
                tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
            }
            tY = CreateTopFace(trianglesY, tY, ring);
            tY = CreateBottomFace(trianglesY, tY, ring);
            mesh.triangles = triangles;
            mesh.subMeshCount = 3;
            mesh.SetTriangles(trianglesZ, 0);
            mesh.SetTriangles(trianglesX, 1);
            mesh.SetTriangles(trianglesY, 2);
        }

        int CreateTopFace(int[] triangles, int t, int ring)
        {
            int v = ring * gridSize;
            for(int x = 0; x < gridSize - 1; ++x, ++v)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            int vMin = ring * (gridSize + 1) - 1;
            int vMid = vMin + 1;
            int vMax = v + 2;

            for(int z = 1; z < gridSize - 1; ++z, --vMin, ++vMid, ++vMax)
            {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
                for (int x = 1; x < gridSize - 1; ++x, ++vMid)
                {
                    t = SetQuad(triangles, t, vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
                }
                t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
            }

            int vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vTop);
            for(int x = 1; x < gridSize - 1; ++x, ++vMid, --vTop)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            }
            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        int CreateBottomFace(int[] triangles, int t, int ring)
        {
            int v = 1;
            int vMid = vertices.Length - (gridSize - 1) * (gridSize - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, v - 1, v);
            for (int x = 1; x < gridSize - 1; ++x, ++v, ++vMid)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            }
            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            int vMin = ring - 2;
            vMid -= gridSize - 2;
            int vMax = v + 2;

            for (int z = 1; z < gridSize - 1; ++z, --vMin, ++vMid, ++vMax)
            {
                t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
                for (int x = 1; x < gridSize - 1; ++x, ++vMid)
                {
                    t = SetQuad(triangles, t, vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
                }
                t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (int x = 1; x < gridSize - 1; ++x, ++vMid, --vTop)
            {
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        void CreateColliders()
        {
            gameObject.AddComponent<SphereCollider>();
        }

        static int SetQuad(int[] triangles, int t, int v00, int v10, int v01, int v11)
        {
            triangles[t] = v00;
            triangles[t + 4] = triangles[t + 1] = v01;
            triangles[t + 3] = triangles[t + 2] = v10;
            triangles[t + 5] = v11;
            return t + 6;
        }

        private void OnDrawGizmos()
        {
            if(vertices == null)
            {
                return;
            }
            //for(int i = 0;i < vertices.Length; ++i)
            //{
            //    Gizmos.color = Color.black;
            //    Gizmos.DrawSphere(vertices[i], .1f);
            //    Gizmos.color = Color.yellow;
            //    Gizmos.DrawRay(vertices[i], normals[i]);
            //}
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
