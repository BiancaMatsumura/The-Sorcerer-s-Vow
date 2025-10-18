using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScenePositioner : MonoBehaviour
{
    
    [System.Serializable]
    public class SceneSpawn
    {
        public string SceneName;
        public Transform SpawnPoint;
    }

    public SceneSpawn[] sceneSpawns;

    private CharacterController cc;

private void Awake()
{
    cc = GetComponent<CharacterController>();
    cc.enabled = false;

    // Try to position the player based on the active scene's spawn point, if any
    var currentScene = SceneManager.GetActiveScene();
    if (sceneSpawns != null)
    {
        foreach (var spawn in sceneSpawns)
        {
            if (spawn != null && !string.IsNullOrEmpty(spawn.SceneName) &&
                spawn.SceneName == currentScene.name && spawn.SpawnPoint != null)
            {
                transform.position = spawn.SpawnPoint.position;
                transform.rotation = spawn.SpawnPoint.rotation;
                break;
            }
        }
    }

    cc.enabled = true;
}


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var spawn in sceneSpawns)
        {
            if (scene.name == spawn.SceneName && spawn.SpawnPoint != null)
            {
                // Chama coroutine para reposicionar o player corretamente
                StartCoroutine(MovePlayerToSpawn(spawn.SpawnPoint.position, spawn.SpawnPoint.rotation));
                break;
            }
        }
    }

    private IEnumerator MovePlayerToSpawn(Vector3 position, Quaternion rotation)
    {
        // Espera 1 frame para garantir que todos os scripts do player estão inicializados
        yield return null;

        // Desativa temporariamente o CharacterController para evitar travamento
        if (cc != null)
            cc.enabled = false;

        // Reposiciona o player
        transform.position = position;
        transform.rotation = rotation;

        // Reativa o CharacterController
        if (cc != null)
            cc.enabled = true;
    }
}
