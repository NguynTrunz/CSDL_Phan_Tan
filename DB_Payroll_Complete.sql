
USE [master]
GO
-- Tạo login cho ke toan vơi admin 
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Login_KeToan')
BEGIN
    CREATE LOGIN [Login_KeToan] WITH PASSWORD = 'KeToan@Pass456',
    DEFAULT_DATABASE = [DB_Payroll],
    CHECK_EXPIRATION = OFF,
    CHECK_POLICY = ON;
END
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Login_Admin')
BEGIN
    CREATE LOGIN [Login_Admin] WITH PASSWORD = 'Admin@SuperPass789',
    DEFAULT_DATABASE = [DB_Personnel],
    CHECK_EXPIRATION = OFF,
    CHECK_POLICY = ON;
    PRINT 'LOGIN Login_Admin đã được tạo.';
END
ELSE
    PRINT 'LOGIN Login_Admin đã tồn tại, bỏ qua.';
GO

-- Tạo Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DB_Payroll')
BEGIN
    CREATE DATABASE [DB_Payroll];
END
GO

USE [DB_Payroll]
GO

-- Tạo User cho Kế Toán
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'User_KeToan')
BEGIN
    CREATE USER [User_KeToan] FOR LOGIN [Login_KeToan] WITH DEFAULT_SCHEMA=[dbo];
END
GO

-- Tạo User cho Admin (Dùng Login_Admin đã tạo từ file Personal)
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'User_Admin')
BEGIN
    CREATE USER [User_Admin] FOR LOGIN [Login_Admin] WITH DEFAULT_SCHEMA=[dbo];
END
GO

-- Cấp quyền tối cao cho Admin tại DB này
ALTER ROLE [db_owner] ADD MEMBER [User_Admin];
GO

-- Tạo cột cho bảng
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhanVien_Luong')
BEGIN
    CREATE TABLE [dbo].[NhanVien_Luong](
        [MaNV]        [varchar](10)   NOT NULL,
        [LuongCoBan] [decimal](18,2) NULL,
        [HeSo]        [float]          NULL,
        [PhuCap]      [decimal](18,2) NULL,
        [SoTaiKhoan] [varchar](20)   NULL,
        [NgayCapNhat] [datetime]      DEFAULT GETDATE(),
        PRIMARY KEY CLUSTERED ([MaNV] ASC)
    );
END
GO

-- DỮ LIỆU MẪU
 
IF NOT EXISTS (SELECT * FROM dbo.NhanVien_Luong WHERE MaNV = 'NV10')
    INSERT [dbo].[NhanVien_Luong] ([MaNV], [LuongCoBan], [HeSo], [PhuCap], [SoTaiKhoan])
    VALUES ('NV10', 20000000.00,  1.5, 2000000.00, '12345678');

IF NOT EXISTS (SELECT * FROM dbo.NhanVien_Luong WHERE MaNV = 'NV11')
    INSERT [dbo].[NhanVien_Luong] ([MaNV], [LuongCoBan], [HeSo], [PhuCap], [SoTaiKhoan])
    VALUES ('NV11', 15000000.00,  1.5, 1000000.00, '12345679');

IF NOT EXISTS (SELECT * FROM dbo.NhanVien_Luong WHERE MaNV = 'NV12')
    INSERT [dbo].[NhanVien_Luong] ([MaNV], [LuongCoBan], [HeSo], [PhuCap], [SoTaiKhoan])
    VALUES ('NV12', 25000000.00,  1.5, 1500000.00, '01234567');
GO

-- View 
IF OBJECT_ID('dbo.View_KeToan_Luong', 'V') IS NOT NULL
    DROP VIEW dbo.View_KeToan_Luong;
GO

CREATE VIEW [dbo].[View_KeToan_Luong] AS
SELECT MaNV,
       LuongCoBan,
       HeSo, 
       PhuCap, 
       SoTaiKhoan,
       (LuongCoBan * HeSo + PhuCap) AS TongThuNhap,
       NgayCapNhat
FROM dbo.NhanVien_Luong;
GO

-- BƯỚC 8: STORED PROCEDURE - CẬP NHẬT LƯƠNG

IF OBJECT_ID('dbo.sp_UpdateLuong', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateLuong;
GO

CREATE PROCEDURE [dbo].[sp_UpdateLuong]
    @MaNV      VARCHAR(10),
    @LuongMoi  DECIMAL(18,2),
    @HeSoMoi   FLOAT,
    @PhuCapMoi DECIMAL(18,2),
    @SoTaiKhoanMoi VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM DB_Payroll.dbo.NhanVien_Luong WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'MaNV %s không tìm thấy trong DB_Payroll.', 16, 1, @MaNV);
        RETURN;
    END

    BEGIN TRY
        UPDATE DB_Payroll.dbo.NhanVien_Luong
        SET 
            LuongCoBan  = @LuongMoi,
            HeSo        = @HeSoMoi,
            PhuCap      = @PhuCapMoi,
            SoTaiKhoan =@SoTaiKhoanMoi,
            NgayCapNhat = GETDATE()
        WHERE MaNV = @MaNV;

        PRINT N'Cập nhật lương nhân viên ' + @MaNV + N' thành công.';
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END
GO

-- 5. Phân quyền cho Kế toán
GRANT SELECT ON dbo.View_KeToan_Luong TO [User_KeToan]; 
GRANT EXECUTE ON dbo.sp_UpdateLuong TO [User_KeToan];
GRANT UPDATE (LuongCoBan, HeSo, PhuCap, SoTaiKhoan) ON dbo.NhanVien_Luong TO [User_KeToan];
DENY DELETE ON dbo.NhanVien_Luong TO [User_KeToan];
DENY INSERT ON dbo.NhanVien_Luong TO [User_KeToan];
GO

PRINT '=== Setup DB_Payroll thành công (Admin & Kế toán đã sẵn sàng) ===';