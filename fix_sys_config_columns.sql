-- =============================================================
--  修复：sys_config 表缺少 IsDeleted 等 BaseEntity 列
--  现象：点击「新建项目」报错
--        MySqlConnector.MySqlException: Unknown column 's.IsDeleted' in 'where clause'
--  原因：sys_config 表早先以 EnsureCreated()/手动方式建立，当时 SysConfig
--        还没有 IsDeleted 属性与全局软删除过滤器；后续 NotificationInit
--        迁移用 CREATE TABLE IF NOT EXISTS 建表，因表已存在被跳过，列未补齐。
--  处理：幂等补齐 BaseEntity 列（已存在则跳过），存量行以默认值填充。
-- =============================================================

USE enterprise_db;

DELIMITER $$

-- 列不存在时才添加，避免重复执行报错
CREATE PROCEDURE add_col_if_missing(
    IN p_table  VARCHAR(64),
    IN p_col    VARCHAR(64),
    IN p_def    VARCHAR(255)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME   = p_table
          AND COLUMN_NAME  = p_col
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` ADD COLUMN `', p_col, '` ', p_def);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

DELIMITER ;

-- 补齐 sys_config 的软删除/审计列（与 SysConfig : BaseEntity 模型一致）
CALL add_col_if_missing('sys_config', 'IsDeleted',  'tinyint(1) NOT NULL DEFAULT 0');
CALL add_col_if_missing('sys_config', 'CreatedAt',  'datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)');
CALL add_col_if_missing('sys_config', 'CreatedBy',  'longtext NOT NULL DEFAULT ('''')');
CALL add_col_if_missing('sys_config', 'UpdatedAt',  'datetime(6) NULL');
CALL add_col_if_missing('sys_config', 'UpdatedBy',  'longtext NULL');

DROP PROCEDURE add_col_if_missing;

-- 验证：列出 sys_config 当前列（确认 IsDeleted 已存在）
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'sys_config'
ORDER BY ORDINAL_POSITION;

-- =============================================================
--  说明：若其它页面也出现同样的 "Unknown column 'x.IsDeleted'"，
--  说明对应表也是「早于软删除过滤器建立、且漏了列」。用同样套路：
--    CALL add_col_if_missing('<表名>', 'IsDeleted', 'tinyint(1) NOT NULL DEFAULT 0');
--  并视需要补齐 CreatedAt/CreatedBy/UpdatedAt/UpdatedBy 即可。
-- =============================================================
