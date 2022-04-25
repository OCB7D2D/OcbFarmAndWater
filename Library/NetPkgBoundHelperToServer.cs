// Decompiled with JetBrains decompiler
// Type: NetPackageItemReload
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F42CB780-236D-4D28-93FD-140D4E7F9F63
// Assembly location: G:\steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class NetPkgBoundHelperToServer : NetPackage
{

    private bool Register = false;
    private Vector3i Helper = Vector3i.zero;

    public NetPkgBoundHelperToServer Setup(Vector3i position, bool register)
    {
        Helper = position;
        Register = register;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        Helper = new Vector3i(
            _br.ReadInt32(),
            _br.ReadInt32(),
            _br.ReadInt32());
        Register = _br.ReadBoolean();
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(Helper.x);
        _bw.Write(Helper.y);
        _bw.Write(Helper.z);
        _bw.Write(Register);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null) return;
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            Log.Out("Process request remote - register client {0} from {1}", Register, Sender.entityId);
            if (Register) BoundHelperManager.Instance.AddListener(Sender, Helper);
            else BoundHelperManager.Instance.RemoveListener(Sender, Helper);
            // Maybe check if we can send some info right away before next update?
        }
        else
        {
            Log.Out("Process request local");
        }
    }

    public override int GetLength() => 28;
}
