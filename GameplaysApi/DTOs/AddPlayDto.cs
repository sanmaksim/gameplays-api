﻿namespace GameplaysApi.DTOs
{
    public class AddPlayDto
    {
        public required int UserId { get; set; }

        public required string GameId { get; set; }

        public required int Status { get; set; }
    }
}
