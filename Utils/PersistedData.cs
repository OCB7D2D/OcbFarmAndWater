using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class PersistedData<T> : SingletonInstance<T> where T : new()
{

    // Some basic info we need to store data
    public abstract string GetBackupPath();
    public abstract string GetStoragePath();
    public abstract string GetThreadKey();

    // Implement your write functionality
    public abstract void Write(BinaryWriter bw);

    // Implement your read functionality
    public abstract void Read(BinaryReader br);

    // Temporary thread object when running
    private ThreadManager.ThreadInfo ThreadInfo;

    public bool IsSaveThreadRunning()
    {
        return !(ThreadInfo == null ||
            ThreadInfo.HasTerminated());
    }

    public void SaveThreaded()
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (!GameManager.Instance.gameStateManager.IsGameStarted()) return;
        if (IsSaveThreadRunning()) StartSaveThread();
    }

    private void StartSaveThread()
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (ThreadInfo != null && ThreadManager.ActiveThreads.ContainsKey(GetThreadKey())) return;
        PooledExpandableMemoryStream expandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
        using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
        {
            pooledBinaryWriter.SetBaseStream(expandableMemoryStream);
            this.Write(pooledBinaryWriter);
        }
        var manager = new ThreadManager.ThreadFunctionLoopDelegate(RunSaveThread);
        ThreadInfo = ThreadManager.StartThread(GetThreadKey(), null,
            manager, null, _parameter: expandableMemoryStream);
    }

    private int RunSaveThread(ThreadManager.ThreadInfo _threadInfo)
    {
        PooledExpandableMemoryStream parameter =
            (PooledExpandableMemoryStream)_threadInfo.parameter;
        string fname = GetStoragePath();
        if (File.Exists(fname)) File.Copy(
            fname, GetBackupPath(), true);
        parameter.Position = 0L;
        StreamUtils.WriteStreamToFile(parameter, fname);
        MemoryPools.poolMemoryStream.FreeSync(parameter);
        return -1;
    }

    protected virtual void CleanupInstance()
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            Log.Out("Saving on close {0}", GetStoragePath());
            StartSaveThread();
            // ToDo: Error if we can't safe!?
            // if (ThreadInfo == null) return;
            ThreadInfo.WaitForEnd();
            ThreadInfo = null;
        }
    }


    public void LoadPersistedData()
    {
        string storage = GetStoragePath();
        // if (!File.Exists(storage)) return;
        try
        {
            using (FileStream fileStream = File.OpenRead(storage))
            {
                using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                {
                    pooledBinaryReader.SetBaseStream(fileStream);
                    this.Read(pooledBinaryReader);
                }
            }
        }
        catch (Exception)
        {
            string backup = GetBackupPath();
            if (!File.Exists(backup)) return;
            using (FileStream fileStream = File.OpenRead(backup))
            {
                using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                {
                    pooledBinaryReader.SetBaseStream(fileStream);
                    this.Read(pooledBinaryReader);
                }
            }
        }
    }

}
