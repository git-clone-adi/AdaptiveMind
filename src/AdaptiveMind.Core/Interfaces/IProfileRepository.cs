namespace AdaptiveMind.Core.Interfaces;

using System;
using System.Threading.Tasks;
using AdaptiveMind.Core.Models;

public interface IProfileRepository
{
    Task<UserBehaviorProfile> GetOrCreateProfileAsync(Guid userId);
    Task UpdateProfileAsync(UserBehaviorProfile profile);
    Task<UserBehaviorProfile?> GetProfileAsync(Guid userId);
}
