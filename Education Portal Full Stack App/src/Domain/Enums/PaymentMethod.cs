namespace EducationPortal.Domain.Enums;

/// <summary>How a manual payment reached the platform owner. There is no card or gateway payment in this phase.</summary>
public enum PaymentMethod
{
    BankTransfer = 1,

    Cash = 2,

    Cheque = 3,

    MobileWallet = 4,

    Other = 5,
}
