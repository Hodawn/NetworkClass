using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;          //JSON ���̺귯�� �߰�
using System;                   //ACTION<> ����� ���� ���ӽ����̽� �߰�
using System.Collections.Generic;

public class GameAPI : MonoBehaviour
{
    private string baseUrl = "https://localhos:4000/api";     //node.js ������ url
    public IEnumerator RigisterPlayer(string playerName, string password)
    {
        var requestData = new { name = playerName, password = password };
        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log($"Registering Player : {jsonData}");

using (UnityWebRequest request=new UnityWebRequest($"{baseUrl}/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error registering player:  {request.result}");
            }
            else
            {
                Debug.Log("Player registerd successfully");
            }
        }
    }

    //�÷��̾� �α��� �޼���
    public IEnumerator LoginPlayer(string playerName, string password, Action<PlayerModel> onSuccess)
    {
        var requestData = new { name = playerName, password = password };
        string jsonData = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error loging in : {request.error}");
            }
            else
            {
                string responseBody = request.downloadHandler.text;

                try
                {
                    var reqponseData = JsonConvert.DeserializeObject > Dictionary<string, object> > (responseBody);

                    PlayerModel playerModel = new PlayerModel(responseData["playerName"].ToString())
                    {
                        metal = Convert.ToInt32(responseData["metal"]),
                        crystal = Convert.ToInt32(reqponseData["crystal"]),
                        deuterium = Convert.ToInt32(reqponseData["deuterium"]),
                        Planets = new List<PlanetModel>()
                    };
                    onSuccess?.Invoke(playerModel);
                    Debug.Log("Login successful");
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Error processing login response: {ex.Message}");
                }
            }
        }
    }
}