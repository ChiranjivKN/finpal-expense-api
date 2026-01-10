-- =============================================
-- Database: FinPal Expense Management System
-- =============================================

CREATE DATABASE FinPalDB;
GO

USE FinPalDB;
GO

-- =============================================
-- Users
-- =============================================

Create Table Users (
	UserID INT IDENTITY(1,1),
	FullName Nvarchar (100) Not Null,
	Email Nvarchar (150) Not Null,
	PasswordHash Nvarchar (256) Not Null,
	CreatedAt DateTime Not Null Default GetDate(),
	IsActive Bit Not Null Default 1
	
	Constraint PK_Users_UserID Primary Key (UserID),
	Constraint UQ_Users_Email Unique (Email)
);

-- =============================================
-- Categories
-- =============================================

Create Table Categories (
	CategoryID INT Identity(1,1),
	UserID Int Not Null,
	CategoryName Nvarchar(100) Not Null,
	IsActive Bit Not Null Default 1,

	Constraint PK_Categories_CategoryID Primary Key (CategoryID),
	Constraint FK_Categories_Users Foreign Key (UserId) References Users(UserID),
	Constraint UQ_Categories_UserID_CategoryID Unique (UserID, CategoryID)
);

-- =============================================
-- Expenses
-- =============================================

Create Table Expenses (
	ExpenseID INT Identity (1,1),
	UserID INT Not Null,
	CategoryID INT Not Null,
	Amount Decimal (10,2) Not Null,
	Description Nvarchar (250),
	ExpenseDate Date Not Null,
	CreatedAt DateTime Not Null Default GetDate(),
	IsDeleted Bit Not Null Default 0,

	Constraint PK_Expenses_ExpenseID Primary Key (ExpenseID),
	Constraint FK_Expenses_Users Foreign Key (UserID) References Users (UserID),
	Constraint FK_Expenses_Categories Foreign Key (CategoryID) References Categories (CategoryID),
	Constraint FK_Expenses_UserID_CategoryID Foreign Key (UserID, CategoryID) References Categories(UserID, CategoryID)
);

-- =============================================
-- Budgets
-- =============================================

Create Table Budgets (
	BudgetID Int Identity(1,1),
	UserID Int Not Null,
	CategoryID Int Not Null,
	[Month] Int Not Null Check ([Month] Between 1 And 12),
	[Year] Int Not Null,
	BudgetAmount Decimal (10,2) Not Null,
	CreatedAt DateTime Not Null Default GetDate(),

	Constraint PK_Budgets_BudgetID Primary Key (BudgetID),
	Constraint FK_Budgets_Users Foreign Key (UserID) References Users (UserID),
	Constraint FK_Budgets_Categories Foreign Key (CategoryID) References Categories (CategoryID),
	Constraint FK_Budgets_UserID_CategoryID Foreign Key (UserID, CategoryID) References Categories (UserID, CategoryID),
	Constraint UQ_Budgets_User_Category_Month_Year Unique (UserID, CategoryID, [Month], [Year])
);

-- =============================================
-- Indexes
-- =============================================

Create Index IX_Expenses_User_Date
ON Expenses (UserID, ExpenseDate)

Create Index IX_Expenses_Category
ON Expenses (CategoryID)