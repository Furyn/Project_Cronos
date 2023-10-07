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

    public void SwitchScene(string sceneName)
    {
        LoginMenu.SetActive(false);
        LoadingMenu.SetActive(true);
        StartCoroutine(ISwitchScene(sceneName));
    }

    IEnumerator ISwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        yield return null;
    }
}
