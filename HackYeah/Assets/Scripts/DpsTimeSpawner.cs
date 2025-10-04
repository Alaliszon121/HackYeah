using Cinemachine;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class DspTimeSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct TimedSpawn
    {
        public CinemachineDollyCart prefab;
        public double spawnTime;
        public CinemachineSmoothPath path;
        public int laneIndex;
        [HideInInspector] public bool judged;
    }

    [Header("Spawn Data")]
    public List<TimedSpawn> spawns = new List<TimedSpawn>();

    private int currentIndex = 0;

    private void Start()
    {
        while (currentIndex < spawns.Count)
        {
            TimedSpawn spawn = spawns[currentIndex];

            if (spawn.prefab == null)
            {
                Debug.LogWarning($"Spawn {currentIndex} has no prefab assigned!");
                currentIndex++;
                continue;
            }

            spawn.prefab.m_Path = spawn.path;
            spawn.prefab.m_Position = (float)spawn.spawnTime / 2f;

            Instantiate(spawn.prefab, Vector3.zero, Quaternion.identity);
            currentIndex++;
        }
    }
}
