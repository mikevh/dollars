

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

create table SyncLogs (
    Id int primary key identity(1,1) not null,    
    SyncDate datetime2 not null,
    [Provider] nvarchar(255) not null,
    Success bit not null,
    ErrorMessage nvarchar(max) null,
    TransactionCount int not null,
    JsonData nvarchar(max) not null,
    CreatedOn datetime2 not null,
    UpdatedOn datetime2 not null,
)
go

create table PlaidSyncState (
    Id int primary key identity(1,1) not null,
    ItemId NVARCHAR(256),
    AccessToken NVARCHAR(512) not null,
    NextCursor NVARCHAR(256) not null,
    LastSyncAt datetime2 not null,
    CreatedAt datetime2 not null,
    UpdatedAt datetime2 not null,
)

GO
create table PlaidTransactions (
    Id int primary key identity(1,1) not null,
    PlaidSyncStateId nvarchar(450) not null,
    AccountId nvarchar(450) not null,
    AccountName nvarchar(256),
    Amount decimal(18, 2) not null,
    [Date] date not null,
    AuthorizedDate date not null,
    [Name] nvarchar(512) not null,
    MerchantName nvarchar(512),
    PersonalFinanceCategory nvarchar(256),
    Pending bit not null,
    PendingTransactionId nvarchar(450),
    PaymentChannel nvarchar(64),
    IsoCurrencyCode nvarchar(10),
    JsonData nvarchar(max) not null,
    CreatedAt datetime2 not null,
    UpdatedAt datetime2 not null
)
