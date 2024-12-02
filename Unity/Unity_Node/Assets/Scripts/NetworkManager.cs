using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

[SerializeField ]
public class NetworkMessage
{
    public string type;
    public string playerid;
    public string message;
    public Vector3Data position;
}

[SerializeField]

public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 v)
    {
        x= v.x;
        y= v.y;
        z= v.z;

    }
    public Vector3 ToVector3()
    {
        return new Vector3(x,y,z);
    }
}

public class NetworkManager : MonoBehaviour
{
    private WebSocket webSocket;
    [SerializeField] private string serverUrl = "ws://localhost: 3000";

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button SendButton;
    [SerializeField] private TextMeshProUGUI chatLog;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefabs;
    private string myPlayerId;
    private GameObject myPlayer;
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();

    private float synclnterval = 0.1f;
    private float syncTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_WEBGL || UNITY_EDITOR
        if(webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
#endif
        if(myPlayer != null)
        {
            syncTimer += Time.deltaTime;
            if(syncTimer >= synclnterval)
            {
                SendPositionUpdate();
                syncTimer = 0f;
            }
        }

    }

    private async void ConnectToServer()
    {
        webSocket = new WebSocket(serverUrl);
        webSocket.OnOpen += () =>
        {
            Debug.Log("연결 성공!");
            UpdateStatusText("연결됨", Color.green);
        };

        webSocket.OnError += (e) =>
        {
            Debug.Log($"에러 : {e}");
            UpdateStatusText("에러 발생", Color.red);
        };
        webSocket.OnClose += (e) =>
        {
            Debug.Log("연결 종료");
            UpdateStatusText("연결 끊김", Color.red);
        };

        webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };
        await webSocket.Connect();
    }
    private void HandleMessage(string json)
    {
        try
        {
            NetworkMessage message = JsonConvert.DeserializeObject<NetworkMessage>(json);

            switch (message.type)
            {
                case "connection":
                    HandleConnection(message);
                    break;
                case "chat":
                    AddToChatLog(message.message);
                    break;
                case "playerPosition":
                    UpdatePlayerPosition(message);
                    break;
                case "playerDisconnect":
                    RemovePlayer(message.playerid);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"메세지 처리 중 에러: {e.Message}");
        }
    }
    private async void SendChatMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;

        if (webSocket.State == WebSocketState.Open)
        {
            NetworkMessage message = new NetworkMessage
            {
                type = "chat",
                message = messageInput.text,
            };

            await webSocket.SendText(JsonConvert.SerializeObject(message));
            messageInput.text = "";
        }

    }
    private void UpdateStatusText(string status, Color color)
    {
        if (statusText != null)
        {
            statusText.text = status;
            statusText.color = color;
        }
    }
    private void AddToChatLog(string message)
    {
        if (chatLog != null)
        {
            chatLog.text += $"\n{message}";
        }
    }

    private async void OnApplicationQuit()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.Close();
        }
    }

    private void HandleConnection(NetworkMessage message)
    {
        myPlayerId = message.playerid;
        AddToChatLog($"서버에 연결됨 (ID: {myPlayerId}");

        Vector3 spawnPosition = new Vector3(0, 1, 0);
        myPlayer = Instantiate(playerPrefabs, spawnPosition, Quaternion.identity);
        myPlayer.name = $"Player_{myPlayerId}";

        PlayerController controller = myPlayer.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetAsMyPlayer();
        }
    }

    private async void SendPositionUpdate()
    {
        if (webSocket.State == WebSocketState.Open && myPlayer != null)
        {
            NetworkMessage message = new NetworkMessage
            {
                type = "PlayerPosition",
                playerid = myPlayerId,
                position = new Vector3Data(myPlayer.transform.position)
            };
            await webSocket.SendText(JsonConvert.SerializeObject(message));
        }
    }

    private void UpdatePlayerPosition(NetworkMessage message)
    {
        if (message.playerid == myPlayerId) return;

        if (!otherPlayers.ContainsKey(message.playerid))
        {
            GameObject newPlayer = Instantiate(playerPrefabs);
            newPlayer.name = $"Player_{message.playerid}";
            otherPlayers.Add(message.playerid, newPlayer);
        }

        otherPlayers[message.playerid].transform.position = message.position.ToVector3();
    }

    //플레이어 제거 함수
    private void RemovePlayer(string playerid)
    {
        if(otherPlayers.ContainsKey(playerid))
        {
            Destroy(otherPlayers[playerid]);
            otherPlayers.Remove(playerid);
            AddToChatLog($"플레이어{playerid}퇴장");
        }
    }
}
