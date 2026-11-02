using System;
using UnityEngine;
using Invector.vCharacterController;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Serializable]
    public class Wave
    {
        public GameObject[] enemyPrefabs;
        public Transform[] spawnPoints;
    }

    [SerializeField] private Wave[] waves;
    [SerializeField] private bool startOnAwake = false;

    private int currentWaveIndex = -1;
    private int aliveInCurrentWave;

    private void Start()
    {
        if (startOnAwake)
        {
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            return;
        }

        Wave wave = waves[currentWaveIndex];
        aliveInCurrentWave = wave.enemyPrefabs.Length;

        for (int i = 0; i < wave.enemyPrefabs.Length; i++)
        {
            Transform point = wave.spawnPoints[i % wave.spawnPoints.Length];
            GameObject enemy = Instantiate(wave.enemyPrefabs[i], point.position, point.rotation);

            vCharacter character = enemy.GetComponent<vCharacter>();
            if (character != null)
            {
                character.onDead.AddListener(_ => OnEnemyDefeated());
            }
            else
            {
                Debug.LogWarning($"[EnemyWaveSpawner] {enemy.name} has no vCharacter component — wave completion won't detect its death.", enemy);
            }
        }
    }

    private void OnEnemyDefeated()
    {
        aliveInCurrentWave--;

        if (aliveInCurrentWave <= 0)
        {
            StartNextWave();
        }
    }
}
