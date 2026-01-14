# API Documentation

Complete REST API reference for MusicForge.

## Base URL

```
http://localhost:5001
```

## Authentication

Currently no authentication required for development. Production deployments should implement appropriate authentication.

---

## Endpoints

### Health

#### Check API Health

```http
GET /api/health
```

**Response:**

```json
{
  "status": "Healthy",
  "version": "1.0.0",
  "workerConnected": true,
  "gpuAvailable": false
}
```

#### Check Worker Health

```http
GET /api/health/worker
```

**Response:**

```json
{
  "status": "healthy",
  "version": "1.0.0",
  "workerConnected": true,
  "gpuAvailable": true
}
```

---

### Projects

#### List All Projects

```http
GET /api/projects
```

**Response:**

```json
[
  {
    "id": "a1b2c3d4-e5f6-...",
    "name": "Summer Vibes",
    "status": "Complete",
    "genre": "Electronic",
    "tempoBpm": 128,
    "durationSeconds": 180,
    "createdAt": "2026-01-14T10:00:00Z",
    "updatedAt": "2026-01-14T11:30:00Z"
  }
]
```

---

#### Get Project Details

```http
GET /api/projects/{id}
```

**Parameters:**

| Name | Type | Description |
|------|------|-------------|
| `id` | GUID | Project unique identifier |

**Response:**

```json
{
  "id": "a1b2c3d4-e5f6-...",
  "name": "Summer Vibes",
  "description": "A chill summer track",
  "status": "Complete",
  "genre": "Electronic",
  "mood": "Chill",
  "tempoBpm": 110,
  "key": "F Major",
  "durationSeconds": 180,
  "hasVocals": true,
  "lyrics": "Feel the summer breeze...",
  "masterFilePath": "/output/summer-vibes-master.wav",
  "stems": [
    {
      "name": "drums",
      "path": "/output/stems/drums.wav",
      "duration": "00:03:00"
    },
    {
      "name": "bass",
      "path": "/output/stems/bass.wav",
      "duration": "00:03:00"
    }
  ],
  "arrangement": {
    "chordProgression": ["F", "Am", "Dm", "Bb"],
    "totalBars": 64,
    "sections": [
      {
        "name": "Intro",
        "startBar": 1,
        "durationBars": 8,
        "energyLevel": 0.3,
        "elements": ["pads", "ambient"]
      }
    ]
  },
  "createdAt": "2026-01-14T10:00:00Z",
  "updatedAt": "2026-01-14T11:30:00Z"
}
```

---

#### Create Project

```http
POST /api/projects
Content-Type: application/json
```

**Request Body:**

```json
{
  "name": "Summer Vibes",
  "description": "A chill summer track with tropical vibes",
  "genre": "Electronic",
  "mood": "Chill",
  "tempoBpm": 110,
  "key": "F Major",
  "durationSeconds": 180,
  "hasVocals": true,
  "lyrics": "Feel the summer breeze..."
}
```

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `name` | string | Yes | Project name |
| `description` | string | Yes | Project description |
| `genre` | string | Yes | Music genre |
| `mood` | string | Yes | Mood/energy |
| `tempoBpm` | int | Yes | Tempo (40-300) |
| `key` | string | Yes | Musical key |
| `durationSeconds` | int | Yes | Target duration |
| `hasVocals` | bool | No | Include vocals |
| `lyrics` | string | No | Vocal lyrics |

**Supported Genres:**

```
Electronic, Rock, Pop, Jazz, Classical, HipHop, 
RnB, Country, Folk, Metal, Ambient, LoFi
```

**Supported Moods:**

```
Energetic, Chill, Dark, Uplifting, Melancholic, 
Aggressive, Peaceful, Romantic, Mysterious, Epic
```

**Response:**

```json
{
  "projectId": "a1b2c3d4-e5f6-...",
  "message": "Project created successfully"
}
```

---

#### Generate Music

```http
POST /api/projects/{id}/generate
Content-Type: application/json
```

**Parameters:**

| Name | Type | Description |
|------|------|-------------|
| `id` | GUID | Project unique identifier |

**Request Body:**

```json
{
  "prompt": "Create an uplifting intro with synth arpeggios and soft pads"
}
```

**Response:**

```json
{
  "success": true,
  "errorMessage": null,
  "masterFilePath": "/output/summer-vibes-master.wav",
  "stemPaths": [
    "/output/stems/drums.wav",
    "/output/stems/bass.wav",
    "/output/stems/melody.wav",
    "/output/stems/vocals.wav"
  ]
}
```

**Error Response:**

```json
{
  "success": false,
  "errorMessage": "Worker timeout: generation took too long",
  "masterFilePath": null,
  "stemPaths": []
}
```

---

#### Iterate with Feedback

```http
POST /api/projects/{id}/iterate
Content-Type: application/json
```

**Request Body:**

```json
{
  "feedback": "Make the bass more prominent and add more reverb to the synths",
  "targetSection": "verse"
}
```

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `feedback` | string | Yes | User feedback |
| `targetSection` | string | No | Specific section to modify |

**Response:** Same as Generate Music

---

#### Delete Project

```http
DELETE /api/projects/{id}
```

**Response:** `204 No Content`

---

## Error Responses

### Standard Error Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid genre: Dubstep",
  "instance": "/api/projects"
}
```

### HTTP Status Codes

| Code | Description |
|------|-------------|
| `200` | Success |
| `201` | Created |
| `204` | No Content |
| `400` | Bad Request |
| `404` | Not Found |
| `500` | Internal Server Error |
| `503` | Service Unavailable (Worker down) |

---

## SignalR Hub

### Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5001/musicHub")
    .build();
```

### Events

#### Progress Update

```javascript
connection.on("ProgressUpdate", (update) => {
    console.log(update);
    // {
    //   "state": "Generating",
    //   "component": "MusicGen",
    //   "progress": 0.45,
    //   "message": "Generating audio... 45%"
    // }
});
```

---

## Rate Limits

No rate limits in development. Production should implement appropriate limits.

## SDK Examples

### cURL

```bash
# Create project
curl -X POST http://localhost:5001/api/projects \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","description":"Test project","genre":"Electronic","mood":"Energetic","tempoBpm":128,"key":"C Minor","durationSeconds":60}'
```

### C#

```csharp
using var client = new HttpClient();
var response = await client.PostAsJsonAsync(
    "http://localhost:5001/api/projects",
    new { name = "Test", genre = "Electronic", ... }
);
```

### Python

```python
import requests

response = requests.post(
    "http://localhost:5001/api/projects",
    json={"name": "Test", "genre": "Electronic", ...}
)
```
