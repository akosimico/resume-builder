using System.Text.Json;
using ResumeBuilder.Models;
namespace ResumeBuilder.Services;
public static class SaveLoadService
{
    static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public static void Save(string path, ResumeData data) => File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
    public static ResumeData? Load(string path) => JsonSerializer.Deserialize<ResumeData>(File.ReadAllText(path), Options);
}
