﻿// Game.ModInfo
using System;

[Serializable]
public struct ModInfo : IEquatable<ModInfo>
{
    public string Name;

    public string Description;

    public string ModVersion;

    public string APIVersion;

    public string Url;

    public string UpdateUrl;

    public string AuthorList;

    public string Credits;

    public string Logo;

    public string Screenshots;

    public string Parent;

    public string Dependency;

    public string FileName;

    public string Dependants;

    public bool UseDependencyInfo;

    public ModInfo(string name, string description, string modversion, string apiversion, string url, string updateUrl, string authorList, string credits, string logo, string screenshots, string parent, string dependency=null, string dependants = null, bool usedependencyInfo = false)
    {
        Name = name;
        Description = description;
        APIVersion = apiversion;
        ModVersion = modversion;
        Url = url;
        UpdateUrl = updateUrl;
        AuthorList = authorList;
        Credits = credits;
        Logo = logo;
        Screenshots = screenshots;
        Parent = parent;
        Dependency = dependency;
        Dependants = dependants;
        UseDependencyInfo = usedependencyInfo;
        FileName = "";
    }

    public override bool Equals(object obj)
    {
        if (obj is ModInfo)
        {
            return ToString() == ((ModInfo)obj).ToString();
        }
        return false;
    }

    public bool Equals(ModInfo other)
    {
        return ToString() == other.ToString();
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name} {ModVersion} - {Description} ({Url})";
    }
}
