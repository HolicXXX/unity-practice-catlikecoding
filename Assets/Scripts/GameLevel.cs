using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class GameLevel : PersistableObject
    {
        public static GameLevel Current { get; private set; }

        [SerializeField]
        SpawnZone spawnZone;

        [SerializeField]
        PersistableObject[] persistableObjects;

        public Vector3 SpawnPoint
        {
            get
            {
                return spawnZone.SpawnPoint;
            }
        }

        private void OnEnable()
        {
            Current = this;
            if(persistableObjects == null)
            {
                persistableObjects = new PersistableObject[0];
            }
        }

        public Shape SpawnShape()
        {
            return spawnZone.SpawnShape();
        }

        public override void Load(GameDataReader reader)
        {
            int count = reader.ReadInt();
            for(int i = 0;i < count; ++i)
            {
                persistableObjects[i].Load(reader);
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(persistableObjects.Length);
            for(int i = 0;i < persistableObjects.Length; ++i)
            {
                persistableObjects[i].Save(writer);
            }
        }
    }
}
