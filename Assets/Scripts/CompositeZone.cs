using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class CompositeZone : SpawnZone
    {
        [SerializeField]
        bool overrideConfig;

        [SerializeField]
        SpawnZone[] spawnZones;

        [SerializeField]
        bool sequential;

        int nextSequentialIndex = 0;

        public override Vector3 SpawnPoint
        {
            get
            {
                int index = 0;
                if (sequential)
                {
                    index = nextSequentialIndex++;
                    if(nextSequentialIndex >= spawnZones.Length)
                    {
                        nextSequentialIndex = 0;
                    }
                }
                else
                {
                    index = Random.Range(0, spawnZones.Length);
                }
                return spawnZones[index].SpawnPoint;
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(nextSequentialIndex);
        }

        public override void Load(GameDataReader reader)
        {
            nextSequentialIndex = reader.ReadInt();
        }

        public override void ConfigureSpawn(Shape shape)
        {
            if (overrideConfig)
            {
                base.ConfigureSpawn(shape);
            }
            else
            {
                int index = 0;
                if (sequential)
                {
                    index = nextSequentialIndex++;
                    if (nextSequentialIndex >= spawnZones.Length)
                    {
                        nextSequentialIndex = 0;
                    }
                }
                else
                {
                    index = Random.Range(0, spawnZones.Length);
                }
                spawnZones[index].ConfigureSpawn(shape);
            }
        }
    }
}
