using Hangfire.API.People.BackgroundTasks;
using Hangfire.API.People.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PeopleController : ControllerBase
    {
        private readonly ILogger<PeopleController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IPersonTask _personTask;        

        public PeopleController(
            ILogger<PeopleController> logger, 
            IConfiguration configuration,
            IBackgroundJobClient backgroundJobClient, 
            IPersonTask personTask)
        {
            _logger = logger;
            _configuration = configuration;
            _backgroundJobClient = backgroundJobClient;
            _personTask = personTask;            
        }
        
        
        [HttpPost("fire-forget")]
        public IActionResult FireForgetJob()
        {
            _logger.LogInformation("Start fire-and-forget jobs");

            var person = new Person("0001","Andre", "andre@email.com");
            
            var jobId = _backgroundJobClient.Enqueue(() => 
            _personTask.ExportAll(person.Code, person.Name, person.Email));

            _logger.LogInformation("End fire-and-forget jobs");

            return CreatedAtAction(nameof(FireForgetJob), jobId);
        }

        [HttpPost("delayed")]
        public IActionResult DelayedJob()
        {
            _logger.LogInformation("Start fire-and-forget jobs");

            var person = new Person("0001", "Andre", "andre@email.com");

            var jobId = _backgroundJobClient.Schedule(() =>
            _personTask.ExportAll(person.Code, person.Name, person.Email),
            TimeSpan.FromSeconds(60));

            _logger.LogInformation("End delayed jobs");

            return CreatedAtAction(nameof(DelayedJob), jobId);
        }        
    }
}
