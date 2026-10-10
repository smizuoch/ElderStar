#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=Scripts/unity-env.sh
source "${SCRIPT_DIR}/unity-env.sh"

nova_require_wslpath

SELECTED_VERSION="$(nova_selected_unity_version)"
UNITY_EXE="$(nova_unity_editor_path)"
PROJECT_PATH="$(nova_repo_root)"
UNITY_PROJECT_PATH="$(nova_prepare_windows_project_mirror "${PROJECT_PATH}")"
RESULT_PATH="${PROJECT_PATH}/Logs/Unity/${SELECTED_VERSION}/EditMode.xml"
LOG_PATH="${PROJECT_PATH}/Logs/Unity/${SELECTED_VERSION}/Editor.log"

mkdir -p "$(dirname "${RESULT_PATH}")"
rm -f -- "${RESULT_PATH}"

PROJECT_PATH_WINDOWS="$(wslpath -w "${UNITY_PROJECT_PATH}")"
RESULT_PATH_WINDOWS="$(wslpath -w "${RESULT_PATH}")"
LOG_PATH_WINDOWS="$(wslpath -w "${LOG_PATH}")"

nova_print_unity_invocation \
    "${SELECTED_VERSION}" \
    "${UNITY_EXE}" \
    "${PROJECT_PATH}" \
    "${PROJECT_PATH_WINDOWS}" \
    "${RESULT_PATH}" \
    "${LOG_PATH}"

"${UNITY_EXE}" \
    -batchmode \
    -projectPath "${PROJECT_PATH_WINDOWS}" \
    -runTests \
    -testPlatform EditMode \
    -testResults "${RESULT_PATH_WINDOWS}" \
    -logFile "${LOG_PATH_WINDOWS}"

nova_validate_test_results "${RESULT_PATH}"
