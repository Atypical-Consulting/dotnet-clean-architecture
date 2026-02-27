// <copyright file="ExternalUserService.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace CleanArchitecture.Infrastructure.ExternalAuthentication;

using System.Security.Claims;
using CleanArchitecture.Application.Services;
using Microsoft.AspNetCore.Http;

/// <inheritdoc />
public sealed class ExternalUserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public ExternalUserService(
        IHttpContextAccessor httpContextAccessor) =>
        this._httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public string GetCurrentUserId()
    {
        ClaimsPrincipal user = this._httpContextAccessor
                .HttpContext!
            .User;

        string id = user.FindFirst("sub")?.Value!;
        return id;
    }
}
