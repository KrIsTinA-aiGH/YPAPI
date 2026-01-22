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

        //конструктор получает контекст БД и логгер через dependency injection
        public ScheduleService(AppDbContext db, ILogger<ScheduleService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //1. главный метод - получает расписание для группы
        public async Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate)
        {
            //логируем начало выполнения запроса
            _logger.LogInformation("запрос расписания для группы {GroupName} с {StartDate} по {EndDate}",
                groupName, startDate, endDate);

            try
            {
                //валидация дат
                ValidateDates(startDate, endDate);
                //поиск группы по названию
                var group = await GetGroupByName(groupName);
                //загрузка расписания из БД
                var schedules = await LoadSchedules(group.GroupId, startDate, endDate);
                //преобразование в DTO
                return BuildScheduleDto(startDate, endDate, schedules);
            }
            catch (Exception ex)
            {
                //логируем ошибку и пробрасываем дальше
                _logger.LogError(ex, "ошибка при получении расписания для группы {GroupName}", groupName);
                throw;
            }
        }

        //2. проверка корректности дат
        private static void ValidateDates(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentOutOfRangeException(nameof(start), "дата начала больше даты окончания.");

            if (start < DateTime.Now.AddYears(-1) || end > DateTime.Now.AddYears(1))
                throw new ArgumentOutOfRangeException("диапазон дат не должен превышать 1 год от текущей даты.");
        }

        //3. поиск группы по названию в базе данных
        private async Task<StudentGroup> GetGroupByName(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("название группы не может быть пустым.");

            try
            {
                _logger.LogDebug("поиск группы с названием: {GroupName}", groupName);

                //ищем группу без отслеживания изменений (AsNoTracking)
                var group = await _db.StudentGroups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupName == groupName);

                if (group == null)
                {
                    _logger.LogWarning("группа с названием '{GroupName}' не найдена", groupName);
                    throw new KeyNotFoundException($"группа '{groupName}' не найдена.");
                }

                _logger.LogDebug("группа найдена: {GroupName} (ID: {GroupId})", group.GroupName, group.GroupId);
                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ошибка при поиске группы {GroupName}", groupName);
                throw new InvalidOperationException($"ошибка при поиске группы '{groupName}': {ex.Message}", ex);
            }
        }

        //4. загрузка записей расписания из базы данных
        private async Task<List<Schedule>> LoadSchedules(int groupId, DateTime start, DateTime end)
        {
            try
            {
                _logger.LogDebug("загрузка расписания для группы ID: {GroupId} с {StartDate} по {EndDate}",
                    groupId, start, end);

                //загружаем расписание с включением связанных данных
                var schedules = await _db.Schedules
                    .AsNoTracking()
                    .Where(s => s.GroupId == groupId &&
                               s.LessonDate >= start.Date &&
                               s.LessonDate <= end.Date)
                    .Include(s => s.Weekday) //день недели
                    .Include(s => s.LessonTime) //время занятия
                    .Include(s => s.Subject) //предмет
                    .Include(s => s.Teacher) //преподаватель
                    .Include(s => s.Classroom) //аудитория
                        .ThenInclude(c => c.Building) //здание аудитории
                    .OrderBy(s => s.LessonDate) //сортировка по дате
                    .ThenBy(s => s.LessonTime.LessonNumber) //и номеру пары
                    .ThenBy(s => s.GroupPart) //и части группы
                    .ToListAsync();

                _logger.LogDebug("загружено {Count} записей расписания", schedules.Count);
                return schedules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ошибка при загрузке расписания для группы ID: {GroupId}", groupId);
                throw new InvalidOperationException($"ошибка при загрузке расписания: {ex.Message}", ex);
            }
        }

        //5. построение DTO из данных расписания
        private static List<ScheduleByDateDto> BuildScheduleDto(DateTime startDate, DateTime endDate, List<Schedule> schedules)
        {
            //группируем расписание по датам
            var scheduleByDate = GroupSchedulesByDate(schedules);
            var result = new List<ScheduleByDateDto>();

            //проходим по всем дням в запрошенном диапазоне
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                //пропускаем воскресенье
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                //если на эту дату нет занятий, создаем пустой день
                if (!scheduleByDate.TryGetValue(date, out var daySchedules) || daySchedules == null || !daySchedules.Any())
                {
                    result.Add(BuildEmptyDayDto(date));
                }
                else
                {
                    //иначе создаем день с занятиями
                    result.Add(BuildDayDto(date, daySchedules));
                }
            }

            return result;
        }

        //6. группировка расписания по датам
        private static Dictionary<DateTime, List<Schedule>> GroupSchedulesByDate(List<Schedule> schedules)
        {
            if (schedules == null || !schedules.Any())
                return new Dictionary<DateTime, List<Schedule>>();

            //группируем по дате занятия
            return schedules
                .GroupBy(s => s.LessonDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        //7. создание DTO для дня с занятиями
        private static ScheduleByDateDto BuildDayDto(DateTime date, List<Schedule> daySchedules)
        {
            if (daySchedules == null || !daySchedules.Any())
                return BuildEmptyDayDto(date);

            var firstSchedule = daySchedules.First();
            //группируем занятия по времени и создаем DTO для каждой пары
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

        //8. создание DTO для пустого дня (без занятий)
        private static ScheduleByDateDto BuildEmptyDayDto(DateTime date)
        {
            return new ScheduleByDateDto
            {
                LessonDate = date,
                Weekday = GetWeekdayName(date.DayOfWeek),
                Lessons = new List<LessonDto>()
            };
        }

        //9. получение названия дня недели на русском
        private static string GetWeekdayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "понедельник",
                DayOfWeek.Tuesday => "вторник",
                DayOfWeek.Wednesday => "среда",
                DayOfWeek.Thursday => "четверг",
                DayOfWeek.Friday => "пятница",
                DayOfWeek.Saturday => "суббота",
                DayOfWeek.Sunday => "воскресенье",
                _ => ""
            };
        }

        //10. создание DTO для одного занятия (пары)
        private static LessonDto BuildLessonDto(IGrouping<dynamic, Schedule> lessonGroup)
        {
            var firstRecord = lessonGroup.First();

            var lessonDto = new LessonDto
            {
                LessonNumber = firstRecord.LessonTime.LessonNumber,
                Time = $"{firstRecord.LessonTime.TimeStart:hh\\:mm}-{firstRecord.LessonTime.TimeEnd:hh\\:mm}",
                GroupParts = new Dictionary<LessonGroupPart, LessonPartDto?>()
            };

            //заполняем данные для каждой части группы (FULL, SUB1, SUB2)
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

            //добавляем FULL часть, если её нет (для целой группы)
            if (!lessonDto.GroupParts.ContainsKey(LessonGroupPart.FULL))
                lessonDto.GroupParts[LessonGroupPart.FULL] = null;

            return lessonDto;
        }

        //11. форматирование ФИО преподавателя
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

            //объединяем части, удаляя пустые
            return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();
        }
    }
}