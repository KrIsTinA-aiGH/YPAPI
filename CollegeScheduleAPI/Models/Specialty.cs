using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("specialties")]
    public class Specialty
    {
        [Key]
        [Column("id")] // Внимание: в БД столбец называется "id", а не "specialty_id"
        public int Id { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; } = null!;
    }
}