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

        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/groups
        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _context.StudentGroups
                .Include(g => g.Specialty)
                .Select(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    g.Course,
                    Specialty = g.Specialty.Name
                })
                .OrderBy(g => g.GroupName)
                .ToListAsync();

            return Ok(groups);
        }
    }
}