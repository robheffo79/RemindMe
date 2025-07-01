using Microsoft.AspNetCore.Mvc;
using RemindMe.Models;

namespace RemindMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskStore _store;

        public TasksController(TaskStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IEnumerable<TodoTask>> Get() => await _store.LoadAsync();

        [HttpPost]
        public async Task<IActionResult> Post(TodoTask task)
        {
            var tasks = await _store.LoadAsync();
            tasks.Add(task);
            await _store.SaveAsync(tasks);
            return Ok();
        }
    }
}
