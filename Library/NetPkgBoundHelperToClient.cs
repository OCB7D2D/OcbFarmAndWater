// Decompiled with JetBrains decompiler
// Type: NetPackageItemReload
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F42CB780-236D-4D28-93FD-140D4E7F9F63
// Assembly location: G:\steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class NetPkgBoundHelperToClient : NetPackage
{
    private Vector3i Helper = Vector3i.zero;
    private Vector3 Position = Vector3.zero;
    private Vector3 Scale = Vector3.one;
    private Color Color = Color.clear;

    public NetPkgBoundHelperToClient Setup(Vector3i helper, Vector3 position, Vector3 scale, Color color)
    {
        Helper = helper;
        Position = position;
        Scale = scale;
        Color = color;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        Helper = new Vector3i(
            _br.ReadInt32(),
            _br.ReadInt32(),
            _br.ReadInt32());
        Position = new Vector3(
            _br.ReadSingle(),
            _br.ReadSingle(),
            _br.ReadSingle());
        Scale = new Vector3(
            _br.ReadSingle(),
            _br.ReadSingle(),
            _br.ReadSingle());
        Color = new Color(
            _br.ReadSingle(),
            _br.ReadSingle(),
            _br.ReadSingle(),
            _br.ReadSingle());
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(Helper.x);
        _bw.Write(Helper.y);
        _bw.Write(Helper.z);
        _bw.Write(Position.x);
        _bw.Write(Position.y);
        _bw.Write(Position.z);
        _bw.Write(Scale.x);
        _bw.Write(Scale.y);
        _bw.Write(Scale.z);
        _bw.Write(Color.r);
        _bw.Write(Color.g);
        _bw.Write(Color.b);
        _bw.Write(Color.a);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null) return;
        if (_world.IsRemote())
        {
            Log.Out("Process local move {0} {1} {2} {3}", Helper, Position, Scale, Color);
            if (LandClaimBoundsHelper.GetBoundsHelper(
                Helper.ToVector3()) is Transform helper)
            {
                helper.localScale = Scale;
                helper.localPosition = Position - Origin.position;
                foreach (Renderer renderer in helper.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.material == null) continue;
                    renderer.material.SetColor("_Color", Color);
                }
            }
        }
        else
        {
            Log.Out("Processing remote world package but is local?");
        }
    }

    public override int GetLength() => 72;
}
