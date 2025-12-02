using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ObjectManagement;

/// <summary>
/// 这个类提供了保存和加载的方法
/// </summary>
public class PersistentStorage : MonoBehaviour
{
    private string path;

    private void Awake()
    {
        path = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    public void Save(PersistableObject o, int version)
    {
        // FileMode.Create是如果不存在就创建，如果存在就直接覆盖
        using var writer = new BinaryWriter(File.Open(path, FileMode.Create));
        writer.Write(-version);
        o.Save(new GameDataWriter(writer));
    }

    public void Load(PersistableObject o)
    {
//        Debug.Log("public void Load(PersistableObject o)");
        using var reader = new BinaryReader(File.Open(path, FileMode.Open));
        o.Load(new GameDataReader(reader, -reader.ReadInt32()));
    }
}
