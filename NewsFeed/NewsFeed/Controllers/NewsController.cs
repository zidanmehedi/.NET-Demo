using Microsoft.AspNetCore.Mvc;
using NewsFeed.Entities;
using NewsFeed.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NewsFeed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsFeedService _newsFeedService;
        public NewsController(INewsFeedService newsFeedSerivce)
        {
            _newsFeedService = newsFeedSerivce;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllNewsAsync()
        {
            var news = await _newsFeedService.GetAllNewsAsync();
            if (news.Any())
            {
                return Ok(news);
            }

            return NotFound();
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsByIdAsync(int id)
        {
            var record = await _newsFeedService.GetNewsByIdAsync(id);
            if (record is not null)
            {
                return Ok(record);
            }

            return NotFound();
        }

        // POST api/<ValuesController>
        [HttpPost]
        public async Task<IActionResult> CreateNewsAsync([FromBody] News news)
        {
            await _newsFeedService.CreateNewsAsync(news);
            return Created();
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNewsAsync(int id, [FromBody] News news)
        {
            var isSuccess = await _newsFeedService.UpdateNewsAsync(id, news);
            return isSuccess ? NoContent() : NotFound();
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isSuccess = await _newsFeedService.DeleteNewsAsync(id);
            return isSuccess ? NoContent() : NotFound();
        }
    }
}
