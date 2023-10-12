using ENet;
using System;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NetworkCore : MonoBehaviour
{
    public static NetworkCore instance;

    [Header("Network")]
    public bool isConnected = false;
    public bool attemptToConnect = false;
    
    public DataPlayer currentPlayer;
    public List<DataPlayer> allPlayers;
    public bool allPlayersUpdated = false;
    private ENet.Host m_enetHost = new ENet.Host();

    public float nextGameTick = 0f;
    public float gameTickInterval = 1.0f / 10.0f;
    public float now = 0f;

    public bool Connect(string addressString)
    {
        ENet.Address address = new ENet.Address();
        if (!address.SetHost(addressString))
            return false;

        address.Port = 14768;

        if(!m_enetHost.IsSet)
            m_enetHost.Create(1, 0);
        currentPlayer.peer = m_enetHost.Connect(address, 0);
        attemptToConnect = true;
        return true;
    }

    public void Disconnect()
    {
        if(currentPlayer.peer.IsSet)
            currentPlayer.peer.Disconnect(0);
        if(m_enetHost.IsSet)
            m_enetHost.Flush();
    }

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

        currentPlayer.switchedMousePos = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!ENet.Library.Initialize())
            throw new Exception("Failed to initialize ENet");

        DontDestroyOnLoad(this.gameObject);
    }
    private void OnApplicationQuit()
    {
        Disconnect();
        ENet.Library.Deinitialize();
    }

    // Update is called once per frame
    private void Update()
    {
        now += Time.deltaTime;
        
        if (now >= nextGameTick)
        {
            GameTick(now - nextGameTick);

            nextGameTick += gameTickInterval;
        }
    }

    private void GameTick(float elapsedTime)
    {
        if (!currentPlayer.peer.IsSet)
            return;

        //Envoie de la pose de la souris au serveur du joueur
        if (currentPlayer.switchedMousePos) {
            currentPlayer.switchedMousePos = false;
            GeneriqueOpCode packet_to_send = new C_PlayerMousePos(currentPlayer.id, currentPlayer.mousePos);
            Packet packet = build_packet(ref packet_to_send, PacketFlags.None);
            currentPlayer.peer.Send(0, ref packet);
            packet.Dispose();
        }

    }

    void FixedUpdate()
    {
        if (!currentPlayer.peer.IsSet)
            return;

        ENet.Event evt = new ENet.Event();
        if (m_enetHost.Service(0, out evt) > 0)
        {
            do
            {
                switch (evt.Type)
                {
                    case ENet.EventType.None:
                        Debug.Log("ERROR EVENT NONE");
                        break;

                    case ENet.EventType.Connect:
                        Debug.Log("Connect");
                        isConnected = true;
                        attemptToConnect = false;

                        //Init player to server with id and name
                        GeneriqueOpCode packet_to_send = new C_PlayerConnexion(currentPlayer.id ,currentPlayer.name);
                        Packet packet = build_packet(ref packet_to_send, PacketFlags.Reliable);
                        currentPlayer.peer.Send(0, ref packet);
                        packet.Dispose();

                        break;

                    case ENet.EventType.Disconnect:
                        Debug.Log("Disconnect");
                        isConnected = false;
                        attemptToConnect = false;
                        break;

                    case ENet.EventType.Receive:
                        byte[] dataPacket = new byte[evt.Packet.Length];
                        evt.Packet.CopyTo(dataPacket);

                        handle_message(dataPacket, evt);

                        evt.Packet.Dispose();
                        break;

                    case ENet.EventType.Timeout:
                        isConnected = false;
                        attemptToConnect = false;
                        Debug.Log("Timeout");
                        break;
                }
            }
            while (m_enetHost.CheckEvents(out evt) > 0);
        }
    }

    private void handle_message(byte[] dataPacket, ENet.Event evt)
    {
        int offset = 0;
        EnetOpCode.OpCode opcode = (EnetOpCode.OpCode)Unserialize_i32(ref dataPacket, ref offset);

        switch (opcode)
        {
            case EnetOpCode.OpCode.S_PlayersPosition:
                {
                    S_PlayerPosition playersPosition = new S_PlayerPosition();
                    playersPosition.Unserialize(ref dataPacket, offset);

                    foreach (DataPlayer dataPlayer in playersPosition.players)
                    {
                        DataPlayer player = allPlayers.Find(x => x.id == dataPlayer.id);

                        allPlayers.Remove(player);
                        player.position = dataPlayer.position;
                        player.rotation = dataPlayer.rotation;
                        player.speed = dataPlayer.speed;
                        allPlayers.Add(player);

                        if (currentPlayer.id == dataPlayer.id)
                        {
                            currentPlayer.position = player.position;
                        }
                    }
                    allPlayersUpdated = true;
                    break;
                }
            case EnetOpCode.OpCode.S_PlayerList:
                {
                    S_PlayerList playerList = new S_PlayerList();
                    playerList.Unserialize(ref dataPacket, offset);

                    allPlayers.Clear();
                    foreach (DataPlayer player in playerList.players)
                    {
                        allPlayers.Add(player);
                    }
                    allPlayersUpdated = true;
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
