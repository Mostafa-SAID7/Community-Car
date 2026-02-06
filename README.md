# Community Car

A comprehensive ASP.NET Core MVC application for automotive community engagement, featuring Q&A forums, social networking, and administrative dashboards.

## 🚀 Features

### Community Features
- **Q&A System**: Ask and answer automotive questions with voting, reactions, and bookmarking
- **Social Networking**: Friend requests, friendships, and user connections
- **Content Management**: Posts, guides, news articles, and reviews
- **Events & Groups**: Community events and group management
- **Interactive Maps**: Location-based features for automotive services

### Dashboard & Analytics
- **Admin Dashboard**: Comprehensive overview of platform metrics
- **User Management**: Role-based access control and permissions
- **Analytics**: User activity tracking and content analytics
- **Audit Logs**: Security and system audit trails
- **KPI Tracking**: Key performance indicators monitoring

### Technical Features
- Clean Architecture (Domain, Infrastructure, Web layers)
- Entity Framework Core with SQL Server
- Identity management with ASP.NET Core Identity
- Localization support (English/Arabic)
- Responsive design with Bootstrap 5
- Real-time notifications
- RESTful API design

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) or SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Node.js](https://nodejs.org/) (for frontend tooling)

## 🛠️ Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Mostafa-SAID7/Community-Car.git
   cd Community-Car
   ```

2. **Update database connection string**
   
   Edit `CommunityCar.Web/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=CommunityCarDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Apply database migrations**
   ```bash
   cd CommunityCar.Web
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   
   Open your browser and navigate to: `https://localhost:5001`

## 📁 Project Structure

```
Community-Car/
├── CommunityCar.Domain/          # Domain layer (Entities, Interfaces, DTOs)
│   ├── Base/                     # Base classes and interfaces
│   ├── Entities/                 # Domain entities
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Interfaces/               # Service interfaces
│   ├── Commands/                 # CQRS commands
│   └── Queries/                  # CQRS queries
├── CommunityCar.Infrastructure/  # Infrastructure layer (Data access, Services)
│   ├── Data/                     # DbContext and configurations
│   ├── Services/                 # Service implementations
│   ├── Repos/                    # Repository implementations
│   └── Migrations/               # EF Core migrations
├── CommunityCar.Web/             # Presentation layer (MVC)
│   ├── Areas/                    # MVC Areas
│   ├── Controllers/              # Controllers
│   ├── Views/                    # Razor views
│   ├── wwwroot/                  # Static files
│   └── Resources/                # Localization resources
└── docs/                         # Documentation
```

## 🔧 Configuration

### Database Configuration

The application uses Entity Framework Core with SQL Server. Configure your connection string in `appsettings.json`.

### Authentication & Authorization

The application uses ASP.NET Core Identity for authentication with role-based authorization.

### Localization

Supports multiple languages (English/Arabic). Language resources are located in `CommunityCar.Web/Resources/`.

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📚 Documentation

- [Architecture Overview](docs/ARCHITECTURE.md)
- [API Documentation](docs/API.md)
- [Database Schema](docs/DATABASE.md)
- [Contributing Guidelines](CONTRIBUTING.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Mostafa SAID** - [Mostafa-SAID7](https://github.com/Mostafa-SAID7)

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Bootstrap team for the UI framework
- All contributors who help improve this project

## 📞 Support

For support, email support@communitycar.com or open an issue in the GitHub repository.

## 🗺️ Roadmap

- [ ] Mobile application (iOS/Android)
- [ ] Real-time chat functionality
- [ ] Advanced search with Elasticsearch
- [ ] AI-powered recommendations
- [ ] Integration with automotive APIs
- [ ] Progressive Web App (PWA) support

---

Made with ❤️ by the Community Car Team
