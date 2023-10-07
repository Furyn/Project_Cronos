using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerList : GeneriqueOpCode
{
    public DataPlayer[] players;

    public S_PlayerList()
    {
        opCode = EnetOpCode.OpCode.S_PlayerList;
    }

    public override void Serialize(ref byte[] byteArray)
    {
        NetworkCore.Serialize_i32(ref byteArray, players.Length);

        foreach (DataPlayer player in players)
        {
            NetworkCore.Serialize_i32(ref byteArray, player.id);
            NetworkCore.Serialize_str(ref byteArray, player.name);
        }
    }

    public override void Unserialize(ref byte[] byteArray, int offset)
    {
        int nb_player = NetworkCore.Unserialize_i32(ref byteArray, ref offset);
        players = new DataPlayer[nb_player];

        for (int i = 0; i < players.Length; i++)
        {
            players[i].id = NetworkCore.Unserialize_i32(ref byteArray, ref offset);
            players[i].name = NetworkCore.Unserialize_str(ref byteArray, ref offset);
        }
    }
}
