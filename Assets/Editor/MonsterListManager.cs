using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Monster_Info;

public static class MonsterListManager
{
    public static string[] MonsterList;

    static MonsterListManager()
    {
        Monster_InfoDataParser.SetXlsxData();

        MonsterList = new string[Monster.Length];
        for (int i = 0; i < Monster.Length; i++)
        {
            MonsterList[i] = Monster[i].Monster_id;
        }
    }
}
