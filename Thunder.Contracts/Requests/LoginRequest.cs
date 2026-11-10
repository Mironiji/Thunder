using System;
using System.Collections.Generic;
using System.Text;

namespace Thunder.Contracts.Requests
{
    public record LoginRequest
    {
        public required string Login { get; init; }
        public required string PasswordHash { get; init; }
    }
}
