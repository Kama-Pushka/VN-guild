using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        if (filePath.StartsWith('/'))
            filePath = FilePaths.root + filePath;

        // Тут мы решили проверить получаемые пути нашими методами
        List<string> lines = new List<string>();
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError($"ex.fileName = {ex.FileName}");
            Debug.LogWarning($"FilePaths.root = {FilePaths.root}");
            Debug.LogError($"File not found: '{filePath}'");
        }

        return lines;
    }

    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true)
    {
        return null;
    }
}
