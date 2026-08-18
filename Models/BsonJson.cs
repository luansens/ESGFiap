using System.Text.Json;
using MongoDB.Bson;

namespace ESGFiap.Models;

/// Bridges flexible BsonDocument fields (used for schema-variable ESG data) into
/// JsonElement so System.Text.Json can serialize them in API responses.
public static class BsonJson
{
    public static JsonElement? ToJsonElement(this BsonDocument? doc) =>
        doc is null ? null : JsonDocument.Parse(doc.ToJson()).RootElement;

    public static List<JsonElement>? ToJsonElements(this List<BsonDocument>? docs) =>
        docs?.Select(d => JsonDocument.Parse(d.ToJson()).RootElement).ToList();

    public static BsonDocument? ToBsonDocument(this JsonElement? element) =>
        element is null ? null : BsonDocument.Parse(element.Value.GetRawText());
}
