#!/usr/bin/env bash
set -euo pipefail

NOVA_UNITY_2022_3_22F1="/mnt/c/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor/Unity.exe"
NOVA_UNITY_6000_0_74F1="/mnt/c/Program Files/Unity/Hub/Editor/6000.0.74f1/Editor/Unity.exe"

nova_repo_root() {
    local script_dir
    script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    cd "${script_dir}/.." && pwd
}

nova_selected_unity_version() {
    printf '%s\n' "${UNITY_VERSION:-2022.3.22f1}"
}

nova_unity_editor_path() {
    if [[ -n "${UNITY_EDITOR:-}" ]]; then
        printf '%s\n' "${UNITY_EDITOR}"
        return 0
    fi

    case "$(nova_selected_unity_version)" in
        2022.3.22f1)
            printf '%s\n' "${NOVA_UNITY_2022_3_22F1}"
            ;;
        6000.0.74f1)
            printf '%s\n' "${NOVA_UNITY_6000_0_74F1}"
            ;;
        *)
            printf 'Unsupported UNITY_VERSION: %s\n' "$(nova_selected_unity_version)" >&2
            printf 'Set UNITY_EDITOR to an explicit Windows Unity.exe path to override version detection.\n' >&2
            return 2
            ;;
    esac
}

nova_require_wslpath() {
    if ! command -v wslpath >/dev/null 2>&1; then
        printf 'wslpath is required to run Windows Unity from WSL.\n' >&2
        return 2
    fi
}

nova_windows_temp_root() {
    local temp_windows
    temp_windows="$(
        cmd.exe /c echo %TEMP% 2>/dev/null \
            | tr -d '\r' \
            | awk '/^[A-Za-z]:\\/ { value = $0 } END { print value }'
    )"

    if [[ -z "${temp_windows}" ]]; then
        printf 'Could not resolve Windows %%TEMP%% from WSL.\n' >&2
        return 2
    fi

    wslpath -u "${temp_windows}"
}

nova_prepare_windows_project_mirror() {
    local source_project_path="$1"
    local temp_root
    local mirror_path

    temp_root="$(nova_windows_temp_root)"
    mirror_path="${temp_root}/NovaShaderBuiltInUnityProject"

    mkdir -p "${mirror_path}"

    rsync -a --delete \
        --exclude '.git/' \
        --exclude 'Library/' \
        --exclude 'Logs/' \
        --exclude 'NovaShader/' \
        --exclude 'Obj/' \
        --exclude 'Temp/' \
        --exclude 'UserSettings/' \
        "${source_project_path}/" \
        "${mirror_path}/"

    printf '%s\n' "${mirror_path}"
}

nova_print_unity_invocation() {
    local selected_version="$1"
    local unity_editor="$2"
    local project_path="$3"
    local project_path_windows="$4"
    local result_path="$5"
    local log_path="$6"

    printf 'Unity version: %s\n' "${selected_version}"
    printf 'Unity executable: %s\n' "${unity_editor}"
    printf 'WSL project path: %s\n' "${project_path}"
    printf 'Windows project path: %s\n' "${project_path_windows}"
    printf 'Result XML path: %s\n' "${result_path}"
    printf 'Unity log path: %s\n' "${log_path}"
}
