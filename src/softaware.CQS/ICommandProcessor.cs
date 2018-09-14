using System.Threading.Tasks;

namespace softaware.CQS
{
    public interface ICommandProcessor
    {
        Task ExecuteAsync(ICommand command);
    }
}
