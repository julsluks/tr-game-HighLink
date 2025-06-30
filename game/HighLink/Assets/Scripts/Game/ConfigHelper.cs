using UnityEngine;

public static class ConfigHelper
{
    public static float GetFloat(string configName)
    {
        if (!ConfigManager.Instance.IsConfigReady)
        {
            Debug.LogError("Configuración no cargada. Verifica LoadAndStoreJSON");
            return 0f;
        }

        var config = LoadAndStoreJSON.GetConfig(configName);
        if (config != null && config.type == "float")
        {
            return config.value;
        }
        Debug.LogError($"Configuración inválida para float: {configName}");
        return 0f;
    }

    public static int GetInt(string configName)
    {
        if (!ConfigManager.Instance.IsConfigReady)
        {
            Debug.LogError("Configuración no cargada");
            return 0;
        }

        var config = LoadAndStoreJSON.GetConfig(configName);
        if (config != null && config.type == "int")
        {
            return (int)config.value;
        }
        Debug.LogError($"Configuración inválida para int: {configName}");
        return 0;
    }

    public static string GetString(string configName)
    {
        if (!ConfigManager.Instance.IsConfigReady)
        {
            return "CONFIG_NOT_LOADED";
        }

        var config = LoadAndStoreJSON.GetConfig(configName);
        return config?.value.ToString() ?? "NOT_FOUND";
    }
}