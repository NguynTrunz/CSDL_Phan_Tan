USE [master]
GO

/* =========================================================
   1. TẠO LOGIN
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Login_KeToan')
BEGIN
    CREATE LOGIN [Login_KeToan]
    WITH PASSWORD = 'KeToan@Pass456',
         DEFAULT_DATABASE = [DB_Payroll],
         CHECK_EXPIRATION = OFF,
         CHECK_POLICY = ON;
    PRINT 'LOGIN Login_KeToan đã được tạo.';
END
ELSE
    PRINT 'LOGIN Login_KeToan đã tồn tại, bỏ qua.';
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Login_Admin')
BEGIN
    CREATE LOGIN [Login_Admin]
    WITH PASSWORD = 'Admin@SuperPass789',
         DEFAULT_DATABASE = [DB_Personnel],
         CHECK_EXPIRATION = OFF,
         CHECK_POLICY = ON;
    PRINT 'LOGIN Login_Admin đã được tạo.';
END
ELSE
    PRINT 'LOGIN Login_Admin đã tồn tại, bỏ qua.';
GO

/* =========================================================
   2. TẠO DATABASE
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DB_Payroll')
BEGIN
    CREATE DATABASE [DB_Payroll];
    PRINT 'DATABASE DB_Payroll đã được tạo.';
END
GO

USE [DB_Payroll]
GO

/* =========================================================
   3. TẠO USER TRONG DB_Payroll
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'User_KeToan')
BEGIN
    CREATE USER [User_KeToan] FOR LOGIN [Login_KeToan] WITH DEFAULT_SCHEMA = [dbo];
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'User_Admin')
BEGIN
    CREATE USER [User_Admin] FOR LOGIN [Login_Admin] WITH DEFAULT_SCHEMA = [dbo];
END
GO

ALTER USER [User_KeToan] WITH LOGIN = [Login_KeToan];
ALTER USER [User_Admin]  WITH LOGIN = [Login_Admin];
GO

/* =========================================================
   4. TẠO BẢNG LƯƠNG
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhanVien_Luong')
BEGIN
    CREATE TABLE [dbo].[NhanVien_Luong](
           NOT NULL,
        [LuongCoBan]  [decimal](18,2) NULL,
        [HeSo]        [float]         NULL,
        [PhuCap]      [decimal](18,2) NULL,
           NULL,
        [NgayCapNhat] [datetime]      DEFAULT GETDATE(),
        CONSTRAINT [PK_NhanVien_Luong] PRIMARY KEY CLUSTERED ([MaNV] ASC)
    );
END
GO

/* =========================================================
   5. DỮ LIỆU MẪU
   ========================================================= */
IF NOT EXISTS (SELECT * FROM dbo.NhanVien_Luong WHERE MaNV = 'NV10')
    INSERT [dbo].[NhanVien_Luong] ([MaNV], [LuongCoBan], [HeSo], [PhuCap], [SoTaiKhoan])
    VALUES ('NV10', 20000000.00, 1.5, 2000000.00, '12345678');

IF NOT EXISTS (SELECT * FROM dbo.NhanVien_Luong WHERE MaNV = 'NV11')
    INSERT [dbo].[NhanVien_Luong] ([MaNV], [LuongCoBan], [HeSo], [PhuCap], [SoTaiKhoan])
    VALUES ('NV11', 15000000.00, 1.5, 1000000.00, '12345679');

IF NOT EXISTS (SELECT * FROM dbo.NhanVien_Luong WHERE MaNV = 'NV12')
    INSERT [dbo].[NhanVien_Luong] ([MaNV], [LuongCoBan], [HeSo], [PhuCap], [SoTaiKhoan])
    VALUES ('NV12', 25000000.00, 1.5, 1500000.00, '01234567');
GO

/* =========================================================
   6. VIEW
   ========================================================= */
IF OBJECT_ID('dbo.View_KeToan_Luong', 'V') IS NOT NULL
    DROP VIEW dbo.View_KeToan_Luong;
GO

CREATE VIEW [dbo].[View_KeToan_Luong] AS
SELECT 
    MaNV,
    LuongCoBan,
    HeSo,
    PhuCap,
    SoTaiKhoan,
    (LuongCoBan * HeSo + PhuCap) AS TongThuNhap,
    NgayCapNhat
FROM dbo.NhanVien_Luong;
GO

/* =========================================================
   7. PROC CŨ - THU HỒI / GIỮ LẠI ĐỂ TƯƠNG THÍCH
   ========================================================= */
IF OBJECT_ID('dbo.sp_UpdateLuong', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateLuong;
GO

CREATE PROCEDURE [dbo].[sp_UpdateLuong]
    @MaNV         VARCHAR(10),
    @LuongMoi     DECIMAL(18,2),
    @HeSoMoi      FLOAT,
    @PhuCapMoi    DECIMAL(18,2),
    @SoTaiKhoanMoi VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien_Luong WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'MaNV %s không tìm thấy trong DB_Payroll.', 16, 1, @MaNV);
        RETURN;
    END

    UPDATE dbo.NhanVien_Luong
    SET LuongCoBan  = @LuongMoi,
        HeSo        = @HeSoMoi,
        PhuCap      = @PhuCapMoi,
        SoTaiKhoan  = @SoTaiKhoanMoi,
        NgayCapNhat = GETDATE()
    WHERE MaNV = @MaNV;
END
GO

/* =========================================================
   8. PROC MỚI - ADMIN CHỈ ĐƯỢC SỬA LƯƠNG
   ========================================================= */
IF OBJECT_ID('dbo.sp_Admin_UpdateLuong', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Admin_UpdateLuong;
GO

CREATE PROCEDURE [dbo].[sp_Admin_UpdateLuong]
    @MaNV      VARCHAR(10),
    @LuongMoi  DECIMAL(18,2),
    @HeSoMoi   FLOAT,
    @PhuCapMoi DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien_Luong WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'MaNV %s không tìm thấy trong DB_Payroll.', 16, 1, @MaNV);
        RETURN;
    END

    UPDATE dbo.NhanVien_Luong
    SET LuongCoBan  = @LuongMoi,
        HeSo        = @HeSoMoi,
        PhuCap      = @PhuCapMoi,
        NgayCapNhat = GETDATE()
    WHERE MaNV = @MaNV;
END
GO

/* =========================================================
   9. PROC MỚI - KẾ TOÁN ĐƯỢC SỬA LƯƠNG + SỐ TK
   ========================================================= */
IF OBJECT_ID('dbo.sp_KeToan_UpdateLuong', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_KeToan_UpdateLuong;
GO

CREATE PROCEDURE [dbo].[sp_KeToan_UpdateLuong]
    @MaNV          VARCHAR(10),
    @LuongMoi      DECIMAL(18,2),
    @HeSoMoi       FLOAT,
    @PhuCapMoi     DECIMAL(18,2),
    @SoTaiKhoanMoi VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien_Luong WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'MaNV %s không tìm thấy trong DB_Payroll.', 16, 1, @MaNV);
        RETURN;
    END

    UPDATE dbo.NhanVien_Luong
    SET LuongCoBan  = @LuongMoi,
        HeSo        = @HeSoMoi,
        PhuCap      = @PhuCapMoi,
        SoTaiKhoan  = @SoTaiKhoanMoi,
        NgayCapNhat = GETDATE()
    WHERE MaNV = @MaNV;
END
GO

/* =========================================================
   10. PHÂN QUYỀN DB_Payroll
   ========================================================= */
GRANT CONNECT TO [User_KeToan];
GRANT CONNECT TO [User_Admin];
GO

GRANT SELECT ON dbo.View_KeToan_Luong TO [User_KeToan];
GRANT SELECT ON dbo.View_KeToan_Luong TO [User_Admin];
GO

GRANT EXECUTE ON dbo.sp_KeToan_UpdateLuong TO [User_KeToan];
GRANT EXECUTE ON dbo.sp_Admin_UpdateLuong  TO [User_Admin];
GO

REVOKE EXECUTE ON dbo.sp_UpdateLuong FROM [User_KeToan];
REVOKE EXECUTE ON dbo.sp_UpdateLuong FROM [User_Admin];
GO

DENY INSERT ON dbo.NhanVien_Luong TO [User_KeToan];
DENY DELETE ON dbo.NhanVien_Luong TO [User_KeToan];
GO

GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.NhanVien_Luong TO [User_Admin];
GO

PRINT '=== Setup DB_Payroll hoàn tất ===';
GO