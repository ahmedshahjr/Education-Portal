using EducationPortal.Application.PriceBooks.Commands.RemovePrice;
using EducationPortal.Application.PriceBooks.Commands.UpdatePriceBook;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.PriceBooks;

/// <summary>The input rules of price books that need no database (SEC-INPUT-01); rules that read the catalogue are in the functional tests.</summary>
public class PriceBookValidatorTests
{
    [Test]
    public void UpdateShouldNeedAnId() =>
        new UpdatePriceBookCommandValidator().Validate(new UpdatePriceBookCommand { Id = Guid.Empty, CurrencyCode = "PKR" }).IsValid.ShouldBeFalse();

    [TestCase("PKR", true)]
    [TestCase("pkr", false)]
    [TestCase("PK", false)]
    [TestCase("", false)]
    public void UpdateShouldCheckTheCurrency(string currency, bool valid) =>
        new UpdatePriceBookCommandValidator().Validate(new UpdatePriceBookCommand { Id = Guid.NewGuid(), CurrencyCode = currency }).IsValid.ShouldBe(valid);

    [Test]
    public void RemoveShouldNeedAPlanOrAModuleButNotBoth()
    {
        var validator = new RemovePriceCommandValidator();
        var id = Guid.NewGuid();

        validator.Validate(new RemovePriceCommand(id, "BASIC", null, BillingCycle.Monthly)).IsValid.ShouldBeTrue();
        validator.Validate(new RemovePriceCommand(id, null, "FEES", BillingCycle.Yearly)).IsValid.ShouldBeTrue();
        validator.Validate(new RemovePriceCommand(id, "BASIC", "FEES", BillingCycle.Monthly)).IsValid.ShouldBeFalse();
        validator.Validate(new RemovePriceCommand(id, null, null, BillingCycle.Monthly)).IsValid.ShouldBeFalse();
        validator.Validate(new RemovePriceCommand(id, "BASIC", null, (BillingCycle)99)).IsValid.ShouldBeFalse();
        validator.Validate(new RemovePriceCommand(Guid.Empty, "BASIC", null, BillingCycle.Monthly)).IsValid.ShouldBeFalse();
    }
}
