using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class CubeZone : SpawnZone
    {
        [SerializeField]
        bool surfaceOnly;

        public override Vector3 SpawnPoint
        {
            get
            {
                Vector3 p;
                p.x = Random.Range(-.5f, .5f);
                p.y = Random.Range(-.5f, .5f);
                p.z = Random.Range(-.5f, .5f);
                if (surfaceOnly)
                {
                    int axis = Random.Range(0, 3);
                    p[axis] = p[axis] < 0f ? -.5f : .5f;
                }
                return transform.TransformPoint(p);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
