using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerList : GeneriqueOpCode
{
    public DataPlayer[] players;

    public S_PlayerList(List<DataPlayer> serverPlayers)
    {
        opCode = EnetOpCode.OpCode.S_PlayerList;
        players = new DataPlayer[serverPlayers.Count];
        for (int i = 0; i < serverPlayers.Count; i++)
        {
            DataPlayer player = serverPlayers[i];
            if (player.peer.IsSet && player.name != "") //< Est-ce que le slot est occupé par un joueur (et est-ce que ce joueur a bien envoyé son nom) ?
            {
                // Oui, rajoutons-le à la liste
                DataPlayer packetPlayer = new DataPlayer();
                packetPlayer.id = player.id;
                packetPlayer.name = player.name;
                players[i] = packetPlayer;
            }
        };
    }

    public override void Serialize(ref byte[] byteArray)
    {
        NetworkCore_S.Serialize_i32(ref byteArray, players.Length);

        foreach (DataPlayer player in players)
        {
            NetworkCore_S.Serialize_i32(ref byteArray, player.id);
            NetworkCore_S.Serialize_str(ref byteArray, player.name);
        }
    }

    public override void Unserialize(ref byte[] byteArray, int offset)
    {
        int nb_player = NetworkCore_S.Unserialize_i32(ref byteArray, ref offset);
        players = new DataPlayer[nb_player];

        for (int i = 0; i < players.Length; i++)
        {
            players[i].id = NetworkCore_S.Unserialize_i32(ref byteArray, ref offset);
            players[i].name = NetworkCore_S.Unserialize_str(ref byteArray, ref offset);
        }
    }
}
