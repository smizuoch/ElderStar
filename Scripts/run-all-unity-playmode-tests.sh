#!/usr/bin/env bash
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

run_playmode_for_version() {
    local version="$1"

    printf '\nRunning PlayMode tests with Unity %s\n' "${version}"
    UNITY_VERSION="${version}" "${SCRIPT_DIR}/run-playmode-tests.sh"
}

primary_status=0
secondary_status=0
secondary_version="${UNITY_SECONDARY_VERSION:-6000.3.23f1}"

run_playmode_for_version "2022.3.22f1" || primary_status=$?
run_playmode_for_version "${secondary_version}" || secondary_status=$?

if [[ "${primary_status}" -ne 0 ]]; then
    printf '\nUnity 2022.3.22f1 PlayMode tests failed with exit code %s.\n' "${primary_status}" >&2
    exit "${primary_status}"
fi

if [[ "${secondary_status}" -ne 0 ]]; then
    printf '\nUnity %s PlayMode tests failed with exit code %s.\n' "${secondary_version}" "${secondary_status}" >&2
    if [[ "${UNITY_STRICT_SECONDARY:-1}" == "1" ]]; then
        exit "${secondary_status}"
    fi

    printf 'Continuing because UNITY_STRICT_SECONDARY is not 1; record this as a secondary compatibility failure.\n' >&2
fi

exit 0
