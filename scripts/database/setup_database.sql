IF DB_ID(N'EyeglassesShop') IS NULL
    CREATE DATABASE EyeglassesShop;
GO
USE EyeglassesShop;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DROP VIEW IF EXISTS dbo.vw_DailyRevenue;
DROP VIEW IF EXISTS dbo.vw_BestSellers;
DROP VIEW IF EXISTS dbo.vw_LowStock;
DROP PROCEDURE IF EXISTS dbo.sp_CreateOrderFromCart;
DROP PROCEDURE IF EXISTS dbo.sp_CreatePaymentAttempt;
DROP PROCEDURE IF EXISTS dbo.sp_ConfirmVnPayPayment;
DROP PROCEDURE IF EXISTS dbo.sp_CancelOrder;
DROP PROCEDURE IF EXISTS dbo.sp_RefundOrder;
DROP PROCEDURE IF EXISTS dbo.sp_UpdateOrderStatus;
DROP PROCEDURE IF EXISTS dbo.sp_ExpireUnpaidOrders;
DROP PROCEDURE IF EXISTS dbo.usp_ReleaseOrderStock;
DROP TABLE IF EXISTS dbo.Notifications;
DROP TABLE IF EXISTS dbo.Reviews;
DROP TABLE IF EXISTS dbo.Wishlists;
DROP TABLE IF EXISTS dbo.OrderStatusHistory;
DROP TABLE IF EXISTS dbo.Payments;
DROP TABLE IF EXISTS dbo.OrderItems;
DROP TABLE IF EXISTS dbo.Orders;
DROP TABLE IF EXISTS dbo.CustomerPrescriptions;
DROP TABLE IF EXISTS dbo.CartItems;
DROP TABLE IF EXISTS dbo.Coupons;
DROP TABLE IF EXISTS dbo.LensOptions;
DROP TABLE IF EXISTS dbo.ProductImages;
DROP TABLE IF EXISTS dbo.ProductVariants;
DROP TABLE IF EXISTS dbo.Products;
DROP TABLE IF EXISTS dbo.Brands;
DROP TABLE IF EXISTS dbo.Categories;
DROP TABLE IF EXISTS dbo.Banners;
DROP TABLE IF EXISTS dbo.UserAddresses;
DROP TABLE IF EXISTS dbo.Users;
DROP TABLE IF EXISTS dbo.OrderStatuses;
DROP TABLE IF EXISTS dbo.Roles;
GO

CREATE TABLE Roles (
    RoleId   INT          NOT NULL PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Users (
    UserId                 INT IDENTITY(1,1) PRIMARY KEY,
    FullName               NVARCHAR(100) NOT NULL,
    Email                  NVARCHAR(150) NOT NULL,
    Phone                  NVARCHAR(20)  NOT NULL,
    PasswordHash           NVARCHAR(255) NOT NULL,
    Gender                 NVARCHAR(10)  NULL CHECK (Gender IN (N'Nữ', N'Nam', N'Khác')),
    BirthDate              DATE          NULL,
    RoleId                 INT           NOT NULL DEFAULT 1 REFERENCES Roles(RoleId),
    IsActive               BIT           NOT NULL DEFAULT 1,

    ResetPasswordTokenHash CHAR(64)      NULL,
    ResetPasswordExpire    DATETIME2     NULL,
    CreatedAt              DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt              DATETIME2     NULL
);
CREATE UNIQUE INDEX UX_Users_Email ON Users(Email);
CREATE UNIQUE INDEX UX_Users_Phone ON Users(Phone);
CREATE INDEX IX_Users_ResetToken ON Users(ResetPasswordTokenHash) WHERE ResetPasswordTokenHash IS NOT NULL;
CREATE INDEX IX_Users_Role_Active ON Users(RoleId, IsActive);

CREATE TABLE UserAddresses (
    AddressId       INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
    RecipientName   NVARCHAR(100) NOT NULL,
    Phone           NVARCHAR(20)  NOT NULL,
    Province        NVARCHAR(100) NOT NULL,
    DistrictWard    NVARCHAR(150) NOT NULL,
    StreetAddress   NVARCHAR(200) NOT NULL,
    IsDefault       BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_UserAddresses_User ON UserAddresses(UserId);

CREATE UNIQUE INDEX UX_UserAddresses_Default ON UserAddresses(UserId) WHERE IsDefault = 1;
GO

CREATE TABLE Categories (
    CategoryId   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Slug         NVARCHAR(120) NOT NULL UNIQUE,
    Description  NVARCHAR(500) NULL,
    IsActive     BIT NOT NULL DEFAULT 1
);

CREATE TABLE Brands (
    BrandId     INT IDENTITY(1,1) PRIMARY KEY,
    BrandName   NVARCHAR(100) NOT NULL UNIQUE,
    Country     NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);

CREATE TABLE Products (
    ProductId    INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode  NVARCHAR(50)  NOT NULL,
    ProductName  NVARCHAR(200) NOT NULL,
    CategoryId   INT NOT NULL REFERENCES Categories(CategoryId),
    BrandId      INT NOT NULL REFERENCES Brands(BrandId),
    Price        DECIMAL(18,0) NOT NULL CHECK (Price >= 0),
    Gender       NVARCHAR(10)  NOT NULL DEFAULT N'Unisex' CHECK (Gender IN (N'Unisex', N'Nữ', N'Nam')),
    FrameShape   NVARCHAR(20)  NULL CHECK (FrameShape IN (N'Chữ nhật', N'Vuông', N'Tròn', N'Oval', N'Mắt mèo', N'Đa giác')),
    Material     NVARCHAR(20)  NULL CHECK (Material IN (N'Nhựa', N'Kim loại', N'Titan')),
    Description  NVARCHAR(MAX) NULL,
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_Products_Code ON Products(ProductCode);
CREATE INDEX IX_Products_Filter ON Products(CategoryId, BrandId, IsActive) INCLUDE (Price, Gender, FrameShape, Material);

CREATE TABLE ProductVariants (
    VariantId     INT IDENTITY(1,1) PRIMARY KEY,
    ProductId     INT NOT NULL REFERENCES Products(ProductId) ON DELETE CASCADE,
    Sku           NVARCHAR(60) NOT NULL,
    ColorName     NVARCHAR(60) NOT NULL,
    ColorGroup    NVARCHAR(20) NOT NULL
                  CHECK (ColorGroup IN (N'Đen', N'Nâu', N'Đồi mồi', N'Trong suốt', N'Hồng / Đỏ', N'Xám / Bạc', N'Vàng')),
    SizeLabel     NVARCHAR(10) NOT NULL DEFAULT N'M',
    LensWidth     SMALLINT NULL,
    BridgeWidth   SMALLINT NULL,
    TempleLength  SMALLINT NULL,
    StockQty      INT NOT NULL DEFAULT 0 CHECK (StockQty >= 0),
    IsActive      BIT NOT NULL DEFAULT 1
);
CREATE UNIQUE INDEX UX_Variants_Sku ON ProductVariants(Sku);
CREATE UNIQUE INDEX UX_Variants_Combo ON ProductVariants(ProductId, ColorName, SizeLabel);

CREATE TABLE ProductImages (
    ImageId    INT IDENTITY(1,1) PRIMARY KEY,
    ProductId  INT NOT NULL REFERENCES Products(ProductId) ON DELETE CASCADE,
    VariantId  INT NULL REFERENCES ProductVariants(VariantId),
    ImageUrl   NVARCHAR(300) NOT NULL,
    SortOrder  INT NOT NULL DEFAULT 0,
    IsPrimary  BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_ProductImages_Product ON ProductImages(ProductId, SortOrder);

CREATE TABLE LensOptions (
    LensOptionId   INT IDENTITY(1,1) PRIMARY KEY,
    LensName       NVARCHAR(150) NOT NULL UNIQUE,
    RefractiveIndex DECIMAL(3,2) NOT NULL,
    ExtraPrice     DECIMAL(18,0) NOT NULL DEFAULT 0 CHECK (ExtraPrice >= 0),
    RxLimit        NVARCHAR(100) NULL,
    Description    NVARCHAR(500) NULL,
    IsActive       BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Coupons (
    CouponId       INT IDENTITY(1,1) PRIMARY KEY,
    CouponCode     NVARCHAR(50) NOT NULL UNIQUE,
    DiscountType   VARCHAR(10)  NOT NULL CHECK (DiscountType IN ('AMOUNT', 'PERCENT')),
    DiscountValue  DECIMAL(18,2) NOT NULL CHECK (DiscountValue > 0),
    MaxDiscount    DECIMAL(18,0) NULL,
    MinOrderAmount DECIMAL(18,0) NOT NULL DEFAULT 0,
    StartDate      DATE NULL,
    ExpireDate     DATE NOT NULL,
    UsageLimit     INT NULL,
    UsedCount      INT NOT NULL DEFAULT 0 CHECK (UsedCount >= 0),
    IsActive       BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_Coupons_Percent CHECK (DiscountType = 'AMOUNT' OR DiscountValue <= 100)
);

CREATE TABLE Banners (
    BannerId   INT IDENTITY(1,1) PRIMARY KEY,
    Title      NVARCHAR(200) NOT NULL,
    LinkUrl    NVARCHAR(300) NOT NULL,
    ImageUrl   NVARCHAR(300) NOT NULL,
    SortOrder  INT NOT NULL DEFAULT 0,
    IsActive   BIT NOT NULL DEFAULT 1,
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    TargetRoleId   INT NULL REFERENCES Roles(RoleId),
    TargetUserId   INT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
    Title          NVARCHAR(200) NOT NULL,
    Content        NVARCHAR(500) NULL,
    LinkUrl        NVARCHAR(300) NULL,
    IsRead         BIT NOT NULL DEFAULT 0,
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Notifications_Target CHECK (TargetRoleId IS NOT NULL OR TargetUserId IS NOT NULL)
);
CREATE INDEX IX_Notifications_Role ON Notifications(TargetRoleId, IsRead, CreatedAt DESC);
CREATE INDEX IX_Notifications_User ON Notifications(TargetUserId, IsRead, CreatedAt DESC);
GO

CREATE TABLE CartItems (
    CartItemId      INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
    VariantId       INT NOT NULL REFERENCES ProductVariants(VariantId),
    LensOptionId    INT NOT NULL DEFAULT 1 REFERENCES LensOptions(LensOptionId),
    Quantity        INT NOT NULL DEFAULT 1 CHECK (Quantity BETWEEN 1 AND 20),
    IsSelected      BIT NOT NULL DEFAULT 1,

    RightSph        DECIMAL(4,2) NULL,
    RightCyl        DECIMAL(4,2) NULL,
    RightAxis       SMALLINT NULL,
    LeftSph         DECIMAL(4,2) NULL,
    LeftCyl         DECIMAL(4,2) NULL,
    LeftAxis        SMALLINT NULL,
    PD              DECIMAL(4,1) NULL,
    PrescriptionImg NVARCHAR(300) NULL,
    Note            NVARCHAR(300) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT CK_CartItems_Rx CHECK (
        (RightSph IS NULL OR RightSph BETWEEN -15 AND 10) AND (LeftSph IS NULL OR LeftSph BETWEEN -15 AND 10)
        AND (RightCyl IS NULL OR RightCyl BETWEEN -6 AND 6) AND (LeftCyl IS NULL OR LeftCyl BETWEEN -6 AND 6)
        AND (RightAxis IS NULL OR RightAxis BETWEEN 0 AND 180) AND (LeftAxis IS NULL OR LeftAxis BETWEEN 0 AND 180)
        AND (PD IS NULL OR PD BETWEEN 50 AND 78)
        AND (ISNULL(RightCyl, 0) = 0 OR RightAxis IS NOT NULL)
        AND (ISNULL(LeftCyl, 0) = 0 OR LeftAxis IS NOT NULL))
);
CREATE INDEX IX_CartItems_User ON CartItems(UserId);

CREATE TABLE CustomerPrescriptions (
    UserPrescriptionId INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
    ProfileName     NVARCHAR(100) NOT NULL DEFAULT N'Độ cận chính của tôi',
    RightSph        DECIMAL(4,2) NULL,
    RightCyl        DECIMAL(4,2) NULL,
    RightAxis       SMALLINT NULL,
    LeftSph         DECIMAL(4,2) NULL,
    LeftCyl         DECIMAL(4,2) NULL,
    LeftAxis        SMALLINT NULL,
    PD              DECIMAL(4,1) NULL,
    PrescriptionImg NVARCHAR(300) NULL,
    IsDefault       BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT CK_CustPresc_Rx CHECK (
        (RightSph IS NULL OR RightSph BETWEEN -15 AND 10) AND (LeftSph IS NULL OR LeftSph BETWEEN -15 AND 10)
        AND (RightCyl IS NULL OR RightCyl BETWEEN -6 AND 6) AND (LeftCyl IS NULL OR LeftCyl BETWEEN -6 AND 6)
        AND (RightAxis IS NULL OR RightAxis BETWEEN 0 AND 180) AND (LeftAxis IS NULL OR LeftAxis BETWEEN 0 AND 180)
        AND (PD IS NULL OR PD BETWEEN 50 AND 78)
        AND (ISNULL(RightCyl, 0) = 0 OR RightAxis IS NOT NULL)
        AND (ISNULL(LeftCyl, 0) = 0 OR LeftAxis IS NOT NULL))
);
CREATE INDEX IX_CustPresc_User ON CustomerPrescriptions(UserId);
CREATE UNIQUE INDEX UX_CustPresc_Default ON CustomerPrescriptions(UserId) WHERE IsDefault = 1;

CREATE TABLE Wishlists (
    UserId    INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
    ProductId INT NOT NULL REFERENCES Products(ProductId) ON DELETE CASCADE,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Wishlists PRIMARY KEY (UserId, ProductId)
);
GO

CREATE TABLE OrderStatuses (
    StatusId    TINYINT NOT NULL PRIMARY KEY,
    StatusName  NVARCHAR(50) NOT NULL
);

CREATE TABLE Orders (
    OrderId        INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode      AS (N'MR-' + RIGHT(N'000000' + CAST(OrderId AS NVARCHAR(10)), 6)) PERSISTED,
    UserId         INT NOT NULL REFERENCES Users(UserId),

    RecipientName  NVARCHAR(100) NOT NULL,
    Phone          NVARCHAR(20)  NOT NULL,
    ShippingAddress NVARCHAR(300) NOT NULL,
    OrderNote      NVARCHAR(300) NULL,
    CouponId       INT NULL REFERENCES Coupons(CouponId),
    CouponCode     NVARCHAR(50) NULL,
    Subtotal       DECIMAL(18,0) NOT NULL CHECK (Subtotal >= 0),
    DiscountAmount DECIMAL(18,0) NOT NULL DEFAULT 0 CHECK (DiscountAmount >= 0),
    ShippingFee    DECIMAL(18,0) NOT NULL DEFAULT 0 CHECK (ShippingFee >= 0),
    TotalAmount    DECIMAL(18,0) NOT NULL CHECK (TotalAmount >= 0),
    Status         TINYINT NOT NULL DEFAULT 0 REFERENCES OrderStatuses(StatusId),
    PaymentExpireAt DATETIME2 NULL,
    HandledBy      INT NULL REFERENCES Users(UserId),
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    PaidAt         DATETIME2 NULL,
    CompletedAt    DATETIME2 NULL,
    CancelledAt    DATETIME2 NULL,
    CancelReason   NVARCHAR(300) NULL,

    RefundedAt          DATETIME2 NULL,
    RefundTransactionNo NVARCHAR(100) NULL,
    RefundAmount        DECIMAL(18,0) NULL,
    RefundReason        NVARCHAR(300) NULL,
    CONSTRAINT CK_Orders_Total CHECK (TotalAmount = Subtotal - DiscountAmount + ShippingFee)
);
CREATE UNIQUE INDEX UX_Orders_Code ON Orders(OrderCode);
CREATE INDEX IX_Orders_User ON Orders(UserId, CreatedAt DESC);
CREATE INDEX IX_Orders_Status ON Orders(Status, CreatedAt DESC);
CREATE INDEX IX_Orders_Expire ON Orders(PaymentExpireAt) WHERE Status = 0;

CREATE TABLE OrderItems (
    OrderItemId   INT IDENTITY(1,1) PRIMARY KEY,
    OrderId       INT NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
    ProductId     INT NULL REFERENCES Products(ProductId),
    VariantId     INT NULL REFERENCES ProductVariants(VariantId),
    ProductName   NVARCHAR(200) NOT NULL,
    ProductCode   NVARCHAR(50)  NOT NULL,
    Sku           NVARCHAR(60)  NOT NULL,
    ColorName     NVARCHAR(60)  NOT NULL,
    SizeLabel     NVARCHAR(10)  NOT NULL,
    LensOptionId  INT NULL REFERENCES LensOptions(LensOptionId),
    LensName      NVARCHAR(150) NOT NULL,
    FramePrice    DECIMAL(18,0) NOT NULL CHECK (FramePrice >= 0),
    LensPrice     DECIMAL(18,0) NOT NULL DEFAULT 0 CHECK (LensPrice >= 0),
    Quantity      INT NOT NULL CHECK (Quantity > 0),
    LineTotal     AS ((FramePrice + LensPrice) * Quantity) PERSISTED,

    RightSph        DECIMAL(4,2) NULL,
    RightCyl        DECIMAL(4,2) NULL,
    RightAxis       SMALLINT NULL,
    LeftSph         DECIMAL(4,2) NULL,
    LeftCyl         DECIMAL(4,2) NULL,
    LeftAxis        SMALLINT NULL,
    PD              DECIMAL(4,1) NULL,
    PrescriptionImg NVARCHAR(300) NULL,
    Note            NVARCHAR(300) NULL,
    CONSTRAINT CK_OrderItems_Rx CHECK (
        (RightSph IS NULL OR RightSph BETWEEN -15 AND 10) AND (LeftSph IS NULL OR LeftSph BETWEEN -15 AND 10)
        AND (RightCyl IS NULL OR RightCyl BETWEEN -6 AND 6) AND (LeftCyl IS NULL OR LeftCyl BETWEEN -6 AND 6)
        AND (RightAxis IS NULL OR RightAxis BETWEEN 0 AND 180) AND (LeftAxis IS NULL OR LeftAxis BETWEEN 0 AND 180)
        AND (PD IS NULL OR PD BETWEEN 50 AND 78)
        AND (ISNULL(RightCyl, 0) = 0 OR RightAxis IS NOT NULL)
        AND (ISNULL(LeftCyl, 0) = 0 OR LeftAxis IS NOT NULL))
);
CREATE INDEX IX_OrderItems_Order ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_Product ON OrderItems(ProductId);

CREATE TABLE Payments (
    PaymentId      INT IDENTITY(1,1) PRIMARY KEY,
    OrderId        INT NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
    Method         NVARCHAR(20) NOT NULL DEFAULT N'VNPAY',
    Amount         DECIMAL(18,0) NOT NULL CHECK (Amount >= 0),
    Status         TINYINT NOT NULL DEFAULT 0 CHECK (Status BETWEEN 0 AND 3),
    TransactionRef NVARCHAR(100) NOT NULL,
    TransactionNo  NVARCHAR(100) NULL,
    BankCode       NVARCHAR(50)  NULL,
    ResponseCode   NVARCHAR(10)  NULL,
    PayDate        DATETIME2 NULL,
    RawResponse    NVARCHAR(MAX) NULL,
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_Payments_TxnRef ON Payments(TransactionRef);
CREATE INDEX IX_Payments_Order ON Payments(OrderId);
CREATE INDEX IX_Payments_TxnNo ON Payments(TransactionNo) WHERE TransactionNo IS NOT NULL;

CREATE TABLE OrderStatusHistory (
    HistoryId  INT IDENTITY(1,1) PRIMARY KEY,
    OrderId    INT NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
    OldStatus  TINYINT NULL REFERENCES OrderStatuses(StatusId),
    NewStatus  TINYINT NOT NULL REFERENCES OrderStatuses(StatusId),
    ChangedBy  INT NULL REFERENCES Users(UserId),
    Note       NVARCHAR(300) NULL,
    ChangedAt  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_OrderHistory_Order ON OrderStatusHistory(OrderId, ChangedAt);
GO

CREATE TABLE Reviews (
    ReviewId    INT IDENTITY(1,1) PRIMARY KEY,
    ProductId   INT NOT NULL REFERENCES Products(ProductId) ON DELETE CASCADE,
    UserId      INT NOT NULL REFERENCES Users(UserId),
    Rating      TINYINT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment     NVARCHAR(1000) NOT NULL,
    Status      TINYINT NOT NULL DEFAULT 0 CHECK (Status IN (0, 1, 2)),
    IsRead      BIT NOT NULL DEFAULT 0,
    ModeratedBy INT NULL REFERENCES Users(UserId),
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_Reviews_UserProduct ON Reviews(UserId, ProductId);
CREATE INDEX IX_Reviews_Product ON Reviews(ProductId, Status);
CREATE INDEX IX_Reviews_Moderation ON Reviews(Status, IsRead, CreatedAt DESC);
GO

CREATE OR ALTER TRIGGER trg_Orders_StatusHistory ON Orders
AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO OrderStatusHistory(OrderId, OldStatus, NewStatus, ChangedBy, Note)
        SELECT i.OrderId, NULL, i.Status, i.UserId, N'Khách hàng khởi tạo đơn hàng'
        FROM inserted i;
    END
    ELSE IF UPDATE(Status)
    BEGIN
        INSERT INTO OrderStatusHistory(OrderId, OldStatus, NewStatus, ChangedBy, Note)
        SELECT i.OrderId, d.Status, i.Status, i.HandledBy,
               CASE
                   WHEN i.Status = 1 THEN N'Thanh toán VNPay thành công (tự động)'
                   WHEN i.Status = 2 THEN N'Nhân viên nhận đơn & bắt đầu cắt mài tròng kính'
                   WHEN i.Status = 3 THEN N'Đơn hàng đang được vận chuyển'
                   WHEN i.Status = 4 THEN N'Đơn hàng giao thành công'
                   WHEN i.Status = 5 AND i.RefundedAt IS NOT NULL THEN N'Admin hủy đơn & hoàn tiền VNPay'
                   WHEN i.Status = 5 THEN ISNULL(i.CancelReason, N'Đơn hàng đã hủy')
                   WHEN i.Status = 6 THEN N'Đơn hàng trả lại'
                   ELSE N'Đổi trạng thái'
               END
        FROM inserted i JOIN deleted d ON d.OrderId = i.OrderId
        WHERE i.Status <> d.Status;
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReleaseOrderStock @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE v SET StockQty = v.StockQty + x.Qty
    FROM ProductVariants v
    JOIN (SELECT VariantId, SUM(Quantity) AS Qty
          FROM OrderItems
          WHERE OrderId = @OrderId AND VariantId IS NOT NULL
          GROUP BY VariantId) x ON x.VariantId = v.VariantId;

    UPDATE c SET UsedCount = CASE WHEN c.UsedCount > 0 THEN c.UsedCount - 1 ELSE 0 END
    FROM Coupons c
    JOIN Orders o ON o.CouponId = c.CouponId
    WHERE o.OrderId = @OrderId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateOrderFromCart
    @UserId          INT,
    @RecipientName   NVARCHAR(100),
    @Phone           NVARCHAR(20),
    @ShippingAddress NVARCHAR(300),
    @OrderNote       NVARCHAR(300) = NULL,
    @CouponCode      NVARCHAR(50)  = NULL,
    @ShippingFee     DECIMAL(18,0) = 0,
    @PaymentWindowMinutes INT = 15,
    @OrderId         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    IF NOT EXISTS (SELECT 1 FROM CartItems WHERE UserId = @UserId AND IsSelected = 1)
        THROW 50001, N'Chưa chọn sản phẩm nào trong giỏ hàng.', 1;

    IF EXISTS (SELECT 1
               FROM CartItems c
               JOIN ProductVariants v ON v.VariantId = c.VariantId
               JOIN Products p        ON p.ProductId = v.ProductId
               JOIN LensOptions l     ON l.LensOptionId = c.LensOptionId
               WHERE c.UserId = @UserId AND c.IsSelected = 1
                 AND (v.IsActive = 0 OR p.IsActive = 0 OR l.IsActive = 0))
        THROW 50002, N'Có sản phẩm hoặc gói tròng đã ngừng bán.', 1;

    DECLARE @need TABLE (VariantId INT PRIMARY KEY, Qty INT NOT NULL);
    INSERT INTO @need(VariantId, Qty)
    SELECT VariantId, SUM(Quantity) FROM CartItems
    WHERE UserId = @UserId AND IsSelected = 1 GROUP BY VariantId;

    UPDATE v SET StockQty = v.StockQty - n.Qty
    FROM ProductVariants v
    JOIN @need n ON n.VariantId = v.VariantId
    WHERE v.StockQty >= n.Qty;

    IF @@ROWCOUNT <> (SELECT COUNT(*) FROM @need)
        THROW 50003, N'Một số sản phẩm không đủ tồn kho.', 1;

    DECLARE @Subtotal DECIMAL(18,0);
    SELECT @Subtotal = SUM((p.Price + l.ExtraPrice) * c.Quantity)
    FROM CartItems c
    JOIN ProductVariants v ON v.VariantId = c.VariantId
    JOIN Products p        ON p.ProductId = v.ProductId
    JOIN LensOptions l     ON l.LensOptionId = c.LensOptionId
    WHERE c.UserId = @UserId AND c.IsSelected = 1;

    DECLARE @CouponId INT = NULL, @Discount DECIMAL(18,0) = 0;
    IF @CouponCode IS NOT NULL AND LTRIM(RTRIM(@CouponCode)) <> N''
    BEGIN
        DECLARE @Type VARCHAR(10), @Val DECIMAL(18,2), @Max DECIMAL(18,0);
        SELECT @CouponId = CouponId, @Type = DiscountType, @Val = DiscountValue, @Max = MaxDiscount
        FROM Coupons WITH (UPDLOCK)
        WHERE CouponCode = @CouponCode AND IsActive = 1
          AND (StartDate IS NULL OR StartDate <= CAST(SYSDATETIME() AS DATE))
          AND ExpireDate >= CAST(SYSDATETIME() AS DATE)
          AND MinOrderAmount <= @Subtotal
          AND (UsageLimit IS NULL OR UsedCount < UsageLimit);

        IF @CouponId IS NULL
            THROW 50004, N'Mã giảm giá không hợp lệ, đã hết hạn hoặc chưa đạt giá trị đơn tối thiểu.', 1;

        SET @Discount = CASE WHEN @Type = 'AMOUNT' THEN @Val ELSE ROUND(@Subtotal * @Val / 100, 0) END;
        IF @Type = 'PERCENT' AND @Max IS NOT NULL AND @Discount > @Max SET @Discount = @Max;
        IF @Discount > @Subtotal SET @Discount = @Subtotal;

        UPDATE Coupons SET UsedCount = UsedCount + 1 WHERE CouponId = @CouponId;
    END

    INSERT INTO Orders(UserId, RecipientName, Phone, ShippingAddress, OrderNote,
                       CouponId, CouponCode, Subtotal, DiscountAmount, ShippingFee, TotalAmount,
                       Status, PaymentExpireAt)
    VALUES (@UserId, @RecipientName, @Phone, @ShippingAddress, @OrderNote,
            @CouponId, CASE WHEN @CouponId IS NULL THEN NULL ELSE @CouponCode END,
            @Subtotal, @Discount, @ShippingFee, @Subtotal - @Discount + @ShippingFee,
            0, DATEADD(MINUTE, @PaymentWindowMinutes, SYSDATETIME()));
    SET @OrderId = SCOPE_IDENTITY();

    INSERT INTO OrderItems(OrderId, ProductId, VariantId, ProductName, ProductCode, Sku, ColorName, SizeLabel,
                           LensOptionId, LensName, FramePrice, LensPrice, Quantity,
                           RightSph, RightCyl, RightAxis, LeftSph, LeftCyl, LeftAxis, PD, PrescriptionImg, Note)
    SELECT @OrderId, p.ProductId, v.VariantId, p.ProductName, p.ProductCode, v.Sku, v.ColorName, v.SizeLabel,
           l.LensOptionId, l.LensName, p.Price, l.ExtraPrice, c.Quantity,
           c.RightSph, c.RightCyl, c.RightAxis, c.LeftSph, c.LeftCyl, c.LeftAxis, c.PD, c.PrescriptionImg, c.Note
    FROM CartItems c
    JOIN ProductVariants v ON v.VariantId = c.VariantId
    JOIN Products p        ON p.ProductId = v.ProductId
    JOIN LensOptions l     ON l.LensOptionId = c.LensOptionId
    WHERE c.UserId = @UserId AND c.IsSelected = 1;

    DELETE FROM CartItems WHERE UserId = @UserId AND IsSelected = 1;

    INSERT INTO Notifications(TargetRoleId, Title, Content, LinkUrl)
    SELECT r.RoleId, N'Đơn mới chờ thanh toán', N'Có đơn hàng mới #' + o.OrderCode, N'/admin-orders.html'
    FROM Orders o CROSS JOIN Roles r
    WHERE o.OrderId = @OrderId AND r.RoleName IN (N'Admin');

    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreatePaymentAttempt @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Total DECIMAL(18,0), @Ref NVARCHAR(100);
    SELECT @Total = TotalAmount FROM Orders
    WHERE OrderId = @OrderId AND Status = 0 AND PaymentExpireAt > SYSDATETIME();

    IF @Total IS NULL
        THROW 50010, N'Đơn không ở trạng thái chờ thanh toán hoặc đã hết hạn.', 1;

    SET @Ref = CAST(@OrderId AS NVARCHAR(20)) + N'T' + FORMAT(SYSDATETIME(), 'yyMMddHHmmss');

    INSERT INTO Payments(OrderId, Amount, Status, TransactionRef) VALUES (@OrderId, @Total, 0, @Ref);

    SELECT @Ref AS TransactionRef, @Total AS Amount;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ConfirmVnPayPayment
    @TransactionRef NVARCHAR(100),
    @ResponseCode   NVARCHAR(10),
    @Amount         DECIMAL(18,0),
    @TransactionNo  NVARCHAR(100) = NULL,
    @BankCode       NVARCHAR(50)  = NULL,
    @PayDate        DATETIME2     = NULL,
    @RawResponse    NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @PaymentId INT, @OrderId INT, @PayStatus TINYINT, @Expected DECIMAL(18,0), @OrderStatus TINYINT;

    BEGIN TRAN;

    SELECT @PaymentId = PaymentId, @OrderId = OrderId, @PayStatus = Status
    FROM Payments WITH (UPDLOCK) WHERE TransactionRef = @TransactionRef;

    IF @PaymentId IS NULL
    BEGIN COMMIT; SELECT 'NOT_FOUND' AS Result; RETURN; END

    SELECT @Expected = TotalAmount, @OrderStatus = Status
    FROM Orders WITH (UPDLOCK) WHERE OrderId = @OrderId;

    IF @PayStatus <> 0
    BEGIN COMMIT; SELECT 'ALREADY_PROCESSED' AS Result; RETURN; END

    IF @Amount <> @Expected
    BEGIN
        UPDATE Payments SET Status = 2, ResponseCode = @ResponseCode, TransactionNo = @TransactionNo,
               BankCode = @BankCode, RawResponse = @RawResponse WHERE PaymentId = @PaymentId;
        COMMIT; SELECT 'INVALID_AMOUNT' AS Result; RETURN;
    END

    IF @ResponseCode = N'00'
    BEGIN
        UPDATE Payments SET Status = 1, ResponseCode = @ResponseCode, TransactionNo = @TransactionNo,
               BankCode = @BankCode, PayDate = ISNULL(@PayDate, SYSDATETIME()), RawResponse = @RawResponse
        WHERE PaymentId = @PaymentId;

        IF @OrderStatus = 0
        BEGIN
            UPDATE Orders SET Status = 1, PaidAt = SYSDATETIME() WHERE OrderId = @OrderId;

            INSERT INTO Notifications(TargetRoleId, Title, Content, LinkUrl)
            SELECT r.RoleId, N'Đơn mới đã thanh toán VNPay',
                   N'Đơn #' + o.OrderCode + N' đã thanh toán ' + FORMAT(o.TotalAmount, 'N0') + N' ₫',
                   N'/staff-orders.html'
            FROM Orders o CROSS JOIN Roles r
            WHERE o.OrderId = @OrderId AND r.RoleName IN (N'Staff', N'Admin');

            COMMIT; SELECT 'SUCCESS' AS Result;
        END
        ELSE
        BEGIN

            INSERT INTO Notifications(TargetRoleId, Title, Content, LinkUrl)
            SELECT r.RoleId, N'Cần đối soát thanh toán',
                   N'Đơn #' + o.OrderCode + N' nhận tiền VNPay khi đang ở trạng thái ' + CAST(o.Status AS NVARCHAR(5)),
                   N'/admin-orders.html'
            FROM Orders o CROSS JOIN Roles r
            WHERE o.OrderId = @OrderId AND r.RoleName = N'Admin';

            COMMIT; SELECT 'NEEDS_RECONCILE' AS Result;
        END
    END
    ELSE
    BEGIN
        UPDATE Payments SET Status = 2, ResponseCode = @ResponseCode, TransactionNo = @TransactionNo,
               BankCode = @BankCode, RawResponse = @RawResponse WHERE PaymentId = @PaymentId;
        COMMIT; SELECT 'FAILED' AS Result;
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CancelOrder
    @OrderId INT,
    @UserId  INT = NULL,
    @Reason  NVARCHAR(300) = N'Khách hàng hủy đơn'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    UPDATE Orders
    SET Status = 5, CancelledAt = SYSDATETIME(), CancelReason = @Reason, HandledBy = NULL
    WHERE OrderId = @OrderId AND Status = 0 AND (@UserId IS NULL OR UserId = @UserId);

    IF @@ROWCOUNT = 0
        THROW 50030, N'Chỉ được hủy đơn đang ở trạng thái Chờ thanh toán.', 1;

    EXEC dbo.usp_ReleaseOrderStock @OrderId;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RefundOrder
    @OrderId             INT,
    @AdminId             INT,
    @RefundTransactionNo NVARCHAR(100),
    @Reason              NVARCHAR(300),
    @RefundAmount        DECIMAL(18,0) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    DECLARE @Status TINYINT, @Total DECIMAL(18,0), @RefundedAt DATETIME2;
    SELECT @Status = Status, @Total = TotalAmount, @RefundedAt = RefundedAt
    FROM Orders WITH (UPDLOCK) WHERE OrderId = @OrderId;

    IF @Status IS NULL THROW 50020, N'Không tìm thấy đơn hàng.', 1;
    IF @RefundedAt IS NOT NULL THROW 50021, N'Đơn này đã được hoàn tiền.', 1;
    IF @Status NOT IN (1, 2, 5, 6) THROW 50022, N'Trạng thái đơn hiện tại không cho phép hoàn tiền.', 1;
    IF NOT EXISTS (SELECT 1 FROM Payments WHERE OrderId = @OrderId AND Status = 1)
        THROW 50023, N'Đơn chưa có giao dịch VNPay thành công để hoàn.', 1;

    SET @RefundAmount = ISNULL(@RefundAmount, @Total);
    IF @RefundAmount <= 0 OR @RefundAmount > @Total THROW 50024, N'Số tiền hoàn không hợp lệ.', 1;

    UPDATE Orders
    SET Status      = CASE WHEN Status IN (1, 2) THEN 5 ELSE Status END,
        CancelledAt = CASE WHEN Status IN (1, 2) THEN SYSDATETIME() ELSE CancelledAt END,
        CancelReason = CASE WHEN Status IN (1, 2) THEN @Reason ELSE CancelReason END,
        HandledBy   = @AdminId,
        RefundedAt  = SYSDATETIME(),
        RefundTransactionNo = @RefundTransactionNo,
        RefundAmount = @RefundAmount,
        RefundReason = @Reason
    WHERE OrderId = @OrderId;

    UPDATE Payments SET Status = 3 WHERE OrderId = @OrderId AND Status = 1;

    IF @Status IN (1, 2) EXEC dbo.usp_ReleaseOrderStock @OrderId;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateOrderStatus
    @OrderId   INT,
    @NewStatus TINYINT,
    @StaffId   INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    DECLARE @Old TINYINT;
    SELECT @Old = Status FROM Orders WITH (UPDLOCK) WHERE OrderId = @OrderId;

    IF @Old IS NULL THROW 50040, N'Không tìm thấy đơn hàng.', 1;
    IF NOT ((@Old = 1 AND @NewStatus = 2) OR (@Old = 2 AND @NewStatus = 3)
         OR (@Old = 3 AND @NewStatus IN (4, 6)) OR (@Old = 4 AND @NewStatus = 6))
        THROW 50041, N'Không thể chuyển sang trạng thái này từ trạng thái hiện tại.', 1;

    UPDATE Orders
    SET Status = @NewStatus, HandledBy = @StaffId,
        CompletedAt = CASE WHEN @NewStatus = 4 THEN SYSDATETIME() ELSE CompletedAt END
    WHERE OrderId = @OrderId;

    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ExpireUnpaidOrders
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @id INT;

    WHILE 1 = 1
    BEGIN
        SET @id = NULL;
        SELECT TOP 1 @id = OrderId FROM Orders
        WHERE Status = 0 AND PaymentExpireAt < SYSDATETIME() ORDER BY OrderId;

        IF @id IS NULL BREAK;

        BEGIN TRAN;
        UPDATE Orders SET Status = 5, CancelledAt = SYSDATETIME(), CancelReason = N'Hết hạn thanh toán'
        WHERE OrderId = @id AND Status = 0;
        IF @@ROWCOUNT = 1 EXEC dbo.usp_ReleaseOrderStock @id;
        COMMIT;
    END
END
GO

CREATE OR ALTER VIEW dbo.vw_DailyRevenue AS
SELECT CAST(PaidAt AS DATE) AS RevenueDate,
       COUNT(*)             AS OrderCount,
       SUM(TotalAmount)     AS Revenue
FROM Orders
WHERE Status IN (1, 2, 3, 4) AND PaidAt IS NOT NULL
GROUP BY CAST(PaidAt AS DATE);
GO

CREATE OR ALTER VIEW dbo.vw_BestSellers AS
SELECT oi.ProductId, oi.ProductName,
       SUM(oi.Quantity)  AS SoldQty,
       SUM(oi.LineTotal) AS Revenue
FROM OrderItems oi
JOIN Orders o ON o.OrderId = oi.OrderId
WHERE o.Status IN (1, 2, 3, 4)
GROUP BY oi.ProductId, oi.ProductName;
GO

CREATE OR ALTER VIEW dbo.vw_LowStock AS
SELECT p.ProductId, p.ProductName, v.VariantId, v.Sku, v.ColorName, v.SizeLabel, v.StockQty
FROM ProductVariants v
JOIN Products p ON p.ProductId = v.ProductId
WHERE v.IsActive = 1 AND p.IsActive = 1 AND v.StockQty <= 5;
GO

INSERT INTO Roles(RoleId, RoleName) VALUES (1, N'Customer'), (2, N'Staff'), (3, N'Admin');

INSERT INTO OrderStatuses(StatusId, StatusName) VALUES
 (0, N'Chờ thanh toán'), (1, N'Đã thanh toán'), (2, N'Đang xử lý (cắt tròng)'),
 (3, N'Đang giao'), (4, N'Hoàn thành'), (5, N'Đã hủy'), (6, N'Trả hàng');

INSERT INTO LensOptions(LensName, RefractiveIndex, ExtraPrice, RxLimit, Description) VALUES
 (N'Chỉ mua gọng kính (Tròng mẫu 0 độ)', 1.50, 0,      NULL,               N'Tròng nhựa mẫu bảo vệ dáng gọng khi vận chuyển'),
 (N'Tròng BlueCut Chống Ánh Sáng Xanh',  1.56, 250000, N'Đến -4.00 độ',    N'Lọc ánh sáng xanh từ màn hình, chống phản quang, chống tia UV400'),
 (N'Tròng Đổi Màu Khói Photochromic',    1.60, 450000, N'Đến -6.00 độ',    N'Đổi màu xám khói khi ra nắng, trong suốt khi ở trong nhà'),
 (N'Tròng Siêu Mỏng Đa Lớp Cao Cấp',     1.67, 650000, N'Từ -6.00 độ trở lên', N'Mỏng hơn 35% so với tròng chuẩn, phủ Nano chống bám nước mưa');

INSERT INTO Categories(CategoryName, Slug) VALUES
 (N'Gọng kính cận',            N'gong-kinh-can'),
 (N'Kính chống ánh sáng xanh', N'kinh-chong-anh-sang-xanh'),
 (N'Kính mát có độ',           N'kinh-mat-co-do'),
 (N'Gọng Titan & Kim loại',    N'gong-titan-kim-loai');

INSERT INTO Brands(BrandName) VALUES
 (N'ALFIO RALDO'), (N'BK EIGHT-G'), (N'KABAOLAI'), (N'LEO GONE'), (N'MIARA'),
 (N'P&L'), (N'PARZIN'), (N'REAL ORIGIN'), (N'SAMEIR');

INSERT INTO Coupons(CouponCode, DiscountType, DiscountValue, MaxDiscount, MinOrderAmount, ExpireDate) VALUES
 (N'MIARA50K', 'AMOUNT',  50000, NULL, 300000, '2026-12-31'),
 (N'MIARA10',  'PERCENT', 10,    NULL, 400000, '2026-11-30');
GO

USE EyeglassesShop;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@miara.vn')
BEGIN
    INSERT INTO Users (FullName, Email, Phone, PasswordHash, Gender, BirthDate, RoleId, IsActive, CreatedAt)
    VALUES
    (N'Quản trị viên MIARA', 'admin@miara.vn', '0988888888', '$2a$12$YdhqeTnc1hTkQpDeADzw3.cm7YEIPf/ifWfRdhKklkKw2oWOky4yu', N'Nam', '1995-01-01', 3, 1, SYSDATETIME()),
    (N'Kỹ thuật viên Cắt kính', 'staff@miara.vn', '0977777777', '$2a$12$YdhqeTnc1hTkQpDeADzw3.cm7YEIPf/ifWfRdhKklkKw2oWOky4yu', N'Nam', '1998-05-15', 2, 1, SYSDATETIME()),
    (N'Nguyễn Thành Đạt', 'datnthe187149@fpt.edu.vn', '0901234567', '$2a$12$YdhqeTnc1hTkQpDeADzw3.cm7YEIPf/ifWfRdhKklkKw2oWOky4yu', N'Nam', '2004-10-20', 1, 1, SYSDATETIME()),
    (N'Trần Thu Hà', 'khachhang@miara.vn', '0912345678', '$2a$12$YdhqeTnc1hTkQpDeADzw3.cm7YEIPf/ifWfRdhKklkKw2oWOky4yu', N'Nữ', '2002-08-12', 1, 1, SYSDATETIME());
END
GO

DECLARE @UserIdDat INT = (SELECT UserId FROM Users WHERE Email = 'datnthe187149@fpt.edu.vn');
IF @UserIdDat IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserAddresses WHERE UserId = @UserIdDat)
BEGIN
    INSERT INTO UserAddresses (UserId, RecipientName, Phone, Province, DistrictWard, StreetAddress, IsDefault, CreatedAt)
    VALUES
    (@UserIdDat, N'Nguyễn Thành Đạt', '0901234567', N'Hà Nội', N'Quận Cầu Giấy, Phường Dịch Vọng Hậu', N'Tòa nhà FPT, Phố Duy Tân', 1, SYSDATETIME()),
    (@UserIdDat, N'Nguyễn Thành Đạt (KTX)', '0901234567', N'Hà Nội', N'Huyện Thạch Thất, Xã Tân Xã', N'Khu ký túc xá Đại học FPT Hòa Lạc', 0, SYSDATETIME());
END
GO

DECLARE @UserIdDat INT = (SELECT UserId FROM Users WHERE Email = 'datnthe187149@fpt.edu.vn');
IF @UserIdDat IS NOT NULL AND NOT EXISTS (SELECT 1 FROM CustomerPrescriptions WHERE UserId = @UserIdDat)
BEGIN
    INSERT INTO CustomerPrescriptions (UserId, ProfileName, RightSph, RightCyl, RightAxis, LeftSph, LeftCyl, LeftAxis, PD, IsDefault, CreatedAt)
    VALUES
    (@UserIdDat, N'Kính học tập & làm việc máy tính', -2.00, -0.50, 180, -2.25, 0.00, 0, 62.0, 1, SYSDATETIME()),
    (@UserIdDat, N'Kính nhìn xa lái xe', -2.50, -0.50, 180, -2.75, 0.00, 0, 62.0, 0, SYSDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM Banners)
BEGIN
    INSERT INTO Banners (Title, LinkUrl, ImageUrl, SortOrder, IsActive, CreatedAt)
    VALUES
    (N'SEE YOURSELF FROM A NEW ANGLE', '/Shop', '/images/products/249k/01-3114-151O16-143-C3/main.jpg', 1, 1, SYSDATETIME()),
    (N'BỘ SƯU TẬP GỌNG KÍNH THỜI TRANG 2026', '/Shop', '/images/products/349k/10-8241-C4-50O20-141/main.jpg', 2, 1, SYSDATETIME()),
    (N'TRÒNG KÍNH BLUECUT CHỐNG ÁNH SÁNG XANH', '/Shop/Lenses', '/images/products/449k/25-BM126-54O18-105-O106/main.jpg', 3, 1, SYSDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('3114', N'ALFIO RALDO 3114', 1, 1, 249000, N'Unisex', N'Chữ nhật', N'Nhựa', N'Gọng kính ALFIO RALDO 3114 dáng chữ nhật thanh lịch, màu nâu - trắng trong, chất liệu nhựa nhẹ bền bỉ.', 1, SYSDATETIME());
    DECLARE @P1 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES
    (@P1, '3114-151O16-143-C3', N'Nâu - trắng trong', N'Nâu', 'M', 51, 16, 143, 15, 1),
    (@P1, '3114-151O16-143-C1', N'Đen bóng', N'Đen', 'M', 51, 16, 143, 20, 1),
    (@P1, '3114-151O16-143-C2', N'Xám khói trong', N'Xám / Bạc', 'L', 53, 17, 145, 10, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P1, '/images/products/249k/01-3114-151O16-143-C3/main.jpg', 0, 1),
    (@P1, '/images/products/249k/01-3114-151O16-143-C3/1.jpg', 1, 0),
    (@P1, '/images/products/249k/01-3114-151O16-143-C3/2.jpg', 2, 0),
    (@P1, '/images/products/249k/01-3114-151O16-143-C3/3.jpg', 3, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('87476', N'ALFIO RALDO 87476', 1, 1, 249000, N'Nữ', N'Mắt mèo', N'Nhựa', N'Gọng kính nữ ALFIO RALDO 87476 dáng mắt mèo thời thượng, tông màu đỏ rượu quý phái.', 1, SYSDATETIME());
    DECLARE @P2 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P2, '87476-52O17-145-C2', N'Đỏ rượu', N'Hồng / Đỏ', 'M', 52, 17, 145, 12, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P2, '/images/products/249k/02-87476-52O17-145-C2/main.jpg', 0, 1),
    (@P2, '/images/products/249k/02-87476-52O17-145-C2/1.jpg', 1, 0),
    (@P2, '/images/products/249k/02-87476-52O17-145-C2/2.jpg', 2, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('Z-1819', N'SAMEIR Z-1819', 1, 9, 249000, N'Unisex', N'Vuông', N'Nhựa', N'Gọng kính vuông SAMEIR Z-1819 cá tính, màu đen cổ điển, đệm mũi êm ái.', 1, SYSDATETIME());
    DECLARE @P3 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P3, 'Z-1819-53O18-146-C2', N'Đen', N'Đen', 'M', 53, 18, 146, 20, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P3, '/images/products/249k/03-Z-1819-53O18-146-C2/main.jpg', 0, 1),
    (@P3, '/images/products/249k/03-Z-1819-53O18-146-C2/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('31220', N'ALFIO RALDO 31220', 1, 1, 249000, N'Nam', N'Chữ nhật', N'Nhựa', N'Thiết kế thể thao hiện đại, bản lề chắc chắn, màu đen - trắng trong tinh tế.', 1, SYSDATETIME());
    DECLARE @P4 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P4, '31220-52O17-C11', N'Đen - trắng trong', N'Trong suốt', 'M', 52, 17, 145, 18, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P4, '/images/products/249k/04-31220-52O17-C11/main.jpg', 0, 1),
    (@P4, '/images/products/249k/04-31220-52O17-C11/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('18109', N'SAMEIR 18109', 2, 9, 249000, N'Unisex', N'Tròn', N'Nhựa', N'Gọng kính tròn retro SAMEIR 18109 trẻ trung, phù hợp học sinh sinh viên làm việc máy tính.', 1, SYSDATETIME());
    DECLARE @P5 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P5, '18109-53O20-148-C1', N'Đen', N'Đen', 'M', 53, 20, 148, 25, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P5, '/images/products/249k/05-18109-53O20-148-C1/main.jpg', 0, 1),
    (@P5, '/images/products/249k/05-18109-53O20-148-C1/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('31200', N'ALFIO RALDO 31200', 1, 1, 249000, N'Nam', N'Vuông', N'Nhựa', N'Gọng vuông nam tính, viền mỏng thanh mảnh, bền đẹp theo thời gian.', 1, SYSDATETIME());
    DECLARE @P6 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P6, '31200-52O17-146-C12', N'Đen', N'Đen', 'L', 52, 17, 146, 10, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P6, '/images/products/249k/07-31200-52O17-146-C12/main.jpg', 0, 1),
    (@P6, '/images/products/249k/07-31200-52O17-146-C12/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('8241-C4', N'P&L 8241 C4', 2, 6, 349000, N'Nữ', N'Oval', N'Nhựa', N'Mẫu kính bán chạy P&L 8241 tông màu hồng trà trong suốt tôn da, dáng oval mềm mại.', 1, SYSDATETIME());
    DECLARE @P7 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P7, '8241-C4-50O20-141', N'Hồng trà trong', N'Hồng / Đỏ', 'S', 50, 20, 141, 14, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P7, '/images/products/349k/10-8241-C4-50O20-141/main.jpg', 0, 1),
    (@P7, '/images/products/349k/10-8241-C4-50O20-141/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('8241-C3', N'P&L 8241 C3', 2, 6, 349000, N'Unisex', N'Oval', N'Nhựa', N'Màu xám trong khói sang trọng, nhẹ nhàng ôm sát sống mũi.', 1, SYSDATETIME());
    DECLARE @P8 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P8, '8241-C3-50O20-141', N'Xám trong', N'Xám / Bạc', 'S', 50, 20, 141, 16, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P8, '/images/products/349k/11-8241-C3-50O20-141/main.jpg', 0, 1),
    (@P8, '/images/products/349k/11-8241-C3-50O20-141/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('1103', N'LEO GONE 1103', 1, 4, 349000, N'Nam', N'Chữ nhật', N'Nhựa', N'Gọng kính cận LEO GONE 1103 chất liệu cao cấp, đệm mũi êm ái, kiểu dáng hiện đại.', 1, SYSDATETIME());
    DECLARE @P9 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P9, '1103-49-20-150-C6S', N'Đen - trắng trong', N'Trong suốt', 'M', 49, 20, 150, 8, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P9, '/images/products/349k/14-1103-49-20-150-C6S/main.jpg', 0, 1),
    (@P9, '/images/products/349k/14-1103-49-20-150-C6S/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('5867', N'LEO GONE 5867', 1, 4, 399000, N'Unisex', N'Vuông', N'Nhựa', N'Tone màu nâu xám trong cá tính, góc cạnh sắc nét, cực kỳ tôn gương mặt.', 1, SYSDATETIME());
    DECLARE @P10 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P10, '5867-52-19-150-C14', N'Nâu xám trong', N'Nâu', 'M', 52, 19, 150, 11, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P10, '/images/products/399k/18-5867-52-19-150-C14/main.jpg', 0, 1),
    (@P10, '/images/products/399k/18-5867-52-19-150-C14/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('BM126', N'BK EIGHT-G BM126', 4, 2, 449000, N'Unisex', N'Đa giác', N'Kim loại', N'Gọng kim loại thanh mảnh BM126, đa giác độc đáo, nhẹ tênh khi đeo liên tục 8 tiếng.', 1, SYSDATETIME());
    DECLARE @P11 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P11, 'BM126-54O18-105-O106', N'Nâu vàng trà nhạt trong', N'Vàng', 'L', 54, 18, 145, 9, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P11, '/images/products/449k/25-BM126-54O18-105-O106/main.jpg', 0, 1),
    (@P11, '/images/products/449k/25-BM126-54O18-105-O106/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('BM-218', N'BK EIGHT-G BM-218', 3, 2, 549000, N'Unisex', N'Tròn', N'Titan', N'Kính mát thời trang Titan cao cấp BM-218, có thể lắp tròng râm cận đổi màu tiện lợi.', 1, SYSDATETIME());
    DECLARE @P12 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P12, 'BM-218-50O19-145-C2', N'Đen - Vàng Titan', N'Đen', 'M', 50, 19, 145, 7, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P12, '/images/products/549k/29-BM-218-50O19-145-C2/main.jpg', 0, 1),
    (@P12, '/images/products/549k/29-BM-218-50O19-145-C2/1.jpg', 1, 0);

    INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, Price, Gender, FrameShape, Material, Description, IsActive, CreatedAt)
    VALUES ('BF-155', N'PARZIN BF-155 Titan', 4, 7, 849000, N'Nam', N'Vuông', N'Titan', N'Gọng kính Pure Titanium siêu nhẹ cao cấp PARZIN BF-155, không gỉ sét, chống kích ứng da.', 1, SYSDATETIME());
    DECLARE @P13 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, Sku, ColorName, ColorGroup, SizeLabel, LensWidth, BridgeWidth, TempleLength, StockQty, IsActive)
    VALUES (@P13, 'BF-155-51-17-145-C2', N'Xám Bạc Titan', N'Xám / Bạc', 'M', 51, 17, 145, 5, 1);

    INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary)
    VALUES
    (@P13, '/images/products/849k/54-BF-155-51-17-145-C2/main.jpg', 0, 1),
    (@P13, '/images/products/849k/54-BF-155-51-17-145-C2/1.jpg', 1, 0);
END
GO

DECLARE @Prod3114 INT = (SELECT ProductId FROM Products WHERE ProductCode = '3114');
DECLARE @Prod8241 INT = (SELECT ProductId FROM Products WHERE ProductCode = '8241-C4');
DECLARE @UserDat INT = (SELECT UserId FROM Users WHERE Email = 'datnthe187149@fpt.edu.vn');
DECLARE @UserHa INT = (SELECT UserId FROM Users WHERE Email = 'khachhang@miara.vn');

IF @Prod3114 IS NOT NULL AND @UserHa IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Reviews WHERE ProductId = @Prod3114 AND UserId = @UserHa)
BEGIN
    INSERT INTO Reviews (ProductId, UserId, Rating, Comment, Status, IsRead, CreatedAt)
    VALUES
    (@Prod3114, @UserHa, 5, N'Gọng kính đeo rất nhẹ, màu nâu trắng trong cực kỳ sang và sáng da. Mình cắt kèm tròng BlueCut nhìn máy tính đỡ mỏi mắt hẳn!', 1, 1, DATEADD(DAY, -3, SYSDATETIME()));
END

IF @Prod8241 IS NOT NULL AND @UserDat IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Reviews WHERE ProductId = @Prod8241 AND UserId = @UserDat)
BEGIN
    INSERT INTO Reviews (ProductId, UserId, Rating, Comment, Status, IsRead, CreatedAt)
    VALUES
    (@Prod8241, @UserDat, 5, N'Màu hồng trà trong suốt ở ngoài đẹp hơn trong ảnh nhiều. Đóng gói cẩn thận có hộp cứng và khăn lau kính rất xịn.', 1, 1, DATEADD(DAY, -1, SYSDATETIME()));
END
GO

DECLARE @UserDat INT = (SELECT UserId FROM Users WHERE Email = 'datnthe187149@fpt.edu.vn');
DECLARE @Prod1 INT = (SELECT ProductId FROM Products WHERE ProductCode = '3114');
DECLARE @Var1 INT = (SELECT TOP 1 VariantId FROM ProductVariants WHERE ProductId = @Prod1);

IF @UserDat IS NOT NULL AND @Var1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Orders WHERE UserId = @UserDat)
BEGIN

    INSERT INTO Orders (UserId, RecipientName, Phone, ShippingAddress, OrderNote, Subtotal, DiscountAmount, ShippingFee, TotalAmount, Status, CreatedAt, PaidAt)
    VALUES (@UserDat, N'Nguyễn Thành Đạt', '0901234567', N'Tòa nhà FPT, Phố Duy Tân, Dịch Vọng Hậu, Cầu Giấy, Hà Nội', N'Cắt tròng cẩn thận giúp mình nhé', 499000, 50000, 0, 449000, 1, DATEADD(HOUR, -2, SYSDATETIME()), DATEADD(HOUR, -2, SYSDATETIME()));

    DECLARE @O1 INT = SCOPE_IDENTITY();

    INSERT INTO OrderItems (OrderId, ProductId, VariantId, ProductName, ProductCode, Sku, ColorName, SizeLabel, LensOptionId, LensName, FramePrice, LensPrice, Quantity, RightSph, RightCyl, RightAxis, LeftSph, LeftCyl, LeftAxis, PD)
    VALUES (@O1, @Prod1, @Var1, N'ALFIO RALDO 3114', '3114', '3114-151O16-143-C3', N'Nâu - trắng trong', 'M', 2, N'Tròng BlueCut Chống Ánh Sáng Xanh', 249000, 250000, 1, -2.00, -0.50, 180, -2.25, 0.00, 0, 62.0);

    INSERT INTO Payments (OrderId, Method, Amount, Status, TransactionRef, TransactionNo, BankCode, ResponseCode, PayDate, CreatedAt)
    VALUES (@O1, N'VNPAY', 449000, 1, 'MR000001', '14589214', 'NCB', '00', DATEADD(HOUR, -2, SYSDATETIME()), DATEADD(HOUR, -2, SYSDATETIME()));

    INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedBy, Note, ChangedAt)
    VALUES (@O1, 0, 1, @UserDat, N'Thanh toán VNPay thành công', DATEADD(HOUR, -2, SYSDATETIME()));
END
GO

USE EyeglassesShop;
GO

DELETE FROM ProductImages;
GO

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(1, '/images/products/249k/01-3114-151O16-143-C3/main.jpg', 0, 1),
(1, '/images/products/249k/01-3114-151O16-143-C3/1.jpg', 1, 0),
(1, '/images/products/249k/01-3114-151O16-143-C3/2.jpg', 2, 0),
(1, '/images/products/249k/01-3114-151O16-143-C3/3.jpg', 3, 0),
(1, '/images/products/249k/01-3114-151O16-143-C3/4.jpg', 4, 0),
(1, '/images/products/249k/01-3114-151O16-143-C3/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(2, '/images/products/249k/02-87476-52O17-145-C2/main.jpg', 0, 1),
(2, '/images/products/249k/02-87476-52O17-145-C2/1.jpg', 1, 0),
(2, '/images/products/249k/02-87476-52O17-145-C2/2.jpg', 2, 0),
(2, '/images/products/249k/02-87476-52O17-145-C2/3.jpg', 3, 0),
(2, '/images/products/249k/02-87476-52O17-145-C2/4.jpg', 4, 0),
(2, '/images/products/249k/02-87476-52O17-145-C2/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(3, '/images/products/249k/03-Z-1819-53O18-146-C2/main.jpg', 0, 1),
(3, '/images/products/249k/03-Z-1819-53O18-146-C2/1.jpg', 1, 0),
(3, '/images/products/249k/03-Z-1819-53O18-146-C2/2.jpg', 2, 0),
(3, '/images/products/249k/03-Z-1819-53O18-146-C2/3.jpg', 3, 0),
(3, '/images/products/249k/03-Z-1819-53O18-146-C2/4.jpg', 4, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(4, '/images/products/249k/04-31220-52O17-C11/main.jpg', 0, 1),
(4, '/images/products/249k/04-31220-52O17-C11/1.jpg', 1, 0),
(4, '/images/products/249k/04-31220-52O17-C11/2.jpg', 2, 0),
(4, '/images/products/249k/04-31220-52O17-C11/3.jpg', 3, 0),
(4, '/images/products/249k/04-31220-52O17-C11/4.jpg', 4, 0),
(4, '/images/products/249k/04-31220-52O17-C11/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(5, '/images/products/249k/05-18109-53O20-148-C1/main.jpg', 0, 1),
(5, '/images/products/249k/05-18109-53O20-148-C1/1.jpg', 1, 0),
(5, '/images/products/249k/05-18109-53O20-148-C1/2.jpg', 2, 0),
(5, '/images/products/249k/05-18109-53O20-148-C1/3.jpg', 3, 0),
(5, '/images/products/249k/05-18109-53O20-148-C1/4.jpg', 4, 0),
(5, '/images/products/249k/05-18109-53O20-148-C1/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(6, '/images/products/249k/07-31200-52O17-146-C12/main.jpg', 0, 1),
(6, '/images/products/249k/07-31200-52O17-146-C12/1.jpg', 1, 0),
(6, '/images/products/249k/07-31200-52O17-146-C12/2.jpg', 2, 0),
(6, '/images/products/249k/07-31200-52O17-146-C12/3.jpg', 3, 0),
(6, '/images/products/249k/07-31200-52O17-146-C12/4.jpg', 4, 0),
(6, '/images/products/249k/07-31200-52O17-146-C12/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(7, '/images/products/349k/10-8241-C4-50O20-141/main.jpg', 0, 1),
(7, '/images/products/349k/10-8241-C4-50O20-141/1.jpg', 1, 0),
(7, '/images/products/349k/10-8241-C4-50O20-141/2.jpg', 2, 0),
(7, '/images/products/349k/10-8241-C4-50O20-141/3.jpg', 3, 0),
(7, '/images/products/349k/10-8241-C4-50O20-141/4.jpg', 4, 0),
(7, '/images/products/349k/10-8241-C4-50O20-141/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(8, '/images/products/349k/11-8241-C3-50O20-141/main.jpg', 0, 1),
(8, '/images/products/349k/11-8241-C3-50O20-141/1.jpg', 1, 0),
(8, '/images/products/349k/11-8241-C3-50O20-141/2.jpg', 2, 0),
(8, '/images/products/349k/11-8241-C3-50O20-141/3.jpg', 3, 0),
(8, '/images/products/349k/11-8241-C3-50O20-141/4.jpg', 4, 0),
(8, '/images/products/349k/11-8241-C3-50O20-141/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(9, '/images/products/349k/14-1103-49-20-150-C6S/main.jpg', 0, 1),
(9, '/images/products/349k/14-1103-49-20-150-C6S/1.jpg', 1, 0),
(9, '/images/products/349k/14-1103-49-20-150-C6S/2.jpg', 2, 0),
(9, '/images/products/349k/14-1103-49-20-150-C6S/3.jpg', 3, 0),
(9, '/images/products/349k/14-1103-49-20-150-C6S/4.jpg', 4, 0),
(9, '/images/products/349k/14-1103-49-20-150-C6S/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(10, '/images/products/399k/18-5867-52-19-150-C14/main.jpg', 0, 1),
(10, '/images/products/399k/18-5867-52-19-150-C14/1.jpg', 1, 0),
(10, '/images/products/399k/18-5867-52-19-150-C14/2.jpg', 2, 0),
(10, '/images/products/399k/18-5867-52-19-150-C14/3.jpg', 3, 0),
(10, '/images/products/399k/18-5867-52-19-150-C14/4.jpg', 4, 0),
(10, '/images/products/399k/18-5867-52-19-150-C14/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(11, '/images/products/449k/25-BM126-54O18-105-O106/main.jpg', 0, 1),
(11, '/images/products/449k/25-BM126-54O18-105-O106/1.jpg', 1, 0),
(11, '/images/products/449k/25-BM126-54O18-105-O106/2.jpg', 2, 0),
(11, '/images/products/449k/25-BM126-54O18-105-O106/3.jpg', 3, 0),
(11, '/images/products/449k/25-BM126-54O18-105-O106/4.jpg', 4, 0),
(11, '/images/products/449k/25-BM126-54O18-105-O106/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(12, '/images/products/549k/29-BM-218-50O19-145-C2/main.jpg', 0, 1),
(12, '/images/products/549k/29-BM-218-50O19-145-C2/1.jpg', 1, 0),
(12, '/images/products/549k/29-BM-218-50O19-145-C2/2.jpg', 2, 0),
(12, '/images/products/549k/29-BM-218-50O19-145-C2/3.jpg', 3, 0),
(12, '/images/products/549k/29-BM-218-50O19-145-C2/4.jpg', 4, 0),
(12, '/images/products/549k/29-BM-218-50O19-145-C2/5.jpg', 5, 0);

INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary) VALUES
(13, '/images/products/849k/54-BF-155-51-17-145-C2/main.jpg', 0, 1),
(13, '/images/products/849k/54-BF-155-51-17-145-C2/1.jpg', 1, 0),
(13, '/images/products/849k/54-BF-155-51-17-145-C2/2.jpg', 2, 0),
(13, '/images/products/849k/54-BF-155-51-17-145-C2/3.jpg', 3, 0),
(13, '/images/products/849k/54-BF-155-51-17-145-C2/4.jpg', 4, 0),
(13, '/images/products/849k/54-BF-155-51-17-145-C2/5.jpg', 5, 0);
GO
