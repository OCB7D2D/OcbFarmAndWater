using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BoundHelper
{

    // public readonly ClientInfo ClientInfo;

    //    private Dictionary<Vector3i, >
    /*
    */

    private Vector3 position = Vector3i.zero;
    private Vector3 scale = Vector3.zero;
    private Color color = Color.clear;

    public bool IsDirty = false;

    private void SetAndMakeDirty<T>(ref T var, T value)
    {
        var = value;
        IsDirty = true;
    }

    public void Update(Vector3 position, Vector3 scale, Color color)
    {
        this.position = position;
        this.scale = scale;
        this.color = color;
        IsDirty = true;
    }

    public Vector3 Position { get => position; set => SetAndMakeDirty(ref position, value); }
    public Vector3 Size { get => scale; set => SetAndMakeDirty(ref scale, value); }
    public Color Color { get => color; set => SetAndMakeDirty(ref color, value); }

    public BoundHelper(Vector3 position, Vector3 scale, Color color)
    {
        this.position = position;
        this.scale = scale;
        this.color = color;
    }

    public NetPackage GetNetPackage(Vector3i helper)
    {
        return NetPackageManager
            .GetPackage<NetPkgBoundHelperToClient>()
            .Setup(helper, position, scale, color);
    }
}
