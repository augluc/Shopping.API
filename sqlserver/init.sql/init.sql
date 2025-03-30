-- Criar Banco de Dados
CREATE DATABASE RendimentoPay;
GO

USE RendimentoPay;
GO

-- Criar tabela Cart
CREATE TABLE Cart (
    CartId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    PayerDocument NVARCHAR(11) NOT NULL,
    DiscountPercentage DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    CreatedAt DATETIME NOT NULL
);
GO

-- Criar tabela Product
CREATE TABLE Product (
    ProductId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    CartId INT NOT NULL,
    ProductName NVARCHAR(100) NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (CartId) REFERENCES Cart (CartId) ON DELETE CASCADE
);
GO

-- Criar tabela Orders
CREATE TABLE Orders (
    OrderId         INT IDENTITY(1,1) NOT NULL,
    CartId          INT NOT NULL,
    PaymentStatus   NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    PaymentId       UNIQUEIDENTIFIER NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
    CONSTRAINT FK_Orders_Carts FOREIGN KEY (CartId) 
        REFERENCES Cart (CartId) 
        ON DELETE CASCADE
);
GO
