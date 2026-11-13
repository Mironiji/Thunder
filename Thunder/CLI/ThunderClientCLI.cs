using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using Thunder.Client.Service;

// прикладная часть приложения, содержит представление данных,
// обеспечивает ввод/вывод и взаимодействие с логикой ThunderService

namespace Thunder.Client.CLI
{
    internal class ThunderClientCLI
    {
        private bool _isRunnung = true;
        private byte _loginTries = 0;
        private string prompt = "> ";
        private ThunderClientService _service;
        public ThunderClientCLI(ThunderClientService service)
        {
            _service = service;
        }

		public async Task RunAsync()
        {
            Animations.PrintThunderAnimation();
            Animations.PrintStartMessage();


            while (_isRunnung)
            {
                Console.Write(prompt);
                var command = Console.ReadLine();
                if (command is null)
                    break;

                try
                {
                    await Excecute(command);
                }
                catch (Exception exception) when (exception is WebSocketException
                    or InvalidDataException or JsonException or OperationCanceledException)
                {
                    Animations.PrintRed(exception.Message);
                }
            }
        }   
        // определеяет команду и вызывает соответствующий метод сервиса
        private async Task Excecute(string? input)
        {
            if (input == null) { return; };

            input.ToLower();
            if (Enum.TryParse<Command>(input, out Command command))
            {
                switch (command)
                {
                    case Command.help: { Animations.PrintHelp(); break; }
					case Command.exit: { Animations.PrintThunderAnimation(); _isRunnung = false; break; }
					case Command.login: await Login(); break;
                    // todo выделить меню в отдельную функцию, показывать счетчик пропущенных
                    case Command.menu: { await Menu(); break; }
                    case Command.signup:
                        {
                            await SignUp();
                            break; 
                        }
                }

            }
        }

        private async Task Menu()
        {
            bool is_auth = _service.IsAuthenticated;
            if (is_auth)
            {
                Animations.PrintAuthtorizedMenu();
            }
            else
            {
                Animations.PrintUnAuthtorizedMenu();
            }
        }

        private async Task SignUp()
        {
            Animations.PrintMagenta("New login (or menu): ");

            Console.Write("> ");
            string? login = Console.ReadLine();
            if (login is null)
                return;

            if (login == Command.menu.ToString())
            {
                Animations.PrintThunderAnimation();
                return;
            }

            if (string.IsNullOrWhiteSpace(login) || login.Length < 2)
            {
                Animations.PrintRed("Incorrect login");
                return;
            }

            if (!await _service.IsFreeLogin(login))
            {
                Animations.PrintRed("Login is already taken");
                await SignUp();
                return;
            }

            Animations.PrintMagenta("New password (at least 3 symbols): ");
            Console.Write("> ");

            var password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
            {
                Animations.PrintRed("Incorrect password");
                return;
            }

            if (!await _service.Signup(login, password))
            {
                Animations.PrintRed("Login is already taken");
                return;
            }

            Animations.PintHorizontalLightAnimation();
            prompt = login + "> ";
            Animations.PrintGreen("Remember your new login & password");
        }
        private void WaitOneMinuteToDeleteTries()
        {

            Thread.Sleep(60000);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("(you can try login again)");
            Console.ForegroundColor = ConsoleColor.White;
            _loginTries = 0;
        }
        private async Task Login()
        {
            // в фоновом режиме отсчитывается минута, во время которой приложение
            // продолжает работать, но не позволяет пользователю спамить попытками логина
            if (_loginTries > 5)
            {
                Animations.PrintRed("Please try minute later");

                Thread timer = new Thread(WaitOneMinuteToDeleteTries);
                timer.Start();

                return;
            }

            Animations.PrintMagenta("Enter 'login:password': ");

            Console.Write("> ");
            var input = Console.ReadLine();
            if (input is null)
                return;
            if (input == Command.menu.ToString())
            {
                Animations.PrintThunderAnimation();
                return;
            }

            var parts = input?.Split(":");

            // если введен шаблон логин:пароль - проверяем на сервере
            if (parts?.Length == 2)
            {
                var login = parts?[0] ?? null;
                var password = parts?[1] ?? null;

                if (login != null && password != null)
                {
                    bool hasLogin = await _service.Login(login, password);
                    if (!hasLogin)
                    {
                        Animations.PrintRed("Please try again (or 'menu' to enter start menu): ");
                        _loginTries++;
                        await Login();
                    }
                    else
                    {
                        prompt = login + "> ";
                        Animations.PrintGreen("Login successful.");
                    }
                }
            }

            // если введен рандомный текст - предупреджаем
            else
            {
                Animations.PrintRed("Please enter in format 'login:password'\nor 'menu' to start menu");
                await Login();
            }
        }
        private void EnterMessage()
        {
        }
	}
}
