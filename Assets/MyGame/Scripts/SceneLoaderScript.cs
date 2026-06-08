using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderScript : MonoBehaviour
{
    [SerializeField] private string[] additiveSceneNames = { "BirthdayParty" };
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool setFirstLoadedSceneActive;

    private readonly HashSet<string> loadingScenes = new HashSet<string>();

    private void Start()
    {
        if (loadOnStart)
        {
            LoadConfiguredScenes();
        }
    }

    public void LoadConfiguredScenes()
    {
        foreach (string sceneName in additiveSceneNames)
        {
            LoadSceneAdditive(sceneName);
        }
    }

    public void LoadSceneAdditive(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        if (SceneManager.GetSceneByName(sceneName).isLoaded || loadingScenes.Contains(sceneName))
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        loadingScenes.Add(sceneName);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            Debug.LogError($"Could not start additive scene load for '{sceneName}'. Make sure it is added to Build Settings.", this);
            loadingScenes.Remove(sceneName);
            yield break;
        }

        yield return loadOperation;

        loadingScenes.Remove(sceneName);

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (setFirstLoadedSceneActive && loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
        }
    }
}
