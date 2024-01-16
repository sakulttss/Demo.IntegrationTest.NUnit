using Grpc.Core;

namespace Server.gRPCAPI.Services;

public class AuMathService(ICalculator calculator) : AuMath.AuMathBase
{
    public override Task<AdditionResponse> Add(AdditionRequest request, ServerCallContext context)
    {
        return Task.FromResult(new AdditionResponse
        {
            Result = calculator.Add(request.First, request.Second)
        });
    }
}


public interface ICalculator
{
    int Add(int first, int second);
}

public class Calculator : ICalculator
{
    public int Add(int first, int second)
        => first + second;

    public class CalculatorBase
    {
    }
}