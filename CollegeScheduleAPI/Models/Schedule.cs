using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("schedule")] //таблица "schedule" - основная таблица расписания
    public class Schedule
    {
        [Key]
        [Column("schedule_id")] //первичный ключ
        public int ScheduleId { get; set; }

        [Column("lesson_date", TypeName = "date")]
        public DateTime LessonDate { get; set; } //дата занятия

        [Column("weekday_id")] //внешний ключ к дню недели
        public int WeekdayId { get; set; }

        [ForeignKey("WeekdayId")]
        public Weekday Weekday { get; set; } = null!;

        [Column("lesson_time_id")] //внешний ключ ко времени занятия
        public int LessonTimeId { get; set; }

        [ForeignKey("LessonTimeId")]
        public LessonTime LessonTime { get; set; } = null!;

        [Column("group_id")] //внешний ключ к учебной группе
        public int GroupId { get; set; }

        [ForeignKey("GroupId")]
        public StudentGroup Group { get; set; } = null!;

        [Column("group_part")]
        public LessonGroupPart GroupPart { get; set; } //часть группы (enum)

        [Column("subject_id")] //внешний ключ к предмету
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; } = null!;

        [Column("teacher_id")] //внешний ключ к преподавателю
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; } = null!;

        [Column("classroom_id")] //внешний ключ к аудитории
        public int ClassroomId { get; set; }

        [ForeignKey("ClassroomId")]
        public Classroom Classroom { get; set; } = null!;
    }
}