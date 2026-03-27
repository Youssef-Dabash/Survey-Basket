using Microsoft.AspNetCore.Authorization;

namespace SurveyBasket.Api.Authentication.Filters;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}