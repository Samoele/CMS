using API.CMS.Services;
using Library.CMS.Models; 
using Microsoft.AspNetCore.Mvc;

namespace API.CMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly CourseMongoService _courseService;

        public CourseController(CourseMongoService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Course>>> Get()
        {
            var courses = await _courseService.GetAsync();
            return Ok(courses);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Course>> Get(int id)
        {
            var course = await _courseService.GetAsync(id);
            if (course is null) return NotFound();
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Course newCourse)
        {
            await _courseService.CreateAsync(newCourse);
            return CreatedAtAction(nameof(Get), new { id = newCourse.Id }, newCourse);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Course updatedCourse)
        {
            var course = await _courseService.GetAsync(id);
            if (course is null) return NotFound();

            await _courseService.UpdateAsync(id, updatedCourse);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseService.GetAsync(id);
            if (course is null) return NotFound();

            await _courseService.RemoveAsync(id);
            return NoContent();
        }
    }
}