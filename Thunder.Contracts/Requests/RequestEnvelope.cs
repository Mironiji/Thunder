using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Thunder.Contracts.Requests
{
    public record RequestEnvelope
    {
        required public RequestType Type { get; init; }
        required public JsonElement Payload { get; init; }
    }
}
