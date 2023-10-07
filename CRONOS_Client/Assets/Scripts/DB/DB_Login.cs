using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DB_Login : MonoBehaviour
{
    [Header("Pannel")]
    public GameObject loginMenu;
    public GameObject registerMenu;
    public GameObject serverMenu;

    [Header("Input")]
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;

    [Header("Message")]
    public TMP_Text errorTxt;

    [Header("Button")]
    public Button submitButton;

    private DB_Request db = new DB_Request();

    public void CallLogin()
    {
        StartCoroutine(LoginPlayer());
    }

    public void RegisterMenu()
    {
        registerMenu.SetActive(true);
        loginMenu.SetActive(false);
    }

    IEnumerator LoginPlayer()
    {
        errorTxt.text = "";
        WWWForm form = new WWWForm();
        form.AddField("name", usernameField.text);
        form.AddField("password", passwordField.text);

        yield return StartCoroutine(this.db.Request("User/login.php",form));

        if (this.db.getError)
            errorTxt.text = this.db.errorMsg;
        else
        {
            serverMenu.SetActive(true);
            loginMenu.SetActive(false);
            NetworkCore.instance.currentPlayer.name = usernameField.text;
            NetworkCore.instance.currentPlayer.id = int.Parse(this.db.response["user_id"]);
        }
    }

    public void VerifyInputs()
    {
        submitButton.interactable = (usernameField.text.Length >= 5 && passwordField.text.Length >= 8);
    }
}
