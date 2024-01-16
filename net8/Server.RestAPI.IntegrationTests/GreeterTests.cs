using FluentAssertions;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.gRPCAPI;

namespace Server.RestAPI.IntegrationTests;
public class GreeterTests
{
    [Test]
    public async Task SayHello()
    {
        var factory = new WebApplicationFactory<gRPCProgram>();
        var handler = factory.Server.CreateHandler();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler });
        var client = new Greeter.GreeterClient(channel);

        var actual = await client.SayHelloAsync(new HelloRequest { Name = "Au" });
        actual.Should().NotBeNull();
        actual.Message.Should().Be("Hello Au");
    }

    [Test]
    public async Task Add()
    {
        var factory = new WebApplicationFactory<gRPCProgram>();
        var handler = factory.Server.CreateHandler();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler });
        var client = new Greeter.GreeterClient(channel);

        var actual = await client.AddAsync(new AddRequest
        {
            First = 50,
            Second = 100
        });
        actual.Should().NotBeNull();
        actual.Result.Should().Be(150);
    }
}