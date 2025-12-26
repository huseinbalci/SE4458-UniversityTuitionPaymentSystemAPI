API PROJECT FOR A UNIVERSITY TUITION PAYMENT SYSTEM

A locally running Node.js/Express API for managing university tuition data, payments, and administrative operations.
The system integrates with Groq API using the model "llama-3.3-70b-versatile", and uses a built-in/local database instead of Azure. The API is not deployed, and works entirely on your local machine. Admins can manually add students and manage tuition records.

1. SYSTEM OVERVIEW

The API provides end-to-end functionality for students and university administrators.

Students can query their tuition balances.

Admins can add students, add tuition, and view unpaid tuition.

2. TECHNOLOGIES USED

Node.js & Express

Local in-built database (no cloud database)

Groq API for AI-assisted responses

JWT Authentication

CORS & JSON body parsing

Swagger UI (optional)

Local hosting (no cloud deployment)

3. SYSTEM ARCHITECTURE
Client → API Server (Express, local DB) → Groq AI Model


The API server handles student and admin requests.

Groq API is used for AI-powered chat responses and tuition tool handling.

Responses flow back to the client via the Express API.

4. CONTROLLERS OVERVIEW

4.1 AuthController (if implemented):

Handles login and JWT token issuance (local authentication).

4.2 MobileController:

Student tuition query (requires StudentNo).

4.3 BankController:

Pay tuition (can integrate with mock or local bank logic).

Query tuition (JWT authentication optional, based on local setup).

4.4 AdminController:

Requires JWT authentication.

Admins can add students manually.

Admins can add tuition manually.

Admins can view all unpaid tuition records (with optional pagination).

5. API DATA MODEL

All data is stored in a local in-memory or file-based database.

Groq AI tools handle student tuition queries and payments logic.

Data model PDF (if still relevant):
UniversityTuitionAPI.pdf

6. SOURCE CODE

GitHub Repository:
https://github.com/huseinbalci/SE4458-UniversityTuitionPaymentSystemAPI

7. PROJECT DEMO VIDEO

Video Link:
https://www.youtube.com/watch?v=BaJOKNplXbw
