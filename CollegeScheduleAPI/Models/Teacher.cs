using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("teacher")] //таблица "teacher"
    public class Teacher
    {
        [Key]
        [Column("teacher_id")] //первичный ключ
        public int TeacherId { get; set; }

        [Column("last_name")]
        [Required]
        public string LastName { get; set; } = null!; //фамилия

        [Column("first_name")]
        [Required]
        public string FirstName { get; set; } = null!; //имя

        [Column("middle_name")]
        public string? MiddleName { get; set; } //отчество (может быть null)

        [Column("position")]
        [Required]
        public string Position { get; set; } = null!; //должность
    }
}