using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("specialties")] //таблица "specialties"
    public class Specialty
    {
        [Key]
        [Column("id")] //первичный ключ
        public int Id { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; } = null!; //название специальности
    }
}