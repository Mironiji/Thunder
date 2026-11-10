using System;
using System.Collections.Generic;
using System.Text;
using Thunder.Client.Interfaces;
using Thunder.Contracts.Requests;

namespace Thunder.Client.Transport
{
    internal class ThunderClientTransport : ITransport
    {
        public ThunderClientTransport()
        {
        }

        // релаизация интерфейса ITransport
        public Task ConnectAsync()
        {
            throw new NotImplementedException();
        }
        public Task<bool> SendAsync(RequestEnvelope request)
        {
            return Task.FromResult(false);
        }
        public Task ReceiveLoopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
