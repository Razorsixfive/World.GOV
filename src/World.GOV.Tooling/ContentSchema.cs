using System.Text.Json;
using Json.Schema;

namespace World.GOV.Tooling;

internal sealed class ContentSchema
{
    private readonly JsonSchema _schema;

    public ContentSchema(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        _schema = JsonSchema.Build(document.RootElement.Clone(), new BuildOptions { Dialect = Dialect.Draft202012, SchemaRegistry = new() });
    }

    public T Read<T>(string path, JsonSerializerOptions options)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var result = _schema.Evaluate(document.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        if (!result.IsValid)
        {
            using var report = JsonDocument.Parse(JsonSerializer.Serialize(result));
            var messages = report.RootElement.GetProperty("details").EnumerateArray()
                .Where(detail => detail.TryGetProperty("errors", out _))
                .SelectMany(detail => detail.GetProperty("errors").EnumerateObject().Select(error =>
                    $"{detail.GetProperty("instanceLocation").GetString()}: {error.Value.GetString()}"));
            throw new JsonException($"Schema validation failed: {string.Join("; ", messages)}");
        }
        return document.RootElement.Deserialize<T>(options) ?? throw new JsonException("Expected a JSON object.");
    }
}
