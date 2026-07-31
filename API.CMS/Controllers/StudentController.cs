using API.CMS.Services;
using Library.CMS.Models; // Adjust namespace
using Microsoft.AspNetCore.Mvc;

namespace API.CMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly StudentMongoService _studentService;

        public StudentController(StudentMongoService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Student>>> Get()
        {
            var students = await _studentService.GetAsync();
            return Ok(students);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Student>> Get(int id)
        {
            var student = await _studentService.GetAsync(id);
            if (student is null) return NotFound();
            return Ok(student);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Student newStudent)
        {
            await _studentService.CreateAsync(newStudent);
            return CreatedAtAction(nameof(Get), new { id = newStudent.Id }, newStudent);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Student updatedStudent)
        {
            var student = await _studentService.GetAsync(id);
            if (student is null) return NotFound();

            await _studentService.UpdateAsync(id, updatedStudent);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetAsync(id);
            if (student is null) return NotFound();

            await _studentService.RemoveAsync(id);
            return NoContent();
        }
    }
}