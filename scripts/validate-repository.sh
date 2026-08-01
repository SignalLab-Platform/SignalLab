#!/usr/bin/env bash

set -u

script_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd "${script_directory}/.." && pwd)"

cd "${repository_root}"

validation_failed=0

required_directories=(
    "apps"
    "apps/api"
    "apps/api/src"
    "apps/api/src/SignalLab.Api"
    "apps/api/src/SignalLab.Application"
    "apps/api/src/SignalLab.Domain"
    "apps/api/src/SignalLab.Infrastructure"
    "apps/api/tests"
    "apps/api/tests/SignalLab.Api.IntegrationTests"
    "apps/web"
    "infrastructure"
    "docs"
    "docs/milestones"
    "docs/product"
    "scripts"
    "tests"
)

required_files=(
    ".editorconfig"
    ".gitattributes"
    ".gitignore"
    "LICENSE"
    "README.md"
    "global.json"
    "apps/api/SignalLab.Api.sln"
    "apps/api/Directory.Build.props"
    "apps/api/README.md"
    "apps/api/src/SignalLab.Api/SignalLab.Api.csproj"
    "apps/api/src/SignalLab.Api/Program.cs"
    "apps/api/src/SignalLab.Application/SignalLab.Application.csproj"
    "apps/api/src/SignalLab.Domain/SignalLab.Domain.csproj"
    "apps/api/src/SignalLab.Infrastructure/SignalLab.Infrastructure.csproj"
    "apps/api/tests/SignalLab.Api.IntegrationTests/SignalLab.Api.IntegrationTests.csproj"
    "apps/api/tests/SignalLab.Api.IntegrationTests/PlatformEndpointsTests.cs"
    "tests/README.md"
)

echo "Validating SignalLab repository structure..."

for directory in "${required_directories[@]}"; do
    if [[ ! -d "${directory}" ]]; then
        echo "Missing directory: ${directory}"
        validation_failed=1
    fi
done

for file in "${required_files[@]}"; do
    if [[ ! -f "${file}" ]]; then
        echo "Missing file: ${file}"
        validation_failed=1
    fi
done

non_empty_files=(
    "LICENSE"
    "README.md"
    "apps/api/README.md"
    "tests/README.md"
)

for file in "${non_empty_files[@]}"; do
    if [[ -f "${file}" && ! -s "${file}" ]]; then
        echo "Empty required file: ${file}"
        validation_failed=1
    fi
done

if [[ "${validation_failed}" -ne 0 ]]; then
    echo "Repository validation failed."
    exit 1
fi

echo "Repository structure is valid."
