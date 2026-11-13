# WebSocket-сервер Thunder

Шаблон использует ASP.NET Core на .NET 10 LTS и встроенный WebSocket middleware. Для сборки нужен SDK .NET 10.

## Запуск

Выполните команды из корня репозитория в PowerShell.

```powershell
dotnet build Thunder.slnx -m:1
dotnet run --project Thunder.Server --launch-profile http
```

Откройте http://localhost:5037/ в браузере. Ответ `{"status":"ok"}` означает, что сервер принимает HTTP-запросы. WebSocket доступен по адресу `ws://localhost:5037/ws`. Обычный HTTP-запрос на `/ws` вернёт статус 400.

На открытой странице выполните следующий код в консоли браузера.

```javascript
const socket = new WebSocket("ws://localhost:5037/ws");
socket.onmessage = event => console.log(JSON.parse(event.data));
socket.onclose = event => console.log("Closed", event.code, event.reason);
socket.onopen = () => socket.send(JSON.stringify({ Type: 1, Payload: "Привет, Thunder!" }));
```

Ожидаемый ответ — `{"Ok":true,"Payload":"Привет, Thunder!"}`. После ответа можно отправить ещё один запрос через `socket.send(...)` или закрыть соединение через `socket.close(1000)`.

Для локального WSS сначала доверьте сертификат разработки, затем запустите профиль HTTPS.

```powershell
dotnet dev-certs https --trust
dotnet run --project Thunder.Server --launch-profile https
```

Откройте https://localhost:7258/ и замените адрес в примере на `wss://localhost:7258/ws`.

## Как устроен шаблон

- `Program.cs` регистрирует обработчик и маршруты. Middleware посылает протокольный ping каждые 30 секунд и ждёт pong до 15 секунд.
- `ThunderWSockServer/WebSocketServer.cs` ведёт сессию. `HandleAsync` принимает соединение, `ReceiveMessagesAsync` собирает полное сообщение, а `HandleMessage` разбирает JSON и выбирает ответ. Сессия завершается при закрытии клиентом, обрыве связи или остановке сервера.
- Запрос использует общий `RequestEnvelope` из `Thunder.Contracts`. Имена полей `Type` и `Payload` учитывают регистр, `Type` передаётся числом. Текущие значения — `Login = 0` и `Message = 1`.
- `Message` принимает строку в `Payload` и возвращает её клиенту. Ошибки имеют вид `{"Ok":false,"Error":"invalid_payload"}`. Возможные коды — `invalid_request` для невалидного JSON или конверта, `invalid_payload` для неверного содержимого `Message` и `unsupported_type` для остальных команд, включая `Login`. После этих ошибок соединение остаётся открытым.
- Входящее сообщение может состоять из нескольких фрагментов. Лимит на всё сообщение — 16 КиБ в UTF-8. Сервер закрывает соединение с кодом 1009 при превышении лимита и с кодом 1003 при бинарном сообщении.

## Как продолжить разработку

1. **Замените демонстрационный ответ.** Откройте `HandleMessage` в `WebSocketServer.cs` и измените ветку `RequestType.Message`. Если вместо строки нужен объект, создайте DTO в `Thunder.Contracts`, десериализуйте `Payload` в этот тип и проверьте поля. Возвращайте `invalid_payload`, если данные команды не прошли проверку.
2. **Добавляйте команды через общий контракт.** Добавьте новое значение с явным числовым кодом в `Thunder.Contracts/Requests/RequestType.cs` и ветку в `HandleMessage`. Сохраните существующие значения `0` и `1`, чтобы клиенты продолжили понимать протокол. Когда клиенту понадобятся типизированные ответы, вынесите модели ответов в `Thunder.Contracts`.
3. **Вынесите бизнес-логику, когда она появится.** Сделайте разбор и обработку команды асинхронными для вызовов БД или внешнего API. Передавайте токен из цикла приёма. Если сервис использует scoped-зависимости вроде `DbContext`, создавайте отдельную область DI на команду, чтобы её срок жизни не равнялся сроку жизни сокета.
4. **Подключите CLI.** В `Thunder/Transport/ThunderClientTransport.cs` реализуйте `ConnectAsync` через `ClientWebSocket` с адресом `ws://localhost:5037/ws` из конфигурации. В `SendAsync` сериализуйте `RequestEnvelope` через `JsonSerializer.SerializeToUtf8Bytes` и отправьте его текстовым сообщением. В `ReceiveLoopAsync` собирайте ответ до `EndOfMessage` и разбирайте поля `Ok`, `Payload` и `Error`. Результат отправки и результат команды различаются — вход считается успешным после соответствующего ответа сервера. Когда понадобятся параллельные команды, добавьте идентификатор запроса и возвращайте его в ответе. Для одного сокета держите один цикл чтения и выполняйте отправки последовательно.
5. **Подключите аутентификацию перед внешним запуском.** Для закрытого сервиса проверяйте пользователя при рукопожатии и права при выполнении команд. Для браузерного клиента с заданными доменами настройте `AllowedOrigins` у `WebSocketOptions`. Опубликуйте endpoint через `wss://` с доверенным TLS-сертификатом. Если есть прокси, проверьте поддержку WebSocket и тайм-аут простоя соединения.
6. **Проверяйте изменения через живое соединение.** После добавления команды проверьте успешный ответ, неверный `Payload`, повторный запрос после ошибки и корректное закрытие. После изменения транспорта проверьте фрагментацию и лимит сообщения.

Документация Microsoft — [WebSocket в ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/websockets?view=aspnetcore-10.0) и [ClientWebSocket в .NET](https://learn.microsoft.com/dotnet/fundamentals/networking/websockets).
