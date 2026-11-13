using System;
using System.Collections.Generic;
using System.Text;
using Thunder.Client.Interfaces;
using Thunder.Contracts.Requests;
using System.Text.Json;

// обработка пользовательского ввода, генерации
// вывода, создания задач для передачи по сети
// для ThunderClientTransport
namespace Thunder.Client.Service
{
    internal class ThunderClientService
    {
        public bool IsAuthenticated { get; private set; } = false;

        // бизнес-логика
        private readonly ITransport _transport;
        public ThunderClientService(ITransport transport)
        {
            _transport = transport;
        }
        public async Task<bool> Login(string login, string password)
        {
            var loginRequest = new LoginRequest
            {
                Login = login,
                Password = password
            };

            var requestEnvelope = new RequestEnvelope
            {
                Type = RequestType.Login,
                Payload = JsonSerializer.SerializeToElement(loginRequest)
            };

            IsAuthenticated = await _transport.RequestAsync(requestEnvelope);
            return IsAuthenticated;
        }
        public async Task<bool> IsFreeLogin(string login)
        {
            var loginRequest = new IsFreeLoginRequest
            {
                Login = login,
            };

            var requestEnvelope = new RequestEnvelope
            {
                Type = RequestType.IsFreeLogin,
                Payload = JsonSerializer.SerializeToElement(loginRequest)
            };

            return await _transport.RequestAsync(requestEnvelope);
        }

        public async Task<bool> Signup(string login, string password)
        {
            var signupRequest = new SignupRequest { Login = login, Password = password };
            var requestEnvelope = new RequestEnvelope
            {
                Type = RequestType.SignUp,
                Payload = JsonSerializer.SerializeToElement(signupRequest)
            };

            var IsAuthenticated = await _transport.RequestAsync(requestEnvelope);
            return IsAuthenticated;
        }

    }
}
