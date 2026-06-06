using FinPal.Expense.Api.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinPal.Expense.Api.Infrastructure.CurrentUser;
using FinPal.Expense.Api.Services.Budgets;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Services.Expense;
using Microsoft.Extensions.Logging.Abstractions;
using FinPal.Expense.Api.DTO.Budgets;
using NuGet.Frameworks;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace FinPal.Expense.Api.Tests.Services.Budgets
{
    internal class BudgetServiceTests
    {
        private FinPalDbContext _db = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private BudgetService _service = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<FinPalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new FinPalDbContext(options);

            _currentUserMock = new Mock<ICurrentUserService>();

            _service = new BudgetService(_db, _currentUserMock.Object, NullLogger<BudgetService>.Instance);
        }

        [Test]
        public async Task CreateAsync_ShouldCreateBudget_WhenValid()
        {
            //Arrange
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User",
                Email = "test@finpal.com",
                Password = "hashedPassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                CategoryId = 1,
                UserId = 1,
                CategoryName = "Travelling",
                IsActive = true
            });

            await _db.SaveChangesAsync();

            var request = new CreateBudgetRequestDto
            {
                CategoryId = 1,
                Year = 2025,
                Month = 11,
                BudgetAmount = 1000
            };

            //Act
            await _service.CreateAsync(request);

            //Assert
            var budgetResponse = await _db.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == 1);

            Assert.Multiple(() =>
            {
                Assert.That(budgetResponse!, Is.Not.Null);
                Assert.That(_db.Budgets.Count(), Is.EqualTo(1));
                Assert.That(budgetResponse!.BudgetAmount, Is.EqualTo(request.BudgetAmount));
                Assert.That(budgetResponse!.UserId, Is.EqualTo(1));
                Assert.That(budgetResponse!.CategoryId, Is.EqualTo(request.CategoryId));
                Assert.That(budgetResponse!.Year, Is.EqualTo(request.Year));
                Assert.That(budgetResponse!.Month, Is.EqualTo(request.Month));
            });
        }

        [Test]
        public async Task CreateAsync_ShouldThrowException_WhenInvalidCategoryOwnership()
        {
            //Arrange
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User",
                Email = "test@finpal.com",
                Password = "hashedPassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                CategoryId = 1,
                UserId = 2,
                CategoryName = "Travelling",
                IsActive = true
            });

            await _db.SaveChangesAsync();

            var request = new CreateBudgetRequestDto
            {
                CategoryId = 1,
                Year = 2025,
                Month = 11,
                BudgetAmount = 1000
            };

            //Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.CreateAsync(request));

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Invalid category"));
                Assert.That(_db.Budgets.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task CreateAsync_ShouldThrowException_WhenBudgetAlreadyExistsForCategoryMonthAndYear()
        {
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User",
                Email = "test@finpal.com",
                Password = "hashedPassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                CategoryId = 1,
                UserId = 1,
                CategoryName = "Travelling",
                IsActive = true
            });

            _db.Budgets.Add(new Entities.Budget
            {
                BudgetId = 1,
                UserId = 1,
                CategoryId = 1,
                Month = 1,
                Year = 2025,
                BudgetAmount = 1000,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            var request = new CreateBudgetRequestDto
            {
                CategoryId = 1,
                Year = 2025,
                Month = 1,
                BudgetAmount = 2000
            };

            //Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));

            var budget = await _db.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == 1);

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Budget already exists for this month"));
                Assert.That(_db.Budgets.Count(), Is.EqualTo(1));
                Assert.That(budget!.BudgetAmount, Is.EqualTo(1000));
            });
        }

        [Test]
        public async Task GetSummaryAsync_ShouldReturnWithinBudgetStatus_WhenExpensesWithinBudget()
        {
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User",
                Email = "test@finpal.com",
                Password = "hashedPassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                CategoryId = 1,
                UserId = 1,
                CategoryName = "Travelling",
                IsActive = true
            });

            _db.Expenses.Add(new Entities.Expenses
            {
                ExpenseId = 1,
                UserId = 1,
                CategoryId = 1,
                Amount = 100,
                Description = "Office commute",
                ExpenseDate = DateTime.Parse("2025-01-01"),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            _db.Budgets.Add(new Entities.Budget
            {
                BudgetId = 1,
                UserId = 1,
                CategoryId = 1,
                Month = 1,
                Year = 2025,
                BudgetAmount = 1000,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            //Act
            var result = await _service.GetSummaryAsync(1, 2025);

            //Assert
            var summary = result.First();            

            Assert.Multiple(() =>
            {
                Assert.That(result!.Count(), Is.EqualTo(1));
                Assert.That(summary!.TotalSpent, Is.EqualTo(100));
                Assert.That(summary!.Remaining, Is.EqualTo(900));
                Assert.That(summary!.Status, Is.EqualTo("Within budget"));
            });
        }

        [Test]
        public async Task GetSummaryAsync_ShouldReturnOverspentStatus_WhenExpensesExceedBudget()
        {
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User",
                Email = "test@finpal.com",
                Password = "hashedPassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                CategoryId = 1,
                UserId = 1,
                CategoryName = "Travelling",
                IsActive = true
            });

            var expense1 = new Entities.Expenses
            {
                ExpenseId = 1,
                UserId = 1,
                CategoryId = 1,
                Amount = 1000,
                Description = "Office commute",
                ExpenseDate = DateTime.Parse("2025-01-01"),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var expense2 = new Entities.Expenses
            {
                ExpenseId = 2,
                UserId = 1,
                CategoryId = 1,
                Amount = 2000,
                Description = "Trip tickets",
                ExpenseDate = DateTime.Parse("2025-01-01"),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _db.Expenses.AddRange(expense1, expense2);

            _db.Budgets.Add(new Entities.Budget
            {
                BudgetId = 1,
                UserId = 1,
                CategoryId = 1,
                Month = 1,
                Year = 2025,
                BudgetAmount = 1000,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            //Act
            var result = await _service.GetSummaryAsync(1, 2025);

            //Assert
            Assert.That(result, Is.Not.Null);

            var summary = result.First();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(summary!.TotalSpent, Is.EqualTo(3000));
                Assert.That(summary!.Remaining, Is.EqualTo(-2000));
                Assert.That(summary!.Status, Is.EqualTo("Overspent"));
            });
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
    }
}
