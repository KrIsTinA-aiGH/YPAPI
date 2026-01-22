using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("subject")] //таблица "subject"
    public class Subject
    {
        [Key]
        [Column("subject_id")] //первичный ключ
        public int SubjectId { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; } = null!; //название предмета
    }
}