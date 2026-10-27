# UniSmartLMS — AI-Powered Smart Library Management System

An intelligent university library management system built with **C#, ASP.NET Core MVC, and SQL Server**, combining library operations, role-based access control, document-based question answering, and a voice-enabled assistant.

**Developer:** Khizar Hayyat  
**Role:** Team Leader — Final Year Project (2025–2026)  
**Institution:** MNS University of Agriculture, Multan, Pakistan

🌐 **Project Portfolio:** [View UniSmartLMS Showcase](https://khizar7553.github.io/unismartlms-showcase/)  
💻 **Source Code:** [GitHub Repository](https://github.com/Khizar7553/UniSmartLMS)

---

## 📌 Project Overview

UniSmartLMS is a web-based library management system designed to improve how university students and library staff interact with library resources.

Alongside conventional library management features, the project explores AI-assisted document question answering and browser-based voice interaction. Users can manage library operations through role-based interfaces and ask questions about uploaded documents using a document-retrieval workflow.

The project demonstrates practical experience with web application development, database integration, authentication, software architecture, document processing, and experimental AI-assisted functionality.

## ✨ Key Features

### 📚 Library Management
- Manage library books and related records.
- Support borrowing requests and library workflows.
- Provide dedicated dashboards for administrators, librarians, and students.
- Organize library operations through a centralized web application.

### 🤖 Document-Based AI Question Answering
- Upload supported documents for processing.
- Divide document content into smaller chunks for retrieval.
- Generate text representations of document content using a lightweight, hash-based character-trigram approach.
- Use cosine similarity to compare text vectors and help retrieve relevant document chunks.
- Present answers based on retrieved document content, depending on the query and available documents.

**Technical note:** The current embedding implementation uses hashed character trigrams rather than a pretrained semantic embedding model. Retrieval quality therefore depends on the text and similarity method used.

### 🎙️ Voice Assistant
- Accept voice input through supported browser speech-recognition functionality.
- Convert recognized speech into text for processing.
- Provide spoken responses using browser text-to-speech functionality.
- Support voice-driven interaction with the application.

Voice features depend on browser and device support.

### 🔬 Research Assistant
- Includes an experimental research-assistance feature intended to support academic information tasks.
- Provides a foundation for further improvements in research workflows and AI-generated assistance.

This feature is a prototype and may require further development to improve reliability and response quality.

### 🔐 Authentication and Role-Based Access
- Provide role-oriented access for administrators, librarians, and students.
- Separate dashboard experiences according to application roles.
- Integrate ASP.NET Core Identity for authentication and identity-related functionality.

---

## 🛠️ Technology Stack

| Category | Technologies |
|---|---|
| Programming language | C# |
| Web framework | ASP.NET Core MVC |
| Target framework | .NET 10 |
| Frontend | HTML, CSS, JavaScript, Razor Views |
| Database | Microsoft SQL Server |
| ORM and data access | Entity Framework Core, Dapper |
| Authentication | ASP.NET Core Identity |
| Document processing | PdfPig, Open XML SDK, Xceed.Words.NET |
| Text similarity | Character-trigram vectors and cosine similarity |
| AI/ML-related libraries | Microsoft ML, ONNX Runtime, LM-Kit.NET |
| Voice interaction | Browser Web Speech APIs |
| Version control | Git and GitHub |

*The libraries listed are project dependencies; not every library necessarily powers every feature. The current `EmbeddingService` implementation uses character-trigram hashing for vector generation.*

## 🏗️ Project Structure

The project follows a modular ASP.NET Core MVC structure.

```text
UniSmartLMS/
├── Controllers/       # Handle incoming requests and application workflows
├── Models/            # Application models and data structures
├── Views/             # Razor views and user interfaces
├── Data/              # Database context and data configuration
├── Repositories/      # Data-access components
├── Services/          # Business logic and AI-related services
├── Helpers/           # Shared helper functionality
├── Migrations/        # Entity Framework Core database migrations
├── wwwroot/           # Static assets, JavaScript, CSS, and resources
├── Properties/        # Project launch and configuration settings
├── Program.cs         # Application startup and service configuration
├── appsettings.json   # Application configuration
└── UniSmartLMS.csproj # Project framework and package dependencies
```

## 🧠 How the Document Question-Answering Workflow Works

The document question-answering feature follows a retrieval-oriented workflow:

1. **Document input:** A supported document is uploaded and processed.
2. **Chunking:** The document content is divided into smaller text sections.
3. **Vector generation:** The current embedding service converts text into normalized, 384-dimensional character-trigram vectors.
4. **Similarity comparison:** Cosine similarity helps compare the query representation with stored document representations.
5. **Retrieval:** Relevant document chunks are selected for the answer workflow.
6. **Response:** The application prepares an answer based on the retrieved content and the implemented answer-generation logic.

This is a **RAG-style retrieval workflow**. The exact answer-generation method depends on the configured services and should not be assumed to use a pretrained semantic embedding model.

## ⚙️ Getting Started

### Prerequisites

Before running the project, install or configure:

- [Visual Studio](https://visualstudio.microsoft.com/) with the ASP.NET and web development workload, or a compatible .NET development environment.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- Microsoft SQL Server and an appropriate database management tool.
- Git, if cloning the repository.

### 1. Clone the Repository

```bash
git clone https://github.com/Khizar7553/UniSmartLMS.git
cd UniSmartLMS
```

### 2. Configure the Database

Review `appsettings.json` and configure the connection string for your local SQL Server instance.

**Security:** Do not commit real database passwords, API keys, or other secrets to GitHub. Keep sensitive configuration in an appropriate local or environment-specific configuration source.

### 3. Restore Dependencies

From the directory containing `UniSmartLMS.csproj`, run:

```bash
dotnet restore
```

### 4. Apply Database Migrations

If the project migrations are compatible with your configured database, run:

```bash
dotnet ef database update
```

If the Entity Framework command is unavailable, install the EF Core command-line tool and use a compatible version for your project.

### 5. Run the Application

```bash
dotnet run
```

Open the local URL shown in the terminal.

**Important:** Initial setup may require additional configuration for authentication, database access, uploaded files, or optional AI-related services. Follow the project's configuration and service implementation where applicable.

## 🖼️ Screenshots and Demonstration

Explore the project showcase for available screenshots and interface examples:

**[Open the UniSmartLMS Portfolio](https://khizar7553.github.io/unismartlms-showcase/)**

The showcase includes examples of the application's dashboards, login and signup pages, AI chat interface, voice assistant, and library-management interfaces.

## 🎯 Learning and Development Objectives

This project provided practical experience in:

- Building an ASP.NET Core MVC web application.
- Working with C# and relational databases.
- Organizing code into controllers, models, repositories, and services.
- Integrating authentication and role-based functionality.
- Processing documents and retrieving relevant text.
- Exploring vector representations and cosine similarity.
- Experimenting with browser-based speech recognition and text-to-speech.
- Managing a software project using Git and GitHub.

## 🚀 Future Improvements

Potential areas for further development include:

- Replacing character-trigram hashing with a pretrained semantic embedding model.
- Improving document retrieval and evaluating answer accuracy.
- Adding stronger source attribution and answer-grounding checks.
- Improving the performance and reliability of the research assistant.
- Expanding automated testing and error handling.
- Improving documentation, deployment, and configuration management.

## 👨‍💻 About the Developer

**Khizar Hayyat** is a BS Information Technology graduate from MNS University of Agriculture, Multan, Pakistan, with a CGPA of 3.67/4.00.

His technical interests include .NET development, SQL databases, web applications, IT support, and practical AI-assisted software systems.

- **GitHub:** [Khizar7553](https://github.com/Khizar7553)
- **LinkedIn:** [Khizar Hayyat](https://linkedin.com/in/khizar-hayyat-9977433a9/)
- **Portfolio:** [UniSmartLMS Showcase](https://khizar7553.github.io/unismartlms-showcase/)

## 📄 License

No open-source license has been specified yet. Unless a license is added by the project owner or authorized contributors, the source code remains subject to applicable copyright restrictions.

---

*UniSmartLMS is an academic project demonstrating web development, library management, document retrieval, and experimental AI-assisted features. Capabilities and results may vary depending on configuration and the documents provided.*
