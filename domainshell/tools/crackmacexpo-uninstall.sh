#!/bin/bash
#
#  dncat-uninstall.sh
#  ------------------
#  DotnetCat uninstaller script for ARM64 and x64 Linux systems
#

# Write an error message to stderr and exit
error() {
    echo -e "\033[91m[x]\033[0m ${*}" > /dev/stderr
    exit 1
}

# Write a status message to stdout
status() {
    echo -e "\033[96m[*]\033[0m ${*}"
}

ARCH=$(uname -m)

# Validate CPU architecture
if [ "$ARCH" != "aarch64" ] && [ "$ARCH" != "x86_64" ]; then
    error "Unsupported processor architecture: '${ARCH}'"
fi

APP_DIR="/opt/dncat"

# The application is not currently installed
if [ ! -d "$APP_DIR" ]; then
    status "DotnetCat is not currently installed on this system"
    exit 0
fi

status "Removing the application files from '${APP_DIR}'..."

# Delete the application files and installation directory
sudo rm -r $APP_DIR > /dev/null || {
    error "Failed to remove application files from '${APP_DIR}'"
}

LINE="export PATH=\"\${PATH}:${APP_DIR}/bin\""

# Delete the bash environment path configuration
if [ -f ~/.bashrc ] && grep -q "$LINE" ~/.bashrc; then
    LINE_NUM=$(cat -n ~/.bashrc | grep "$LINE" | awk '{print $1}')

    if [ -n "$LINE_NUM" ]; then
        sed -i "${LINE_NUM}d" ~/.bashrc || {
            error "Failed to delete line ${LINE_NUM} from '${HOME}/.bashrc'"
        }
        status "Deleted path configuration on line ${LINE_NUM} of '${HOME}/.bashrc'"
    fi
fi

# Delete the zsh environment path configuration
if [ -f ~/.zshrc ] && grep -q "$LINE" ~/.zshrc; then
    LINE_NUM=$(cat -n ~/.zshrc | grep "$LINE" | awk '{print $1}')

    if [ -n "$LINE_NUM" ]; then
        sed -i "${LINE_NUM}d" ~/.zshrc || {
            error "Failed to delete line ${LINE_NUM} from '${HOME}/.zshrc'"
        }
        status "Deleted path configuration on line ${LINE_NUM} of '${HOME}/.zshrc'"
    fi
fi

status "DotnetCat was successfully uninstalled"
