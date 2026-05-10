using System.ComponentModel.DataAnnotations;

namespace LogisticPackageTrackingApiNet.Domain.Entities;

public class User
{
    [Key]
    [StringLength(200)]
    public string Mail { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";
}