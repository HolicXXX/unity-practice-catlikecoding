using UnityEngine;
using System.IO;

namespace ObjectManagement
{
    public class PersistentStorage : MonoBehaviour {

        private string savePath;

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "saveFile");
        }

        // Use this for initialization
        void Start () {
		
	    }
	
	    // Update is called once per frame
	    void Update () {
		
	    }

        public void Save(PersistableObject o, int version)
        {
            using (var writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                writer.Write(-version);
                o.Save(new GameDataWriter(writer));
            }
        }

        public void Load(PersistableObject o)
        {
            using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
            {
                o.Load(new GameDataReader(reader, -reader.ReadInt32()));
            }
        }
    }
}
