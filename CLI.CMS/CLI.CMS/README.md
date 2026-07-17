# Canvas Clone CLI - Course Management System (CMS)

Welcome to the **Canvas Clone CLI**, a modular, lightweight console application designed to simulate the core workflows of a modern Learning Management System (LMS). Built using **C#** and **.NET 8.0/9.0**, this application acts as the foundational engine for a future cross-platform .NET MAUI application.

---

## 🚀 Completed Features 

The system is fully decoupled into a presentation layer (CLI Handlers) and a persistent in-memory database layer (Service Proxy).

### **Instructor Dashboard**
* **Course Creation & Management:** Create, update, and delete courses.
* **Module System:** Build out structured modules and attach interactive content.
* **Roster Engine:** View class rosters, enroll existing university students, and securely unenroll them.
* **Assignment Management:** Full CRUD (Create, Read, Update, Delete) suite for coursework, including assignment name, descriptions, maximum points, and due dates.
* **Grading Utility:** Scan submissions by assignment, view student work, and assign grades.

### **Student Portal**
* **Role Switcher:** Securely log in using a designated student profile ID.
* **Course Portal:** Access published materials, modules, and assignment instructions.
* **Submission Center:** Upload text-based submissions directly to active course assignments.
* **Grade Book:** View current submissions and review graded items instantly.

---

## 📁 Architecture & Project Structure

CMS/
├── CLI.CMS/                    
│   ├── CLI.CMS/                 
│   │   ├── Library.CMS/         <-- Shared Class Library (Business Logic)
│   │   │   ├── Models/
│   │   │   │   ├── Assignment.cs
│   │   │   │   ├── Course.cs
│   │   │   │   ├── Module.cs
│   │   │   │   ├── Submission.cs
│   │   │   │   └── User.cs
│   │   │   └── Services/
│   │   │       └── SiteServiceProxy.cs
│   │   ├── CLI.CMS.csproj
│   │   ├── Program.cs
│   │   ├── StudentMenuHandlers.cs
│   │   └── TeacherMenuHandlers.cs
│   └── CLI.CMS.sln
├── .gitignore
└── LICENSE