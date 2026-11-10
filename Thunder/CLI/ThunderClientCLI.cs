using System;
using System.Collections.Generic;
using System.Text;

using Thunder.Client.Service;

// прикладная часть приложения, содержит представление данных,
// обеспечивает ввод/вывод и взаимодействие с логикой ThunderService

namespace Thunder.Client.CLI
{
    internal class ThunderClientCLI
    {
        private bool _isRunnung = true;
        private byte _loginTries = 0;
        private ThunderClientService _service = new();
        

		public async Task RunAsync()
        {
            Animations.PrintStartMessage();

			while (_isRunnung)
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                await Excecute(command);
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
				}

            }
        }
        private async Task WaitOneMinuteToDeleteTries()
        {
            await Task.Delay(60000);
            _loginTries = 0;
        }
        private async Task Login()
        {

            // в фоновом режиме отсчитывается минута, во время которой приложение
            // продолжает работать, но не позволяет пользователю спамить попытками логина
            if (_loginTries > 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please try minute later");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" /\n|\n/");
                Console.ForegroundColor = ConsoleColor.White;
                await WaitOneMinuteToDeleteTries();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Enter 'login:password': ");
            Console.ForegroundColor = ConsoleColor.White;

            var input = Console.ReadLine();
            if (input == Command.menu.ToString())
            {
                Animations.PrintStartMessage();
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Please try again (or 'menu' to enter start menu): ");
                        Console.ForegroundColor = ConsoleColor.White;
                        _loginTries++;
                        await Login();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Login successful.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }

            // если введен рандомный текст - предупреджаем
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter in format 'login:password'\nor 'menu' to start menu");
                await Login();
            }
        }
        private void EnterMessage()
        {
        }
	}
}
