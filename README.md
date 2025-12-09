# ManualMate

<p align="center">
  <img src="assets/RAG.png" alt="Web-based RAG Preview" width="550">
</p>

<p align="center">
  <strong>RAG-Powered PDF Manuals Q&A System</strong><br>
  A .NET Web API project designed for processing PDF user manuals, generating embeddings, and providing a Q&A service using Retrieval Augmented Generation (RAG), with integrated caching.
</p>

---


## Table of Contents

* [Features](#features)
* [Usage](#usage)
* [Performance](#performance)
* [Key Components](#key-components)
* [Requirements](#requirements)
* [Setup](#setup)

---

## Features

* **Retrieval Augmented Generation (RAG):** Implements a Q&A system for user manuals by retrieving relevant information and augmenting an LLM's response.
* **Text Chunking and Embedding:** Processes manual content into chunks and generates embeddings using Hugging Face models.
* **Caching with Redis:** Uses Redis to store data so repeated questions are answered much faster (for example, from ~2782 ms down to ~45 ms, which is about 98% faster).
* **PDF Extraction:** Extracts text content from PDF manuals.
* **RESTful API:** Provides a clean API for interacting with the manual processing and Q&A services.



---

## Usage

### Upload Product Manual

`POST /api/product/upload-manual/{id}`

---

### Process Product Manual

`POST /api/product/process-manual/{id}`

---

### Ask Questions

`POST api/product/ask/1?question=`

**Response:**

```json
{
  "question": "How do I clean the filter?",
  "answer": "According to the manual, clean the filter weekly by removing it and rinsing under warm water."
}
```

---

## Performance

### Caching Impact

Example request:

```
GET {{base-url}}/Product/ask/8?question=how to sleep
```

(Product **8** = PlayStation 5 manual)

* **Without Caching:** responded in **2782.38 ms**
* **With Caching:** responded in **45.63 ms**

This results are roughly **98% faster** for repeated queries.

---

## Key Components

### 1. RAG Service (`ManualQaService.cs`)

* Converts questions to embeddings
* Performs similarity search
* Generates answers using LLM (`gemini-2.5-flash-lite`)

### 2. Caching Service (`RedisService.cs`)

* Implements Redis-based caching
* Stores frequently asked questions

### 3. Embedding Service (`HuggingFaceEmbeddingService.cs`)

* Converts text to semantic vectors
* Uses Hugging Face models (`bge-small-en-v1.5`)
* Calculates cosine similarity

### 5. LLM Service (`GeminiLlmService.cs`)

* Generates nl answers
* Uses Google Gemini API
* Grounded only in retrieved context

### 6. Manual Processing (`ManualProcessingService.cs`)

* Extracts text from PDFs
* Chunks text into manageable pieces
* Generates and stores embeddings

---

## Requirements

### System Requirements

* **.NET 8.0 SDK** or later
* **SQL Server**
* **Redis**

### API Keys

* **Hugging Face API Token** - For Vector embeddings service
* **Google Gemini API Key** - For LLM service

---
## Setup

### 1. Clone Repository

```bash
git clone https://github.com/yourusername/manualmate.git
cd manualmate
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Install and Start Redis

**Windows:**

```bash
# Using Chocolatey
choco install redis-64

# Start Redis
redis-server
```

### 4. Database Setup

Update `appsettings.json` with your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ManualMateDB;Trusted_Connection=True;TrustServerCertificate=True",
    "Redis": "localhost:6379"
  }
}
```

Run migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Configure API Keys

Add to `appsettings.json`:

```json
{
  "HuggingFace": {
    "ApiToken": "hf_your_token_here"
  },
  "Gemini": {
    "ApiKey": "your_gemini_api_key_here"
  }
}
```

### 6. Run Application

```bash
dotnet run
```

---


