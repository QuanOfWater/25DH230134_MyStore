USE mystore;
EXEC sp_rename 'dbo.Product.ProductDecription', 'ProductDescription', 'COLUMN';

ALTER TABLE [dbo].[Order]
    ADD [ShippingAddress] NVARCHAR(MAX) NOT NULL DEFAULT N'',
        [DeliveryMethod]  NVARCHAR(100) NULL,
        [PaymentMethod]   NVARCHAR(100) NULL;
GO

ALTER TABLE [dbo].[OrderDetail]
    ADD [TotalPrice] AS ([Quantity] * [UnitPrice]) PERSISTED;
GO

--UPDATE [dbo].[Order] SET ShippingAddress = AddressDelivery
--WHERE ShippingAddress IS NULL OR ShippingAddress = N'';
--ALTER TABLE [dbo].[Order] DROP COLUMN AddressDelivery;

-- Thêm cột trạng thái đơn hàng
ALTER TABLE [dbo].[Order] ADD OrderStatus NVARCHAR(50) NULL;
GO
UPDATE [dbo].[Order] SET OrderStatus = N'Đang xử lý' WHERE OrderStatus IS NULL;
GO

USE [MyStore];   -- đúng tên DB trong Web.config của bạn
GO

-- 1. Thêm cột OrderStatus nếu chưa có
IF COL_LENGTH('dbo.[Order]', 'OrderStatus') IS NULL
    ALTER TABLE [dbo].[Order] ADD OrderStatus NVARCHAR(50) NULL;
GO

-- Gán trạng thái mặc định cho các đơn cũ
EXEC(N'UPDATE [dbo].[Order] SET OrderStatus = N''Đang xử lý'' WHERE OrderStatus IS NULL');
GO

-- 2. Nếu còn cột AddressDelivery thì chép sang ShippingAddress rồi xóa
IF COL_LENGTH('dbo.[Order]', 'AddressDelivery') IS NOT NULL
BEGIN
    EXEC(N'UPDATE [dbo].[Order] SET ShippingAddress = AddressDelivery
           WHERE ShippingAddress IS NULL OR ShippingAddress = N''''');

    -- Xóa ràng buộc DEFAULT của cột (nếu có), nếu không sẽ không DROP được
    DECLARE @df NVARCHAR(200);
    SELECT @df = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.[Order]') AND c.name = 'AddressDelivery';
    IF @df IS NOT NULL EXEC(N'ALTER TABLE [dbo].[Order] DROP CONSTRAINT [' + @df + ']');

    ALTER TABLE [dbo].[Order] DROP COLUMN AddressDelivery;
END
GO

-- 3. Kiểm tra lại các cột của bảng Order
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Order';

    USE [MyStore];
    GO
    IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE Username = N'admin')
        INSERT INTO [dbo].[User] (Username, Password, UserRole) VALUES (N'admin', N'123456', N'A');
    GO