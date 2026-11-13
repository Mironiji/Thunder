using System;
using System.Collections.Generic;
using System.Text;

namespace Thunder.Contracts.Requests
{
    public record IsFreeLoginRequest
    {
        required public string Login { get; init; }
    }
}
