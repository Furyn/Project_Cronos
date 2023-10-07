using UnityEngine;

public class C_PlayerMousePos : GeneriqueOpCode
{
    public int id;
    public Vector2 mousePos;

    public C_PlayerMousePos()
    {
        opCode = EnetOpCode.OpCode.C_PlayerMousePos;
    }

    public override void Serialize(ref byte[] byteArray)
    {
        NetworkCore_S.Serialize_i32(ref byteArray, id);
        NetworkCore_S.Serialize_float(ref byteArray, mousePos.x);
        NetworkCore_S.Serialize_float(ref byteArray, mousePos.y);
    }

    public override void Unserialize(ref byte[] byteArray, int offset)
    {
        this.id = NetworkCore_S.Unserialize_i32(ref byteArray, ref offset);
        this.mousePos.x = NetworkCore_S.Unserialize_float(ref byteArray, ref offset);
        this.mousePos.y = NetworkCore_S.Unserialize_float(ref byteArray, ref offset);
    }
}
