using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DB_Register : MonoBehaviour
{

    [Header("Pannel")]
    public GameObject loginMenu;
    public GameObject registerMenu;
    public GameObject serverMenu;

    [Header("Input")]
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordField2;

    [Header("Message")]
    public TMP_Text errorTxt;

    [Header("Button")]
    public Button submitButton;

    private DB_Request db = new DB_Request();

    public void CallRegister()
    {
        StartCoroutine(Register());
    }

    public void LoginMenu()
    {
        loginMenu.SetActive(true);
        registerMenu.SetActive(false);
    }

    IEnumerator Register()
    {
        errorTxt.text = "";
        WWWForm form = new WWWForm();
        form.AddField("name", usernameField.text);
        form.AddField("password", passwordField.text);

        yield return StartCoroutine(this.db.Request("User/register.php", form));

        if (this.db.getError)
            errorTxt.text = this.db.errorMsg;
        else
        {
            NetworkCore.instance.currentPlayer.name = usernameField.text;
            serverMenu.SetActive(true);
            registerMenu.SetActive(false);
        }
    }

    public void VerifyInputs()
    {
        submitButton.interactable = (usernameField.text.Length >= 5 && passwordField.text.Length >= 8 && passwordField.text == passwordField2.text);
    }
}
