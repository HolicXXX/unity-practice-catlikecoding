using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject {

        [SerializeField]
        private Shape[] prefabs;

        [SerializeField]
        private Material[] materials;

        public Shape Get(int shapeId = 0, int materialId = 0)
        {
            var ret = Instantiate(prefabs[shapeId]);
            ret.ShapeId = shapeId;
            ret.SetMaterial(materials[materialId], materialId);
            return ret;
        }

        public Shape GetRandom()
        {
            return Get(Random.Range(0, prefabs.Length), Random.Range(0, materials.Length));
        }
    }
}
