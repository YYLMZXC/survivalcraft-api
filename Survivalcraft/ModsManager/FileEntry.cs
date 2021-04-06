﻿using System.IO;

public class FileEntry
{
    public enum StorageType
    { 
        InZip,
        InStorage
    }
    public string Filename;
    public StorageType storageType;
    public Stream Stream;
}
