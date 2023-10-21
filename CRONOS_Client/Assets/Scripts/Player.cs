using ENet;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public struct PlayerTransform
{
    public Vector2 position;
    public float rotation;
}

[Serializable]
public class DataPlayer
{
    public int id;
    public string name;
    public Peer peer;
    public Vector2 position;
    public Vector2 mousePos;
    public float rotation;
    public float speed;
    public bool switchedMousePos;
}

public class Player : MonoBehaviour
{
    public TMP_Text nameText;
    public LayerMask layerToHit;
    public GameObject playerCamera;
    public GameObject playerVisu;
    public GameObject playerMarkerDestination;
    public GameObject pauseMenuPrefabs;
    public bool onPauseMenu = false;
    [HideInInspector]
    public bool isCurrentPlayer = false;

    [SerializeField]
    private DataPlayer data;
    private Vector3 screenPos;
    private Camera cam;

    private void Awake()
    {
        cam = playerCamera.GetComponent<Camera>();
    }

    private void Update()
    {
        if (data == null)
            return;

        //Direction du regard du player vers son prochain deplacement
        playerVisu.transform.eulerAngles = new Vector3(0, data.rotation, 0);

        //Mise à jours de la pos du player
        this.transform.position = new Vector3(data.position.x, 0, data.position.y);

        if (!isCurrentPlayer)
            return;

        //Click gauche raycast pour envoyer au serveur la direction souhaité
        if (!onPauseMenu && Input.GetMouseButton(0))
        {
            screenPos = Input.mousePosition;

            Ray ray = cam.ScreenPointToRay(screenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerToHit))
            {
                data.mousePos = new Vector2(hit.point.x, hit.point.z);
            }
        }

        //Si différence entre le serveur et le joueur alors on indique au serveur qu'on à changer de direction
        if (data.mousePos != NetworkCore.instance.currentPlayer.mousePos)
        {
            NetworkCore.instance.currentPlayer.mousePos = data.mousePos;
            NetworkCore.instance.currentPlayer.switchedMousePos = true;
        }

        //Mise à jours du marker de destination du player
        if (!this.playerMarkerDestination.activeSelf && this.data.mousePos != this.data.position)
            this.playerMarkerDestination.SetActive(true);
        else if (this.playerMarkerDestination.activeSelf && this.data.mousePos == this.data.position)
            this.playerMarkerDestination.SetActive(false);
        this.playerMarkerDestination.transform.position = new Vector3(data.mousePos.x, 0, data.mousePos.y);

        //Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape) && !onPauseMenu)
        {
            onPauseMenu = true;

            GameObject go = Instantiate(pauseMenuPrefabs);
            go.GetComponent<PauseMenu>().player = this;
        }
    }

    public int GetId()
    {
        return data.id;
    }

    public string GetName()
    {
        return data.name;
    }

    public void InitData(DataPlayer data)
    {
        this.data = data;
        nameText.text = data.name;
    }

    public void SetActiveCamera(bool active)
    {
        playerCamera.SetActive(active);
    }

}
