using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class GhostSpawner : MonoBehaviour
{
    public GameObject ghostPrefab;
    public int ghostCount = 3;

    void Start()
    {
        // Find all GhostSpawn markers in the generated level
        GameObject[] spawnMarkers = GameObject.FindGameObjectsWithTag("GhostSpawn");

        if (spawnMarkers.Length == 0) return;

        // Shuffle and take 3 distinct spawn points
        List<GameObject> shuffled = spawnMarkers.OrderBy(x => Random.value).ToList();
        int count = Mathf.Min(ghostCount, shuffled.Count);

        for (int i = 0; i < count; i++)
        {
            Transform spawn = shuffled[i].transform;
            Instantiate(ghostPrefab, spawn.position, spawn.rotation);
        }
    }
}
