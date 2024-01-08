using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Server.RestAPI.Controllers;
using Server.RestAPI.Repositories;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Server.RestAPI.IntegrationTests;

[TestFixture]
public class StudentTests
{
    //private HttpClient client;

    [SetUp]
    public void Setup()
    {
        // Global setup here.
        //var factory = new WebApplicationFactory<Program>();
        //client = factory.CreateClient();
    }

    [Test]
    public async Task GetAllStudents()
    {
        var factory = new WebApplicationFactory<Program>();
        HttpClient? client = factory.CreateClient();

        var actual = await client.GetFromJsonAsync<IEnumerable<Student>>("/students");

        actual.Should().BeEquivalentTo(new[]
        {
            new Student(1, "John"),
            new Student(2, "Jane"),
            new Student(3, "Jack"),
        });
    }

    [TestCase(1, "John")]
    [TestCase(2, "Jane")]
    [TestCase(3, "Jack")]
    public async Task GetStudentById(int id, string expectedStudentName)
    {
        var factory = new WebApplicationFactory<Program>();
        HttpClient? client = factory.CreateClient();

        var actual = await client.GetFromJsonAsync<Student>($"/students/{id}");
        actual.Should().NotBeNull();
        actual!.Name.Should().Be(expectedStudentName);
    }

    [Test]
    public async Task CreateNewStudent()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var newStudent = new Student(4, "Jill");
        var actual = await client.PostAsJsonAsync<Student>("/students", newStudent);
        actual.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task ConfigDependencyInjection()
    {
        Student[] students = [
            new(1, "Au"),
            new(2, "Tom"),
            new(3, "Biw"),
            new(4, "Tu"),
            new(5, "Marn"),
        ];

        var repoMock = new Mock<IStudentRepository>();

        // Mock Get all students
        repoMock
            .Setup(it => it.GetAllStudents())
            .Returns(students);

        // Mock Get student by id
        repoMock
            .Setup(it => it.GetStudentById(It.IsAny<int>()))
            .Returns<int>(id => students.FirstOrDefault(it => it.Id == id));

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(it => it.ServiceType == typeof(IStudentRepository));
                    services.Remove(descriptor);

                    services.AddTransient<IStudentRepository>(pvd => repoMock.Object);
                });
                builder.UseEnvironment("Development");
            });
        var client = factory.CreateClient();

        // Test get all students
        var actualAllStudents = await client.GetFromJsonAsync<IEnumerable<Student>>("/students");
        actualAllStudents.Should().BeEquivalentTo(students);

        // Test get specific student
        var actual = await client.GetFromJsonAsync<Student>("/students/5");
        actual!.Name.Should().Be("Marn");
    }

    [Test]
    public async Task UpdateStudentWithoutAuth()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var student = new Student(10, "Au");
        var actual = await client.PutAsJsonAsync<Student>($"/students/1", student);

        actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task UpdateStudentWithAuth()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                });
            });
        var client = factory.CreateClient();

        var student = new Student(10, "Au");
        var actual = await client.PutAsJsonAsync<Student>($"/students/1", student);

        actual.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task UpdateStudentWithAuth_BySetupContentType_And_CORS()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                    // Set allow CORS here
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAll", builder =>
                        {
                            builder
                                .AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                        });
                    });
                });
            });
        var client = factory.CreateClient();

        var student = new Student(10, "Au");

        // Set content type here
        var studentJson = JsonSerializer.Serialize(student);
        var content = new StringContent(studentJson, Encoding.UTF8, "application/json");
        var actual = await client.PutAsync($"/students/1", content);

        actual.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// Auth handler for testing
public class TestAuthHandler
    (IOptionsMonitor<AuthenticationSchemeOptions> options, Microsoft.Extensions.Logging.ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Setup user's claims here. (Roles, Privilege, etc.)
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}