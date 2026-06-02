using Microsoft.AspNetCore.Identity;

namespace ESGFiap.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public UserType UserType { get; set; } = UserType.Residential;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WaterSensor> Sensors { get; set; } = [];
    public ICollection<WaterImpactReport> Reports { get; set; } = [];
}

public enum UserType { Residential = 1, Commercial = 2 }
