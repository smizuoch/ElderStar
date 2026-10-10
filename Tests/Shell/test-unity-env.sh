#!/usr/bin/env bash
set -euo pipefail
source "$(cd "$(dirname "$0")/../.." && pwd)/Scripts/unity-env.sh"

# A failed Windows TEMP lookup must stop before creating/copying a mirror.
nova_windows_temp_root() { return 2; }
mkdir() { return 0; }
rsync() { return 0; }
if nova_prepare_windows_project_mirror /tmp/nova-source >/dev/null 2>&1; then
    printf 'FAIL: failed TEMP lookup was ignored\n' >&2
    exit 1
fi

# Separate Unity versions must never share a writable verification checkout.
nova_windows_temp_root() { printf '/mnt/c/Users/Test/AppData/Local/Temp\n'; }
export UNITY_VERSION=6000.3.23f1
mirror="$(nova_prepare_windows_project_mirror /tmp/nova-source)"
[[ "$mirror" == "/mnt/c/Users/Test/AppData/Local/Temp/NovaShaderBuiltInUnityProject/6000.3.23f1" ]]
[[ "$(nova_unity_editor_path)" == "/mnt/c/Program Files/Unity/Hub/Editor/6000.3.23f1/Editor/Unity.exe" ]]
rsync() { return 7; }
if nova_prepare_windows_project_mirror /tmp/nova-source >/dev/null 2>&1; then
    printf 'FAIL: failed mirror copy was ignored\n' >&2
    exit 1
fi
unset -f mkdir rsync nova_windows_temp_root
source "$(cd "$(dirname "$0")/../.." && pwd)/Scripts/unity-env.sh"

# WSL can disable Windows PATH inheritance while still allowing interop.
if [[ -f /mnt/c/Windows/System32/cmd.exe ]]; then
    temp="$(PATH=/usr/bin:/bin nova_windows_temp_root)"
    [[ "$temp" == /mnt/?/* && "$temp" != *%TEMP%* ]]
fi
printf 'PASS: TEMP failure propagation, per-version mirrors and Windows interop\n'
