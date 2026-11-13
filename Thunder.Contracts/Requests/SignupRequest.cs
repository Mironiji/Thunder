using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Thunder.Contracts.Requests
{
    public record SignupRequest
    {
        public required string Login { get; init; }
        public required string Password { get; init; }
    }
}
