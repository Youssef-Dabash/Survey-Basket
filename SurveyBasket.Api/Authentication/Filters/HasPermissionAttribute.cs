using Microsoft.AspNetCore.Authorization;

namespace SurveyBasket.Api.Authentication.Filters;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission)
{
}