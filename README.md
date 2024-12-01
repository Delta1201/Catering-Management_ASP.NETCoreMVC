# Catering Management_ASP.NETCoreMVC
 The Catering Management System is a web-based application built with ASP.NET Core MVC to manage catering functions, customers, and staff roles for a catering business. This project demonstrates full-stack development capabilities with authentication, authorization, email integration, and cloud deployment on Azure.

This project was completed as part of the coursework for PROG1322 Advanced Web Development at Niagara College and showcases a range of advanced web development skills.

Key Features
1. Authentication & Authorization with ASP.NET Identity
   Configured ASP.NET Identity for secure user authentication and role-based authorization.
   Seeded user roles and accounts with specific access privileges:
   Admin: Full control over the application.
   Supervisor: Manage functions, reports, and lookup values.
   Staff: Create and edit functions and customers, manage documents.
   Security: Assign users to roles.
   User: Basic access to view functions and customers.
2. Role Management System
   Developed a UserRole controller to manage role assignments.
   Allows users in the Security role to add or remove roles for any user.
3. Email Services Integration
   Implemented IEmailSender for sending email confirmations and password recovery links.
   Configured the system to only send emails to specific email addresses for testing and security purposes.
   Developed a business email feature allowing marketing emails to be sent to selected customers.
4. Customer Management with Master/Detail View
   Created a CustomerFunction controller and a master-detail page displaying customer details along with their associated functions.
   Features include:
   Sorting, filtering, and pagination of functions by type, name, and date range.
   Adding, updating, and deleting functions directly from the customer's detail page.
5. Cloud Deployment on Microsoft Azure
   Deployed the application on Microsoft Azure for live testing and demonstration.
   Integrated the Azure deployment link on the home page for easy access.

Technologies Used
   Frontend: Razor Pages, HTML5, CSS3, Bootstrap
   Backend: ASP.NET Core MVC, Entity Framework Core, ASP.NET Identity
   Database: Microsoft SQL Server
   Email Services: SMTP (Gmail/Outlook)
   Cloud Deployment: Microsoft Azure

 Credits Developed by: Dhaval Tailor Course: PROG 1332 Advanced Web Development Fall 2024 Instructor: Dave Stovell
