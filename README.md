# Integration testing with NUnit
## Agenda
* Integration testing
* REST API
    * Basic
    * Dependency Injection
    * Authentication
        * Unauthorized
        * Authorized
* gRPC

---

## Integration testing

Helpful NuGet packages
```
Moq
FluentAssertions
System.Net.Http.Json
```

---

## REST API
* [Document](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0)
* [Source code](https://github.com/dotnet/AspNetCore.Docs.Samples/tree/main/test/integration-tests/IntegrationTestsSample)
### Basic
**Test project**
```csharp
// (NuGet) Microsoft.AspNetCore.Mvc.Testing 
var factory = new WebApplicationFactory<Program>()
```

**Server project**
```csharp
public partial class Program;
```

### Dependency Injection
```csharp
var factory = new WebApplicationFactory<Program>()
    .WithWebHostBuilder(builder => 
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(it => it.ServiceType == typeof(IStudentRepository));
            services.Remove(descriptor);

            services.AddTransient<IStudentRepository>(pvd => StudentRepositoryMock.Object);
        });
        builder.UseEnvironment("Development");
    });
```

### Authentication
#### 401 Unauthorized
* [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-8.0)

**Server project**
```csharp
// (NuGet) Microsoft.AspNetCore.Authentication.JwtBearer
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, 
        options => builder.Configuration.Bind("JwtSettings", options));
```

Program.cs
```csharp
app.UseAuthorization();
```

#### Authorized
* [Mock Authentication](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#mock-authentication)


**Test project**

Configuration
```csharp
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
```

Authentication handler
```csharp
public class TestAuthHandler
(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
: AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}
```

---

# gRPC
* [Document](https://learn.microsoft.com/en-us/aspnet/core/grpc/test-services?view=aspnetcore-8.0)
* [Source code](https://github.com/dotnet/AspNetCore.Docs/tree/main/aspnetcore/grpc/test-services/sample)

**Test project**

NuGet packages
```
Grpc.Tools
Grpc.Net.ClientFactory
Google.Protobuf
Microsoft.AspNetCore.TestHost
```

Configuration
```csharp
var factory = new WebApplicationFactory<Program>();
var handler = factory.Server.CreateHandler();
var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler });
var client = new Greeter.GreeterClient(channel);
var actual = await client.SayHelloAsync(new HelloRequest { Name = "Au" });
```