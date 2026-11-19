namespace EducationPortal.Domain.Enums;

/// <summary>
/// How an admission enquiry reached the school: the public form, a phone call, or a visit in person. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum EnquiryChannel
{
    Online = 1,
    Phone = 2,
    InPerson = 3,
}
