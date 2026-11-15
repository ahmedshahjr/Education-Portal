using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Schools.Registration;

/// <summary>The fields a school is registered with (the master adding it, or the approval of a school request).</summary>
public sealed record SchoolRegistrationRequest(
    string Name,
    string? Domain,
    Guid CountryId,
    DataRegion? DataRegion,
    string AdministratorEmail,
    string AdministratorPassword);

/// <summary>The ids of a registered school and of its first administrator, for the response and for the audit records.</summary>
public sealed record RegisteredSchool(int Id, Guid PublicId, string AdministratorId);

/// <summary>
/// What a school gets when it is registered: its country, the modules that are on by default (free, for the whole school),
/// and a free trial of the Full plan for <see cref="TrialDays"/> days, from the registration. The trial is a subscription,
/// priced from the country's price book, so the country must have one. The trial gives the plan's modules and every core
/// module; the default grants stay on when the trial ends.
/// </summary>
public static class SchoolRegistration
{
    /// <summary>The length of the free trial a registered school gets.</summary>
    public const int TrialDays = 30;

    /// <summary>
    /// Registers a school: the school, its default grants, its trial and its first administrator. The caller runs it inside
    /// <see cref="IApplicationDbContext.ExecuteInTransactionAsync{T}"/>, so every save here is committed or rolled back
    /// together with the caller's own changes. Audit records are written by the caller after the commit.
    /// </summary>
    public static async Task<RegisteredSchool> RegisterAsync(
        IApplicationDbContext context,
        IIdentityService identity,
        TimeProvider timeProvider,
        SchoolRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var school = School.Create(request.Name, request.Domain);
        school.AssignCountry(await context.Countries.ResolveIdAsync(request.CountryId, cancellationToken));
        school.AssignDataRegion(request.DataRegion);

        context.Schools.Add(school);
        await context.SaveChangesAsync(cancellationToken);

        await SchoolDefaultModules.GrantAsync(context, school.Id, now, cancellationToken);
        await StartTrialAsync(context, school, now, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var (result, administratorId) = await identity.CreateSchoolAdministratorAsync(
            school.Id,
            request.AdministratorEmail.Trim(),
            request.AdministratorPassword);

        if (!result.Succeeded)
        {
            // Throwing makes the caller's transaction roll the school back too. The policy messages say what to change.
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(result.Errors.Select(e => new ValidationFailure(nameof(request.AdministratorPassword), e)));
        }

        return new RegisteredSchool(school.Id, school.PublicId, administratorId);
    }

    /// <summary>
    /// Starts the trial. Its price is the Full plan's monthly price in the country's price book. The caller saves the changes.
    /// </summary>
    private static async Task StartTrialAsync(IApplicationDbContext context, School school, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (school.CountryId is not { } countryId)
        {
            throw new InvalidOperationException("A registered school has a country, so it can be given a trial.");
        }

        var countryCode = await context.Countries
            .Where(c => c.Id == countryId)
            .Select(c => c.Code)
            .SingleAsync(cancellationToken);

        var quote = await SubscriptionPlanner.QuoteAsync(
            context, countryCode, PricingCatalog.Full, [], BillingCycle.Monthly, cancellationToken);

        var subscription = Subscription.Create(
            school.Id, quote.Plan, quote.PriceBook, BillingCycle.Monthly, quote.Amount, now, TrialDays);

        var mainCampus = await SchoolCampuses.MainAsync(context, school.Id, cancellationToken);
        foreach (var module in quote.Modules)
        {
            subscription.AddModule(module, mainCampus);
        }

        context.Subscriptions.Add(subscription);
    }
}
