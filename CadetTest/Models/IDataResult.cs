using CadetTest.Models.ResultModels;

namespace CadetTest.Models
{
    public interface IDataResult<T> : IResult
    {
        T Data { get; set; }
    }
}
