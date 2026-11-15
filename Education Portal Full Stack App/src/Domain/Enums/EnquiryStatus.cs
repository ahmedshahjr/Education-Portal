namespace EducationPortal.Domain.Enums;

/// <summary>
/// Where an admission enquiry is. Enrolled and Closed are final. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum EnquiryStatus
{
    New = 1,
    Contacted = 2,
    VisitScheduled = 3,
    Applied = 4,
    Enrolled = 5,
    Closed = 6,
}
