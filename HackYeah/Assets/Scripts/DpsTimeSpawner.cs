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

            // Instantiate the prefab -> runtime instance
            var clone = Instantiate(spawn.prefab, Vector3.zero, Quaternion.identity);
            clone.m_Path = spawn.path;
            clone.m_Position = (float)spawn.spawnTime / 2f;

            // store reference to the instance so we can destroy it later
            spawn.instance = clone;

            spawns[currentIndex] = spawn; // update list entry
            currentIndex++;
        }
    }
}
