using ENet;
using System;
using TMPro;
using UnityEngine;

[Serializable]
public class DataPlayer
{
    public int id = 0;
    public string name = "";
    public Peer peer;
    public Vector2 position = Vector2.zero;
    public Vector2 mousePos = Vector2.zero;
    public Vector2 direction = Vector2.zero;
    public bool switchedMousePos = false;
    public float speed = 10f;

    public DataPlayer()
    {
    }

    public DataPlayer(Peer peer)
    {
        this.peer = peer;
    }

    public void UpdatePhysics(float elapsedTime)
    {
        if (position != mousePos)
        {
            direction = (mousePos - position).normalized;
            if (Vector2.Distance(mousePos, position) <= 0.2f)
                position = mousePos;
            else
                position += direction * speed * elapsedTime;
        }
    }
}

public class Player : MonoBehaviour
{
    public TMP_Text nameText;
    public GameObject playerCapsule;
    public DataPlayer data;

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

        this.transform.position = new Vector3(data.position.x, 0, data.position.y);
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

}
