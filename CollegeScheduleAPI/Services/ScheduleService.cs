using CollegeSchedule.Data;
using CollegeSchedule.DTO;
using CollegeSchedule.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegeSchedule.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _db;

        public ScheduleService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate)
        {
            ValidateDates(startDate, endDate);
            var group = await GetGroupByName(groupName);
            var schedules = await LoadSchedules(group.GroupId, startDate, endDate);
            return BuildScheduleDto(startDate, endDate, schedules); // ИСПРАВЛЕНО: передаем параметры
        }

        private static void ValidateDates(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentOutOfRangeException(nameof(start), "Дата начала больше даты окончания.");
        }

        private async Task<StudentGroup> GetGroupByName(string groupName)
        {
            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g => g.GroupName == groupName);

            if (group == null)
                throw new KeyNotFoundException($"Группа {groupName} не найдена.");

            return group;
        }

        private async Task<List<Schedule>> LoadSchedules(int groupId, DateTime start, DateTime end)
        {
            return await _db.Schedules
                .Where(s => s.GroupId == groupId &&
                           s.LessonDate >= start &&
                           s.LessonDate <= end)
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                    .ThenInclude(c => c.Building)
                .OrderBy(s => s.LessonDate)
                .ThenBy(s => s.LessonTime.LessonNumber)
                .ThenBy(s => s.GroupPart)
                .ToListAsync();
        }

        // ИСПРАВЛЕННЫЙ МЕТОД: добавлены параметры и правильная логика
        private static List<ScheduleByDateDto> BuildScheduleDto(DateTime startDate, DateTime endDate, List<Schedule> schedules)
        {
            var scheduleByDate = GroupSchedulesByDate(schedules);
            var result = new List<ScheduleByDateDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (!scheduleByDate.TryGetValue(date, out var daySchedules))
                {
                    result.Add(BuildEmptyDayDto(date));
                }
                else
                {
                    result.Add(BuildDayDto(daySchedules));
                }
            }

            return result;
        }

        private static Dictionary<DateTime, List<Schedule>> GroupSchedulesByDate(List<Schedule> schedules)
        {
            return schedules
                .GroupBy(s => s.LessonDate)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static ScheduleByDateDto BuildDayDto(List<Schedule> daySchedules)
        {
            var lessons = daySchedules
                .GroupBy(s => new { s.LessonTime.LessonNumber, s.LessonTime.TimeStart, s.LessonTime.TimeEnd })
                .Select(BuildLessonDto)
                .ToList();

            return new ScheduleByDateDto
            {
                LessonDate = daySchedules.First().LessonDate,
                Weekday = daySchedules.First().Weekday.Name,
                Lessons = lessons
            };
        }

        private static ScheduleByDateDto BuildEmptyDayDto(DateTime date)
        {
            // ИСПРАВЛЕНО: нужно преобразовать DayOfWeek в русское название
            string russianWeekday = date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                _ => ""
            };

            return new ScheduleByDateDto
            {
                LessonDate = date,
                Weekday = russianWeekday,
                Lessons = new List<LessonDto>()
            };
        }

        // ИСПРАВЛЕННЫЙ МЕТОД: правильное форматирование времени и учителя
        private static LessonDto BuildLessonDto(IGrouping<dynamic, Schedule> lessonGroup)
        {
            var firstRecord = lessonGroup.First();

            var lessonDto = new LessonDto
            {
                LessonNumber = firstRecord.LessonTime.LessonNumber,
                Time = $"{firstRecord.LessonTime.TimeStart:hh\\:mm}-{firstRecord.LessonTime.TimeEnd:hh\\:mm}",
                GroupParts = new Dictionary<LessonGroupPart, LessonPartDto?>()
            };

            foreach (var part in lessonGroup)
            {
                lessonDto.GroupParts[part.GroupPart] = new LessonPartDto
                {
                    Subject = part.Subject.Name,
                    Teacher = $"{part.Teacher.LastName} {part.Teacher.FirstName} {part.Teacher.MiddleName ?? ""}".Trim(),
                    TeacherPosition = part.Teacher.Position,
                    Classroom = part.Classroom.RoomNumber,
                    Building = part.Classroom.Building.Name,
                    Address = part.Classroom.Building.Address
                };
            }

            // Если пара общая для всей группы
            if (lessonGroup.Any(s => s.GroupPart == LessonGroupPart.FULL))
            {
                var fullPart = lessonGroup.First(s => s.GroupPart == LessonGroupPart.FULL);
                lessonDto.Subject = fullPart.Subject.Name;
                lessonDto.Teacher = $"{fullPart.Teacher.LastName} {fullPart.Teacher.FirstName} {fullPart.Teacher.MiddleName ?? ""}".Trim();
                lessonDto.TeacherPosition = fullPart.Teacher.Position;
                lessonDto.Classroom = fullPart.Classroom.RoomNumber;
                lessonDto.Building = fullPart.Classroom.Building.Name;
                lessonDto.Address = fullPart.Classroom.Building.Address;
            }
            else
            {
                // Если нет FULL, берем данные из первой подгруппы
                var firstPart = lessonGroup.First();
                lessonDto.Subject = firstPart.Subject.Name;
                lessonDto.Teacher = $"{firstPart.Teacher.LastName} {firstPart.Teacher.FirstName} {firstPart.Teacher.MiddleName ?? ""}".Trim();
                lessonDto.TeacherPosition = firstPart.Teacher.Position;
                lessonDto.Classroom = firstPart.Classroom.RoomNumber;
                lessonDto.Building = firstPart.Classroom.Building.Name;
                lessonDto.Address = firstPart.Classroom.Building.Address;
            }

            if (!lessonDto.GroupParts.ContainsKey(LessonGroupPart.FULL))
                lessonDto.GroupParts.TryAdd(LessonGroupPart.FULL, null);

            return lessonDto;
        }
    }
}