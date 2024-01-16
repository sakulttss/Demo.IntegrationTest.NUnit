using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.RestAPI.Repositories;

namespace Server.RestAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController(IStudentRepository repository) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Student> Get()
        => repository.GetAllStudents();

    [HttpGet("{id}")]
    public Student? Get(int id)
        => repository.GetStudentById(id);

    [HttpPost]
    public void Post([FromBody] Student student)
        => repository.CreateStudet(student);

    [Authorize]
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] Student student)
        => repository.UpdateStudent(student with { Id = id });
}

public record Student(int Id, string Name);