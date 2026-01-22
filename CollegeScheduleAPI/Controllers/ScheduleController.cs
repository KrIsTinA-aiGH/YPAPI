using CollegeSchedule.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollegeSchedule.Controllers
{
    [ApiController]
    [Route("api/schedule")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _service;

        //конструктор получает сервис расписания через dependency injection
        public ScheduleController(IScheduleService service)
        {
            _service = service;
        }

        //GET: api/schedule/group/{groupName}?start=2026-01-12&end=2026-01-17
        //получает расписание для конкретной группы за указанный период
        [HttpGet("group/{groupName}")]
        public async Task<IActionResult> GetSchedule(string groupName, DateTime start, DateTime end)
        {
            //используем сервис для получения расписания
            var result = await _service.GetScheduleForGroup(groupName, start.Date, end.Date);
            return Ok(result);
        }
    }
}