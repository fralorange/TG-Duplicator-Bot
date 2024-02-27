using Telegram.Bot.Types;

namespace FreelanceBotBase.Bot.Commands.Interface
{
    public interface IExternalCommand
    {
        Task<int[]> ExecuteAsync(List<Message> messages, CancellationToken cancellationToken);
    }
}
