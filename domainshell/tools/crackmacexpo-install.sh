#!/bin/bash
#
#  dncat-install.sh
#  ----------------
#  DotnetCat installer script for ARM64 and x64 Linux systems
#
ORIG_DIR=$PWD

# Write an error message to stderr and exit
error() {
    echo -e "\033[91m[x]\033[0m ${*}" > /dev/stderr
    cd "$ORIG_DIR" && exit 1 || exit 1
}

# Write a status message to stdout
status() {
    echo -e "\033[96m[*]\033[0m ${*}"
}

# Check that 'curl' is installed
if ! command -v curl &> /dev/null; then
    error "Missing installer dependency 'curl'"
fi

# Check that 'unzip' is installed
if ! command -v unzip &> /dev/null; then
    error "Missing installer dependency 'unzip'"
fi

ZIP_URL=
ARCH=$(uname -m)

REPO_ROOT="https://raw.githubusercontent.com/vandavey/DotnetCat/master"

# Assign zip file URL and validate CPU architecture
if [ "$ARCH" == "aarch64" ]; then
    ZIP_URL="${REPO_ROOT}/src/DotnetCat/bin/Zips/DotnetCat_linux-arm64.zip"
elif [ "$ARCH" == "x86_64" ]; then
    ZIP_URL="${REPO_ROOT}/src/DotnetCat/bin/Zips/DotnetCat_linux-x64.zip"
else
    error "Unsupported processor architecture: '${ARCH}'"
fi

APP_DIR="/opt/dncat"
BIN_DIR="${APP_DIR}/bin"
SHARE_DIR="${APP_DIR}/share"
ZIP_PATH="${APP_DIR}/dncat.zip"

# Remove the existing installation
if [ -d $APP_DIR ]; then
    status "Removing existing installation from '${APP_DIR}'..."

    sudo rm -r $APP_DIR > /dev/null || {
        error "Failed to remove existing installation from '${APP_DIR}'"
    }
fi

# Create the installation directory
sudo mkdir -p $APP_DIR $BIN_DIR $SHARE_DIR > /dev/null || {
    error "Failed to create one or more directories in '${APP_DIR}'"
}

HTTP_STATUS=$(curl -sLSw "%{http_code}" $ZIP_URL -o /dev/null)

# Failed to communicate with the repository server
if [ "$HTTP_STATUS" -ne 200 ]; then
    error "Unable to download zip file: HTTP ${HTTP_STATUS}"
fi

status "Downloading temporary zip file to '${ZIP_PATH}'..."

# Download the temporary application zip file
sudo curl -sLS $ZIP_URL -o $ZIP_PATH > /dev/null || {
    error "Failed to download zip file from '${ZIP_URL}'"
}

# Navigate to installation directory before unpacking zip file
cd "$APP_DIR" || error "An unexpected error occurred"

status "Unpacking '${ZIP_PATH}' contents to '${APP_DIR}'..."

# Unpack the temporary zip file contents
sudo unzip -j $ZIP_PATH > /dev/null || {
    error "Failed to unpack contents of zip file '${ZIP_PATH}'"
}

status "Deleting temporary zip file '${ZIP_PATH}'..."

# Remove the temporary zip file
sudo rm $ZIP_PATH > /dev/null || {
    error "Failed to remove temporary zip file '${ZIP_PATH}'"
}

status "Installing the application files to '${APP_DIR}'..."

# Move markdown files into the share directory
sudo mv $APP_DIR/*.md $SHARE_DIR > /dev/null || {
    error "Failed to restructure application files in '${APP_DIR}'"
}

# Move executables into the bin directory
sudo mv $APP_DIR/dncat $APP_DIR/*.sh $BIN_DIR > /dev/null || {
    error "Failed to restructure application files in '${APP_DIR}'"
}

# Enable execute permissions on all executable files
sudo chmod +x $BIN_DIR/* > /dev/null || {
    error "Failed to enable execute permissions"
}

LINE="export PATH=\"\${PATH}:${BIN_DIR}\""

# Create a bash configuration file
if [ ! -f ~/.bashrc ] && [ ! -f ~/.zshrc ]; then
    status "Creating bash configuration file '${HOME}/.bashrc'..."
    touch ~/.bashrc
fi

# Add a bash environment path configuration
if [ -f ~/.bashrc ] && ! grep -q "$LINE" ~/.bashrc; then
    echo -e "\n${LINE}" | sudo tee -a ~/.bashrc > /dev/null
    status "Updated environment path configuration in '${HOME}/.bashrc'"
fi

# Add a zsh environment path configuration
if [ -f ~/.zshrc ] && ! grep -q "$LINE" ~/.zshrc; then
    echo -e "\n${LINE}" | sudo tee -a ~/.zshrc > /dev/null
    status "Updated environment path configuration in '${HOME}/.zshrc'"
fi

status "DotnetCat was successfully installed, please restart your shell"
cd "$ORIG_DIR" || exit 1
