using UnityEngine;

public class C_PlayerMousePos : GeneriqueOpCode
{
    public int id;
    public Vector2 mousePos;

    public C_PlayerMousePos(int id, Vector2 newMousePos)
    {
        opCode = EnetOpCode.OpCode.C_PlayerMousePos;
        this.mousePos = newMousePos;
        this.id = id;
    }

    public override void Serialize(ref byte[] byteArray)
    {
        NetworkCore.Serialize_i32(ref byteArray, id);
        NetworkCore.Serialize_float(ref byteArray, mousePos.x);
        NetworkCore.Serialize_float(ref byteArray, mousePos.y);
    }

    public override void Unserialize(ref byte[] byteArray, int offset)
    {
        this.id = NetworkCore.Unserialize_i32(ref byteArray, ref offset);
        this.mousePos.x = NetworkCore.Unserialize_float(ref byteArray, ref offset);
        this.mousePos.y = NetworkCore.Unserialize_float(ref byteArray, ref offset);
    }
}
