using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // ilk once postgresql-de plain formatda backup çıxarıb onu .sql fayl etmek lazımdır daha sonra hemin faylın full pathini daxil edirik. meselen C:\backups\mydb.sql
        Console.Write("Please enter the full path of the .sql file: ");
        var inputFile = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputFile) || !File.Exists(inputFile))
        {
            Console.WriteLine("❌ File not found or path is empty.");
            return;
        }

        var baseDir = Path.GetDirectoryName(inputFile)!;
        var createFile = Path.Combine(baseDir, "view_create_scripts.txt");
        var dropFile = Path.Combine(baseDir, "view_drop_scripts.txt");

        var createRegex = new Regex(
            @"CREATE\s+(OR\s+REPLACE\s+)?VIEW\s+(?<full>((?<schema>""?\w+""?)\.)?(?<name>""?\w+""?))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var buffer = new StringBuilder();
        var creates = new List<string>();
        var viewNames = new List<string>();

        using (var reader = new StreamReader(inputFile))
        {
            string? line;
            bool insideView = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!insideView)
                {
                    if (createRegex.IsMatch(line))
                    {
                        insideView = true;
                        buffer.Clear();
                        buffer.AppendLine(line);
                    }
                }
                else
                {
                    buffer.AppendLine(line);

                    if (line.TrimEnd().EndsWith(";"))
                    {
                        var script = buffer.ToString();
                        creates.Add(script);

                        var m = createRegex.Match(script);
                        if (m.Success)
                        {
                            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value.Trim('"') : "public";
                            var name = m.Groups["name"].Value.Trim('"');

                            static string Q(string ident) => ident.StartsWith("\"") ? ident : $"\"{ident}\"";
                            schema = Q(schema);
                            name = Q(name);

                            viewNames.Add($"{schema}.{name}");
                        }

                        insideView = false;
                    }
                }
            }
        }

        File.WriteAllLines(createFile, creates);

        using (var writer = new StreamWriter(dropFile))
        {
            for (int i = viewNames.Count - 1; i >= 0; i--)
            {
                writer.WriteLine($"DROP VIEW IF EXISTS {viewNames[i]} CASCADE;");
            }
        }

        Console.WriteLine($"✔ CREATE VIEW scripts saved to {createFile}");
        Console.WriteLine($"✔ DROP VIEW scripts saved to {dropFile}");
    }
}
