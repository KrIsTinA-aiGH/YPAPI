using CollegeSchedule.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeSchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        //конструктор получает контекст базы данных через dependency injection
        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        //GET: api/groups - получает все учебные группы
        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            //загружаем группы из базы данных вместе со связанными специальностями
            var groups = await _context.StudentGroups
                .Include(g => g.Specialty) //загружаем связанную специальность
                .Select(g => new //проекция в анонимный объект для клиента
                {
                    g.GroupId,
                    g.GroupName,
                    g.Course,
                    Specialty = g.Specialty.Name //название специальности вместо ID
                })
                .OrderBy(g => g.GroupName) //сортируем по названию группы
                .ToListAsync();

            //возвращаем результат в формате JSON
            return Ok(groups);
        }
    }
}