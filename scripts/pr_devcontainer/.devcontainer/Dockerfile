ARG VARIANT=ubuntu-20.04
FROM mcr.microsoft.com/vscode/devcontainers/base:0-${VARIANT}

COPY ./binaries /tmp/binaries

# Install Bicep
RUN \
    # Install bicep CLI
    arch=$(dpkg --print-architecture) \
    && cp /tmp/binaries/bicep-release-linux-$arch /usr/local/bin/bicep \
    && chmod +x /usr/local/bin/bicep \
    # Symbolic link for AzCLI to pick up Bicep binary
    && mkdir -p /home/vscode/.azure/bin \
    && ln -s /usr/local/bin/bicep /home/vscode/.azure/bin/bicep \
    && chown -R vscode /home/vscode/.azure \
    # Move VSCode extension to /usr/local/share
    && cp /tmp/binaries/vscode-bicep.vsix /usr/local/share/vscode-bicep.vsix \
    # Clean up
    && rm -Rf /tmp/binaries