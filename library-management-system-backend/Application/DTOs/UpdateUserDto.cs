namespace library_management_system_backend.Application.DTOs
{
    public class UpdateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class BlockUserDto
    {
        public bool Block { get; set; }
    }

}
