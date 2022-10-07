#!/bin/bash
# This script fetches the VSCode VSIX from the latest CI run for a given PR ID
# For now, it only works for PR branches within the repo (no forked PRs)
#
# Usage: ./scripts/pr_install_vsix.sh <pr_id>

set -e
usage="Usage: ./scripts/pr_devcontainer/run.sh <pr_id>"
prId=${1:?"Missing PR id. ${usage}"}

command -v devcontainer >/dev/null 2>&1 || { echo "devcontainer CLI isn't installed. Use VSCode to install it first." >&2; exit 1; }

scriptDir=$(cd `dirname -- "$0"` && pwd)
branch=$(gh pr view $prId --json headRefName --jq ".headRefName")
runId=$(gh run list -b $branch --limit 1 --json databaseId --jq ".[0].databaseId")

tmpBinariesDir=/tmp/bicep-binaries
binariesDir=$scriptDir/.devcontainer/binaries
mkdir -p $tmpBinariesDir
mkdir -p $binariesDir

rm -Rf $tmpBinariesDir
gh run download $runId -n bicep-release-linux-x64 -D $tmpBinariesDir
mv $tmpBinariesDir/bicep $binariesDir/bicep-release-linux-x64

rm -Rf $tmpBinariesDir
gh run download $runId -n bicep-release-linux-arm64 -D $tmpBinariesDir
mv $tmpBinariesDir/bicep $binariesDir/bicep-release-linux-arm64

rm -Rf $tmpBinariesDir
gh run download $runId -n vscode-bicep.vsix -D $tmpBinariesDir
mv $tmpBinariesDir/vscode-bicep.vsix $binariesDir/vscode-bicep.vsix

devcontainer up --remove-existing-container --workspace-folder $scriptDir
code --folder-uri=vscode-remote://dev-container+$(printf "%s" "$scriptDir" | xxd -p -c 100000)/workspaces/bicep