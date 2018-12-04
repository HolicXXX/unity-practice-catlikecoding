using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ObjectManagement
{
    public class Game : PersistableObject {

        [SerializeField]
        private ShapeFactory[] shapeFactorys;

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

        [SerializeField]
        private bool resetOnLoad;

        [SerializeField]
        private Slider creationSpeedSlider;

        [SerializeField]
        private Slider destructionSpeedSlider;

        public SpawnZone SpawnZoneofLevel { get; set; }

        public float CreationSpeed { get; set; }

        public float DestructionSpeed { get; set; }

        private readonly List<Shape> shapes = new List<Shape>();

        private const int saveVersion = 5;

        private float creationProgress;

        private float destructionProgress;

        private int loadedLevelBuildIndex;

        Random.State mainRandomState;

        private void Awake()
        {
            
        }

        private void OnEnable()
        {
            if(shapeFactorys[0].FactoryId != 0)
            {
                for (int i = 0; i < shapeFactorys.Length; ++i)
                {
                    shapeFactorys[i].FactoryId = i;
                }
            }
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
            mainRandomState = Random.state;
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
            BeginNewGame();
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
                StartCoroutine(LoadLevel(loadedLevelBuildIndex - 3));
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
        }

        private void FixedUpdate()
        {
            shapes.ForEach(s => s.GameUpdate());
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
            shapes.Add(GameLevel.Current.SpawnShape());
        }

        void DestroyShape()
        {
            if(shapes.Count > 0)
            {
                int index = Random.Range(0, shapes.Count);
                shapes[index].Recycle();
                shapes[index] = shapes[shapes.Count - 1];
                shapes.RemoveAt(shapes.Count - 1);
            }
        }

        void BeginNewGame()
        {
            Random.state = mainRandomState;
            int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
            mainRandomState = Random.state;
            Random.InitState(seed);
            creationSpeedSlider.value = destructionSpeedSlider.value = CreationSpeed = DestructionSpeed = 0f;
            shapes.ForEach(t => t.Recycle());
            shapes.Clear();
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(shapes.Count);
            writer.Write(Random.state);
            writer.Write(CreationSpeed);
            writer.Write(creationProgress);
            writer.Write(DestructionSpeed);
            writer.Write(destructionProgress);
            writer.Write(loadedLevelBuildIndex - 3);
            GameLevel.Current.Save(writer);
            for (int i = 0; i < shapes.Count; i++)
            {
                writer.Write(shapes[i].OriginFactory.FactoryId);
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

            StartCoroutine(LoadGame(reader));
        }

        IEnumerator LoadGame(GameDataReader reader)
        {
            int version = reader.Version;
            int count = version > 0 ? reader.ReadInt() : -version;
            if (version >= 3)
            {
                Random.State state = reader.ReadRandomState();
                if (resetOnLoad)
                {
                    Random.state = state;
                }
                creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
                creationProgress = reader.ReadFloat();
                destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
                destructionProgress = reader.ReadFloat();
            }
            yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
            if (version >= 3)
            {
                GameLevel.Current.Load(reader);
            }
            var tran = transform;
            for (int i = 0; i < count; i++)
            {
                int factoryId = version >= 5 ? reader.ReadInt() : 0;
                int shapeId = version > 0 ? reader.ReadInt() : 0;
                int matId = version > 0 ? reader.ReadInt() : 0;
                Shape o = shapeFactorys[factoryId].Get(shapeId, matId);
                o.transform.parent = tran;
                o.Load(reader);
                shapes.Add(o);
            }
        }
    }
}
