using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("lesson_time")] //таблица "lesson_time"
    public class LessonTime
    {
        [Key]
        [Column("lesson_time_id")] //первичный ключ
        public int LessonTimeId { get; set; }

        [Column("lesson_number")]
        public int LessonNumber { get; set; } //номер пары (1, 2, 3...)

        [Column("time_start")]
        public TimeOnly TimeStart { get; set; } //время начала

        [Column("time_end")]
        public TimeOnly TimeEnd { get; set; } //время окончания
    }
}