using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerPosition : GeneriqueOpCode
{
    public DataPlayer[] players;

    public S_PlayerPosition()
    {
        opCode = EnetOpCode.OpCode.S_PlayersPosition;
    }

    public override void Serialize(ref byte[] byteArray)
    {
        NetworkCore.Serialize_i32(ref byteArray, players.Length);

        foreach (DataPlayer player in players)
        {
            NetworkCore.Serialize_i32(ref byteArray, player.id);
            NetworkCore.Serialize_float(ref byteArray, player.position.x);
            NetworkCore.Serialize_float(ref byteArray, player.position.y);
        }
    }

    public override void Unserialize(ref byte[] byteArray, int offset)
    {
        int nb_player = NetworkCore.Unserialize_i32(ref byteArray, ref offset);
        players = new DataPlayer[nb_player];

        for (int i = 0; i < players.Length; i++)
        {
            players[i].id = NetworkCore.Unserialize_i32(ref byteArray, ref offset);
            players[i].position.x = NetworkCore.Unserialize_float(ref byteArray, ref offset);
            players[i].position.y = NetworkCore.Unserialize_float(ref byteArray, ref offset);
        }
    }
}
