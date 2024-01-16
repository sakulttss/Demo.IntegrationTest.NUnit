using FluentAssertions;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.gRPCAPI;

namespace Server.RestAPI.IntegrationTests;

public class AuMathTests
{
    [Test]
    public async Task SayHello()
    {
        var factory = new WebApplicationFactory<gRPCProgram>();
        var handler = factory.Server.CreateHandler();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler });
        var client = new AuMath.AuMathClient(channel);

        var actual = await client.AddAsync(new AdditionRequest { First = 1, Second = 2 });
        actual.Should().NotBeNull();
        actual.Result.Should().Be(3);
    }
}