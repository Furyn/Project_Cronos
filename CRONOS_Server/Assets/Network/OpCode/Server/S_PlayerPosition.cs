using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerPosition : GeneriqueOpCode
{
    public DataPlayer[] players;
    public int tickIndex;

    public S_PlayerPosition(List<DataPlayer> serverPlayers, int networkTickIndex)
    {
        opCode = EnetOpCode.OpCode.S_PlayersPosition;
        tickIndex = networkTickIndex;
        players = new DataPlayer[serverPlayers.Count];
        for (int i = 0; i < serverPlayers.Count; i++)
        {
            DataPlayer player = serverPlayers[i];
            if (player.peer.IsSet && player.name != "") //< Est-ce que le slot est occupé par un joueur (et est-ce que ce joueur a bien envoyé son nom) ?
            {
                // Oui, rajoutons-le à la liste
                DataPlayer packetPlayer = new DataPlayer();
                packetPlayer.id = player.id;
                packetPlayer.position = player.position;
                packetPlayer.rotation = player.rotation;
                packetPlayer.speed = player.speed;
                players[i] = packetPlayer;
            }
        };
    }

    public override void Serialize(ref byte[] byteArray)
    {
        NetworkCore_S.Serialize_i32(ref byteArray, tickIndex);
        NetworkCore_S.Serialize_i32(ref byteArray, players.Length);

        foreach (DataPlayer player in players)
        {
            NetworkCore_S.Serialize_i32(ref byteArray, player.id);
            NetworkCore_S.Serialize_float(ref byteArray, player.position.x);
            NetworkCore_S.Serialize_float(ref byteArray, player.position.y);
            NetworkCore_S.Serialize_float(ref byteArray, player.rotation);
            NetworkCore_S.Serialize_float(ref byteArray, player.speed);
        }
    }

    public override void Unserialize(ref byte[] byteArray, int offset)
    {
        tickIndex = NetworkCore_S.Unserialize_i32(ref byteArray, ref offset);
        int nb_player = NetworkCore_S.Unserialize_i32(ref byteArray, ref offset);
        players = new DataPlayer[nb_player];

        for (int i = 0; i < players.Length; i++)
        {
            players[i].id = NetworkCore_S.Unserialize_i32(ref byteArray, ref offset);
            players[i].position.x = NetworkCore_S.Unserialize_float(ref byteArray, ref offset);
            players[i].position.y = NetworkCore_S.Unserialize_float(ref byteArray, ref offset);
            players[i].rotation = NetworkCore_S.Unserialize_float(ref byteArray, ref offset);
            players[i].speed = NetworkCore_S.Unserialize_float(ref byteArray, ref offset);
        }
    }
}
