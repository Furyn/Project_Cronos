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

    [HideInInspector]
    public bool isConnected = false;
    [HideInInspector]
    public bool attemptToConnect = false;

    [Header("Data")]
    public DataPlayer currentPlayer;
    public List<DataPlayer> allPlayers;
    [HideInInspector]
    public bool allPlayersUpdated = false;
    private ENet.Host m_enetHost = new ENet.Host();

    [Header("Network")]

    public float now = 0f;
    public float nextGameTick = 0f;
    public float gameTickInterval = 1.0f / 10.0f;
    public float networkTickInterval = 1.0f / 10.0f;

    [Header("Interpolation buffer")]
    public float interpolationTime = 0f;
    public int targetInterpolationBufferSize = 5;
    [SerializeField]
    private List<S_PlayerPosition> interpolationBuffer = new List<S_PlayerPosition>();

    public bool Connect(string addressString)
    {
        ENet.Address address = new ENet.Address();
        if (!address.SetHost(addressString))
            return false;

        address.Port = 14768;

        if (!m_enetHost.IsSet)
            m_enetHost.Create(1, 0);
        currentPlayer.peer = m_enetHost.Connect(address, 0);
        attemptToConnect = true;
        return true;
    }

    public void Disconnect()
    {
        if (currentPlayer.peer.IsSet)
            currentPlayer.peer.Disconnect(0);
        if (m_enetHost.IsSet)
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

        InterpolatePosition();
    }

    private void GameTick(float elapsedTime)
    {
        if (!currentPlayer.peer.IsSet)
            return;

        //Envoie de la pose de la souris au serveur du joueur
        if (currentPlayer.switchedMousePos)
        {
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
                        GeneriqueOpCode packet_to_send = new C_PlayerConnexion(currentPlayer.id, currentPlayer.name);
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

                    if (interpolationBuffer.Count == 0)
                    {
                        // Position queue vide ? (soit début de game, soit grosse perte de paquet)
                        // On initialise notre position queue avec le même paquet plusieurs fois
                        int tickIndex = playersPosition.tickIndex;
                        for (int i = 0; i < targetInterpolationBufferSize; ++i)
                        {
                            S_PlayerPosition copiePlayersPosition = new S_PlayerPosition();
                            copiePlayersPosition.players = playersPosition.players;
                            // Correction du tick index
                            copiePlayersPosition.tickIndex = (tickIndex - (targetInterpolationBufferSize - i - 1));
                            interpolationBuffer.Add(copiePlayersPosition);
                        }
                    }
                    else
                        interpolationBuffer.Add(playersPosition);
                    break;
                }
            case EnetOpCode.OpCode.S_PlayerList:
                {
                    S_PlayerList playerList = new S_PlayerList();
                    playerList.Unserialize(ref dataPacket, offset);

                    foreach (DataPlayer player in playerList.players)
                    {
                        bool hasPlayer = allPlayers.Any(x => x.id == player.id);
                        if (!hasPlayer)
                            allPlayers.Add(player);
                    }
                    allPlayersUpdated = true;
                    break;
                }
            default:
                break;
        }

    }

    void InterpolatePosition()
    {
        if (interpolationBuffer.Count < 2)
            return;

        S_PlayerPosition from = interpolationBuffer[0];
        S_PlayerPosition to = interpolationBuffer[1];

        int currentTick = from.tickIndex;
        int packetDiff = to.tickIndex - from.tickIndex;

        float interpolationIncr = Time.deltaTime / networkTickInterval;
        interpolationIncr /= packetDiff;

        // Si on accumule trop de positions, on accélère légèrement le facteur d'interpolation
        if (interpolationBuffer.Count >= targetInterpolationBufferSize)
            interpolationIncr *= 1f + 0.2f * (interpolationBuffer.Count - targetInterpolationBufferSize);
        else
        {
            float temp = 1f - 0.2f * (targetInterpolationBufferSize - interpolationBuffer.Count);
            if (temp > 0)
                interpolationIncr *= temp;
            else
                interpolationIncr *= 0;
        }

        foreach (DataPlayer fromPlayer in from.players)
        {
            bool hasPlayer = allPlayers.Any(x => x.id == fromPlayer.id);
            if (!hasPlayer)
                continue;
            DataPlayer player = allPlayers.Find(x => x.id == fromPlayer.id);

            hasPlayer = to.players.Any(x => x.id == fromPlayer.id);
            if (!hasPlayer)
                continue; // Le joueur n'existe plus dans le snapshot de destination, on y touche pas

            DataPlayer toPlayer = to.players.ToList().Find(x => x.id == fromPlayer.id);

            player.position = Vector2.Lerp(fromPlayer.position, toPlayer.position, interpolationTime);
            player.rotation = Mathf.Lerp(fromPlayer.rotation, toPlayer.rotation, interpolationTime);
            player.speed = Mathf.Lerp(fromPlayer.speed, toPlayer.speed, interpolationTime);

            if (player.id == currentPlayer.id)
            {
                currentPlayer.position = player.position;
                currentPlayer.rotation = player.rotation;
                currentPlayer.speed = player.speed;
            }
        }

        interpolationTime += interpolationIncr;
        if (interpolationTime >= 1f)
        {
            interpolationBuffer.RemoveAt(0);
            interpolationTime -= 1f;
        }
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
