using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameView : MonoBehaviour
{

    public Text playerNameText;
    public Text metalText;
    public Text crystalText;
    public Text deuteriumText;
    public InputField playerNameInput;

    public Button registerButton;
    public Button loginButton;
    public Button collectButton;
    public Button developButton;
    public Slider progressBar;

    public void SetPlayerName(string name)
    {
        playerNameText.text = name;

    }
    public void UpdateResources(int metal, int crystal, int deuterium)
    {
        metalText.text = $"Metal: {metal}";
        crystalText.text = $"Crystal : {crystal}";
        deuteriumText.text = $"Deuteriu : {deuterium}";
    }

    public void UpdateProgressBar(float value)
    {
        progressBar.value = value;
    }

    public void SetRegisterButtonListener(UnityEngine.Events.UnityAction action)
    {
        registerButton.onClick.RemoveAllListeners();
        registerButton.onClick.AddListener(action);
    }
    public void SetLoginButtonListener(UnityEngine.Events.UnityAction action)
    {
        registerButton.onClick.RemoveAllListeners();
        registerButton.onClick.AddListener(action);
    }
    public void SetCollectButtonListener(UnityEngine.Events.UnityAction action)
    {
        registerButton.onClick.RemoveAllListeners();
        registerButton.onClick.AddListener(action);
    }
    public void SetDevelopButtonListener(UnityEngine.Events.UnityAction action)
    {
        registerButton.onClick.RemoveAllListeners();
        registerButton.onClick.AddListener(action);
    }
}
