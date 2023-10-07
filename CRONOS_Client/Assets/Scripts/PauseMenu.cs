using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public Player player;

    public void ContinueButtons()
    {
        player.onPauseMenu = false;
        Destroy(this.gameObject);
    }

    public void MainMenuButtons(string sceneName)
    {
        NetworkCore.instance.Disconnect();
        SceneManager.LoadScene(sceneName);
    }

    public void ExitButtons()
    {
        NetworkCore.instance.Disconnect();
        Application.Quit();
    }

}
