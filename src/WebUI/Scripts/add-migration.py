#!/usr/bin/env python
import os
import sys

if len(sys.argv) != 2:
    print("Usage: python add-migration.py MigrationName")
    sys.exit(1)

project_path = "../Infrastructure"
output_dir = "../Infrastructure/Data/Migrations"
startup_project = "../WebUI"

migration_name = sys.argv[1]

os.system(f"dotnet ef migrations add {migration_name} --project {project_path} --output-dir {output_dir} --startup-project {startup_project}")