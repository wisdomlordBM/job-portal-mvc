# 💼 Job Portal Web Application

A full-featured recruitment and hiring platform built with **ASP.NET Core MVC** and **SQL Server**. The system handles the entire hiring process — from job listing and candidate application, to automated skill assessment, CV submission, and final hiring decision with notifications.

---

## 🚀 Features

### 👤 Candidate (User) Side
- Browse all available job listings without an account
- Register and log in securely
- Apply for any job listing
- Take a **timed skill assessment test** specific to the job applied for
- Assessment is **auto-marked** and returns a **percentage score** instantly
- Upload CV after completing the test
- Receive **real-time notifications** on application status (accepted or rejected)

### 🛠️ Admin Side
- View all job applications in the admin dashboard
- See each candidate's **test score and percentage performance**
- Download and review candidate **CVs**
- Accept or reject candidates based on test performance and CV
- Manage job listings

---

## 🧰 Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core MVC (C#) |
| Frontend | HTML, CSS, Razor Views |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Auth | ASP.NET Core Identity |

---

## 📸 How It Works

```
User visits site → Views job listings → Registers/Logs in
      ↓
Applies for a job → Takes timed assessment test
      ↓
Test is auto-marked → Score shown as percentage
      ↓
User uploads CV
      ↓
Admin reviews score + CV → Accepts or Rejects
      ↓
User receives notification of decision
```

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express
- Visual Studio 2022 or VS Code

### Setup

1. **Clone the repository**
```bash
git clone https://github.com/wisdomlordBM/job-portal-mvc.git
cd NewRepo/Jobportalwebsite
```

2. **Update the connection string** in `appsettings.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=JobPortalDb;Trusted_Connection=True;"
}
```

3. **Apply database migrations**
```bash
dotnet ef database update
```

4. **Run the application**
```bash
dotnet run
```

5. Open your browser and navigate to `https://localhost:5001`

---

## 📁 Project Structure

```
Jobportalwebsite/
├── Controllers/        # MVC Controllers (Jobs, Applications, Admin, Auth)
├── Models/             # Data models and ViewModels
├── Views/              # Razor HTML views
├── Data/               # DbContext and migrations
└── wwwroot/            # Static files (CSS, JS)
```

---

## 🔐 Roles

| Role | Access |
|---|---|
| **User/Candidate** | Browse jobs, apply, take tests, upload CV, view notifications |
| **Admin** | Manage jobs, view applications, review scores & CVs, approve/reject |

---

## 👨‍💻 Author

**Onyebuchi I.**  
Full-Stack Developer — ASP.NET Core MVC | React | REST APIs  
🔗 [GitHub](https://github.com/wisdomlordBM)

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
