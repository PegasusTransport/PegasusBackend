﻿namespace PegasusBackend.DTOs.DriverDTO
{
    public class AllDriversDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;

    }
}