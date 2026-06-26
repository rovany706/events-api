# EventManager API (API для управления мероприятиями)

## Запуск

```bash
dotnet run --project ./src/EventManager.API/EventManager.API.csproj
```

После запуска Swagger будет доступен по адресу: http://localhost:5080/swagger

## Сборка

Для сборки требуется:

- .NET 10

```bash
dotnet publish ./src/EventManager.API/EventManager.API.csproj -c Release -o publish
```

## Тестирование

Тестирование можно запустить с помощью команды:

```bash
dotnet test
```

## Описание API

После запуска сервер доступен по адресу http://localhost:5080 (HTTP), https://localhost:5443 (HTTPS)

Доступные эндпоинты:

### `GET /api/v1/events`

Получение всех мероприятий

Запрос:

```
GET /api/v1/events
```

```bash
curl -X 'GET' \
  'http://localhost:5080/api/v1/Events?Title=Workshop&From=2026-01-01&To=2026-06-01&Page=1&PageSize=10' \
  -H 'accept: text/plain'
```

Параметры запроса:

- Title (опциональный) - фильтр по названию мероприятия (регистронезависимый, частичное совпадение)
- From (опциональный) - фильтр мероприятий, которые начинаются не раньше указанной даты (в формате RFC 3339)
- To (опциональный) - фильтр мероприятий, которые заканчиваются не позже указанной даты (в формате RFC 3339)
- Page (опциональный) - номер страницы результатов
- PageSize (опциональный) - размер страницы

Ответ:

- `HTTP 200` - страница меротприятий

```json
{
  "items": [
    {
      "id": 1,
      "title": "Tech Workshop 2026",
      "description": "Annual gathering of tech leaders discussing AI, cloud computing, and the future of software development.",
      "startAt": "2026-03-10T09:00:00",
      "endAt": "2026-03-12T18:00:00"
    },
    {
      "id": 6,
      "title": "Photography Workshop",
      "description": "Hands-on workshop covering portrait, landscape, and street photography techniques.",
      "startAt": "2026-05-08T10:00:00",
      "endAt": "2026-05-08T16:00:00"
    }
  ],
  "itemCount": 2,
  "currentPage": 1,
  "totalPages": 1,
  "totalItems": 2
}
```

### `GET /api/v1/events/{id}`

Получение мероприятия по идентификатору

Запрос:

```
GET /api/v1/events/1
```

```bash
curl -X 'GET' \
  'http://localhost:5080/api/v1/Events/1' \
  -H 'accept: text/plain'
```

Ответы:

- `HTTP 200` - мероприятие

```json
{
  "id": 1,
  "title": "Rock Concert",
  "description": "Metallica in Moscow",
  "startAt": "2026-05-07T15:00:00Z",
  "endAt": "2026-05-07T17:00:00Z"
}
```

- `HTTP 404` - Мероприятие не найдено

### `POST /api/v1/events`

Создание мероприятия

Запрос:

```
POST /api/v1/events
```

```bash
curl -X 'POST' \
  'http://localhost:5080/api/v1/Events' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "title": "Rock Concert",
  "description": "Metallica in Moscow",
  "startAt": "2026-05-07T15:00:00.000Z",
  "endAt": "2026-05-07T17:00:00.000Z"
}'
```

Ответы:

- `HTTP 201` - Мероприятие создано

```json
{
  "id": 1,
  "title": "Rock Concert",
  "description": "Metallica in Moscow",
  "startAt": "2026-05-07T15:00:00Z",
  "endAt": "2026-05-07T17:00:00Z"
}
```

### `PUT /api/v1/events/{id}`

Обновление информации о мероприятии

Запрос:

```
PUT /api/v1/events/1
```

```bash
curl -X 'PUT' \
  'http://localhost:5080/api/v1/Events/1' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "title": "Jazz Concert",
  "description": "Frank Sinatra in Moscow",
  "startAt": "2026-05-08T15:00:00.000Z",
  "endAt": "2026-05-08T17:00:00.000Z"
}'
```

Ответы:

- `HTTP 204` - Мероприятие обновлено
- `HTTP 404` - Мероприятие не найдено

### `DELETE /api/v1/events/{id}`

Удаление мероприятия

Запрос:

```
DELETE /api/v1/events/1
```

```bash
curl -X 'DELETE' \
  'http://localhost:5080/api/v1/Events/1' \
  -H 'accept: */*'
```

Ответы:

- `HTTP 204` - Мероприятие удалено
- `HTTP 404` - Мероприятие не найдено

### POST /api/v1/events/{id}/book

Бронирование мероприятия

Запрос:

```
GET /api/v1/events/1/book
```

```bash
curl -X 'POST' \
  'http://localhost:5080/api/v1/Events/1/book' \
  -H 'accept: */*'
```

Ответы:

- `HTTP 202` - Бронь зарегистрирована
```json
{
  "id": 1,
  "eventId": 1,
  "status": "Pending"
}
```
- `HTTP 404` - Мероприятие не найдено

### GET /api/v1/bookings/{id}

Получение бронирования по идентификатору

Запрос:

```
GET /api/v1/bookings/1
```

```bash
curl -X 'POST' \
  'http://localhost:5080/api/v1/Bookings/1' \
  -H 'accept: */*'
```

Ответы:

- `HTTP 200` - Возвращается бронирование
```json
{
  "id": 2,
  "eventId": 2,
  "status": "Confirmed",
  "createdAt": "2026-06-25T22:56:10.7207025Z",
  "processedAt": "2026-06-25T22:56:16.8534174Z"
}
```
- `HTTP 404` - Бронирование не найдено

### Бронирование мероприятий

Обработка бронирований на мероприятия осуществляется в фоновом режиме сервисом `BookingProcessorService`.
Он периодически опрашивает очередь задач на наличие новых бронирований, забирает задачу и приступает к её обработке.
Далее, если мероприятие не было удалено, пока задача стояла в очереди, сервис присваевает брони статус "Обработано" или "Отклонено" и сохраняет её в хранилище.

### Ошибки

Ошибки возвращаются в формате ProblemDetails (RFC 7807):

```json
{
  "title": "An error occurred",
  "status": 500,
  "detail": "Fail"
}
```