using System;
using System.Collections.Generic;

[Serializable]
public class ContentPackManifest
{
    public string contentId;
    public string contentName;
    public string version;
    public List<FileData> scenarios= new List<FileData>();

    [Serializable]
    public class FileData
    {
        public string name;
        public string file;

        public FileData(string file, string name)
        {
            this.file = file;
            this.name = name;
        }
    }
}
