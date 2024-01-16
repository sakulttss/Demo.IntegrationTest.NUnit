using Server.RestAPI.Controllers;

namespace Server.RestAPI.Repositories;

public interface IStudentRepository
{
    Student? GetStudentById(int id);
    IEnumerable<Student> GetAllStudents();
    void CreateStudet(Student student);
    void UpdateStudent(Student student);
}

public class StudentRepository : IStudentRepository
{
    private List<Student> _students = new()
    {
        new(1, "John"),
        new(2, "Jane"),
        new(3, "Jack"),
    };

    public Student? GetStudentById(int id)
        => _students.FirstOrDefault(it => it.Id == id);

    public IEnumerable<Student> GetAllStudents()
        => _students;

    public void CreateStudet(Student student)
    {
        if (string.IsNullOrWhiteSpace(student?.Name)) return;
        _students.Add(student);
    }

    public void UpdateStudent(Student student)
    {
        if (!_students.Any(it => it.Id == student.Id)) return;

        var index = _students.FindIndex(it => it.Id == student.Id);
        _students[index] = student;
    }
}