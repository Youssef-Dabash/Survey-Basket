global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Mapster;
global using FluentValidation;

global using SurveyBasket.Entities;
global using SurveyBasket.Persistence;
global using SurveyBasket.Contracts.Polls;
global using SurveyBasket.Contracts.Authentication;

global using Asp.Versioning;
global using Asp.Versioning.ApiExplorer;
global using FluentValidation.AspNetCore;
global using Hangfire;
global using MapsterMapper;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.UI.Services;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.IdentityModel.Tokens;
global using Serilog;
global using SurveyBasket.Api.Authentication.Filters;
global using SurveyBasket.Api.Entities;
global using SurveyBasket.Api.Errors;
global using SurveyBasket.Api.Extensions;
global using SurveyBasket.Api.Health;
global using SurveyBasket.Api.Services.ClassServices;
global using SurveyBasket.Api.Services.InterfaceServices;
global using SurveyBasket.Api.Settings;
global using SurveyBasket.Authentication;
global using System.Reflection;
global using System.Text;
global using System.Threading.RateLimiting;
global using SurveyBasket.Api.OpenApiTransformers;

global using HangfireBasicAuthenticationFilter;
global using HealthChecks.UI.Client;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Scalar.AspNetCore;
global using SurveyBasket.Api;

global using System.ComponentModel.DataAnnotations;

global using SurveyBasket.Api.Abstractions;
global using SurveyBasket.Api.Abstractions.Consts;
global using SurveyBasket.Api.Contracts.Authentication;
global using SurveyBasket.Api.Contracts.Users;
global using SurveyBasket.Api.Contracts.Common;
global using SurveyBasket.Api.Contracts.Questions;
global using SurveyBasket.Api.Contracts.Roles;
global using Microsoft.AspNetCore.OutputCaching;
global using SurveyBasket.Api.Contracts.Votes;
