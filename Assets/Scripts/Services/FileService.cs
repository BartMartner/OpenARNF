using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileService
{
    public string persistentDataPath;

    public virtual void Initialize()
    {
        persistentDataPath = Application.persistentDataPath;
    }

    public virtual void OnDestroy() { }

    public virtual string Read(string key)
    {
        var path = persistentDataPath + "/" + key + ".dat";
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        else
        {
            return null;
        }
    }

    public virtual void Write(string key, string value)
    {
        //Debug.Log("FileService.Write Called");
        File.WriteAllText(persistentDataPath + "/" + key + ".dat", value);
    }

    public virtual void Delete(string key)
    {
        var path = persistentDataPath + "/" + key + ".dat";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
