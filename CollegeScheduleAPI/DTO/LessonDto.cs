using CollegeSchedule.Models;

namespace CollegeSchedule.DTO
{
    public class LessonDto
    {
        public int LessonNumber { get; set; } //номер пары
        public string Time { get; set; } = null!; //время пары (например "09:00-10:30")
        public string Subject { get; set; } = null!; //название предмета
        public string Teacher { get; set; } = null!; //ФИО преподавателя
        public string TeacherPosition { get; set; } = null!; //должность преподавателя
        public string Classroom { get; set; } = null!; //номер аудитории
        public string Building { get; set; } = null!; //здание
        public string Address { get; set; } = null!; //адрес здания

        //словарь с данными для каждой части группы
        //ключ - часть группы (FULL, SUB1, SUB2), значение - данные занятия
        public Dictionary<LessonGroupPart, LessonPartDto?> GroupParts { get; set; } = new();
    }
}