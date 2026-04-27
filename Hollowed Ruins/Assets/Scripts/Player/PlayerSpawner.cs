using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        // Find the PlayerSpawn marker in the generated level
        GameObject spawnMarker = GameObject.Find("PlayerSpawn");

        if (spawnMarker != null)
        {
            Transform spawn = spawnMarker.transform;
            Instantiate(playerPrefab, spawn.position, spawn.rotation);
        }
        else
        {
            Debug.LogWarning("No PlayerSpawn marker found in level!");
        }
    }
}
