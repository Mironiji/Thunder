using System;
using System.Collections.Generic;
using System.Text;
using Thunder.Contracts.Requests;

namespace Thunder.Client.Interfaces
{
    public interface ITransport
    {
        Task ConnectAsync();

        // возвращает успешность отправки запроса
        Task<bool> SendAsync(RequestEnvelope request);

        Task ReceiveLoopAsync();
    }
}
