using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("lesson_time")]
    public class LessonTime
    {
        [Key]
        [Column("lesson_time_id")]
        public int LessonTimeId { get; set; }

        [Column("lesson_number")]
        public int LessonNumber { get; set; } // 1, 2, 3, 4

        [Column("time_start")]
        public TimeOnly TimeStart { get; set; } // Тип TimeOnly для времени

        [Column("time_end")]
        public TimeOnly TimeEnd { get; set; }
    }
}