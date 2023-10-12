using ENet;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class DataPlayer
{
    public int id = 0;
    public string name = "";
    public Peer peer;
    public Vector2 position = Vector2.zero;
    public Vector2 mousePos = Vector2.zero;
    public float rotation = 0f;
    public bool switchedMousePos = false;
    public float speed = 10f;

    public DataPlayer()
    {
    }

    public DataPlayer(Peer peer)
    {
        this.peer = peer;
    }
}

public class Player : MonoBehaviour
{
    public TMP_Text nameText;
    public GameObject playerCapsule;
    public DataPlayer data;
    public NavMeshAgent agent;

    private void Update()
    {
        if (data.switchedMousePos)
        {
            data.switchedMousePos = false;
            agent.SetDestination(new Vector3(data.mousePos.x,0,data.mousePos.y));
        }
        data.position = new Vector2(transform.position.x, transform.position.z);
        data.rotation = transform.eulerAngles.y;
        data.speed = agent.velocity.magnitude;
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

}
