using UnityEngine;

[CreateAssetMenu(fileName = "ServerSettings", menuName = "Config/Server")]
public class ServerConfig : ScriptableObject
{
    [Header ("Server Settings")]
    public string backURL = "http://localhost:4000/";
    public string statsAPIURL = "http://localhost:4001/";
    public string jsonDataURL => backURL + "api/config";
    public string loginURL => backURL + "api/users/login";
    public string registerURL => backURL + "api/users";
    public string gameURL => backURL + "api/games";
    public string checkStatsURL => backURL + "state-stats";
}
