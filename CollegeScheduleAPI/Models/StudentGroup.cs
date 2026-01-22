using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("student_group")] //таблица "student_group"
    public class StudentGroup
    {
        [Key]
        [Column("group_id")] //первичный ключ
        public int GroupId { get; set; }

        [Column("group_name")]
        [Required]
        public string GroupName { get; set; } = null!; //название группы

        [Column("course")]
        public int Course { get; set; } //курс

        [Column("specialty_id")] //внешний ключ к специальности
        public int SpecialtyId { get; set; }

        [ForeignKey("SpecialtyId")] //связь с Specialty
        public Specialty Specialty { get; set; } = null!;
    }
}