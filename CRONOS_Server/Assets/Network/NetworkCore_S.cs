using ENet;
using System;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public struct ServerData
{
    public ENet.Host host;
    public List<DataPlayer> players;
    public bool gameStarted;
};

public class NetworkCore_S : MonoBehaviour
{
    public static NetworkCore_S instance;
    public ServerData serverData;

    [Header("Network")]
    public int maxClients = 10;

    public float nextGameTick = 0f;
    public float gameTickInterval = 1.0f / 30.0f;
    public float nextNetworkTick = 0f;
    public float networkTickInterval = 1.0f / 10.0f;
    public float now = 0f;

    [Header("Rendu")]
    public bool UpdateAllPlayers = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!ENet.Library.Initialize())
            throw new Exception("Failed to initialize ENet");

        ENet.Address address = new Address();
        address.Port = 14768;
        serverData.host = new Host();
        serverData.host.Create(address, maxClients);

        serverData.players = new List<DataPlayer>();

        if (!serverData.host.IsSet)
            throw new Exception("Failed to create ENet host");

        DontDestroyOnLoad(this.gameObject);

    }

    private void OnApplicationQuit()
    {
        serverData.host.Dispose();
        ENet.Library.Deinitialize();
    }

    private void Update()
    {
        now += Time.deltaTime;

        if (now >= nextGameTick)
        {
            GameTick(now - nextGameTick + gameTickInterval);

            nextGameTick += gameTickInterval;
        }

        if (now >= nextNetworkTick)
        {
            network_tick(now - nextNetworkTick + networkTickInterval);

            nextNetworkTick += networkTickInterval;
        }
    }

    private void GameTick(float elapsedTime)
    {
        foreach (DataPlayer player in serverData.players)
        {
            player.UpdatePhysics(elapsedTime);
        }

    }

    private void network_tick(float elapsedTime)
    {
        // On envoie la position de tous les joueurs à chaque tick réseaux
        GeneriqueOpCode packet_to_send = new S_PlayerPosition(serverData.players);
        foreach (DataPlayer player in serverData.players)
        {
            Packet packet = build_packet(ref packet_to_send, PacketFlags.None);
            player.peer.Send(0, ref packet);
            packet.Dispose();
        }
    }

    void FixedUpdate()
    {

        ENet.Event evt = new ENet.Event();
        if (serverData.host.Service(0, out evt) > 0)
        {
            do
            {
                switch (evt.Type)
                {
                    case ENet.EventType.None:
                        Debug.Log("ERROR EVENT NONE");
                        break;

                    case ENet.EventType.Connect:
                        serverData.players.Add(new DataPlayer(evt.Peer));
                        break;

                    case ENet.EventType.Disconnect:
                        DisconnectPeer(evt.Peer);
                        break;

                    case ENet.EventType.Receive:
                        byte[] dataPacket = new byte[evt.Packet.Length];
                        evt.Packet.CopyTo(dataPacket);
                        handle_message(dataPacket, evt);
                        evt.Packet.Dispose();
                        break;

                    case ENet.EventType.Timeout:
                        Debug.Log("Timeout");
                        DisconnectPeer(evt.Peer);
                        break;
                }
            }
            while (serverData.host.CheckEvents(out evt) > 0);
        }
    }

    private void DisconnectPeer(Peer peer)
    {
        DataPlayer playerToRemove = new DataPlayer();
        foreach (DataPlayer item in serverData.players)
        {
            if (item.peer.ID == peer.ID)
            {
                playerToRemove = item;
            }
        }
        if (playerToRemove.peer.IsSet)
        {
            Debug.Log(playerToRemove.name + " disconnected");
            serverData.players.Remove(playerToRemove);
        }
        SendPlayerlistPacket(ref serverData);
    }

    private void handle_message(byte[] dataPacket, ENet.Event evt)
    {
        int offset = 0;
        EnetOpCode.OpCode opcode = (EnetOpCode.OpCode)Unserialize_i32(ref dataPacket, ref offset);

        switch (opcode)
        {
            case EnetOpCode.OpCode.C_PlayerConnexion:
                {
                    C_PlayerConnexion playerConnexion = new C_PlayerConnexion();
                    playerConnexion.Unserialize(ref dataPacket, offset);

                    DataPlayer findPlayer = serverData.players.Find(x => x.peer.ID == evt.Peer.ID);
                    serverData.players.Remove(findPlayer);
                    findPlayer.name = playerConnexion.name;
                    findPlayer.id = playerConnexion.id;
                    serverData.players.Add(findPlayer);

                    Debug.Log(findPlayer.name + " connected");

                    SendPlayerlistPacket(ref serverData);
                    break;
                }
            case EnetOpCode.OpCode.C_PlayerMousePos:
                {
                    C_PlayerMousePos playerMousePos = new C_PlayerMousePos();
                    playerMousePos.Unserialize(ref dataPacket, offset);

                    DataPlayer findPlayer = serverData.players.Find(x => x.peer.ID == evt.Peer.ID);
                    serverData.players.Remove(findPlayer);
                    findPlayer.mousePos.x = playerMousePos.mousePos.x;
                    findPlayer.mousePos.y = playerMousePos.mousePos.y;
                    serverData.players.Add(findPlayer);
                    break;
                }
            default:
                break;
        }

    }

    public static String DisplayBinary(Byte[] data)
    {
        return string.Join(" ", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
    }

    #region Int Serialisation
    public static void Serialize_i32(ref byte[] byteArray, Int32 value)
    {
        int offset = byteArray.Length;
        Array.Resize(ref byteArray, offset + sizeof(Int32));
        Serialize_i32(ref byteArray, offset, value);
    }

    public static void Serialize_i32(ref byte[] byteArray, int offset, Int32 value)
    {
        //htonl;
        value = IPAddress.HostToNetworkOrder(value);

        byte[] valueByte = BitConverter.GetBytes(value);

        for (int i = 0; i < valueByte.Length; i++)
        {
            byteArray[offset + i] = valueByte[i];
        }
    }

    public static Int32 Unserialize_i32(ref byte[] byteArray, ref int offset)
    {
        Int32 value;

        byte[] intByte = new byte[sizeof(Int32)];

        for (int i = offset; i < offset + sizeof(Int32); i++)
        {
            intByte[i - offset] = byteArray[i];
        }

        value = BitConverter.ToInt32(intByte);

        //ntohl
        value = IPAddress.NetworkToHostOrder(value);

        offset += sizeof(Int32);

        return value;
    }
    #endregion

    #region Float Serialisation
    public static void Serialize_float(ref byte[] byteArray, float value)
    {
        int offset = byteArray.Length;
        Array.Resize(ref byteArray, offset + sizeof(float));
        Serialize_float(ref byteArray, offset, value);
    }

    public static void Serialize_float(ref byte[] byteArray, int offset, float value)
    {
        byte[] valueByte = BitConverter.GetBytes(value);

        int x = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(valueByte));

        valueByte = BitConverter.GetBytes(x);

        for (int i = 0; i < valueByte.Length; i++)
        {
            byteArray[offset + i] = valueByte[i];
        }
    }

    public static float Unserialize_float(ref byte[] byteArray, ref int offset)
    {
        float value;

        byte[] intByte = new byte[sizeof(float)];

        for (int i = offset; i < offset + sizeof(float); i++)
        {
            intByte[i - offset] = byteArray[i];
        }

        int x = BitConverter.ToInt32(intByte);
        x = IPAddress.NetworkToHostOrder(x);
        byte[] valueByte = BitConverter.GetBytes(x);

        value = BitConverter.ToSingle(valueByte);

        offset += sizeof(float);

        return value;
    }

    #endregion

    #region String Serialisation
    public static void Serialize_str(ref byte[] byteArray, string value)
    {
        int offset = byteArray.Length;
        Serialize_str(ref byteArray, offset, value);
    }

    public static void Serialize_str(ref byte[] byteArray, int offset, string value)
    {
        byte[] valueByte = Encoding.UTF8.GetBytes(value);

        Array.Resize(ref byteArray, offset + sizeof(Int32) + valueByte.Length);
        Serialize_i32(ref byteArray, offset, valueByte.Length);
        offset += sizeof(int);

        for (int i = 0; i < valueByte.Length; i++)
        {
            byteArray[offset + i] = valueByte[i];
        }
    }

    public static string Unserialize_str(ref byte[] byteArray, ref int offset)
    {
        int length = Unserialize_i32(ref byteArray, ref offset);

        byte[] strByte = new byte[length];

        for (int i = offset; i < offset + length; i++)
        {
            strByte[i - offset] = byteArray[i];
        }

        offset += length;
        return Encoding.UTF8.GetString(strByte);
    }
    #endregion

    private void SendPlayerlistPacket(ref ServerData serverData)
    {
        GeneriqueOpCode packet_to_send = new S_PlayerList(serverData.players);
        foreach (DataPlayer player in serverData.players)
        {
            Packet packet = build_packet(ref packet_to_send, PacketFlags.Reliable);
            player.peer.Send(0, ref packet);
            packet.Dispose();
        }
        UpdateAllPlayers = true;
    }

    public static Packet build_packet(ref GeneriqueOpCode packetOpCode, PacketFlags flags)
    {
        // On sérialise l'opcode puis le contenu du packet dans un byte[]
        byte[] byteArray = new byte[0];

        Serialize_i32(ref byteArray, (int)(packetOpCode.opCode));
        packetOpCode.Serialize(ref byteArray);

        Packet packet = default(Packet);
        packet.Create(byteArray, flags);

        return packet;
    }

}
