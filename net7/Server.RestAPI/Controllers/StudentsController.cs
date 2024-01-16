using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.RestAPI.Repositories;

namespace Server.RestAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentRepository _repository;

    public StudentsController(IStudentRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IEnumerable<Student> Get()
        => _repository.GetAllStudents();

    [HttpGet("{id}")]
    public Student? Get(int id)
        => _repository.GetStudentById(id);

    [HttpPost]
    public void Post([FromBody] Student student)
        => _repository.CreateStudet(student);

    [Authorize]
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] Student student)
        => _repository.UpdateStudent(student with { Id = id });
}

public record Student(int Id, string Name);