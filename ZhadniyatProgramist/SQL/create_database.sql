-- =====================================================================
--  Кръчма „Жадният Програмист“ — SQLite скрипт
-- =====================================================================
--  Създава двете свързани таблици (Programmers и Tabs) и зарежда
--  примерните данни на бай Пешо.
--
--  ВАЖНО: Приложението изпълнява същата логика АВТОМАТИЧНО при
--  първо стартиране (виж DatabaseHelper.InitializeDatabase),
--  така че този скрипт служи за документация и за ръчно създаване:
--
--      sqlite3 krachma.db < create_database.sql
-- =====================================================================

-- Включваме проверката на външните ключове (в SQLite тя е опционална)
PRAGMA foreign_keys = ON;

-- ---------------------------------------------------------------------
--  Таблица: Programmers (клиентите на кръчмата)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Programmers (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,  -- Primary Key, Auto Increment
    Name             TEXT NOT NULL,                      -- име на програмиста
    FavoriteLanguage TEXT NOT NULL                       -- любим език за програмиране
);

-- ---------------------------------------------------------------------
--  Таблица: Tabs (тефтерът — какво дължи всеки)
--  Връзка: Tabs.ProgrammerId -> Programmers.Id (Foreign Key)
--  ON DELETE CASCADE: изтрием ли програмист, тръгват си и сметките му.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Tabs (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,      -- Primary Key, Auto Increment
    ProgrammerId INTEGER NOT NULL,                       -- Foreign Key към Programmers.Id
    ItemName     TEXT NOT NULL,                          -- какво е поръчал
    Price        REAL NOT NULL,                          -- цена в лева
    Date         TEXT NOT NULL,                          -- дата на поръчката (ISO формат)
    FOREIGN KEY (ProgrammerId) REFERENCES Programmers (Id) ON DELETE CASCADE
);

-- ---------------------------------------------------------------------
--  Примерни програмисти
-- ---------------------------------------------------------------------
INSERT INTO Programmers (Name, FavoriteLanguage) VALUES
    ('Иван Debugger',        'C#'),
    ('Мария Frontendova',    'JavaScript'),
    ('Георги Stackoverflow', 'Python'),
    ('Петър NullReference',  'Java'),
    ('Алекс ConsoleLog',     'TypeScript');

-- ---------------------------------------------------------------------
--  Примерни поръчки „на тефтер“
--  (Георги Stackoverflow традиционно е най-затъналият)
-- ---------------------------------------------------------------------
INSERT INTO Tabs (ProgrammerId, ItemName, Price, Date) VALUES
    (1, 'Крафт бира',           6.50, datetime('now', '-240 minutes')),
    (1, 'Пържени картофи',      5.50, datetime('now', '-180 minutes')),
    (2, 'Бургер „404“',        11.90, datetime('now', '-200 minutes')),
    (2, 'Крафт бира',           6.50, datetime('now',  '-90 minutes')),
    (3, 'Шот „Null Pointer“',   7.00, datetime('now', '-300 minutes')),
    (3, 'Крафт бира',           6.50, datetime('now', '-250 minutes')),
    (3, 'Ракия „Legacy Code“', 13.00, datetime('now', '-210 minutes')),
    (3, 'Пържени картофи',      5.50, datetime('now', '-160 minutes')),
    (3, 'Бургер „404“',        11.90, datetime('now', '-100 minutes')),
    (3, 'Крафт бира',           6.50, datetime('now',  '-40 minutes')),
    (4, 'Пържени картофи',      5.50, datetime('now', '-130 minutes')),
    (4, 'Шот „Null Pointer“',   7.00, datetime('now',  '-60 minutes')),
    (5, 'Салата „Clean Code“',  8.90, datetime('now', '-150 minutes')),
    (5, 'Бургер „404“',        11.90, datetime('now',  '-45 minutes'));

-- =====================================================================
--  Полезни справки (използвани и в приложението)
-- =====================================================================

-- JOIN: всички поръчки с имена вместо ID-та
-- SELECT p.Name, p.FavoriteLanguage, t.ItemName, t.Price, t.Date
-- FROM Tabs t
-- INNER JOIN Programmers p ON p.Id = t.ProgrammerId
-- ORDER BY t.Date DESC;

-- SUM: общият дълг на конкретен програмист (в кода Id-то е параметър!)
-- SELECT COALESCE(SUM(Price), 0) FROM Tabs WHERE ProgrammerId = @ProgrammerId;

-- Най-затъналият програмист
-- SELECT p.Name, SUM(t.Price) AS Total
-- FROM Tabs t
-- INNER JOIN Programmers p ON p.Id = t.ProgrammerId
-- GROUP BY p.Id, p.Name
-- ORDER BY Total DESC
-- LIMIT 1;
