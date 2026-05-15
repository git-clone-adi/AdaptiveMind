namespace AdaptiveMind.Core.Interfaces;

using System.Threading.Tasks;
using AdaptiveMind.Core.Models;

public interface IAdaptiveResponseService
{
    Task<string> GenerateResponseAsync(UserTicket ticket, UserBehaviorProfile profile);
}
