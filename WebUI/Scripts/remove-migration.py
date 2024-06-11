#!/usr/bin/env python
import os
import sys

project_path = "../Infrastructure"
output_dir = "../Infrastructure/Data/Migrations"
startup_project = "../WebUI"

os.system(f"dotnet ef migrations remove --project {project_path} --startup-project {startup_project}")