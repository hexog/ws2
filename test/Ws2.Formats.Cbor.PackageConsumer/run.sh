#!/usr/bin/env bash
set -euo pipefail

target_framework="${1:?Usage: run.sh <target-framework>}"
fixture_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
package_cache="$(mktemp -d "${TMPDIR:-/tmp}/ws2-cbor-nuget-packages.XXXXXX")"
trap 'rm -rf -- "$package_cache"' EXIT
export NUGET_PACKAGES="$package_cache"
case "$target_framework" in
  net8.0)
    converter_project="$fixture_dir/Converters/Converters.csproj"
    models_project="$fixture_dir/Models/Models.csproj"
    app_project="$fixture_dir/App/App.csproj"
    ;;
  net10.0)
    converter_project="$fixture_dir/Converters/Converters.Net10.csproj"
    models_project="$fixture_dir/Models/Models.Net10.csproj"
    app_project="$fixture_dir/App/App.Net10.csproj"
    ;;
  *) echo "Unsupported target framework: $target_framework" >&2; exit 2 ;;
esac
dotnet restore "$converter_project"
dotnet restore "$models_project"
dotnet restore "$app_project"
dotnet build "$converter_project" -c Release --no-restore
dotnet build "$models_project" -c Release --no-restore
dotnet build "$app_project" -c Release --no-restore
dotnet "$fixture_dir/App/bin/Release/$target_framework/App.dll"
