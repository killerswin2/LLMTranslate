using System;
using System.Text;
using System.Text.RegularExpressions;
using System.CommandLine;
using System.Reflection;
using System.Collections.Generic;
public class CSVLine
{
    public CSVLine(string originalText)
    {
        OriginalText = originalText;

    }
    public CSVLine(string originalText, string transalted)
    {
        OriginalText = originalText;
        FinalText = transalted;
    }

    public string? OriginalText { get; set; } = string.Empty;

    public string? FinalText { get; set; } = string.Empty;

    public override string ToString()
    {
        return string.Format("{0}#{1}", OriginalText, FinalText);
    }
}

public class Corrections
{
    public static List<CSVLine> CSVLines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_Qoute_Lines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_HF_Lines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_SF_Lines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_TN_Lines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_SL_Lines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_HL_Lines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_TA_Lines { get; } = new List<CSVLine>();
    public static List<CSVLine> Missing_PT_Lines { get; } = new List<CSVLine>();

    public static List<CSVLine> Missing_FinalLine { get; } = new List<CSVLine>();

    public static void IterateAndWriteWarnings(List<CSVLine> csvLines, string printedString, string filename)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding("Shift-JIS");
        FileStream file = File.Create(filename);
        foreach (CSVLine line in csvLines)
        {
            string outstring = $"{printedString} ::[Line]= {line.ToString()}\n";
            file.Write(encoding.GetBytes(outstring), 0, encoding.GetByteCount(outstring));
        }
        file.Close();
    }

    public static void PrintWarnings()
    {
        IterateAndWriteWarnings(Missing_Qoute_Lines, "Warning, this line has bad qoutes in final output.", "Warning-Qoute-Lines.txt");
        IterateAndWriteWarnings(Missing_HF_Lines, "Warning, this line has a bad count of /HF in the final output compared to original.", "Warning-HF-Lines.txt");
        IterateAndWriteWarnings(Missing_SF_Lines, "Warning, this line has a bad count of /SF in the final output compared to original.", "Warning-SF-Lines.txt");
        IterateAndWriteWarnings(Missing_TN_Lines, "Warning, this line has a bad count of /TN in the final output compared to original.", "Warning-TN-Lines.txt");
        IterateAndWriteWarnings(Missing_SL_Lines, "Warning, this line has a bad count of /SL in the final output compared to original.", "Warning-SL-Lines.txt");
        IterateAndWriteWarnings(Missing_HL_Lines, "Warning, this line has a bad count of /HL in the final output compared to original.", "Warning-HL-Lines.txt");
        IterateAndWriteWarnings(Missing_TA_Lines, "Warning, this line has a bad count of /TA in the final output compared to original.", "Warning-TA-Lines.txt");
        IterateAndWriteWarnings(Missing_PT_Lines, "Warning, this line has a bad count of /PT in the final output compared to original.", "Warning-PT-Lines.txt");
        IterateAndWriteWarnings(Missing_FinalLine, "Warning, this line is missing its final part", "Warning-Final-Lines.txt");
    }

    public static void IterateUnique(HashSet<string> seenKey, List<CSVLine> csvLines)
    {
        for (int i = 0; i < csvLines.Count; i++)
        {
            if (!seenKey.Contains(csvLines[i].OriginalText))
            {
                seenKey.Add(csvLines[i].OriginalText);
            }
        }
    }

    public static void GenerateLLMTxt()
    {

        HashSet<string> seenKey = new HashSet<string>();
        IterateUnique(seenKey, Missing_Qoute_Lines);
        IterateUnique(seenKey, Missing_HF_Lines);
        IterateUnique(seenKey, Missing_SF_Lines);
        IterateUnique(seenKey, Missing_TN_Lines);
        IterateUnique(seenKey, Missing_SL_Lines);
        IterateUnique(seenKey, Missing_HL_Lines);
        IterateUnique(seenKey, Missing_TA_Lines);
        IterateUnique(seenKey, Missing_PT_Lines);
        IterateUnique(seenKey, Missing_FinalLine);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding("Shift-JIS");
        FileStream file = File.Create("LLMTranslateLines.txt");
        foreach (string element in seenKey)
        {
            string lineStr = element + "\n";
            file.Write(encoding.GetBytes(lineStr), 0, encoding.GetByteCount(lineStr));
        }
        file.Close();
    }

    public static void IterateUniqueCSVLine(HashSet<string> seenKey, List<CSVLine> csvLines, List<CSVLine> output)
    {
        for (int i = 0; i < csvLines.Count; i++)
        {
            if (!seenKey.Contains(csvLines[i].OriginalText))
            {
                seenKey.Add(csvLines[i].OriginalText);
                output.Add(csvLines[i]);
            }
        }
    }


    public static void GenerateCSVLLMTxt()
    {
        HashSet<string> seenKey = new HashSet<string>();
        List<CSVLine> csvLines = new List<CSVLine>();

        IterateUniqueCSVLine(seenKey, Missing_Qoute_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_HF_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_SF_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_TN_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_SL_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_HL_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_TA_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_PT_Lines, csvLines);
        IterateUniqueCSVLine(seenKey, Missing_FinalLine, csvLines);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding("Shift-JIS");
        FileStream file = File.Create("LLMTranslateLinesCSV.txt");
        foreach (CSVLine element in csvLines)
        {
            string lineStr = element.ToString() + "\n";
            file.Write(encoding.GetBytes(lineStr), 0, encoding.GetByteCount(lineStr));
        }
        file.Close();
    }


    public static void Main(string[] args)
    {
        Option<bool> Generate = new("--Generate")
        {
            Description = "Generates Warnings about the file."
        };

        Option<bool> Print = new("--Print")
        {
            Description = "Prints out a corrected file."
        };

        Option<bool> LLMGenerate = new("--LLM")
        {
            Description = "Generates a LLMTranslateLinesTXT file."
        };

        Option<bool> LLMGenerateCSV = new("--GenerateCSVLLM")
        {
            Description = "Generates a LLMTranslateLinesTXT file but with the final translation with it."
        };

        RootCommand rootCommand = new("Application that either generates warnings or applies warning fixes");
        rootCommand.Options.Add(Generate);
        rootCommand.Options.Add(Print);
        rootCommand.Options.Add(LLMGenerate);
        rootCommand.Options.Add(LLMGenerateCSV);
        ParseResult parseResult = rootCommand.Parse(args);

        GetInputData();
        Dictionary<string, string> correctedLines = GetCorrectionsData();
        ApplyCorrections(correctedLines);
        DetermineMissingLines();
        CheckForMissingFinalLines();




        if (parseResult.GetValue(Generate))
        {
            PrintWarnings();

        }

        if (parseResult.GetValue(LLMGenerate))
        {
            GenerateLLMTxt();
            if (parseResult.GetValue(LLMGenerateCSV))
            {
                GenerateCSVLLMTxt();
            }
        }

        if (parseResult.GetValue(Print))
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("Shift-JIS");
            FileStream file = File.Create("output.txt");
            for (int i = 0; i < CSVLines.Count; i++)
            {

                string lineStr = CSVLines[i].ToString() + "\n";
                file.Write(encoding.GetBytes(lineStr), 0, encoding.GetByteCount(lineStr));
            }
            file.Close();
        }
    }
    public static void CheckForMissingFinalLines()
    {
        foreach (CSVLine line in CSVLines)
        {
            if (line.FinalText == string.Empty)
            {
                Missing_FinalLine.Add(line);
            }
        }
    }
    public static void ApplyCorrections(Dictionary<string, string> correctedLines)
    {
        for (int i = 0; i < CSVLines.Count; i++)
        {
            if (correctedLines.ContainsKey(CSVLines[i].OriginalText))
            {
                CSVLines[i].FinalText = correctedLines[CSVLines[i].OriginalText];
            }
        }
    }

    public static void PatternMatcherMult(string originalPattern, string finalPattern, List<CSVLine> bucket)
    {
        Regex originalRegex = new Regex(originalPattern);
        Regex finalRegex = new Regex(finalPattern);
        for (int i = 0; i < CSVLines.Count; i++)
        {
            MatchCollection matchesOriginal = originalRegex.Matches(CSVLines[i].OriginalText);
            if (matchesOriginal.Count > 0)
            {
                MatchCollection matchesFinal = finalRegex.Matches(CSVLines[i].FinalText);
                // if the count doesn't match on the original side to the final side, there might be a problem, add for warning.
                if (matchesOriginal.Count != matchesFinal.Count)
                {
                    bucket.Add(CSVLines[i]);
                }
            }
        }
    }

    public static void PatternMatcher(string pattern, List<CSVLine> bucket)
    {
        Regex regex = new Regex(pattern);
        for (int i = 0; i < CSVLines.Count; i++)
        {
            MatchCollection matchesOriginal = regex.Matches(CSVLines[i].OriginalText);
            if (matchesOriginal.Count > 0)
            {
                MatchCollection matchesFinal = regex.Matches(CSVLines[i].FinalText);
                // if the count doesn't match on the original side to the final side, there might be a problem, add for warning.
                if (matchesOriginal.Count != matchesFinal.Count)
                {
                    bucket.Add(CSVLines[i]);
                }
            }
        }
    }

    public static void DetermineMissingLines()
    {
        //string Genericpattern = @"/[a-zA-Z][a-zA-Z]";
        string HFpattern = @"/HF";
        string SFpattern = @"/SF";
        string TNpattern = @"/TN";
        string SLpattern = @"/SL";
        string HLpattern = @"/HL";
        string TApattern = @"/TA";
        string PTpattern = @"/PT";
        string OriginalQoutepattern = @"[「」]";
        string FinalQoutepattern = "\"";

        PatternMatcher(HFpattern, Missing_HF_Lines);
        PatternMatcher(SFpattern, Missing_SF_Lines);
        PatternMatcher(TNpattern, Missing_TN_Lines);
        PatternMatcher(SLpattern, Missing_SL_Lines);
        PatternMatcher(HLpattern, Missing_HL_Lines);
        PatternMatcher(TApattern, Missing_TA_Lines);
        PatternMatcher(PTpattern, Missing_PT_Lines);
        PatternMatcherMult(OriginalQoutepattern, FinalQoutepattern, Missing_Qoute_Lines);
    }


    public static void GetInputData()
    {
        List<string> lines = new List<string>();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        byte[] fileBytes = File.ReadAllBytes("Input.csv");
        Encoding encoding = Encoding.GetEncoding("Shift-JIS");

        string decodedText = encoding.GetString(fileBytes);
        StringReader reader = new StringReader(decodedText);

        string line = reader.ReadLine();
        while (line != null)
        {
            if (line != string.Empty)
            {
                lines.Add(line);
            }
            line = reader.ReadLine();
        }
        for (int i = 0; i < lines.Count; i++)
        {
            string[] splitLine = lines[i].Split('#');
            if (splitLine.Length == 1 || splitLine[1] == string.Empty)
            {
                CSVLine csvLine = new CSVLine(splitLine[0]);
                CSVLines.Add(csvLine);
            }
            else
            {
                CSVLine csvLine = new CSVLine(splitLine[0], splitLine[1]);
                CSVLines.Add(csvLine);
            }
        }
    }

    public static Dictionary<string, string> GetCorrectionsData()
    {
        Dictionary<string, string> correctedLines = new Dictionary<string, string>();
        List<string> lines = new List<string>();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        byte[] fileBytes = File.ReadAllBytes("CorrectedLines.csv");
        Encoding encoding = Encoding.GetEncoding("Shift-JIS");

        string decodedText = encoding.GetString(fileBytes);
        StringReader reader = new StringReader(decodedText);

        string line = reader.ReadLine();
        while (line != null)
        {
            if (line != string.Empty)
            {
                lines.Add(line);
            }
            line = reader.ReadLine();
        }
        for (int i = 0; i < lines.Count; i++)
        {
            string[] splitLine = lines[i].Split('#');
            if (splitLine.Length == 1 || splitLine[1] == string.Empty)
            {

            }
            else
            {
                if (correctedLines.ContainsKey(splitLine[0]))
                {
                    correctedLines[splitLine[0]] = splitLine[1];
                }
                else
                {
                    correctedLines.Add(splitLine[0], splitLine[1]);
                }

            }

        }
        return correctedLines;
    }
}
