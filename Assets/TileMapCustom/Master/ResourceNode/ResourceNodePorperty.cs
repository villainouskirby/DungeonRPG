using UnityEditor;
using UnityEngine;
using System;

[System.Serializable]
public class ResourceNodeProperty
{
    public string ResourceNodeName;

    public ResourceNodeProperty(string resourceNodeName)
    {
        ResourceNodeName = resourceNodeName;
    }
}