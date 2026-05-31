using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.Infrastructure.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinPal.Expense.Api.Services.Expense;
using Castle.Core.Logging;
using FinPal.Expense.Api.Services.Categories;
using Microsoft.Extensions.Logging.Abstractions;
using FinPal.Expense.Api.DTO.Expenses;
using NuGet.Frameworks;

namespace FinPal.Expense.Api.Tests.Services.Expenses
{
    internal class ExpenseServiceTests
    {
        private FinPalDbContext _db = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private ExpenseService _service = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<FinPalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new FinPalDbContext(options);

            _currentUserMock = new Mock<ICurrentUserService>();

            _service = new ExpenseService(_db, _currentUserMock.Object, NullLogger<ExpenseService>.Instance);
        }

        [Test]
        public void CreateAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            //Arrange
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            var request = new CreateExpenseRequestDto
            {
                CategoryId = 1,
                Amount = 1000,
                Description = "Dinner",
                ExpenseDate = DateTime.UtcNow
            };

            //Act and Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.CreateAsync(request));

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Invalid User"));
                Assert.That(_db.Expenses.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task CreateAsync_ShouldThrowException_WhenCategoryDoesNotBelongToUser()
        {
            //Arrange
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User",
                Email = "test@finpal.com",
                Password = "hashed",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Users.Add(new Entities.User
            {
                UserID = 2,
                FullName = "Test User 2",
                Email = "test2@finpal.com",
                Password = "hashed2",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                CategoryId = 1,
                UserId = 2,
                CategoryName = "Shopping",
                IsActive = true
            });

            await _db.SaveChangesAsync();

            var request = new CreateExpenseRequestDto
            {
                CategoryId = 1,
                Amount = 1000,
                Description = "Test expense",
                ExpenseDate = DateTime.UtcNow
            };

            //Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.CreateAsync(request));

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Invalid category"));
                Assert.That(_db.Expenses.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task CreateAsync_ShouldCreateExpense_WhenRequestIsValid()
        {
            //Arrange
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User",
                Email = "testuser@finpal.com",
                Password = "hashedPassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                CategoryId = 1,
                UserId = 1,
                CategoryName = "Food",
                IsActive = true
            });

            await _db.SaveChangesAsync();

            var request = new CreateExpenseRequestDto
            {
                Amount = 1000,
                Description = "Lunch",
                ExpenseDate = DateTime.UtcNow,
                CategoryId = 1
            };

            //Act
            await _service.CreateAsync(request);

            //Assert
            var expense = await _db.Expenses.FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(_db.Expenses.Count(), Is.EqualTo(1));
                Assert.That(expense.UserId, Is.EqualTo(1));
                Assert.That(expense.CategoryId, Is.EqualTo(1));
                Assert.That(expense.Amount, Is.EqualTo(1000));
                Assert.That(expense.Description, Is.EqualTo("Lunch"));
            });            
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
    }
}
