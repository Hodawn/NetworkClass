using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;

[System.Serializable]

public class Player
{
    public int player_id;
    public string username;
    public int level;
}

[System.Serializable]

public class InventoryItem
{
    public int item_id;
    public string name;
    public string description;
    public int value;
    public int quantity;
}

[System.Serializable]
public class Quest
{
    public int quest_id;
    public string title;
    public string description;
    public int reward_exp;
    public int reward_item_id;
    public string status;
}

public class GameDataManager : MonoBehaviour
{
    private string serverUrl = "https://localhost:3000";
    private Player currentPlayer;

    public List<InventoryItem> inventoryItems = new List<InventoryItem>();
    public List<Quest> playerQuests = new List<Quest>();

    //�α��� ���� �� ����� �̺�Ʈ
    public delegate void OnLoginSuccessHandler(Player player);
    public event OnLoginSuccessHandler OnLoginSuccess;

    //������ ������Ʈ�� ����� �̺�Ʈ
    public delegate void OnInventoryUpdateHandler(List<InventoryItem> items);
    public event OnInventoryUpdateHandler OnInventoryUpdate;

    //����Ʈ ������Ʈ�� ����� �̺�Ʈ
    public delegate void OnQuestsUpdateHandler(List<Quest> items);
    public event OnQuestsUpdateHandler OnQuestsUpdate;

    public IEnumerator Login(string username, string passwordHash)
    {
        var loginData = new Dictionary<string, string>
        {
            { "username", username },
            { "password_Hash", passwordHash },
        };

        string jsonData=JsonConvert.SerializeObject(loginData);
        var www = new UnityWebRequest($"{serverUrl}/login", "POST");
        byte[]jsonToSend=new System.Text.UTF8Encoding().GetBytes(jsonData);
        www.uploadHandler =(UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            var resoponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
            if((bool)resoponse["success"])
            {
                currentPlayer = JsonConvert.DeserializeObject<Player>(resoponse["player"].ToString());
                Debug.Log($"�α��� ����: {currentPlayer.username}");

                OnLoginSuccess?.Invoke(currentPlayer);

                //�α��� ���� �� ������ ��ȸ
                yield return StartCoroutine(GetInventory());
                yield return StartCoroutine(GetQuest());
            }
        }
        else
        {
            Debug.LogError("�α��� ����: " + www.error);
        }
        www.Dispose();
    }

    private IEnumerator GetInventory()
    {
        if (currentPlayer == null) yield break;
        using (UnityWebRequest www = UnityWebRequest.Get($"{serverUrl}/Inventory/{currentPlayer.player_id}"))
        {
            yield return www.SendWebRequest();
            if(www.result == UnityWebRequest.Result.Success)
            {
                inventoryItems=JsonConvert.DeserializeObject<List<InventoryItem>>(www.downloadHandler.text);
                Debug.Log("�κ��丮 ������:");
                foreach(var item in inventoryItems)
                {
                    Debug.Log($"-{item.name} x {item.quantity}(��ġ: {item.value})");
                }
                OnInventoryUpdate?.Invoke(inventoryItems);
            }
            else
            {
                Debug.LogError("�κ��丮 ��ȸ ����: " + www.error);
            }
        }
    }
    private IEnumerator GetQuest()
    {
        if (currentPlayer == null) yield break;
        using (UnityWebRequest www = UnityWebRequest.Get($"{serverUrl}/quests/{currentPlayer.player_id}"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                playerQuests = JsonConvert.DeserializeObject<List<Quest>>(www.downloadHandler.text);
                Debug.Log("�������� ����Ʈ :");
                foreach (var quest in playerQuests)
                {
                    Debug.Log($"-{quest.title} {quest.status}");
                }
                OnQuestsUpdate?.Invoke(playerQuests);
            }
            else
            {
                Debug.LogError("����Ʈ ��ȸ ����: " + www.error);
            }
        }
    }

    void Start()
    {
        StartCoroutine(Login("hero123", "hashed_password1"));
    }

    public InventoryItem GetInventoryItem(int itemId)
    {
        return inventoryItems.Find(item => item.item_id == itemId);

    }

    public Quest GetQuest(int questId)
    {
        return playerQuests.Find(quest => quest.quest_id == questId);
    }

    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }
}