using Grpc.Core;

namespace Server.gRPCAPI.Services;
public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override Task<AddReply> Add(AddRequest request, ServerCallContext context)
    {
        var result = request.First + request.Second;
        return Task.FromResult(new AddReply 
        {
            Result = result
        });
    }
}