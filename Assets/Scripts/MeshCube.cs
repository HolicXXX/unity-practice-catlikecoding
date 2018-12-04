using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomMesh
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshCube : MonoBehaviour
    {
        public int xSize, ySize, zSize;

        Mesh mesh;

        private Vector3[] vertices;

        private void Awake()
        {
            Generate();
        }

        void Generate()
        {
            mesh = new Mesh
            {
                name = "Procedural Cube"
            };
            GetComponent<MeshFilter>().mesh = mesh;
            CreateVertices();
            CreateTriangles();
            //mesh.RecalculateNormals();
        }

        void CreateVertices()
        {
            int cornerVertices = 8;
            int edgeVertices = 4 * (xSize + ySize + zSize - 3);
            int faceVertices = ((xSize - 1) * (ySize - 1) + (ySize - 1) * (zSize - 1) + (xSize - 1) * (zSize - 1)) * 2;
            vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

            //To make center at local center
            Vector3 half = new Vector3(xSize / 2f, ySize / 2f, zSize / 2f);

            int x, y, z, v = 0;
            for (y = 0; y <= ySize; ++y)
            {
                for (x = 0; x <= xSize; ++x)
                {
                    vertices[v++] = new Vector3(x, y, 0) - half;
                }

                for (z = 1; z <= zSize; ++z)
                {
                    vertices[v++] = new Vector3(xSize, y, z) - half;
                }

                for (x = xSize - 1; x >= 0; --x)
                {
                    vertices[v++] = new Vector3(x, y, zSize) - half;
                }

                for (z = zSize - 1; z > 0; --z)
                {
                    vertices[v++] = new Vector3(0, y, z) - half;
                }
            }

            for (z = 1; z < zSize; ++z)
            {
                for (x = 1; x < xSize; ++x)
                {
                    vertices[v++] = new Vector3(x, ySize, z) - half;
                }
            }

            for (z = 1; z < zSize; ++z)
            {
                for (x = 1; x < xSize; ++x)
                {
                    vertices[v++] = new Vector3(x, 0, z) - half;
                }
            }

            mesh.vertices = vertices;
        }

        void CreateTriangles()
        {
            int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
            int[] triangles = new int[quads * 6];
            int ring = (xSize + zSize) * 2;

            int v = 0, startIndex = 0;
            for(int y = 0; y < ySize; ++y, ++v)
            {
                for (int q = 0; q < ring - 1; ++q, ++v)
                {
                    startIndex = SetQuad(triangles, startIndex, v, v + 1, v + ring, v + ring + 1);
                }
                startIndex = SetQuad(triangles, startIndex, v, v - ring + 1, v + ring, v + 1);
            }
            startIndex = CreateTopFace(triangles, startIndex, ring);
            startIndex = CreateBottomFace(triangles, startIndex, ring);
            mesh.triangles = triangles;
        }

        int CreateTopFace(int[] triangles, int t, int ring)
        {
            int v = ring * ySize;
            for(int x = 0; x < xSize - 1; ++x, ++v)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            int vMin = ring * (ySize + 1) - 1;
            int vMid = vMin + 1;
            int vMax = v + 2;

            for(int z = 1; z < zSize - 1; ++z, --vMin, ++vMid, ++vMax)
            {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
                for (int x = 1; x < xSize - 1; ++x, ++vMid)
                {
                    t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                }
                t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
            }

            int vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vTop);
            for(int x = 1; x < xSize - 1; ++x, ++vMid, --vTop)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            }
            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        int CreateBottomFace(int[] triangles, int t, int ring)
        {
            int v = 1;
            int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, v - 1, v);
            for (int x = 1; x < xSize - 1; ++x, ++v, ++vMid)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            }
            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            int vMin = ring - 2;
            vMid -= xSize - 2;
            int vMax = v + 2;

            for (int z = 1; z < zSize - 1; ++z, --vMin, ++vMid, ++vMax)
            {
                t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
                for (int x = 1; x < xSize - 1; ++x, ++vMid)
                {
                    t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
                }
                t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (int x = 1; x < xSize - 1; ++x, ++vMid, --vTop)
            {
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        static int SetQuad(int[] triangles, int startIndex, int v00, int v10, int v01, int v11)
        {
            triangles[startIndex] = v00;
            triangles[startIndex + 4] = triangles[startIndex + 1] = v01;
            triangles[startIndex + 3] = triangles[startIndex + 2] = v10;
            triangles[startIndex + 5] = v11;
            return startIndex + 6;
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
