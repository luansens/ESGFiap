using MongoDB.Bson;

namespace ESGFiap.Models;

/// Ids are stored as ObjectId under the hood (via BsonRepresentation), so any filter
/// that compares against a malformed id throws a FormatException instead of just
/// finding nothing. Callers must check this before querying by id.
public static class IdValidation
{
    public static bool IsValidObjectId(this string? id) => id is not null && ObjectId.TryParse(id, out _);
}
