using System.Collections.Generic;

public static class UIHelpers
{
    public static string WrapText(string text, int wrapWidth = 40)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string[] words = text.Split(' ');
        List<string> lines = new();
        string currentLine = "";

        foreach (string word in words)
        {
            if ((currentLine.Length + word.Length + 1) > wrapWidth)
            {
                if (!string.IsNullOrEmpty(currentLine)) lines.Add(currentLine.TrimEnd());
                currentLine = "";
            }

            if (word.Length > wrapWidth)
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine.TrimEnd());
                    currentLine = "";
                }

                int start = 0;
                while (start < word.Length)
                {
                    int length = System.Math.Min(wrapWidth, word.Length - start);
                    lines.Add(word.Substring(start, length));
                    start += length;
                }
            }
            else currentLine += word + " ";
        }

        if (!string.IsNullOrEmpty(currentLine)) lines.Add(currentLine.TrimEnd());

        return string.Join("\n", lines);
    }
}
