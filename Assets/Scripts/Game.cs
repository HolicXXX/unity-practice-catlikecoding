using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectManagement
{
    public class Game : PersistableObject {

        [SerializeField]
        private ShapeFactory shapeFactory;

        [SerializeField]
        private KeyCode createKey = KeyCode.C;

        [SerializeField]
        private KeyCode destroyKey = KeyCode.X;

        [SerializeField]
        private KeyCode newGameKey = KeyCode.N;

        [SerializeField]
        private KeyCode saveKey = KeyCode.S;

        [SerializeField]
        private KeyCode loadKey = KeyCode.L;

        [SerializeField]
        private PersistentStorage storage;

        [SerializeField]
        private int levelCount;

        public float CreationSpeed { get; set; }

        public float DestructionSpeed { get; set; }

        private readonly List<Shape> shapes = new List<Shape>();

        private const int saveVersion = 2;

        private float creationProgress;

        private float destructionProgress;

        private int loadedLevelBuildIndex;

        private void Awake()
        {

        }

        IEnumerator LoadLevel(int index)
        {
            enabled = false;
            index += 3;
            if(loadedLevelBuildIndex > 0)
            {
                yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
            }
            yield return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(index));
            loadedLevelBuildIndex = index;
            enabled = true;
        }

        // Use this for initialization
        void Start () {
            //Scene loadedScene = SceneManager.GetSceneByName("Level1");
            //if (loadedScene.isLoaded)
            //{
            //    SceneManager.SetActiveScene(loadedScene);
            //    return;
            //}
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name.Contains("Level"))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
            StartCoroutine(LoadLevel(1));
        }

        // Update is called once per frame
        void Update () {
            if (Input.GetKeyDown(createKey))
            {
                CreateShape();
            }
            else if (Input.GetKeyDown(newGameKey))
            {
                BeginNewGame();
            }
            else if (Input.GetKeyDown(saveKey))
            {
                storage.Save(this, saveVersion);
            }
            else if (Input.GetKeyDown(loadKey))
            {
                BeginNewGame();
                storage.Load(this);
            }
            else if (Input.GetKeyDown(destroyKey))
            {
                DestroyShape();
            }
            else
            {
                for(int i = 1; i <= levelCount; ++i)
                {
                    if(Input.GetKeyDown(KeyCode.Alpha0 + i))
                    {
                        BeginNewGame();
                        StartCoroutine(LoadLevel(i));
                        return;
                    }
                }
            }

            creationProgress += Time.deltaTime * CreationSpeed;
            while(creationProgress >= 1f)
            {
                creationProgress -= 1f;
                CreateShape();
            }

            destructionProgress += Time.deltaTime * DestructionSpeed;
            while(destructionProgress >= 1f)
            {
                destructionProgress -= 1f;
                DestroyShape();
            }
        }

        void CreateShape()
        {
            var instance = shapeFactory.GetRandom();
            var tran = instance.transform;
            tran.localPosition = Random.insideUnitSphere * 5f;
            tran.localRotation = Random.rotation;
            tran.localScale = Vector3.one * Random.Range(0.1f, 1f);
            instance.SetColor(Random.ColorHSV(
                hueMin: 0f, hueMax: 1f,
                saturationMin: 0.5f, saturationMax: 1f,
                valueMin: 0.25f, valueMax: 1f,
                alphaMin: 1f, alphaMax: 1f
            ));
            shapes.Add(instance);
        }

        void DestroyShape()
        {
            if(shapes.Count > 0)
            {
                int index = Random.Range(0, shapes.Count);
                shapeFactory.Reclaim(shapes[index]);
                shapes[index] = shapes[shapes.Count - 1];
                shapes.RemoveAt(shapes.Count - 1);
            }
        }

        void BeginNewGame()
        {
            shapes.ForEach(t =>
            {
                shapeFactory.Reclaim(t);
            });
            shapes.Clear();
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(shapes.Count);
            writer.Write(loadedLevelBuildIndex - 3);
            for (int i = 0; i < shapes.Count; i++)
            {
                writer.Write(shapes[i].ShapeId);
                writer.Write(shapes[i].MaterialId);
                shapes[i].Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            int version = reader.Version;
            if (version > saveVersion)
            {
                Debug.LogError("Unsupported future save version " + version);
                return;
            }

            int count = version > 0 ? reader.ReadInt() : -version;
            StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));
            var tran = transform;
            for (int i = 0; i < count; i++)
            {
                int shapeId = version > 0 ? reader.ReadInt() : 0;
                int matId = version > 0 ? reader.ReadInt() : 0;
                Shape o = shapeFactory.Get(shapeId, matId);
                o.transform.parent = tran;
                o.Load(reader);
                shapes.Add(o);
            }
        }
    }
}
