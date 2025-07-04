# 🕒 ShiftWise - Backend API

ShiftWise is a smart employee shift scheduling backend built with ASP.NET Core and SQL Server. It powers the core features of the ShiftWise platform — managing organizations, employees, shifts, assignments, and schedule exports.

---

## ✨ Features

- Create and manage employee shifts  
- Assign and unassign employees to shifts  
- Multi-day shift creation with weekend skipping  
- Export schedules to PDF  
- Send schedules via email  
- Dashboard insights for managers  
- Public holiday calendar integration (by country)

---

## 🛠 Tech Stack

- ASP.NET Core 9 (Web API)  
- Entity Framework Core  
- SQL Server / Azure SQL  
- JWT Authentication  
- SMTP Email (Brevo)  
- PDF Export (DinkToPdf)

---

## ▶️ How to Run (Local)

1. Clone the repository  
2. Open in Visual Studio or VS Code
3. Add .env files with JWT and Connection string
4. Run the database with EF Core  
5. Press `F5` or run:

```bash
dotnet run
