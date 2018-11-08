using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class Game : PersistableObject {

        [SerializeField]
        private ShapeFactory shapeFactory;

        [SerializeField]
        private KeyCode createKey = KeyCode.C;

        [SerializeField]
        private KeyCode newGameKey = KeyCode.N;

        [SerializeField]
        private KeyCode saveKey = KeyCode.S;

        [SerializeField]
        private KeyCode loadKey = KeyCode.L;

        [SerializeField]
        private PersistentStorage storage;

        private readonly List<Shape> shapes = new List<Shape>();

        private const int saveVersion = 1;

        private void Awake()
        {
            
        }

        // Use this for initialization
        void Start () {
        
	    }
	
	    // Update is called once per frame
	    void Update () {
            if (Input.GetKeyDown(createKey))
            {
                CreateShape();
            }else if (Input.GetKeyDown(newGameKey))
            {
                BeginNewGame();
            }else if(Input.GetKeyDown(saveKey))
            {
                storage.Save(this, saveVersion);
            }else if(Input.GetKeyDown(loadKey))
            {
                BeginNewGame();
                storage.Load(this);
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
            tran.parent = transform;
            shapes.Add(instance);
        }

        void BeginNewGame()
        {
            shapes.ForEach(t =>
            {
                Destroy(t.gameObject);
            });
            shapes.Clear();
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(shapes.Count);
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
