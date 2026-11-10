using System;
using System.Collections.Generic;
using System.Text;
using Thunder.Client.Transport;
using System.Security.Cryptography;
using Thunder.Contracts.Requests;
using System.Text.Json;

// СИнглтон для обработки пользовательского ввода, генерации
// вывода, создания задач для передачи по сети
// для ThunderClientTransport
namespace Thunder.Client.Service
{
    internal class ThunderClientService
    {
        private bool _hasLogin = false;

        // бизнес-логика
        private ThunderClientTransport _transport = new();
        public async Task<bool> Login(string login, string password)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));

            var loginRequest = new LoginRequest
            {
                Login = login,
                PasswordHash = Convert.ToHexString(hash)
            };

            var requestEnvelope = new RequestEnvelope
            {
                Type = RequestType.Login,
                Payload = JsonSerializer.SerializeToElement(loginRequest)
            };

            bool success = await _transport.SendAsync(requestEnvelope);

            if (success)
            {
               _hasLogin = true;
            }
            else
            {
                _hasLogin = false;
            }
            // при учпешном логине сессия сохраняется до выхода из приложения
            return success;
        }
    }
}
