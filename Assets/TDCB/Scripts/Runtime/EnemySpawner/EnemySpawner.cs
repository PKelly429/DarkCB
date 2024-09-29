using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPositions;
        [SerializeField] private GameObject BrutePrefab;
        [SerializeField] private GameObject SpeedlingPrefab;

        private const float SpawnPeriod = 4 * 60;
        private const float SpawnRate = 0.1f;

        private float StartTime;
        private WaitForSeconds waveDelay = new WaitForSeconds(SpawnPeriod);
        private WaitForSeconds spawnDelay = new WaitForSeconds(SpawnRate);
        private int GetWaveSpawnCount()
        {
            float t = (Time.time - StartTime)/60f;
            return Mathf.FloorToInt(Mathf.Pow(t, 1.8f));
        }

        private void SpawnEnemyAtPosition(Vector3 pos)
        {
            var enemy = Random.Range(0f, 1f) >= 0.85f ? BrutePrefab : SpeedlingPrefab;
            var newEnemy =Instantiate(enemy, pos + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), Quaternion.identity, transform);
            newEnemy.GetComponent<Enemy>().MoveToCenterOfMap();
        }

        private IEnumerator Start()
        {
            StartTime = Time.time;
            while (true)
            {
                yield return waveDelay;

                var spawnPos = spawnPositions[Random.Range(0, spawnPositions.Count)].position;
                int toSpawn = GetWaveSpawnCount();
                
                for(int i = 0; i<toSpawn; i++)
                {
                    SpawnEnemyAtPosition(spawnPos);
                    yield return spawnDelay;
                }
            }
        }
    }
}
