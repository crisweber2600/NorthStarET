#!/usr/bin/env bash

set -e

# Transition from specification branch (###-feature-name-spec) to implementation branch (###-feature-name)
# Constitution v2.1.0 - Specification Branch Workflow

JSON_MODE=false

while [ $# -gt 0 ]; do
    case "$1" in
        --json) JSON_MODE=true ;;
        --help|-h)
            echo "Usage: $0 [--json]"
            echo ""
            echo "Description:"
            echo "  Transitions from specification branch (###-feature-name-spec) to implementation branch (###-feature-name)."
            echo "  This script MUST be run from a specification branch."
            echo ""
            echo "Options:"
            echo "  --json     Output in JSON format"
            echo "  --help, -h Show this help message"
            echo ""
            echo "Workflow:"
            echo "  1. Verifies current branch is a specification branch (ends with -spec)"
            echo "  2. Creates implementation branch from current spec branch"
            echo "  3. Preserves all specification artifacts in implementation branch"
            echo "  4. Loads layer metadata from .layer file"
            echo "  5. Checks out implementation branch"
            echo ""
            echo "Example:"
            echo "  # From specification branch 003-user-auth-spec:"
            echo "  $0 --json"
            echo "  # Creates and checks out branch: 003-user-auth"
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            echo "Use --help for usage information" >&2
            exit 1
            ;;
    esac
    shift
done

# Function to find the repository root
find_repo_root() {
    local dir="$1"
    while [ "$dir" != "/" ]; do
        if [ -d "$dir/.git" ] || [ -d "$dir/.specify" ]; then
            echo "$dir"
            return 0
        fi
        dir="$(dirname "$dir")"
    done
    return 1
}

# Get repository root
SCRIPT_DIR="$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if git rev-parse --show-toplevel >/dev/null 2>&1; then
    REPO_ROOT=$(git rev-parse --show-toplevel)
    HAS_GIT=true
else
    REPO_ROOT="$(find_repo_root "$SCRIPT_DIR")"
    if [ -z "$REPO_ROOT" ]; then
        echo "Error: Could not determine repository root." >&2
        exit 1
    fi
    HAS_GIT=false
fi

cd "$REPO_ROOT"

if [ "$HAS_GIT" = false ]; then
    echo "Error: Git repository required for branch transitions" >&2
    exit 1
fi

# Get current branch
CURRENT_BRANCH=$(git branch --show-current)

# Verify current branch is a specification branch (layer-prefixed, ends with -spec)
if [[ ! "$CURRENT_BRANCH" =~ ^[^/]+/[0-9]+-[^/]+-spec$ ]]; then
    echo "Error: Must be on a specification branch ({LayerName}/###-feature-name-spec) to transition to implementation" >&2
    echo "Current branch: $CURRENT_BRANCH" >&2
    exit 1
fi

# Extract layer, implementation branch name, feature number, and short name
if [[ "$CURRENT_BRANCH" =~ ^([^/]+)/([0-9]{3})-(.+)-spec$ ]]; then
    LAYER="${BASH_REMATCH[1]}"
    FEATURE_NUM="${BASH_REMATCH[2]}"
    SHORT_NAME="${BASH_REMATCH[3]}"
    IMPL_BRANCH="${LAYER}/${FEATURE_NUM}-${SHORT_NAME}"
else
    echo "Error: Specification branch name does not match expected pattern {LayerName}/###-feature-name-spec" >&2
    echo "Current branch: $CURRENT_BRANCH" >&2
    exit 1
fi

# Check if implementation branch already exists
if git show-ref --verify --quiet "refs/heads/$IMPL_BRANCH"; then
    echo "Error: Implementation branch '$IMPL_BRANCH' already exists" >&2
    echo "Use 'git checkout $IMPL_BRANCH' to switch to it" >&2
    exit 1
fi

# Find feature directory using layer from branch name
FEATURE_DIR_NAME="${FEATURE_NUM}-${SHORT_NAME}"
FEATURE_DIR="$REPO_ROOT/Plan/$LAYER/specs/$FEATURE_DIR_NAME"

if [ ! -d "$FEATURE_DIR" ]; then
    echo "Error: Could not find feature directory at expected location: $FEATURE_DIR" >&2
    exit 1
fi

# Verify layer metadata matches branch prefix
LAYER_METADATA="$FEATURE_DIR/.layer"
if [ -f "$LAYER_METADATA" ]; then
    LAYER_FROM_FILE=$(cat "$LAYER_METADATA" | tr -d '[:space:]')
    if [ -n "$LAYER_FROM_FILE" ] && [ "$LAYER_FROM_FILE" != "$LAYER" ]; then
        echo "Error: Layer mismatch - branch indicates '$LAYER' but .layer file contains '$LAYER_FROM_FILE'" >&2
        exit 1
    fi
fi

>&2 echo "[specify] Transitioning from specification to implementation"
>&2 echo "[specify] Specification Branch: $CURRENT_BRANCH"
>&2 echo "[specify] Implementation Branch: $IMPL_BRANCH"
>&2 echo "[specify] Target Layer: $LAYER"
>&2 echo "[specify] Feature Directory: $FEATURE_DIR"

# Verify specification artifacts exist
REQUIRED_FILES=("$FEATURE_DIR/spec.md" "$FEATURE_DIR/plan.md" "$FEATURE_DIR/tasks.md")
MISSING_FILES=()

for file in "${REQUIRED_FILES[@]}"; do
    if [ ! -f "$file" ]; then
        MISSING_FILES+=("$(basename "$file")")
    fi
done

if [ ${#MISSING_FILES[@]} -gt 0 ]; then
    echo "Error: Required specification artifacts missing:" >&2
    for missing in "${MISSING_FILES[@]}"; do
        echo "  - $missing" >&2
    done
    echo "Run /speckit.specify, /speckit.plan, and /speckit.tasks before transitioning" >&2
    exit 1
fi

# Create implementation branch from current spec branch
>&2 echo "[specify] Creating implementation branch from current specification branch..."
git checkout -b "$IMPL_BRANCH"

>&2 echo "[specify] Implementation branch created successfully"
>&2 echo "[specify] Specification artifacts preserved from spec branch"
>&2 echo ""
>&2 echo "[specify] Next steps:"
>&2 echo "[specify]   1. Review tasks.md in $FEATURE_DIR"
>&2 echo "[specify]   2. Run /speckit.implement to begin implementation"
>&2 echo "[specify]   3. Specification branch ($CURRENT_BRANCH) remains for reference"

# Set environment variable
export SPECIFY_FEATURE="$IMPL_BRANCH"

if $JSON_MODE; then
    printf '{"SPEC_BRANCH":"%s","IMPL_BRANCH":"%s","FEATURE_NUM":"%s","LAYER":"%s","FEATURE_DIR":"%s"}\n' \
        "$CURRENT_BRANCH" "$IMPL_BRANCH" "$FEATURE_NUM" "$LAYER" "$FEATURE_DIR"
else
    echo "SPEC_BRANCH: $CURRENT_BRANCH"
    echo "IMPL_BRANCH: $IMPL_BRANCH (current)"
    echo "FEATURE_NUM: $FEATURE_NUM"
    echo "LAYER: $LAYER"
    echo "FEATURE_DIR: $FEATURE_DIR"
    echo "SPECIFY_FEATURE environment variable set to: $IMPL_BRANCH"
fi
