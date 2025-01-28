using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Unity.VisualScripting.FullSerializer;

public class ParserGenerator : IParserGenerator
{
    StringBuilder _xlsxSb = new();
    ConcurrentDictionary<string, StringBuilder> _sheetSb = new();

    /// <summary>
    /// Parser Class 제작을 시작한다.
    /// </summary>
    /// <param name="xlsxName">Xlsx 이름</param>
    public void StartDataParserGenerate(string xlsxName, string[] sheetNames, string dataFilePath)
    {
        _xlsxSb = new();
        string className = $"{xlsxName}DataParser";
        BuildClassDefault(className, _xlsxSb, true); // Sheet 기본 틀 제작
        _xlsxSb.AppendLine($"    readonly private static string _dataFilePath = \"{dataFilePath}\";"); // DataFilePath 기록
        _xlsxSb.AppendLine($"    private static Dictionary<string, int> _typeByteLength = new();");

        BuildSetXlsxDataMethod(_xlsxSb, sheetNames);
    }

    public void BuildSetXlsxDataMethod(StringBuilder sb, string[] sheetNames)
    {
        sb.AppendLine("    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");
        sb.AppendLine("    public static void SetXlsxData()");
        sb.AppendLine("    {");
        foreach(string sheetName in sheetNames)
        {
            sb.AppendLine($"        ConvertRowData2{sheetName}();");
        }

        sb.AppendLine("    }");
    }

    /// <summary>
    /// Parser Class 제작을 마무리한다.
    /// </summary>
    /// <param name="xlsxName">Xlsx 이름</param>
    /// <param name="sheetName">Xlsx가 가지고 있는 Sheet들의 이름</param>
    /// <param name="dataClassPath">Parser가 저장될 위치</param>
    public void FinishDataParserGenerate(string xlsxName, string dataClassPath)
    {
        foreach(var sheetSb in _sheetSb.Values)
        {
            _xlsxSb.AppendLine(sheetSb.ToString());
        }

        // 각 sheet에서 작업한 string들을 합친다.
        string className = $"{xlsxName}DataParser";
        string dataParserPath = Path.Combine(dataClassPath, xlsxName, "Parser");
        FinishClass(className, dataParserPath, _xlsxSb);
    }

    /// <summary>
    /// Sheet Data Parser Method를 만든다.
    /// </summary>
    public void GenerateSheetDataParserMethod(string xlsxName, string sheetName, Dictionary<int, string> dataTypePair, Dictionary<int, string> dataNamePair, HashSet<int> skipColumns)
    {
        StringBuilder sb = new();
        sb.AppendLine($"    public static void ConvertRowData2{sheetName}()");
        sb.AppendLine("    {");
        sb.AppendLine("        int bufferSize = 2048;");
        sb.AppendLine($"        FileStream fileStream = new(Path.Combine(_dataFilePath, \"{xlsxName}\",\"{sheetName}.bin\"), FileMode.Open, FileAccess.Read);");
        sb.AppendLine("        Span<byte> buffer = stackalloc byte[bufferSize];");
        sb.AppendLine("        int offset = 0;");
        sb.AppendLine("        fileStream.Read(buffer);");
        sb.AppendLine("        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));");
        sb.AppendLine("        offset += 8;");
        sb.AppendLine($"        {xlsxName}.{sheetName} = new {xlsxName}_{sheetName}[rows];");
        sb.AppendLine("        for(int row = 0; row < rows; row++)");
        sb.AppendLine("        {");
        sb.AppendLine($"            {xlsxName}_{sheetName} sheetRowData = new();");

        foreach (var dataName in dataNamePair)
        {
            if (skipColumns.Contains(dataName.Key))
                continue;

            if(dataTypePair[dataName.Key] == "string")
                BuildStringSetter(sb, dataName.Value);
            else
                BuildVariableSetter(sb, String2TypeByteConverter.TypeByteLength[dataTypePair[dataName.Key]], dataName.Value, dataTypePair[dataName.Key]);
        }

        sb.AppendLine($"            {xlsxName}.{sheetName}[row] = sheetRowData;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");

        _sheetSb[sheetName] = sb;
    }

    private void BuildStringSetter(StringBuilder sb, string dataName)
    {
        sb.AppendLine($"            if (offset + 4 >= buffer.Length)");
        sb.AppendLine("            {");
        sb.AppendLine("                byte[] leftData = buffer.Slice(offset).ToArray();");
        sb.AppendLine("                buffer.Clear();");
        sb.AppendLine("                leftData.CopyTo(buffer);");
        sb.AppendLine("                fileStream.Read(buffer.Slice(leftData.Length));");
        sb.AppendLine("                offset = 0;");
        sb.AppendLine("            }");
        sb.AppendLine($"            int stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));");
        sb.AppendLine("            offset += 4;");
        sb.AppendLine($"            if (offset + stringLength >= buffer.Length)");
        sb.AppendLine("            {");
        sb.AppendLine("                byte[] leftData = buffer.Slice(offset).ToArray();");
        sb.AppendLine("                buffer.Clear();");
        sb.AppendLine("                leftData.CopyTo(buffer);");
        sb.AppendLine("                fileStream.Read(buffer.Slice(leftData.Length));");
        sb.AppendLine("                offset = 0;");
        sb.AppendLine("            }");
        sb.AppendLine($"            sheetRowData.{dataName} = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));");
        sb.AppendLine("            offset += stringLength;");
    }

    private void BuildVariableSetter(StringBuilder sb, int byteLength, string dataName, string dataType)
    {
        sb.AppendLine($"            if (offset + {byteLength} >= buffer.Length)");
        sb.AppendLine("            {");
        sb.AppendLine("                byte[] leftData = buffer.Slice(offset).ToArray();");
        sb.AppendLine("                buffer.Clear();");
        sb.AppendLine("                leftData.CopyTo(buffer);");
        sb.AppendLine("                fileStream.Read(buffer.Slice(leftData.Length));");
        sb.AppendLine("                offset = 0;");
        sb.AppendLine("            }");
        sb.AppendLine($"            sheetRowData.{dataName} = TypeByte2TypeConverter.ConvertTypeByte2{dataType}(buffer.Slice(offset, {byteLength}));");
        sb.AppendLine($"            offset += {byteLength};");
    }

    public void BuildClassDefault(string className, StringBuilder sb, bool isStatic = false)
    {
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using System.IO;");
        sb.AppendLine("using System;");
        sb.Append("\n\n\n\n");
        sb.AppendLine($"public {(isStatic ? "static" : "")} class {className}");
        sb.AppendLine("{");
    }

    /// <summary>
    /// Class를 닫고 cs 파일을 생성한다.
    /// </summary>
    public void FinishClass(string className, string path, StringBuilder sb)
    {
        sb.AppendLine("}");

        string filePath = $"{path}/{className}.cs";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (File.Exists(filePath))
            File.Delete(filePath);

        File.WriteAllText(filePath, sb.ToString());
    }
}
