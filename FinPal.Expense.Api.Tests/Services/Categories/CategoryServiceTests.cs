using FinPal.Expense.Api.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinPal.Expense.Api.Infrastructure.CurrentUser;
using FinPal.Expense.Api.Services.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using FinPal.Expense.Api.DTO.Categories;
using Microsoft.Identity.Client;

namespace FinPal.Expense.Api.Tests.Services.Category
{
    internal class CategoryServiceTests
    {
        private FinPalDbContext _db = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private CategoryService _service = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<FinPalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new FinPalDbContext(options);

            _currentUserMock = new Mock<ICurrentUserService>();

            _service = new CategoryService(_db, _currentUserMock.Object, NullLogger<CategoryService>.Instance);
        }

        [Test]
        public async Task CreateAsync_Task_ShouldCreateCategory_WhenValid()
        {
            //Arrange
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "UnitTestUser1",
                Email = "UnitTest@cherry.com",
                Password = "password",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            await _db.SaveChangesAsync();

            var request = new CreateCategoryRequestDto
            {
                CategoryName = "Test Category"
            };

            //Act
            var result = await _service.CreateAsync(request);

            //Assert
            Assert.That(result.CategoryName, Is.EqualTo("Test Category"));
            Assert.That(_db.Categories.Count(), Is.EqualTo(1));

        }

        [Test]
        public async Task CreateAsync_ShouldThrowException_WhenDuplicateCategoryExists()
        {
            //Arrange
            _currentUserMock.Setup(u => u.UserId).Returns(1);

            _db.Users.Add(new Entities.User
            {
                UserID = 1,
                FullName = "Test User1",
                Email = "test@finpal.com",
                Password = "hashedPassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.Categories.Add(new Entities.Category
            {
                UserId = 1,
                CategoryName = "Entertainment",
                IsActive = true
            });

            await _db.SaveChangesAsync();

            var request = new CreateCategoryRequestDto
            {
                CategoryName = "Entertainment"
            };

            //Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));

            Assert.That(ex!.Message, Is.EqualTo("Category already exists"));
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
    }
}
