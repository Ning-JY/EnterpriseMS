-- =============================================================
--  EnterpriseMS 通知中心 + 历史结构漂移 一次性 DDL 脚本
--  适用库: enterprise_db  引擎: InnoDB  字符集: utf8mb4
--  说明:
--   1) 本脚本与 EF 迁移 20260724124805_NotificationInit 的 Up() 一一对应，
--      仅新增结构，不修改任何已有数据（尤其不会动 sys_user.password_hash）。
--   2) 末尾会写入 __EFMigrationsHistory，使应用启动时的 MigrateAsync() 认为
--      该迁移已应用，避免重复执行报错。
--   3) 请在目标库（尚未应用该迁移的库）上执行一次。
-- =============================================================

USE enterprise_db;

-- -------------------------------------------------------------
-- 1) proj_project 扩展字段
-- -------------------------------------------------------------
ALTER TABLE proj_project
    ADD COLUMN contract_scan_file  longtext     NULL,
    ADD COLUMN contract_signed     tinyint(1)   NOT NULL DEFAULT 0,
    ADD COLUMN contract_signed_date datetime(6) NULL,
    ADD COLUMN project_category    longtext     NULL,
    ADD COLUMN project_leader_id   bigint       NULL,
    ADD COLUMN project_overview    longtext     NULL;

-- -------------------------------------------------------------
-- 2) hr_employee 扩展字段
-- -------------------------------------------------------------
ALTER TABLE hr_employee
    ADD COLUMN address            longtext NULL,
    ADD COLUMN bank_account       longtext NULL,
    ADD COLUMN bank_name          longtext NULL,
    ADD COLUMN birth_date         datetime(6) NULL,
    ADD COLUMN emergency_contact  longtext NULL,
    ADD COLUMN emergency_phone    longtext NULL,
    ADD COLUMN graduate_school    longtext NULL,
    ADD COLUMN highest_degree     longtext NULL,
    ADD COLUMN major              longtext NULL,
    ADD COLUMN nationality        longtext NULL,
    ADD COLUMN native_place       longtext NULL,
    ADD COLUMN political_status   longtext NULL,
    ADD COLUMN profile_photo      longtext NULL,
    ADD COLUMN social_insurance_no longtext NULL,
    ADD COLUMN technical_level    longtext NULL,
    ADD COLUMN technical_title    longtext NULL,
    ADD COLUMN work_start_date    datetime(6) NULL;

-- -------------------------------------------------------------
-- 3) hr_education（学历）
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS hr_education (
    Id            bigint       NOT NULL AUTO_INCREMENT,
    employee_id   bigint       NOT NULL,
    school_name   longtext     NOT NULL,
    major         longtext     NOT NULL,
    degree        longtext     NOT NULL,
    start_date    datetime(6)  NULL,
    end_date      datetime(6)  NULL,
    is_full_time  tinyint(1)   NOT NULL,
    remark        longtext     NULL,
    CreatedAt     datetime(6)  NOT NULL,
    CreatedBy     longtext     NOT NULL,
    UpdatedAt     datetime(6)  NULL,
    UpdatedBy     longtext     NULL,
    IsDeleted     tinyint(1)   NOT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_hr_education_hr_employee_employee_id
        FOREIGN KEY (employee_id) REFERENCES hr_employee (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -------------------------------------------------------------
-- 4) hr_work_experience（工作经历）
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS hr_work_experience (
    Id            bigint       NOT NULL AUTO_INCREMENT,
    employee_id   bigint       NOT NULL,
    company_name  longtext     NOT NULL,
    position      longtext     NOT NULL,
    start_date    datetime(6)  NULL,
    end_date      datetime(6)  NULL,
    remark        longtext     NULL,
    CreatedAt     datetime(6)  NOT NULL,
    CreatedBy     longtext     NOT NULL,
    UpdatedAt     datetime(6)  NULL,
    UpdatedBy     longtext     NULL,
    IsDeleted     tinyint(1)   NOT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_hr_work_experience_hr_employee_employee_id
        FOREIGN KEY (employee_id) REFERENCES hr_employee (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -------------------------------------------------------------
-- 5) sys_notification_read（按用户的已读记录）
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sys_notification_read (
    Id             bigint       NOT NULL AUTO_INCREMENT,
    UserId         bigint       NOT NULL,
    NotificationId bigint       NOT NULL,
    ReadAt         datetime(6)  NOT NULL,
    CreatedAt      datetime(6)  NOT NULL,
    CreatedBy      longtext     NOT NULL,
    UpdatedAt      datetime(6)  NULL,
    UpdatedBy      longtext     NULL,
    IsDeleted      tinyint(1)   NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_sys_notification_read_UserId_NotificationId (UserId, NotificationId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -------------------------------------------------------------
-- 6) sys_notification（通知主表）
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sys_notification (
    Id            bigint       NOT NULL AUTO_INCREMENT,
    Type          longtext     NOT NULL,
    Title         longtext     NOT NULL,
    Content       longtext     NOT NULL,
    Link          longtext     NULL,
    Level         longtext     NOT NULL,
    RequiredPerm  longtext     NULL,
    RecipientType longtext     NOT NULL,
    RecipientId   bigint       NULL,
    SourceKey     longtext     NULL,
    IsRead        tinyint(1)   NOT NULL,
    ReadAt        datetime(6)  NULL,
    CreatedAt     datetime(6)  NOT NULL,
    CreatedBy     longtext     NOT NULL,
    UpdatedAt     datetime(6)  NULL,
    UpdatedBy     longtext     NULL,
    IsDeleted     tinyint(1)   NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -------------------------------------------------------------
-- 7) sys_config（系统配置）
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sys_config (
    Id           bigint       NOT NULL AUTO_INCREMENT,
    config_key   longtext     NOT NULL,
    config_value longtext     NOT NULL,
    config_type  longtext     NOT NULL,
    group_name   longtext     NOT NULL,
    sort         int          NOT NULL,
    remark       longtext     NULL,
    CreatedAt    datetime(6)  NOT NULL,
    CreatedBy    longtext     NOT NULL,
    UpdatedAt    datetime(6)  NULL,
    UpdatedBy    longtext     NULL,
    IsDeleted    tinyint(1)   NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -------------------------------------------------------------
-- 8) 索引 + proj_project 外键
-- -------------------------------------------------------------
CREATE INDEX IX_proj_project_project_leader_id
    ON proj_project (project_leader_id);

CREATE INDEX IX_hr_education_employee_id
    ON hr_education (employee_id);

CREATE INDEX IX_hr_work_experience_employee_id
    ON hr_work_experience (employee_id);

-- （可选性能索引）SyncExpiryAsync 按 SourceKey 幂等 upsert，建索引可加速查找：
-- CREATE INDEX IX_sys_notification_SourceKey ON sys_notification (SourceKey);

ALTER TABLE proj_project
    ADD CONSTRAINT FK_proj_project_hr_employee_project_leader_id
    FOREIGN KEY (project_leader_id) REFERENCES hr_employee (Id);

-- -------------------------------------------------------------
-- 9) 登记迁移版本，避免应用启动重复迁移
-- -------------------------------------------------------------
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20260724124805_NotificationInit', '9.0.0');

-- =============================================================
--  执行结束。可在 MySQL 客户端直接 source 本文件，或整体粘贴执行。
-- =============================================================
