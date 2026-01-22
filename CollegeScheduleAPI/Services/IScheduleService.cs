using CollegeSchedule.DTO;

namespace CollegeSchedule.Services
{
    public interface IScheduleService
    {
        //метод для получения расписания группы за указанный период
        Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate);
    }
}