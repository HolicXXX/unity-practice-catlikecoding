using UnityEngine;

namespace NucleonPerformance
{
    public class NucleonSpawner: MonoBehaviour {

        [SerializeField]
        private float timeBetweenSpawns = 0f;

        [SerializeField]
        private float spawnDistance = 0f;

        [SerializeField]
        private Nucleon[] nucleonPrefabs = null;

        private float timeSinceLastSpawn = 0f;

	    // Use this for initialization
	    void Start () {
		
	    }

        private void FixedUpdate()
        {
            timeSinceLastSpawn += Time.deltaTime;
            if(timeSinceLastSpawn >= timeBetweenSpawns)
            {
                timeSinceLastSpawn -= timeBetweenSpawns;
                SpawnNucleon();
            }
        }

        private void SpawnNucleon()
        {
            Nucleon prefab = nucleonPrefabs[Random.Range(0, nucleonPrefabs.Length)];
            Nucleon spawn = Instantiate(prefab, transform);
            spawn.transform.localPosition = Random.onUnitSphere * spawnDistance;
        }
    }
}

