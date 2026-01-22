namespace CollegeSchedule.DTO
{
    public class LessonPartDto
    {
        public string Subject { get; set; } = null!; //предмет для этой части группы
        public string Teacher { get; set; } = null!; //преподаватель
        public string TeacherPosition { get; set; } = null!; //должность
        public string Classroom { get; set; } = null!; //аудитория
        public string Building { get; set; } = null!; //здание
        public string Address { get; set; } = null!; //адрес
    }
}