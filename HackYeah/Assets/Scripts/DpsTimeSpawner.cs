using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class DspTimeSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct TimedSpawn
    {
        public CinemachineDollyCart prefab;  // prefab asset (used only for instantiation)
        public double spawnTime;
        public CinemachineSmoothPath path;
        public int laneIndex;

        [HideInInspector] public bool judged;
        [HideInInspector] public CinemachineDollyCart instance; // runtime clone
    }

    [Header("Spawn Data")]
    public List<TimedSpawn> spawns = new List<TimedSpawn>();

    [Header("Timing Settings")]
    public double initialOffset = 0.0; // first note offset in seconds
    public double interval = 2.33333333333333; // time between consecutive notes

    private int currentIndex = 0;

    private void Start()
    {
        // Automatically assign spawnTime based on offset + i * interval
        for (int i = 0; i < spawns.Count; i++)
        {
            var spawn = spawns[i];
            spawn.spawnTime = initialOffset + i * interval;
            spawns[i] = spawn;
        }

        // Instantiate clones and store instance references
        while (currentIndex < spawns.Count)
        {
            TimedSpawn spawn = spawns[currentIndex];

            if (spawn.prefab == null)
            {
                Debug.LogWarning($"Spawn {currentIndex} has no prefab assigned!");
                currentIndex++;
                continue;
            }

            var clone = Instantiate(spawn.prefab, Vector3.zero, Quaternion.identity);
            clone.m_Path = spawn.path;
            clone.m_Position = (float)spawn.spawnTime / 2f;

            spawn.instance = clone;
            spawns[currentIndex] = spawn;

            currentIndex++;
        }
    }
}
