using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Application.UnitTests.Common;

/// <summary>
/// Provides mock data resources for unit tests
/// </summary>
public static class SharedResources
{
    private static readonly Guid User1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid User2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid User3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Provider1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid Provider2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid Listing1Id = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid Document1Id = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid Booking1Id = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid Payment1Id = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid InventoryItem1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Job1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid JobApplication1Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid Ticket1Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid Profile1Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    #region Users

    public static List<User> GetUsers()
    {
        return new List<User>
        {
            new User
            {
                Id = User1Id,
                Email = "john.doe@example.com",
                PasswordHash = "hashedpassword123",
                Role = UserRole.User,
                EmailVerified = true,
                Version = 1,
                Created = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "system"
            },
            new User
            {
                Id = User2Id,
                Email = "jane.smith@example.com",
                PasswordHash = "hashedpassword456",
                Role = UserRole.Provider,
                EmailVerified = true,
                Version = 1,
                Created = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "system"
            },
            new User
            {
                Id = User3Id,
                Email = "admin@example.com",
                PasswordHash = "hashedpassword789",
                Role = UserRole.Admin,
                EmailVerified = true,
                Version = 1,
                Created = DateTime.UtcNow.AddDays(-60),
                CreatedBy = "system"
            }
        };
    }

    #endregion

    #region UserProfiles

    public static List<UserProfile> GetUserProfiles()
    {
        return new List<UserProfile>
        {
            new UserProfile
            {
                Id = Profile1Id,
                UserId = User1Id,
                FirstName = "John",
                LastName = "Doe",
                Phone = "+1234567890",
                Nationality = "USA",
                Bio = "Software developer interested in immigration opportunities",
                Preferences = "{\"language\":\"en\",\"notifications\":true}",
                Created = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "system"
            },
            new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = User2Id,
                FirstName = "Jane",
                LastName = "Smith",
                Phone = "+0987654321",
                Nationality = "USA",
                Bio = "Immigration law specialist with 10 years experience",
                Preferences = "{\"language\":\"en\",\"notifications\":true}",
                Created = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "system"
            }
        };
    }

    #endregion

    #region ServiceProviders

    public static List<ServiceProvider> GetServiceProviders()
    {
        return new List<ServiceProvider>
        {
            new ServiceProvider
            {
                Id = Provider1Id,
                UserId = User2Id,
                ProviderName = "Immigration Law Associates",
                ProviderType = "Legal",
                Verified = true,
                ProviderMetadata = "{\"licenseNumber\":\"LIC-2024-001\",\"specialization\":\"Immigration Law\",\"yearsOfExperience\":10}",
                Created = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "system"
            },
            new ServiceProvider
            {
                Id = Provider2Id,
                UserId = User3Id,
                ProviderName = "Global Migration Services",
                ProviderType = "Translation",
                Verified = true,
                ProviderMetadata = "{\"licenseNumber\":\"LIC-2024-002\",\"specialization\":\"Document Translation\",\"yearsOfExperience\":5}",
                Created = DateTime.UtcNow.AddDays(-15),
                CreatedBy = "system"
            }
        };
    }

    #endregion

    #region Documents

    public static List<Document> GetDocuments()
    {
        return new List<Document>
        {
            new Document
            {
                Id = Document1Id,
                UserId = User1Id,
                DocType = "Passport",
                StoragePath = "documents/passport_scan.pdf",
                Verified = true,
                UploadedAt = DateTimeOffset.UtcNow.AddDays(-10),
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "john.doe@example.com"
            },
            new Document
            {
                Id = Guid.NewGuid(),
                UserId = User1Id,
                DocType = "BirthCertificate",
                StoragePath = "documents/birth_certificate.pdf",
                Verified = false,
                UploadedAt = DateTimeOffset.UtcNow.AddDays(-8),
                Created = DateTime.UtcNow.AddDays(-8),
                CreatedBy = "john.doe@example.com"
            }
        };
    }

    #endregion

    #region Bookings

    public static List<Booking> GetBookings()
    {
        return new List<Booking>
        {
            new Booking
            {
                Id = Booking1Id,
                UserId = User1Id,
                ListingId = Listing1Id,
                InventoryItemId = InventoryItem1Id,
                StartAt = DateTimeOffset.UtcNow.AddDays(5),
                EndAt = DateTimeOffset.UtcNow.AddDays(5).AddHours(1),
                Status = BookingStatus.Confirmed,
                PaymentId = Payment1Id,
                IdempotencyKey = "idem-key-001",
                Version = 1,
                Created = DateTime.UtcNow.AddDays(-2),
                CreatedBy = "john.doe@example.com"
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = User1Id,
                ListingId = Listing1Id,
                InventoryItemId = null,
                StartAt = DateTimeOffset.UtcNow.AddDays(3),
                EndAt = DateTimeOffset.UtcNow.AddDays(3).AddMinutes(30),
                Status = BookingStatus.Pending,
                PaymentId = null,
                IdempotencyKey = "idem-key-002",
                Version = 1,
                Created = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "john.doe@example.com"
            }
        };
    }

    #endregion

    #region Payments

    public static List<Payment> GetPayments()
    {
        return new List<Payment>
        {
            new Payment
            {
                Id = Payment1Id,
                UserId = User1Id,
                Amount = 150.00m,
                Currency = "AUD",
                GatewayReference = "TXN-2024-001",
                Status = PaymentGatewayStatus.Success,
                Meta = "{\"method\":\"CreditCard\",\"last4\":\"1234\"}",
                IdempotencyKey = "pay-idem-001",
                Created = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "john.doe@example.com"
            }
        };
    }

    #endregion

    #region JobApplications

    public static List<JobApplication> GetJobApplications()
    {
        return new List<JobApplication>
        {
            new JobApplication
            {
                Id = JobApplication1Id,
                UserId = User1Id,
                JobId = Job1Id,
                ResumeFileId = Document1Id,
                Status = JobApplicationSubmissionStatus.Submitted,
                AppliedAt = DateTimeOffset.UtcNow.AddDays(-15),
                Created = DateTime.UtcNow.AddDays(-15),
                CreatedBy = "john.doe@example.com"
            }
        };
    }

    #endregion

    #region SupportTickets

    public static List<SupportTicket> GetSupportTickets()
    {
        return new List<SupportTicket>
        {
            new SupportTicket
            {
                Id = Ticket1Id,
                UserId = User1Id,
                Subject = "Question about visa status",
                Body = "I need help understanding my visa application status.",
                Status = SupportTicketStatus.Open,
                Created = DateTime.UtcNow.AddDays(-3),
                CreatedBy = "john.doe@example.com"
            }
        };
    }

    #endregion

    #region ServiceTypes

    public static List<ServiceType> GetServiceTypes()
    {
        return new List<ServiceType>
        {
            new ServiceType
            {
                Id = 1,
                ServiceKey = "legal-consultation",
                DisplayName = "Legal Consultation",
                SchemaHint = "{\"duration\":60,\"category\":\"legal\"}",
                Enabled = true
            },
            new ServiceType
            {
                Id = 2,
                ServiceKey = "document-translation",
                DisplayName = "Document Translation",
                SchemaHint = "{\"category\":\"translation\"}",
                Enabled = true
            }
        };
    }

    #endregion

    #region Listings

    public static List<Listing> GetListings()
    {
        return new List<Listing>
        {
            new Listing
            {
                Id = Listing1Id,
                ServiceTypeId = 1,
                ProviderId = Provider1Id,
                Title = "Immigration Law Consultation",
                Description = "Expert consultation for all immigration matters",
                Attributes = "{\"duration\":60,\"language\":\"English\"}",
                Price = 150.00m,
                Currency = "AUD",
                Status = ListingStatus.Active,
                AvailableFrom = DateTimeOffset.UtcNow,
                AvailableTo = DateTimeOffset.UtcNow.AddMonths(6),
                Version = 1,
                Created = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "jane.smith@example.com"
            }
        };
    }

    #endregion

    #region InventoryItems

    public static List<InventoryItem> GetInventoryItems()
    {
        return new List<InventoryItem>
        {
            new InventoryItem
            {
                Id = InventoryItem1Id,
                ListingId = Listing1Id,
                Sku = "CONS-001",
                Metadata = "{\"sessionType\":\"video\",\"maxParticipants\":1}",
                Active = true
            }
        };
    }

    #endregion

    #region Jobs

    public static List<Job> GetJobs()
    {
        return new List<Job>
        {
            new Job
            {
                Id = Job1Id,
                ListingId = Listing1Id,
                JobType = "FullTime",
                Responsibilities = "Assist with immigration paperwork and client consultations",
                Requirements = "Bachelor's degree, paralegal certificate, 2+ years experience",
                PostedAt = DateTimeOffset.UtcNow.AddDays(-10)
            }
        };
    }

    #endregion

    #region AuditLogs

    public static List<AuditLog> GetAuditLogs()
    {
        return new List<AuditLog>
        {
            new AuditLog
            {
                Id = 1,
                Entity = "Document",
                EntityId = Document1Id,
                Action = "CREATE",
                Payload = "{\"docType\":\"Passport\",\"verified\":false}",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            }
        };
    }

    #endregion
}
