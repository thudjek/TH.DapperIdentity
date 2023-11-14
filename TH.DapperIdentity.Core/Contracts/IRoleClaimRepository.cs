﻿using Microsoft.AspNetCore.Identity;

namespace TH.DapperIdentity.Core.Contracts;
public interface IRoleClaimRepository<TKey, TRoleClaim>
    where TKey : IEquatable<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    Task<IEnumerable<TRoleClaim>> GetClaimsAsync(TKey roleId);
}