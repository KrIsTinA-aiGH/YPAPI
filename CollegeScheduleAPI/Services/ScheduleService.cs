using CollegeSchedule.Data;
using CollegeSchedule.DTO;
using CollegeSchedule.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegeSchedule.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(AppDbContext db, ILogger<ScheduleService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 1. Главный метод
        public async Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Запрос расписания для группы {GroupName} с {StartDate} по {EndDate}",
                groupName, startDate, endDate);

            try
            {
                ValidateDates(startDate, endDate);
                var group = await GetGroupByName(groupName);
                var schedules = await LoadSchedules(group.GroupId, startDate, endDate);
                return BuildScheduleDto(startDate, endDate, schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении расписания для группы {GroupName}", groupName);
                throw;
            }
        }

        // 2. Проверка дат
        private static void ValidateDates(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentOutOfRangeException(nameof(start), "Дата начала больше даты окончания.");

            if (start < DateTime.Now.AddYears(-1) || end > DateTime.Now.AddYears(1))
                throw new ArgumentOutOfRangeException("Диапазон дат не должен превышать 1 год от текущей даты.");
        }

        // 3. Поиск группы
        private async Task<StudentGroup> GetGroupByName(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("Название группы не может быть пустым.");

            try
            {
                _logger.LogDebug("Поиск группы с названием: {GroupName}", groupName);

                var group = await _db.StudentGroups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupName == groupName);

                if (group == null)
                {
                    _logger.LogWarning("Группа с названием '{GroupName}' не найдена", groupName);
                    throw new KeyNotFoundException($"Группа '{groupName}' не найдена.");
                }

                _logger.LogDebug("Группа найдена: {GroupName} (ID: {GroupId})", group.GroupName, group.GroupId);
                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске группы {GroupName}", groupName);
                throw new InvalidOperationException($"Ошибка при поиске группы '{groupName}': {ex.Message}", ex);
            }
        }

        // 4. Загрузка расписания
        private async Task<List<Schedule>> LoadSchedules(int groupId, DateTime start, DateTime end)
        {
            try
            {
                _logger.LogDebug("Загрузка расписания для группы ID: {GroupId} с {StartDate} по {EndDate}",
                    groupId, start, end);

                var schedules = await _db.Schedules
                    .AsNoTracking()
                    .Where(s => s.GroupId == groupId &&
                               s.LessonDate >= start.Date &&
                               s.LessonDate <= end.Date)
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

                _logger.LogDebug("Загружено {Count} записей расписания", schedules.Count);
                return schedules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке расписания для группы ID: {GroupId}", groupId);
                throw new InvalidOperationException($"Ошибка при загрузке расписания: {ex.Message}", ex);
            }
        }

        // 5. Построение DTO
        private static List<ScheduleByDateDto> BuildScheduleDto(DateTime startDate, DateTime endDate, List<Schedule> schedules)
        {
            var scheduleByDate = GroupSchedulesByDate(schedules);
            var result = new List<ScheduleByDateDto>();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (!scheduleByDate.TryGetValue(date, out var daySchedules) || daySchedules == null || !daySchedules.Any())
                {
                    result.Add(BuildEmptyDayDto(date));
                }
                else
                {
                    result.Add(BuildDayDto(date, daySchedules));
                }
            }

            return result;
        }

        // 6. Группировка по датам
        private static Dictionary<DateTime, List<Schedule>> GroupSchedulesByDate(List<Schedule> schedules)
        {
            if (schedules == null || !schedules.Any())
                return new Dictionary<DateTime, List<Schedule>>();

            return schedules
                .GroupBy(s => s.LessonDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // 7. DTO для дня с занятиями
        private static ScheduleByDateDto BuildDayDto(DateTime date, List<Schedule> daySchedules)
        {
            if (daySchedules == null || !daySchedules.Any())
                return BuildEmptyDayDto(date);

            var firstSchedule = daySchedules.First();
            var lessons = daySchedules
                .GroupBy(s => new { s.LessonTime.LessonNumber, s.LessonTime.TimeStart, s.LessonTime.TimeEnd })
                .Select(g => BuildLessonDto(g))
                .OrderBy(l => l.LessonNumber)
                .ToList();

            return new ScheduleByDateDto
            {
                LessonDate = date,
                Weekday = firstSchedule.Weekday?.Name ?? GetWeekdayName(date.DayOfWeek),
                Lessons = lessons
            };
        }

        // 8. DTO для пустого дня
        private static ScheduleByDateDto BuildEmptyDayDto(DateTime date)
        {
            return new ScheduleByDateDto
            {
                LessonDate = date,
                Weekday = GetWeekdayName(date.DayOfWeek),
                Lessons = new List<LessonDto>()
            };
        }

        // 9. Получение названия дня недели
        private static string GetWeekdayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                DayOfWeek.Sunday => "Воскресенье",
                _ => ""
            };
        }

        // 10. DTO для одной пары
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
                    Subject = part.Subject?.Name ?? string.Empty,
                    Teacher = FormatTeacherName(part.Teacher),
                    TeacherPosition = part.Teacher?.Position ?? string.Empty,
                    Classroom = part.Classroom?.RoomNumber ?? string.Empty,
                    Building = part.Classroom?.Building?.Name ?? string.Empty,
                    Address = part.Classroom?.Building?.Address ?? string.Empty
                };
            }

            // Добавляем FULL часть, если её нет
            if (!lessonDto.GroupParts.ContainsKey(LessonGroupPart.FULL))
                lessonDto.GroupParts[LessonGroupPart.FULL] = null;

            return lessonDto;
        }

        // 11. Форматирование имени преподавателя
        private static string FormatTeacherName(Teacher teacher)
        {
            if (teacher == null)
                return string.Empty;

            var parts = new List<string>
            {
                teacher.LastName?.Trim() ?? string.Empty,
                teacher.FirstName?.Trim() ?? string.Empty,
                teacher.MiddleName?.Trim() ?? string.Empty
            };

            return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();
        }
    }
}