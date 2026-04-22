using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class CustomerDto
    {
        public int Id    { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Region { get; set; } = null!;

    }
    public class LeadCreateDto
    {

        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        public string Name { get; set; } = null!;
        public string? Notes { get; set; }
        public LeadSource Source { get; set; }

    }
    public class LeadListItemDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
        public string? ConvertedCustomerId { get; set; }
        public DateTime? ConvertedAt { get; set; }
        public string? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public LeadSource Source { get; set; }
        public LeadStatus Status { get; set; }

    }
}
