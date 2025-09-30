PostgreSQL View Script Extractor

A fast and lightweight console app that extracts all CREATE VIEW statements from a PostgreSQL backup file (backup.sql) and generates two ready-to-use scripts:

view_create_scripts.txt → contains all CREATE VIEW statements.

view_drop_scripts.txt → contains safe DROP VIEW IF EXISTS ... CASCADE statements in reverse order to handle dependencies correctly.

Why is it useful?

🚀 Fast & simple – run directly as a console app, no setup required.

🔎 Automatically separates view definitions from a full database backup.

🛠 Quickly recreate or drop views during migrations, testing, or debugging.

📂 Handles schema-qualified names (schema.view), defaults to public if missing.

🔄 Dependency-safe drop order (views are dropped in reverse sequence).

✅ Portable: just requires .NET 6+ (or newer).

Usage
dotnet run -- <backup.sql>


Example:

dotnet run -- ./backup.sql


Output files:

view_create_scripts.txt

view_drop_scripts.txt

✔ Perfect for developers, DBAs, and DevOps who need a quick and reliable way to manage PostgreSQL view scripts.
