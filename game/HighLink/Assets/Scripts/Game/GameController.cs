using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Runtime.Serialization;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [SerializeField] private ServerConfig serverSettings;
    // // private string ServerUri = "https://highlink.dam.inspedralbes.cat/back";
    // private string ServerUri = "http://localhost:4000";
    // // private string StatsAPIUri = "https://highlink.dam.inspedralbes.cat/back/stats";
    // private string StatsAPIUri = "http://localhost:4001";
    // private string GameAPIUri = "/api/games";
    // private string CheckStatsUri = "/state-stats";
    private bool ServerOnline = false;
    private bool StatsOnline = false;
    private float heightToShow = 0f;
    private int GameId;
    [SerializeField] private TMP_Text gameIdText;

    private string searchGameURL()
    {
        return serverSettings.gameURL;
    }

    private string searchCheckStatsURL()
    {
        return serverSettings.checkStatsURL;
    }

    private string searchStatsApiURL()
    {
        return serverSettings.statsAPIURL;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameStart();
    }

    private void GameStart() {
        AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_Game);  
        StartCoroutine("CreateGame");

    }

    IEnumerator CreateGame() {
        // GameURL
        string gameURL = searchGameURL();

        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(gameURL, "")) {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError(req.error);
            } else {
                var response = req.downloadHandler.text;
                var game = JsonUtility.FromJson<Game>(response);
                ShowGameId(game.id);
                ServerOnline = true;
                CheckStatsService();
            }
        }
    }

    private void CheckStatsService() {
        StartCoroutine("CheckStats");
    }

    IEnumerator CheckStats() {
        // CheckStatsURL
        string checkStatsURL = searchCheckStatsURL();

        using (UnityWebRequest req = UnityWebRequest.Get(checkStatsURL)) {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError(req.error);
            } else {
                var response = req.downloadHandler.text;
                var Stats = JsonUtility.FromJson<StatsServiceState>(response);
                if (Stats.state == "running") {
                    StatsOnline = true;
                    InvokeRepeating("StartSendingStats", 0, 2.5f);
                }
                
            }
        }
    }

    private void StartSendingStats()
    {
        StartCoroutine("SendStats");
    }

    IEnumerator SendStats() {
        // StatsURL
        string statsApiURL = searchStatsApiURL();

        if (!(ServerOnline && StatsOnline)) {
            Debug.Log("Server or Stats service is offline");
            yield break;
        };


        StatsData jsonData = new StatsData(GameId, Mathf.Round(heightToShow * 100f) / 100f);

        // Convert the JSON object to a string
        string jsonString = JsonUtility.ToJson(jsonData);


        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(statsApiURL + $"?game_id={GameId}&height={Mathf.Round(heightToShow * 100f) / 100f}", "POST")) {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError(req.error);
                StatsOnline = false;
                CancelInvoke("StartSendingStats");
            }
        }
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        // Add your Camera position update logic here
        var heightToSave = newPosition.y;

        heightToShow = (heightToSave + 1.012f)*10;

        HeightController.Instance.UpdateHeight(heightToShow);
    }

    private void ShowGameId(int id)
    {
        GameId = id;
        gameIdText.text = "Game ID: " + GameId.ToString();
    }
}

public class Game
{
    public int id = -1;
}

public class StatsServiceState
{
    public string state = "";
}

[System.Serializable]
public class StatsData
{
    public int game_id;
    public float height;

    public StatsData(int gameId, float height)
    {
        this.game_id = gameId;
        this.height = height;
    }
}