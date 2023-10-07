using System;

namespace EnetOpCode
{
    public enum OpCode : Int32
    {
        C_PlayerConnexion = 0,
        C_PlayerMousePos = 1,
        S_PlayerList = 100,
        S_PlayersPosition = 101,
    }
}


public abstract class GeneriqueOpCode
{
    public EnetOpCode.OpCode opCode;
    public abstract void Serialize(ref byte[] byteArray);
    public abstract void Unserialize(ref byte[] byteArray, Int32 offset);
}
