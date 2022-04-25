// Decompiled with JetBrains decompiler
// Type: NetPackageItemReload
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F42CB780-236D-4D28-93FD-140D4E7F9F63
// Assembly location: G:\steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class NetPkgCustomInfo : NetPackage
{

    public static Vector3i LastPosition = Vector3i.zero;
    public static string LastText = string.Empty;
    public static ulong LastTick = 0;
    public static ulong LastAsk = 0;

    private Vector3i Position = Vector3i.zero;
    private string Text = string.Empty;

    public NetPkgCustomInfo Setup(Vector3i position, string text = null)
    {
        Position = position == null ? Vector3i.zero : position;
        Text = text == null ? string.Empty : text;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        Position = new Vector3i(
            _br.ReadInt32(),
            _br.ReadInt32(),
            _br.ReadInt32());
        Text = _br.ReadString();
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(Position.x);
        _bw.Write(Position.y);
        _bw.Write(Position.z);
        _bw.Write(Text);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null) return;
        if (_world.IsRemote())
        {
            Log.Out("Got answer from server {0}", Text);
            LastPosition = Position; LastText = Text;
            LastAsk = LastTick = GameTimer.Instance.ticks;
        }
        else
        {
            BlockValue block = _world.GetBlock(Position);
            string info = "SERVER ERROR";
            if (block.type != BlockValue.Air.type)
            {
                info = block.Block.GetCustomDescription(Position, block);
            }
            Log.Out("Sending to client {0}", info);
            Sender.SendPackage(NetPackageManager
                .GetPackage<NetPkgCustomInfo>()
                    .Setup(Position, info));
        }
    }

    public override int GetLength() => 24 + Text.Length;
}
