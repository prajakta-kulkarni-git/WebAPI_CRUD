using System.ComponentModel.DataAnnotations;

namespace UserWebAPI.Model
{
    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public long? ContactNumber { get; set; }
    }
}
