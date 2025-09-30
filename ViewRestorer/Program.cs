using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run -- <backup.sql>");
            return;
        }

        var inputFile = args[0];
        var createFile = "view_create_scripts.txt";
        var dropFile = "view_drop_scripts.txt";

        // Yalnız CREATE VIEW üçün regex
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

                        // View adını çıxar
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

        // CREATE VIEW scriptlərini yaz
        File.WriteAllLines(createFile, creates);

        // DROP VIEW scriptlərini sondan önə yaz
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
