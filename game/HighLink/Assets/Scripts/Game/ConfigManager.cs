using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }
    private LoadAndStoreJSON _jsonLoader;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLoader();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLoader()
    {
        _jsonLoader = FindAnyObjectByType<LoadAndStoreJSON>(FindObjectsInactive.Include);
        if (_jsonLoader == null)
        {
            _jsonLoader = gameObject.AddComponent<LoadAndStoreJSON>();
        }
    }

    public bool IsConfigReady => LoadAndStoreJSON.configDictionary.Count > 0;
}