USE [master]
GO

/* =========================================================
   1. TẠO LOGIN
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Login_NhanVienIT')
BEGIN
    CREATE LOGIN [Login_NhanVienIT]
    WITH PASSWORD = 'NhanVienIT@Pass123',
         DEFAULT_DATABASE = [DB_Personnel],
         CHECK_EXPIRATION = OFF,
         CHECK_POLICY = ON;
    PRINT 'LOGIN Login_NhanVienIT đã được tạo.';
END
ELSE
    PRINT 'LOGIN Login_NhanVienIT đã tồn tại, bỏ qua.';
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

/* =========================================================
   2. TẠO DATABASE
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DB_Personnel')
BEGIN
    CREATE DATABASE [DB_Personnel];
    PRINT 'DATABASE DB_Personnel đã được tạo.';
END
GO

USE [DB_Personnel]
GO

/* =========================================================
   3. TẠO USER TRONG DB_Personnel
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'User_IT')
BEGIN
    CREATE USER [User_IT] FOR LOGIN [Login_NhanVienIT] WITH DEFAULT_SCHEMA = [dbo];
    PRINT 'USER User_IT đã được tạo.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'User_Admin')
BEGIN
    CREATE USER [User_Admin] FOR LOGIN [Login_Admin] WITH DEFAULT_SCHEMA = [dbo];
    PRINT 'USER User_Admin đã được tạo.';
END
GO

/* =========================================================
   4. TẠO BẢNG NHÂN VIÊN HÀNH CHÍNH
   ========================================================= */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhanVien_HanhChinh')
BEGIN
    CREATE TABLE [dbo].[NhanVien_HanhChinh](
           NOT NULL,
         NOT NULL,
          NULL,
          NULL,
        CONSTRAINT [PK_NhanVien_HanhChinh] PRIMARY KEY CLUSTERED ([MaNV] ASC)
    );
    PRINT 'TABLE NhanVien_HanhChinh đã được tạo.';
END
GO

/* =========================================================
   5. DỮ LIỆU MẪU
   ========================================================= */
IF NOT EXISTS (SELECT * FROM dbo.NhanVien_HanhChinh WHERE MaNV = 'NV10')
    INSERT [dbo].[NhanVien_HanhChinh] VALUES ('NV10', N'Lê Văn Luyện',   N'Kỹ thuật', N'Kỹ sư');

IF NOT EXISTS (SELECT * FROM dbo.NhanVien_HanhChinh WHERE MaNV = 'NV11')
    INSERT [dbo].[NhanVien_HanhChinh] VALUES ('NV11', N'Nguyễn Thị Lan', N'Kế toán',  N'Kế toán viên');

IF NOT EXISTS (SELECT * FROM dbo.NhanVien_HanhChinh WHERE MaNV = 'NV12')
    INSERT [dbo].[NhanVien_HanhChinh] VALUES ('NV12', N'Trần Minh Tuấn', N'Nhân sự',  N'Chuyên viên');
GO

/* =========================================================
   6. VIEW
   ========================================================= */
IF OBJECT_ID('dbo.View_Admin_Full_Profile', 'V') IS NOT NULL
    DROP VIEW dbo.View_Admin_Full_Profile;
GO

CREATE VIEW [dbo].[View_Admin_Full_Profile] AS
SELECT 
    H.MaNV,
    H.Ten,
    H.PhongBan,
    H.ChucVu,
    L.LuongCoBan,
    L.HeSo,
    L.PhuCap,
    L.SoTaiKhoan,
    (L.LuongCoBan * L.HeSo + L.PhuCap) AS TongThuNhap,
    L.NgayCapNhat
FROM DB_Personnel.dbo.NhanVien_HanhChinh H
LEFT JOIN DB_Payroll.dbo.NhanVien_Luong L ON H.MaNV = L.MaNV;
GO

IF OBJECT_ID('dbo.View_IT_HanhChinh', 'V') IS NOT NULL
    DROP VIEW dbo.View_IT_HanhChinh;
GO

CREATE VIEW [dbo].[View_IT_HanhChinh] AS
SELECT 
    MaNV,
    Ten,
    PhongBan,
    ChucVu
FROM DB_Personnel.dbo.NhanVien_HanhChinh;
GO

/* =========================================================
   7. STORED PROCEDURE - THÊM NHÂN VIÊN
   ========================================================= */
IF OBJECT_ID('dbo.sp_InsertEmployeeFull', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertEmployeeFull;
GO

CREATE PROCEDURE [dbo].[sp_InsertEmployeeFull]
    @MaNV      VARCHAR(10),
    @Ten       NVARCHAR(100),
    @PB        NVARCHAR(50),
    @CV        NVARCHAR(50),
    @Luong     DECIMAL(18,2),
    @HeSo      FLOAT,
    @PC        DECIMAL(18,2),
    @STK       VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM DB_Personnel.dbo.NhanVien_HanhChinh WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'MaNV %s đã tồn tại. Vui lòng dùng mã khác.', 16, 1, @MaNV);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO DB_Personnel.dbo.NhanVien_HanhChinh (MaNV, Ten, PhongBan, ChucVu)
        VALUES (@MaNV, @Ten, @PB, @CV);

        INSERT INTO DB_Payroll.dbo.NhanVien_Luong (MaNV, LuongCoBan, HeSo, PhuCap, SoTaiKhoan)
        VALUES (@MaNV, @Luong, @HeSo, @PC, @STK);

        COMMIT TRANSACTION;
        PRINT N'Thêm nhân viên ' + @MaNV + N' thành công vào cả 2 DB.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSev, 1);
    END CATCH
END
GO

/* =========================================================
   8. STORED PROCEDURE - XÓA NHÂN VIÊN
   ========================================================= */
IF OBJECT_ID('dbo.sp_DeleteEmployeeFull', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DeleteEmployeeFull;
GO

CREATE PROCEDURE [dbo].[sp_DeleteEmployeeFull]
    @MaNV VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM DB_Personnel.dbo.NhanVien_HanhChinh WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'MaNV %s không tồn tại trong hệ thống.', 16, 1, @MaNV);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        DELETE FROM DB_Payroll.dbo.NhanVien_Luong
        WHERE MaNV = @MaNV;

        DELETE FROM DB_Personnel.dbo.NhanVien_HanhChinh
        WHERE MaNV = @MaNV;

        COMMIT TRANSACTION;
        PRINT N'Xóa nhân viên ' + @MaNV + N' khỏi cả 2 DB thành công.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSev, 1);
    END CATCH
END
GO

/* =========================================================
   9. STORED PROCEDURE - CẬP NHẬT HÀNH CHÍNH
   ========================================================= */
IF OBJECT_ID('dbo.sp_UpdateNhanVienHanhChinh', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateNhanVienHanhChinh;
GO

CREATE PROCEDURE [dbo].[sp_UpdateNhanVienHanhChinh]
    @MaNV VARCHAR(10),
    @Ten  NVARCHAR(100),
    @PB   NVARCHAR(50),
    @CV   NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien_HanhChinh WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'Không tìm thấy nhân viên.', 16, 1);
        RETURN;
    END

    UPDATE dbo.NhanVien_HanhChinh
    SET Ten = @Ten,
        PhongBan = @PB,
        ChucVu = @CV
    WHERE MaNV = @MaNV;
END
GO

/* =========================================================
   10. STORED PROCEDURE - ADMIN CẬP NHẬT FULL PROFILE
   ========================================================= */
IF OBJECT_ID('dbo.sp_Admin_UpdateFullProfile', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Admin_UpdateFullProfile;
GO

CREATE PROCEDURE [dbo].[sp_Admin_UpdateFullProfile]
    @MaNV      VARCHAR(10),
    @Ten       NVARCHAR(100),
    @PB        NVARCHAR(50),
    @CV        NVARCHAR(50),
    @LuongMoi  DECIMAL(18,2),
    @HeSoMoi   FLOAT,
    @PhuCapMoi DECIMAL(18,2),
    @STKMoi    VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien_HanhChinh WHERE MaNV = @MaNV)
    BEGIN
        RAISERROR(N'Không tìm thấy nhân viên mã %s', 16, 1, @MaNV);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE dbo.NhanVien_HanhChinh
        SET Ten = @Ten,
            PhongBan = @PB,
            ChucVu = @CV
        WHERE MaNV = @MaNV;

        UPDATE DB_Payroll.dbo.NhanVien_Luong
        SET LuongCoBan = @LuongMoi,
            HeSo = @HeSoMoi,
            PhuCap = @PhuCapMoi,
            SoTaiKhoan = @STKMoi,
            NgayCapNhat = GETDATE()
        WHERE MaNV = @MaNV;

        COMMIT TRANSACTION;
        PRINT N'Admin đã cập nhật thành công toàn bộ hồ sơ NV: ' + @MaNV;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* =========================================================
   11. PHÂN QUYỀN DB_Personnel
   ========================================================= */
GRANT SELECT ON dbo.View_IT_HanhChinh TO [User_IT];
DENY  SELECT ON dbo.View_Admin_Full_Profile TO [User_IT];
DENY  SELECT ON dbo.NhanVien_HanhChinh TO [User_IT];
DENY  EXECUTE ON dbo.sp_InsertEmployeeFull TO [User_IT];
DENY  EXECUTE ON dbo.sp_DeleteEmployeeFull TO [User_IT];
GO

GRANT SELECT  ON dbo.View_Admin_Full_Profile TO [User_Admin];
GRANT SELECT  ON dbo.NhanVien_HanhChinh TO [User_Admin];
GRANT EXECUTE ON dbo.sp_InsertEmployeeFull TO [User_Admin];
GRANT EXECUTE ON dbo.sp_DeleteEmployeeFull TO [User_Admin];
GRANT EXECUTE ON dbo.sp_Admin_UpdateFullProfile TO [User_Admin];
GRANT EXECUTE ON dbo.sp_UpdateNhanVienHanhChinh TO [User_Admin];
GO

PRINT '=== Setup DB_Personnel hoàn tất ===';
GO