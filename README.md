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
  'http://localhost:5080/api/v1/Events' \
  -H 'accept: text/plain'
```

Ответ:

- `HTTP 200` - мероприятия

```json
[
  {
    "id": 1,
    "title": "Rock Concert",
    "description": "Metallica in Moscow",
    "startAt": "2026-05-07T15:00:00Z",
    "endAt": "2026-05-07T17:00:00Z"
  },
  {
    "id": 2,
    "title": "New Year 2026",
    "description": "",
    "startAt": "2025-12-31T22:00:00Z",
    "endAt": "2026-01-01T17:01:00Z"
  }
]
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
