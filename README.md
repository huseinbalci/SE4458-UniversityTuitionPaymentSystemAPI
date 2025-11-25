== API PROJECT FOR A UNIVERSITY TUITION PAYMENT SYSTEM == 

A cloud-hosted ASP.NET Core Web API solution for managing university tuition data, payments, and administrative operations.
The system includes 4 controllers, supports JWT authentication, integrates with Azure API Gateway, and uses Azure SQL Database for persistent storage.

1-SYSTEM OVERVIEW

  The API provides end-to-end functionality for students, and university administrators.

2-TECHNOLOGIES USED

  * ASP.NET Core Web API
  * Azure SQL Database & Azure SQL Server
  * Azure API Management (Gateway) – routing and rate limiting
  * Azure Application Insights – request/response logging
  * JWT Authentication
  * Swagger UI (OpenAPI)
  * Azure App Service (Hosting)

3-SYSTEM ARCHITECTURE

  Client → Azure API Gateway → Backend API → Azure SQL Database
  Responses flow back in reverse.

  The Azure API Gateway handles:

    * Routing
    * Subscription key validation
    * Global rate limiting
    * Global request logging

  API Versioning:

    * Uses /api/v1/...
    * Allows future updates without breaking existing clients.

4-CONTROLLERS OVERVIEW

  AuthController:
    * Issues JWT tokens for authenticated users (Admin or Bank).
    * Handles login.

  MobileController:
    * Student tuition query (no authentication required).
    * Students enter StudentNo to view tuition information.

  BankController:
    * Pay tuition (no authentication required).
    * Query tuition (JWT authentication required).

  AdminController:
    * Requires full JWT authentication.
    * Admins can upload tuition data using a .csv file.
    * Admins can add tuition manually.
    * Admins can view all unpaid tuition records (with paging).

5-API DATA MODEL

  Data Model PDF:
  https://github.com/huseinbalci/SE4458-UniversityTuitionPaymentSystemAPI/blob/master/UniversityTuitionAPI.pdf
    
6-SOURCE CODE

  GitHub Repository:
  https://github.com/huseinbalci/SE4458-UniversityTuitionPaymentSystemAPI

7-PROJECT DEMO VIDEO

  Video Link:
  <Insert your presentation/demo video link here>








