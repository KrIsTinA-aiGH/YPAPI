namespace CollegeSchedule.DTO
{
    public class ScheduleByDateDto
    {
        public DateTime LessonDate { get; set; } //дата
        public string Weekday { get; set; } = null!; //день недели
        public List<LessonDto> Lessons { get; set; } = new(); //список занятий в этот день
    }
}