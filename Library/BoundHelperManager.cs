using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class BoundHelperManager : SingletonInstance<BoundHelperManager>
{

    private Dictionary<ClientInfo, HashSet<Vector3i>> Clients
        = new Dictionary<ClientInfo, HashSet<Vector3i>>();

    private Dictionary<Vector3i, BoundHelper> Helpers
        = new Dictionary<Vector3i, BoundHelper>();

    public BoundHelperManager()
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            ConnectionManager.OnClientDisconnected += OnClientDisconnected;
        }

    private void OnClientDisconnected(ClientInfo client)
    {
        Clients.Remove(client);
    }

    public void AddListener(ClientInfo client, Vector3i position)
    {
        if (!(Clients.TryGetValue(client, out HashSet<Vector3i> listeners)))
        {
            Log.Out("New client registered {0}", client.entityId);
            listeners = new HashSet<Vector3i>();
            Clients.Add(client, listeners);
        }
            if (Helpers.TryGetValue(position, out BoundHelper bh))
                client.SendPackage(bh.GetNetPackage(position));
            listeners.Add(position);
        }

    public void RemoveListener(ClientInfo client, Vector3i position)
    {
        if (Clients.TryGetValue(client, out HashSet<Vector3i> listeners))
        {
            listeners.Remove(position);
        }
        else
        {
            Log.Error("Could not remove listener for unknown user");
        }
    }

    private void UpdateLocalTransform(Transform transform, Vector3 position, Vector3 scale, Color color)
    {
        if (transform == null) return;
        Log.Out("Updating the local scale of the bound now");
        transform.localScale = scale;
        transform.localPosition = position - Origin.position;
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            if (renderer.material == null) continue;
            renderer.material.SetColor("_Color", color);
        }
    }

    private void CreateOrUpdateHelper(Vector3i helper, Vector3 position, Vector3 scale, Color color)
    {
        if (LandClaimBoundsHelper.GetBoundsHelper(helper.ToVector3()) is Transform transform)
        {
            UpdateLocalTransform(transform, position, scale, color);
        }
    }

    public void AdjustHelper(Vector3i helper, Vector3 position, Vector3 scale, Color color)
    {
        // Pure client side implementation (no server part available)
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            Log.Error("AdjustHelper must not be called client side!");
            //SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
            //    NetPackageManager.GetPackage<NetPackageBoundHelperToServer>()
            //        .Setup(helper, true));
        }
        // All the info is directly available
        else if (Helpers.TryGetValue(helper, out BoundHelper bh))
        {
            Log.Out("Adjust helper now");
            // Create the network package once
            var package = bh.GetNetPackage(helper);
            bh.Update(position, scale, color);
            // Process all connected clients
            foreach (var kv in Clients)
            {
                // Send if they are interested
                if (kv.Value.Contains(helper))
                    kv.Key.SendPackage(package);
            }
        }
        else
        {
            Log.Error("Bound Helper to adjust was not found {0}", helper);
        }
        if (!GameManager.IsDedicatedServer)
        {
            CreateOrUpdateHelper(helper, position, scale, color);
        }
    }

    public void AddHelper(Vector3i helper, Vector3 position, Vector3 scale, Color color)
    {
        Log.Warning("AddHelper");
        // Pure client side implementation (no server part available)
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            Log.Out("Sending Interest to server");
            // Request further updates from the server (on changes)
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPkgBoundHelperToServer>()
                    .Setup(helper, true));
        }
        // We are playing the server side
        else
        {
            if (Helpers.ContainsKey(helper))
            {
                Log.Warning("Bound Helper already exists {0}", helper);
            }
            else
            {
                Log.Out("Add a helper and position it");
                BoundHelper bh = new BoundHelper(position, scale, color);
                // Create the network package once
                var package = bh.GetNetPackage(helper);
                // Process all connected clients
                foreach (var kv in Clients)
                {
                    // Send if they are interested
                    if (kv.Value.Contains(helper))
                        kv.Key.SendPackage(package);
                }
                Helpers.Add(helper, bh);
            }
        }
        if (!GameManager.IsDedicatedServer)
        {
            CreateOrUpdateHelper(helper, position, scale, color);
        }
    }

    public void RemoveHelper(Vector3i helper)
    {
        // Pure client side implementation (no server part available)
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            // Request no further updates from the server
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPkgBoundHelperToServer>()
                    .Setup(helper, false));
        }
        // We are playing the server side
        else
        {
            if (!Helpers.Remove(helper))
            {
                Log.Warning("Bound Helper not known {0}", helper);
            }
        }
        if (!GameManager.IsDedicatedServer)
        {
            LandClaimBoundsHelper.RemoveBoundsHelper(helper.ToVector3());
        }
    }

    public void Tick()
    {
        if (!GameManager.IsDedicatedServer) return;
        foreach (var kv in Clients)
        {

        }
    }

}
