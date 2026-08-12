# ManualMate

<p align="center">
  <img src="assets/RAG.png" alt="Web-based RAG Preview" width="550">
</p>

<p align="center">
  <strong>RAG-Powered PDF Manuals Q&A System</strong><br>
  A .NET Full Stack Web Application for processing PDF user manuals, generating embeddings asynchronously with RabbitMQ, and providing a Q&A service using Retrieval Augmented Generation (RAG), with integrated PostgreSQL vector search and Redis caching.
</p>

---

## Table of Contents

* [Features](#features)
* [Architecture](#architecture)
* [Usage](#usage)
* [Performance](#performance)
* [Key Components](#key-components)
* [Requirements](#requirements)
* [Setup](#setup)

---

## Features

* **Retrieval Augmented Generation (RAG):** Implements a Q&A system for user manuals by retrieving relevant information and augmenting an LLM's response.
* **Asynchronous PDF Processing:** Uses RabbitMQ to process uploaded manuals asynchronously without blocking the API request.
* **Message-Based Processing:** When a new manual is created, its ID is published to a RabbitMQ queue and processed by a background consumer.
* **Smart Vector Storage (pgvector):** Uses PostgreSQL with `pgvector` to perform fast similarity searches directly in the database.
* **Text Chunking and Embedding:** Processes manual content into chunks and generates embeddings using Hugging Face models (`bge-small-en-v1.5`).
* **Caching with Redis:** Stores answers to repeated questions for faster retrieval.
* **PDF Extraction:** Extracts text content from PDF manuals.
* **RESTful API:** Provides clean endpoints for managing manuals and asking questions.

---

## Architecture

Manual processing is handled asynchronously using **RabbitMQ**.

Instead of making the client wait while the PDF is extracted, chunked, and embedded, the API publishes a message containing the manual's ID to a RabbitMQ queue and immediately returns a successful response.

A background consumer then receives the message and performs the potentially expensive PDF processing and embedding generation.

### Manual Processing Flow

```text
Client
  │
  │ Create / Upload Manual
  ▼
API
  │
  │ 1. Save Manual
  │ 2. Publish Manual ID
  ▼
RabbitMQ Queue
  │
  │ Manual ID
  ▼
Background Consumer
  │
  ├── Retrieve Manual
  ├── Extract PDF Text
  ├── Split Text into Chunks
  ├── Generate Embeddings
  └── Store Chunks + Embeddings
             │
             ▼
        PostgreSQL
        + pgvector
```

### Why RabbitMQ?

PDF extraction and embedding generation can be relatively expensive operations. Performing them directly inside the HTTP request would force the client to wait until the entire process finishes.

RabbitMQ decouples manual creation from manual processing:

1. The API receives the request and creates the manual.
2. The manual ID is published to a RabbitMQ queue.
3. The API immediately returns a success response.
4. A background consumer receives the manual ID.
5. The consumer retrieves and processes the PDF.
6. Text chunks and their embeddings are stored in PostgreSQL.
7. The manual becomes available for semantic Q&A.

This makes the API more responsive and allows manual processing to happen independently in the background.

---

## Usage

<p align="center">
  <img src="assets/ManualMate Manual Management.png" alt="Manual management" width="950">
</p>

### Upload Product Manual

`POST /api/product/upload-manual/{id}`

The manual is uploaded and associated with the product.

When a new manual/product is created, the API publishes a message containing the manual's ID to RabbitMQ.

**Example message:**

```json
{
  "itemId": 8
}
```

The API does not wait for PDF extraction or embedding generation to finish. Instead, it returns a successful response after the message has been successfully published.

---

### Background Manual Processing

After the message is placed on the RabbitMQ queue, a background consumer processes the manual.

The consumer:

1. Retrieves the manual using the provided ID.
2. Extracts text from the PDF.
3. Splits the text into smaller chunks.
4. Generates embeddings for each chunk using `bge-small-en-v1.5`.
5. Stores the chunks and embeddings in PostgreSQL using `pgvector`.

This processing happens independently from the original HTTP request.

---

### Ask Questions

<p align="center">
  <img src="assets/ManualMate Chat Example.png" alt="chat example" width="950">
</p>

<p align="center">
  <img src="assets/Manual Mate Chat Example 2.png" alt="chat example" width="950">
</p>

Once the manual has been processed and its embeddings are stored, questions can be asked against the manual:

`POST /api/product/ask/{id}?question=How do I reset the device?`

**Response:**

```json
{
  "question": "How do I reset the device?",
  "answer": "According to the manual, press and hold the reset button for 5 seconds until the LED blinks."
}
```

The Q&A flow uses vector similarity search to retrieve the most relevant chunks from the manual before sending the retrieved context to the LLM.

---

## Performance

Performance is optimized through **PostgreSQL with pgvector**, **Redis caching**, and asynchronous processing with **RabbitMQ**.

### 1. Asynchronous Processing (RabbitMQ)

RabbitMQ prevents expensive PDF processing and embedding generation from blocking the API request.

**Without RabbitMQ:**

```text
HTTP Request
     │
     ├── PDF Extraction
     ├── Chunking
     ├── Embedding Generation
     ├── Database Storage
     │
     ▼
HTTP Response
```

The client has to wait for the entire processing pipeline to finish.

**With RabbitMQ:**

```text
HTTP Request
     │
     ├── Save Manual
     ├── Publish Message
     │
     ▼
HTTP Success Response

     ...

RabbitMQ
     │
     ▼
Consumer
     │
     ├── PDF Extraction
     ├── Chunking
     ├── Embedding Generation
     └── Database Storage
```

The API can respond immediately while the consumer handles the expensive work in the background.

### 2. Vector Search Speed (PostgreSQL + pgvector)

By using `pgvector`, similarity searches are performed directly by PostgreSQL instead of manually loading all embeddings into application memory.

* **Old Way:** Loaded all data into memory to find matching chunks.
* **New Way (pgvector):** Uses PostgreSQL vector search and HNSW indexes to efficiently find relevant chunks without loading unnecessary data into application memory.

### 3. Caching Impact (Redis)

We cache final answers to prevent repeated questions from triggering the entire RAG pipeline again.

**Example Request:**

`GET .../ask/8?question=how to sleep`

*(PlayStation 5 manual)*

* **First Request (Database Search):** ~2782 ms
* **Second Request (Redis Cache):** ~45 ms

**Result: ~98% faster** for repeated questions.

---

## Key Components

### 1. RAG Service (`ManualQaService.cs`)

* Coordinates the question-answering flow.
* Performs vector similarity searches.
* Retrieves relevant manual chunks.
* Sends the retrieved context to the LLM.

### 2. RabbitMQ Message Broker

* Decouples manual creation from PDF processing.
* Publishes the manual/item ID to a queue.
* Allows the API to return a successful response without waiting for processing to complete.
* Provides messages to the background consumer for asynchronous processing.

### 3. Manual Processing Consumer

* Consumes manual IDs from RabbitMQ.
* Retrieves the corresponding manual.
* Extracts text from the PDF.
* Splits the extracted text into chunks.
* Generates embeddings for the chunks.
* Stores the processed chunks and embeddings in PostgreSQL.

### 4. Database Service (`PostgreSQL + pgvector`)

* Stores manual content and vector embeddings.
* Performs cosine-distance similarity searches.
* Uses `pgvector` to efficiently retrieve relevant manual chunks.

### 5. Caching Service (`RedisService.cs`)

* Caches generated answers.
* Returns previously generated answers without repeating the vector search and LLM generation pipeline.

### 6. Embedding Service (`HuggingFaceEmbeddingService.cs`)

* Converts text chunks into 384-dimensional vectors using `bge-small-en-v1.5`.

### 7. LLM Service (`GeminiLlmService.cs`)

* Generates natural language answers using Google Gemini.
* Uses the relevant manual chunks retrieved by the RAG pipeline as context.

---

## Requirements

### System Requirements

* **.NET 8.0 SDK** or later
* **PostgreSQL** with the `pgvector` extension enabled
* **Redis**
* **RabbitMQ**

### API Keys

* **Hugging Face API Token** — used for generating embeddings.
* **Google Gemini API Key** — used for answer generation.

---

## Setup

### 1. Clone Repository

```bash
git clone
cd manualmate
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Infrastructure Services

ManualMate requires the following infrastructure services:

* PostgreSQL
* Redis
* RabbitMQ

Make sure all three services are running before starting the application.

---

### 4. Database Setup (PostgreSQL)

Ensure PostgreSQL is installed. Update `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "ManualMateDbContext": "",
    "Redis": ""
  }
}
```

Run migrations:

```bash
dotnet ef database update
```

The migration automatically enables the PostgreSQL `vector` extension.

---

### 5. RabbitMQ Configuration

Configure the RabbitMQ connection in `appsettings.json`:

```json
{
  "RabbitMQ": {
    "Host": "",
    "Username": "",
    "Password": ""
  }
}
```

The application uses RabbitMQ to publish manual IDs when new manuals are created. The background consumer listens to the queue and processes the corresponding PDFs.

---

### 6. Configure API Keys

Add the required API keys to `appsettings.json`:

```json
{
  "HuggingFace": {
    "ApiToken": ""
  },
  "Gemini": {
    "ApiKey": ""
  }
}
```

---

### 7. Run Application

```bash
dotnet run
```

Once the application is running, create/upload a manual. The API will publish the manual ID to RabbitMQ and return a successful response immediately. The consumer will then process the PDF and generate the embeddings asynchronously.
