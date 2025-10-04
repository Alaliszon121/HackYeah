using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int prefabIndex;
    // A list of transforms representing locations where a special object can spawn.
    // You should create empty GameObjects as children of your platform prefab
    // and drag them into this list in the Inspector.
    public List<Transform> specialSpawnPoints;

    // A separate list of transforms for the new category of power-up items.
    public List<Transform> powerUpSpawnPoints;
}