#!/usr/bin/env bash
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

run_editmode_for_version() {
    local version="$1"

    printf '\nRunning EditMode tests with Unity %s\n' "${version}"
    UNITY_VERSION="${version}" "${SCRIPT_DIR}/run-editmode-tests.sh"
}

primary_status=0
secondary_status=0

run_editmode_for_version "2022.3.22f1" || primary_status=$?
run_editmode_for_version "6000.0.74f1" || secondary_status=$?

if [[ "${primary_status}" -ne 0 ]]; then
    printf '\nUnity 2022.3.22f1 EditMode tests failed with exit code %s.\n' "${primary_status}" >&2
    exit "${primary_status}"
fi

if [[ "${secondary_status}" -ne 0 ]]; then
    printf '\nUnity 6000.0.74f1 EditMode tests failed with exit code %s.\n' "${secondary_status}" >&2
    if [[ "${UNITY_STRICT_SECONDARY:-0}" == "1" ]]; then
        exit "${secondary_status}"
    fi

    printf 'Continuing because UNITY_STRICT_SECONDARY is not 1; record this as a secondary compatibility failure.\n' >&2
fi

exit 0
