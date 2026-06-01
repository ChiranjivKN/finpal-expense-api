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
using Microsoft.Extensions.Configuration.UserSecrets;
using NuGet.Frameworks;

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
            Assert.Multiple(() =>
            {
                Assert.That(result.CategoryName, Is.EqualTo("Test Category"));
                Assert.That(_db.Categories.Count(), Is.EqualTo(1));
            });            
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

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Category already exists"));
                Assert.That(_db.Categories.Count(), Is.EqualTo(1));
            });           
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenValid()
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

            var category = new Entities.Category
            {
                UserId = 1,
                CategoryName = "Entertainment",
                IsActive = true
            };

            _db.Categories.Add(category);

            await _db.SaveChangesAsync();

            var request = new CreateCategoryRequestDto
            {
                CategoryName = "Fun and Entertainment"
            };

            //Act
            await _service.UpdateAsync(category.CategoryId, request);

            //Assert
            var updatedCategory = await _db.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId && c.UserId == 1 && c.IsActive);

            Assert.Multiple(() =>
            {
                Assert.That(_db.Categories.Count(), Is.EqualTo(1));
                Assert.That(updatedCategory, Is.Not.Null);
                Assert.That(updatedCategory!.CategoryName, Is.EqualTo("Fun and Entertainment"));
            });            
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowException_WhenCategoryNotFound()
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

            await _db.SaveChangesAsync();

            var request = new CreateCategoryRequestDto
            {
                CategoryName = "Food"
            };

            //Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.UpdateAsync(1, request));

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Category does not exist"));
                Assert.That(_db.Categories.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowException_WhenCategoryNameAlreadyExists()
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

            var category = new Entities.Category
            {
                UserId = 1,
                CategoryName = "Food",
                IsActive = true
            };

            var category2 = new Entities.Category
            {
                UserId = 1,
                CategoryName = "Eating",
                IsActive = true
            };

            _db.Categories.AddRange(category, category2);

            await _db.SaveChangesAsync();

            var request = new CreateCategoryRequestDto
            {
                CategoryName = "Food"
            };

            //Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(category2.CategoryId, request));

            var updatedCategory = await _db.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == category2.CategoryId && c.UserId == 1 && c.IsActive);

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Duplicate category"));
                Assert.That(_db.Categories.Count(), Is.EqualTo(2));
                Assert.That(updatedCategory!.CategoryName, Is.EqualTo("Eating"));
            });
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteCategory_WhenValid()
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

            var category = new Entities.Category
            {
                UserId = 1,
                CategoryName = "Food",
                IsActive = true
            };

            _db.Categories.Add(category);

            await _db.SaveChangesAsync();

            //Act
            await _service.DeleteAsync(category.CategoryId);

            //Assert
            var updatedCategory = await _db.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId && c.UserId == 1);

            Assert.Multiple(() =>
            {
                Assert.That(updatedCategory, Is.Not.Null);
                Assert.That(updatedCategory!.IsActive, Is.False);
                Assert.That(updatedCategory!.CategoryName, Is.EqualTo("Food"));
            });
        }

    [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
    }
}
