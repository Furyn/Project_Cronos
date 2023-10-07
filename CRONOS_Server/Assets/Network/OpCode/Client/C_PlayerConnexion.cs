using UnityEngine;

public class C_PlayerConnexion : GeneriqueOpCode
{
    public int id;
    public string name;

    public C_PlayerConnexion()
    {
        opCode = EnetOpCode.OpCode.C_PlayerConnexion;
    }

    public C_PlayerConnexion(int id, string name)
    {
        opCode = EnetOpCode.OpCode.C_PlayerConnexion;
        this.name = name;
        this.id = id;
    }

    public override void Serialize(ref byte[] byteArray)
    {
        NetworkCore_S.Serialize_i32(ref byteArray, id);
        NetworkCore_S.Serialize_str(ref byteArray, name);
    }

    public override void Unserialize(ref byte[] byteArray, int offset)
    {
        this.id = NetworkCore_S.Unserialize_i32(ref byteArray, ref offset);
        this.name = NetworkCore_S.Unserialize_str(ref byteArray, ref offset);
    }
}
