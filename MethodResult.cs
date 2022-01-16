
namespace prowl;

public abstract class MethodResult 
{
    public readonly string Message;
    public MethodResult(string msg) { Message = msg; }
}

public class Success : MethodResult 
{
    public Success(string msg) : base(msg) {}
}

public class Failure : MethodResult
{
    public Failure(string msg) :base(msg) {}
}