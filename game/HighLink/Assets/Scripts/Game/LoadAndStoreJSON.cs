using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LoadAndStoreJSON : MonoBehaviour
{
    [SerializeField] private ServerConfig serverSettings;
    public static Dictionary<string, ConfigData> configDictionary = new Dictionary<string, ConfigData>();

    [System.Serializable]
    public class ConfigData
    {
        public string name;
        public float value; // Cambiado a float para manejar decimales directamente
        public string type;
    }

    [System.Serializable]
    private class RawJsonData
    {
        public ConfigData[] Config;
    }

    private string searchJsonURL()
    {
        return serverSettings.jsonDataURL;
    }

    void Start()
    {
        string jsonURL = searchJsonURL();

        StartCoroutine(GetJSONData(jsonURL));
    }

    IEnumerator GetJSONData(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error al conectar: {request.error}");
                yield break;
            }

            string jsonText = request.downloadHandler.text;

            try
            {
                RawJsonData wrapper = JsonUtility.FromJson<RawJsonData>(jsonText);

                if (wrapper?.Config == null)
                {
                    Debug.LogError("El JSON no tiene el formato esperado");
                    yield break;
                }

                configDictionary.Clear();
                foreach (var config in wrapper.Config)
                {
                    if (config != null && !string.IsNullOrEmpty(config.name))
                    {
                        configDictionary[config.name] = config;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al procesar JSON: {e.Message}");
            }
        }
    }

    public static ConfigData GetConfig(string configName)
    {
        if (configDictionary.TryGetValue(configName, out ConfigData config))
        {
            return config;
        }
        Debug.LogWarning($"Configuración '{configName}' no encontrada");
        return null;
    }
}