using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TileMapMaster;

public class Test2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Dictionary<int, string> a = new Dictionary<int, string>() { { 1, "sd"}, { 2, "dd" } };
        JJSave.Save(a, "ex");

        JJSave.Load(out Dictionary<int, string> b, "ex");
        foreach(var i in b)
            Debug.Log(i.Key + i.Value);
    }
}