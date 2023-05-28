namespace CadetTest.Models.ResultModels
{
    public interface IResult
    {
        bool IsSuccess { get; }
        string Message { get; }
    }
}
