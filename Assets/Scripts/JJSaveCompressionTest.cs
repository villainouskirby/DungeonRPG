using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using System.IO.Compression;
using System.Linq;
using Debug = UnityEngine.Debug;
using CompressionLevel = System.IO.Compression.CompressionLevel;

[System.Serializable]
public class TestData
{
    public string id;
    public int level;
    public List<string> inventory;
    public float[] position;
    public List<Vector2Int> exploredTiles;

    // 추가된 필드: 다양한 자료형 테스트
    public bool isActive;
    public double accuracy;
    public List<bool> flags;
    public NestedData nested;
    public List<NestedData> nestedList;
    public TestEnum testEnum;
    public int[] scores;

    [Serializable]
    public class NestedData
    {
        public string name;
        public int value;
    }

    public enum TestEnum
    {
        OptionA,
        OptionB,
        OptionC
    }

    /// <summary>
    /// 다양한 데이터 타입을 포함하는 테스트 데이터를 생성합니다.
    /// </summary>
    /// <param name="inventorySize">인벤토리 항목 개수</param>
    /// <param name="tileCount">타일 개수</param>
    /// <param name="nestedCount">중첩 데이터 개수</param>
    /// <param name="scoreCount">점수 배열 크기</param>
    public static TestData GenerateMock(int inventorySize = 10, int tileCount = 1000, int nestedCount = 10, int scoreCount = 5)
    {
        return new TestData
        {
            id = "player_001",
            level = UnityEngine.Random.Range(1, 100),
            inventory = GenerateInventory(inventorySize),
            position = new float[] {
                UnityEngine.Random.Range(-500f, 500f),
                UnityEngine.Random.Range(-500f, 500f),
                UnityEngine.Random.Range(-500f, 500f)
            },
            exploredTiles = GenerateTiles(tileCount),
            isActive = UnityEngine.Random.value > 0.5f,
            accuracy = UnityEngine.Random.Range(0.0f, 100.0f),
            flags = GenerateFlags(5),
            nested = new NestedData { name = "Nested", value = UnityEngine.Random.Range(0, 1000) },
            nestedList = GenerateNestedList(nestedCount),
            testEnum = (TestEnum)UnityEngine.Random.Range(0, Enum.GetValues(typeof(TestEnum)).Length),
            scores = GenerateScores(scoreCount)
        };
    }

    private static List<string> GenerateInventory(int count)
    {
        string[] sampleItems = { "sword", "shield", "potion", "apple", "gem", "key", "scroll", "ring" };
        var list = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(sampleItems[UnityEngine.Random.Range(0, sampleItems.Length)]);
        }
        return list;
    }

    private static List<Vector2Int> GenerateTiles(int count)
    {
        var list = new List<Vector2Int>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new Vector2Int(
                UnityEngine.Random.Range(0, 1024),
                UnityEngine.Random.Range(0, 1024)
            ));
        }
        return list;
    }

    private static List<bool> GenerateFlags(int count)
    {
        var list = new List<bool>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(UnityEngine.Random.value > 0.5f);
        }
        return list;
    }

    private static List<NestedData> GenerateNestedList(int count)
    {
        var list = new List<NestedData>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new NestedData
            {
                name = "Nested " + i,
                value = UnityEngine.Random.Range(0, 100)
            });
        }
        return list;
    }

    private static int[] GenerateScores(int count)
    {
        int[] scores = new int[count];
        for (int i = 0; i < count; i++)
        {
            scores[i] = UnityEngine.Random.Range(0, 100);
        }
        return scores;
    }
}


public static class JJCompressor
{
    public static byte[] Compress(byte[] input)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            gzip.Write(input, 0, input.Length);
        return output.ToArray();
    }

    public static byte[] Decompress(byte[] input)
    {
        using var inputStream = new MemoryStream(input);
        using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }
}

public class JJSaveCompressionTest : MonoBehaviour
{
    [ContextMenu("Run Save + Load Compression Test")]
    public void RunFullTest()
    {
        var data = TestData.GenerateMock(100, 500); // 아이템 100개, 타일 5000개

        string basePath = Application.persistentDataPath;
        string gzipPath = Path.Combine(basePath, "test_data_gzip.bytes");
        string jsonPath = Path.Combine(basePath, "test_data_json.json");

        // GZip 저장
        Stopwatch swGzipSave = Stopwatch.StartNew();
        byte[] bin = Type2TypeByteConverter.Convert(data).ToArray();
        byte[] compressed = JJCompressor.Compress(bin);
        File.WriteAllBytes(gzipPath, compressed);
        swGzipSave.Stop();

        /*
        // JSON 저장
        Stopwatch swJsonSave = Stopwatch.StartNew();
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(jsonPath, json, Encoding.UTF8);
        swJsonSave.Stop();
        */

        // GZip 로드
        Stopwatch swGzipLoad = Stopwatch.StartNew();
        byte[] gzipLoaded = File.ReadAllBytes(gzipPath);
        byte[] decompressed = JJCompressor.Decompress(gzipLoaded);
        var dataFromGzip = TypeByte2TypeConverter.Convert<TestData>(decompressed);
        swGzipLoad.Stop();

        /*
        // JSON 로드
        Stopwatch swJsonLoad = Stopwatch.StartNew();
        string jsonLoaded = File.ReadAllText(jsonPath, Encoding.UTF8);
        var dataFromJson = JsonUtility.FromJson<TestData>(jsonLoaded);
        swJsonLoad.Stop();
        */

        long gzipSize = new FileInfo(gzipPath).Length;
        //long jsonSize = new FileInfo(jsonPath).Length;

        Debug.Log("저장 비교:");
        Debug.Log($"GZIP 저장 시간: {swGzipSave.ElapsedMilliseconds} ms");
        //Debug.Log($"JSON 저장 시간: {swJsonSave.ElapsedMilliseconds} ms");

        Debug.Log("불러오기 비교:");
        Debug.Log($"GZIP 로드 + 해제 + 역직렬화: {swGzipLoad.ElapsedMilliseconds} ms");
        //Debug.Log($"JSON 로드 + 파싱: {swJsonLoad.ElapsedMilliseconds} ms");

        Debug.Log("크기 비교:");
        Debug.Log($"GZIP 크기: {gzipSize} bytes");
        //Debug.Log($"JSON 크기: {jsonSize} bytes");
        //Debug.Log($"압축률: {(1f - (float)gzipSize / jsonSize) * 100f:F2}% ↓");
    }

}
