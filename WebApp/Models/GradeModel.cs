using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models;

public class GradeModel
{
    [Key]
    public int id { get; set; }
    [Required]
    [ForeignKey(nameof(UserModel))]
    public int userId { get; set; }
    [Required]
    public Subject subject { get; set; }
    [Required]
    [Range(0.00, 10.00)]
    public decimal grade { get; set; }
    [Required]
    [Range(0, 100)]
    public int percentageInfluence { get; set; } = 100;
    [Required]
    public DateOnly dateAdded { get; set; }
}
