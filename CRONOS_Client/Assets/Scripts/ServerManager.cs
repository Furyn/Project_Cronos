using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{

    [SerializeField]
    private GameObject LoginMenu;

    [SerializeField]
    private GameObject LoadingMenu;

    [SerializeField]
    private TMP_InputField ipField;

    public void ConnectWithIp()
    {
        NetworkCore.instance.Connect(ipField.text);
    }

    public void ConnectLocalhostP1()
    {
        NetworkCore.instance.currentPlayer.name = "P1";
        NetworkCore.instance.currentPlayer.id = 5;
        NetworkCore.instance.Connect("localhost");
    }

    public void ConnectLocalhostP2()
    {
        NetworkCore.instance.currentPlayer.name = "P2";
        NetworkCore.instance.currentPlayer.id = 6;
        NetworkCore.instance.Connect("localhost");
    }

    public void ConnectLocalhost()
    {
        NetworkCore.instance.Connect("localhost");
    }

    public void SwitchScene(string sceneName)
    {
        LoginMenu.SetActive(false);
        LoadingMenu.SetActive(true);
        StartCoroutine(ISwitchScene(sceneName));
    }

    IEnumerator ISwitchScene(string sceneName)
    {
        bool loadScene = false;

        //Deja connecter au moment de la coroutine
        if (NetworkCore.instance.isConnected)
        {
            loadScene = true;
        }

        //En attente de connexion
        while (NetworkCore.instance.attemptToConnect && !loadScene)
        {
            yield return new WaitForSeconds(0.1f);
            if (NetworkCore.instance.isConnected)
            {
                loadScene = true;
            }
        }

        if (loadScene)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            LoginMenu.SetActive(true);
            LoadingMenu.SetActive(false);
        }
        yield return null;
    }
}
