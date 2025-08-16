-- Medical Lab Analyzer - Database Initialization
-- إنشاء جداول المعايرة والسجلات

-- جدول إعدادات المعايرة
CREATE TABLE IF NOT EXISTS Calibration (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MicronsPerPixel REAL NOT NULL,
    FPS REAL NOT NULL,
    UserName TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL
);

-- جدول السجل (Audit Logs)
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Action TEXT NOT NULL,
    Description TEXT,
    CreatedAt DATETIME NOT NULL
);

-- إدراج بيانات تجريبية للمعايرة
INSERT OR IGNORE INTO Calibration (MicronsPerPixel, FPS, UserName, CreatedAt) 
VALUES (0.5, 25.0, 'admin', datetime('now'));

-- إدراج سجل تجريبي
INSERT OR IGNORE INTO AuditLogs (Action, Description, CreatedAt) 
VALUES ('System', 'تم إنشاء قاعدة البيانات', datetime('now'));