using System;
using System.Collections.Generic;
using System.Text;
using Thunder.Contracts.Requests;

namespace Thunder.Client.Interfaces
{
    public interface ITransport
    {
        Task<bool> RequestAsync(RequestEnvelope request);
    }
}
