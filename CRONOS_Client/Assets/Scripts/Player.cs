using ENet;
using System;
using TMPro;
using UnityEngine;

[Serializable]
public struct DataPlayer
{
    public int id;
    public string name;
    public Peer peer;
    public Vector2 position;
    public bool switchedMousePos;
    public Vector2 mousePos;
}

public class Player : MonoBehaviour
{
    public TMP_Text nameText;
    public LayerMask layerToHit;
    public GameObject playerCamera;
    public GameObject playerCapsule;
    public GameObject playerMarkerDestination;
    public GameObject pauseMenuPrefabs;
    public bool onPauseMenu = false;
    [HideInInspector]
    public bool isCurrentPlayer = false;

    [SerializeField]
    private DataPlayer data = new DataPlayer();
    private Vector3 screenPos;
    private Camera cam;

    private void Awake()
    {
        cam = playerCamera.GetComponent<Camera>();
        data.switchedMousePos = false;
    }

    private void Update()
    {
        //Direction du regard du player vers son prochain deplacement
        if (this.transform.position != new Vector3(data.position.x, 0, data.position.y))
        {
            Vector3 direction = new Vector3(data.position.x, 0, data.position.y) - transform.position;
            direction /= direction.magnitude;

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            playerCapsule.transform.rotation = rotation;
        }
        

        //Mise � jours de la pos du player
        this.transform.position = new Vector3(data.position.x, 0, data.position.y);

        if (!isCurrentPlayer)
            return;

        //Click gauche raycast pour envoyer au serveur la direction souhait�
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

        //Si diff�rence entre le serveur et le joueur alors on indique au serveur qu'on � changer de direction
        if (data.mousePos != NetworkCore.instance.currentPlayer.mousePos)
        {
            NetworkCore.instance.currentPlayer.mousePos = data.mousePos;
            NetworkCore.instance.currentPlayer.switchedMousePos = true;
        }

        //Mise � jours du marker de destination du player
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

    public void SetPos(Vector2 newPos)
    {
        this.data.position = newPos;
    }

    public void SetActiveCamera(bool active)
    {
        playerCamera.SetActive(active);
    }

}
