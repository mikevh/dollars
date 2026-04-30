

create table Accounts (
    Id int primary key identity(1,1) not null,
    UserId int not null,
    SourceId nvarchar(255) not null,
    [Name] NVARCHAR(255) not null,
    CreatedOn datetime2 not null,
    UpdatedOn datetime2 not null,
)
GO
create table AccountBalances (
    Id int primary key identity(1,1) not null,
    AccountId int not null references Accounts(Id),
    Balance decimal(18, 2) not null,
    [Date] datetime2 not null,
    CreatedOn datetime2 not null,
    UpdatedOn datetime2 not null,
)
GO
create table Transactions (
    Id int primary key identity(1,1) not null,
    AccountId int not null references Accounts(Id),
    SourceId nvarchar(255) not null,
    Payee nvarchar(255) not null,
    Amount decimal(18, 2) not null,
    [Date] datetime2 not null,
    [Description] NVARCHAR(255) not null,
    Memo NVARCHAR(255) not null,
    CreatedOn datetime2 not null,
    UpdatedOn datetime2 not null,
    INDEX IX_Transactions_Date_SoruceId ([Date], SourceId)
)
GO
