using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ClosedXML.Excel;

class Program
{
    static async Task Main()
    {
        string? difficultyFilter = null; // Use "Fácil", "Médio", "Difícil" ou null para tudo
        await ExportLeetCodeToExcel(difficultyFilter);
    }

    static async Task ExportLeetCodeToExcel(string? difficultyFilter)
    {
        var client = new HttpClient();
        var url = "https://leetcode.com/api/problems/all/";
        string json = await client.GetStringAsync(url);

        var root = JObject.Parse(json);
        var problems = root["stat_status_pairs"];
        if (problems == null)
        {
            Console.WriteLine("❌ Erro: JSON inválido.");
            return;
        }

        var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("LeetCode");
        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Título";
        ws.Cell(1, 3).Value = "Dificuldade";
        ws.Cell(1, 4).Value = "Tags";

        var diffMap = new Dictionary<int, string>
        {
            { 1, "Fácil" },
            { 2, "Médio" },
            { 3, "Difícil" }
        };

        int row = 2;

        foreach (var item in problems)
        {
            var stat = item["stat"];
            if (stat == null) continue;

            int id = stat["frontend_question_id"]?.Value<int>() ?? 0;
            string title = stat["question__title"]?.ToString() ?? "Sem título";

            int level = item["difficulty"]?["level"]?.Value<int>() ?? 1;
            string difficulty = diffMap.GetValueOrDefault(level, "Fácil");

            if (!string.IsNullOrEmpty(difficultyFilter) && difficulty != difficultyFilter)
                continue;

            var tagsArray = item["topic_tags"];
            List<string> tags = new();
            if (tagsArray != null)
            {
                foreach (var tag in tagsArray)
                {
                    string? name = tag["name"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        tags.Add(name);
                }
            }

            ws.Cell(row, 1).Value = id;
            ws.Cell(row, 2).Value = title;
            ws.Cell(row, 3).Value = difficulty;
            ws.Cell(row, 4).Value = string.Join(", ", tags);
            row++;
        }

        ws.Columns().AdjustToContents();
        string fileName = difficultyFilter == null
            ? "leetcode_problems.xlsx"
            : $"leetcode_{difficultyFilter.ToLower()}.xlsx";

        wb.SaveAs(fileName);
        Console.WriteLine($"✅ Arquivo gerado com sucesso: {fileName}");
    }
}
