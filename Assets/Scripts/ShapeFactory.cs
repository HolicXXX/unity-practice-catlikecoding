using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectManagement
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject {

        [SerializeField]
        private Shape[] prefabs;

        [SerializeField]
        private Material[] materials;

        [SerializeField]
        private bool recycle;

        private List<Shape>[] pools;

        private Scene poolScene;

        void CreatePools()
        {
            pools = new List<Shape>[prefabs.Length];
            for(int i = 0; i < pools.Length; ++i)
            {
                pools[i] = new List<Shape>();
            }
            if (Application.isEditor)
            {
                poolScene = SceneManager.GetSceneByName(name);
                if (poolScene.isLoaded)
                {
                    var rootGameobjects = poolScene.GetRootGameObjects();
                    foreach(var obj in rootGameobjects)
                    {
                        Shape s = obj.GetComponent<Shape>();
                        if(!obj.activeSelf)
                        {
                            pools[s.ShapeId].Add(s);
                        }
                    }
                    return;
                }
            }
            poolScene = SceneManager.CreateScene(name);
        }

        public Shape Get(int shapeId = 0, int materialId = 0)
        {
            Shape ret = null;
            if (recycle)
            {
                if (pools == null)
                {
                    CreatePools();
                }
                var pool = pools[shapeId];
                int lastIndex = pool.Count - 1;
                if (lastIndex >= 0)
                {
                    ret = pool[lastIndex];
                    ret.gameObject.SetActive(true);
                    pool.RemoveAt(lastIndex);
                }
                else
                {
                    ret = Instantiate(prefabs[shapeId]);
                    ret.ShapeId = shapeId;
                    SceneManager.MoveGameObjectToScene(ret.gameObject, poolScene);
                }
            }
            else
            {
                ret = Instantiate(prefabs[shapeId]);
                ret.ShapeId = shapeId;
            }
            ret.SetMaterial(materials[materialId], materialId);
            return ret;
        }

        public Shape GetRandom()
        {
            return Get(Random.Range(0, prefabs.Length), Random.Range(0, materials.Length));
        }

        public void Reclaim(Shape shapeToRecycle)
        {
            if(recycle)
            {
                if(pools == null)
                {
                    CreatePools();
                }
                pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
                shapeToRecycle.gameObject.SetActive(false);
            }
            else
            {
                Destroy(shapeToRecycle.gameObject);
            }
        }
    }
}
