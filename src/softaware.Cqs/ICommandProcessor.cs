using System.Threading.Tasks;

namespace softaware.Cqs
{
    public interface ICommandProcessor
    {
        Task ExecuteAsync(ICommand command);
    }
}
