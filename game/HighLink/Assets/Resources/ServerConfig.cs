using UnityEngine;

[CreateAssetMenu(fileName = "ServerSettings", menuName = "Config/Server")]
public class ServerConfig : ScriptableObject
{
    [Header ("Server Settings")]
    public string backURL = "http://localhost:4000/";
    public string jsonDataURL => backURL + "api/config";
    public string loginURL => backURL + "api/users/login";
    public string registerURL => backURL + "api/users";
}
