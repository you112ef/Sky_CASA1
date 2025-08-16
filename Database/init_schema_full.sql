PRAGMA foreign_keys = ON;

-- Roles & Users
CREATE TABLE IF NOT EXISTS Roles (
    RoleId INTEGER PRIMARY KEY AUTOINCREMENT,
    RoleName TEXT NOT NULL UNIQUE -- 'Admin','LabTech','Reception'
);

CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FullName TEXT,
    RoleId INTEGER NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Patients
CREATE TABLE IF NOT EXISTS Patients (
    PatientId INTEGER PRIMARY KEY AUTOINCREMENT,
    MRN TEXT NOT NULL UNIQUE,
    FullName TEXT NOT NULL,
    Gender TEXT,
    DOB DATE,
    Phone TEXT,
    Address TEXT,
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Exams
CREATE TABLE IF NOT EXISTS Exams (
    ExamId INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId INTEGER NOT NULL,
    ExamType TEXT NOT NULL, -- 'CASA','CBC','URINE','STOOL'
    OrderedBy TEXT,
    CollectedAt DATETIME,
    PerformedBy INTEGER,
    PerformedAt DATETIME,
    ResultJson TEXT,
    ReportPath TEXT,
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (PerformedBy) REFERENCES Users(UserId)
);

-- CASA results
CREATE TABLE IF NOT EXISTS CASA_Results (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ExamId INTEGER NOT NULL UNIQUE,
    VCL REAL, VSL REAL, VAP REAL, ALH REAL, BCF REAL,
    MotilityPercent REAL, ProgressivePercent REAL, TrackCount INTEGER,
    MetaJson TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ExamId) REFERENCES Exams(ExamId)
);

-- CBC results
CREATE TABLE IF NOT EXISTS CBC_Results (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ExamId INTEGER NOT NULL UNIQUE,
    WBC REAL, RBC REAL, HGB REAL, HCT REAL,
    MCV REAL, MCH REAL, MCHC REAL, RDW REAL,
    PLT REAL, MPV REAL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ExamId) REFERENCES Exams(ExamId)
);

-- Urine results
CREATE TABLE IF NOT EXISTS Urine_Results (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ExamId INTEGER NOT NULL UNIQUE,
    Color TEXT, Turbidity TEXT, pH REAL, SpecificGravity REAL,
    Protein TEXT, Glucose TEXT, Ketones TEXT, Blood TEXT,
    LeukocyteEsterase TEXT, Nitrite TEXT, MicroscopyNotes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ExamId) REFERENCES Exams(ExamId)
);

-- Calibration
CREATE TABLE IF NOT EXISTS Calibration (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MicronsPerPixel REAL NOT NULL,
    FPS REAL NOT NULL,
    CameraName TEXT,
    UserName TEXT NOT NULL,
    Notes TEXT,
    CreatedAt DATETIME NOT NULL
);

-- Audit logs
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Action TEXT NOT NULL,
    Description TEXT,
    CreatedAt DATETIME NOT NULL
);

-- Backups
CREATE TABLE IF NOT EXISTS Backups (
    BackupId INTEGER PRIMARY KEY AUTOINCREMENT,
    FilePath TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Insert default roles
INSERT OR IGNORE INTO Roles (RoleName) VALUES ('Admin');
INSERT OR IGNORE INTO Roles (RoleName) VALUES ('LabTech');
INSERT OR IGNORE INTO Roles (RoleName) VALUES ('Reception');

-- Create initial admin if not exists (password 'admin123' hashed should be inserted by app)