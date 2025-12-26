== API PROJECT FOR A UNIVERSITY TUITION PAYMENT SYSTEM ==

A locally running Node.js/Express API for managing university tuition data, payments, and administrative operations.
The system integrates with Groq API using the model "llama-3.3-70b-versatile", and uses a built-in/local database instead of Azure. The API is not deployed, and works entirely on your local machine. Admins can manually add students and manage tuition records.

I.SYSTEM OVERVIEW

* The API provides end-to-end functionality for students and university administrators.
* Students can query their tuition balances.
* Admins can add students, add tuition, and view unpaid tuition.

II.TECHNOLOGIES USED

* Node.js & Express
* Local in-built database (no cloud database)
* Groq API for AI-assisted responses
* JWT Authentication
* CORS & JSON body parsing
* Local hosting

III.SYSTEM ARCHITECTURE
* Client → API Server (Express, local DB) → Groq AI Model
* The API server handles student and admin requests.
* Groq API is used for AI-powered chat responses and tuition tool handling.
* Responses flow back to the client via the Express API.

IV.CONTROLLERS OVERVIEW

* AuthController: Handles login and JWT token issuance.
* MobileController: Student tuition query.
* BankController: Pay tuition and Query tuition.
* AdminController: Requires JWT authentication. Admins can add students manually, add tuition manually, and view all unpaid tuition records.

V.SOURCE CODE

GitHub Repository:
https://github.com/huseinbalci/SE4458-UniversityTuitionPaymentSystemAPI

VI.PROJECT DEMO VIDEO

Video Link:
https://www.youtube.com/watch?v=5byLwyOVro8
