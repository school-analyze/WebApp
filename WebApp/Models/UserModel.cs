using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class UserModel
{
    [Key]
    public int id { get; set; }
    [Required]
    [MaxLength(50)]
    public string userName { get; set; }
    [Required]
    public string passwordHash { get; set; }
}