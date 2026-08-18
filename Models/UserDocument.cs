using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ESGFiap.Models;

public class UserDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public UserType UserType { get; set; } = UserType.Residential;

    public List<string> Roles { get; set; } = ["User"];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// Flexible ESG profile: shape varies by UserType (e.g. commercial users carry
    /// certifications/reuse programs, residential users carry simple household goals).
    [BsonIgnoreIfNull]
    public BsonDocument? SustainabilityProfile { get; set; }
}

public enum UserType { Residential = 1, Commercial = 2 }
