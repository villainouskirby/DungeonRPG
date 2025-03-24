using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class JJSave
{
    public static readonly string DataFilePath = "JJSave/";

    public static void Save<T>(T target, string saveDataName)
    {
        ReadOnlySpan<byte> typeByte = Type2TypeByteConverter.Convert<T>(target);
        string path = GetSavePath(saveDataName);

        try
        {
            using var fileStream = new FileStream(path, FileMode.Create);
            fileStream.Write(typeByte);
            Debug.Log($"JJSave : Data saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"JJSave : Failed to save data - {e}");
        }
    }

    // out을 이용해서 반환 값을 받는다.
    // T 명시를 생략하기 위해서임.
    public static void Load<T>(out T result,  string saveDataName)
    {
        result = Load<T>(saveDataName);
    }

    // 반환 값으로 Load 값을 받는다.
    public static T Load<T>(string saveDataName)
    {
        string path = GetSavePath(saveDataName);
        T result = default;

        if (!File.Exists(path))
        {
            Debug.LogWarning($"JJSave : No save file found at: {path}");
            return result;
        }

        try
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            long length = fileStream.Length;

            if (length <= 0 || length > int.MaxValue)
            {
                // 이 함수는 Save로 저장한 파일들만 읽어오는 Load 함수이다.
                // Save 단계에서 길이가 벗어나는 파일은 이미 걸러지기에
                // Load 단계에서 길이가 벗어난다면 무조건 잘못 된 파일이다.
                // 따라서 제외한다.
                throw new IOException("JJSave : Invalid or corrupted file.");
            }

            byte[] buffer = new byte[length];
            int totalRead = 0;
            while (totalRead < length)
            {
                int read = fileStream.Read(buffer, totalRead, (int)length - totalRead);
                if (read == 0)
                    throw new IOException("JJSave : Unexpected end of file.");
                totalRead += read;
            }

            result = TypeByte2TypeConverter.Convert<T>(buffer);
            Debug.Log($"JJSave : Data loaded from {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"JJSave : Failed to load data - {e}");
        }

        return result;
    }

    private static string GetSavePath(string fileName)
    {
        StringBuilder sb = new();

#if UNITY_ANDROID && !UNITY_EDITOR
        sb.Append(Application.persistentDataPath);
        sb.Append("/");
        sb.Append(DataFilePath);
#elif UNITY_EDITOR
        sb.Append(Application.dataPath);
        sb.Append("/");
        sb.Append(DataFilePath);
#elif UNITY_STANDALONE_WIN
        sb.Append(Application.persistentDataPath);
        sb.Append("/");
        sb.Append(DataFilePath);
#endif

        CheckSavePath(sb.ToString());

        sb.Append($"{fileName}.bytes");
        return sb.ToString();
    }

    private static void CheckSavePath(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }
}
