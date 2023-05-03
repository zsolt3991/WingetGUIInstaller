# Winget-GUI-Installer

## Intro

GUI to search and install Apps at once with Winget package manager, with single click Update.

## Installation and Usage

1. Download [latest](https://github.com/zsolt3991/WingetGUIInstaller/releases/) ` WingetGUIInstaller_0.2.6.3_x64.msix` (As WingetGUIInstaller is self signed, we need to install the Certificate manually)
2. Install certificate: [stackoveflow](https://stackoverflow.com/a/24372483)
3. Search for an app, or find one from the Recommended list
4. If you encounter any bugs or have any feedbacks, please ceate an [issue](https://github.com/zsolt3991/WingetGUIInstaller/issues/new/choose). You can also file a bug from the app itself.

## Requirements

The [system requirements](https://drew-naylor.com/guinget/system-requirements) include .NET Framework 4.8 for version 0.1.2 and newer (previous versions require at least .NET Framework 4.6.1) as mentioned above and [winget](https://github.com/microsoft/winget-cli/releases/latest), but otherwise they don't require all that much power. 

Faster computers will perform better with few (if any) lock-ups when it comes to extracting and loading the package list, though. One goal is to eventually eliminate or reduce that lock-up as much as possible, which will require async loading. Not sure how to reliably do it with a datagridview yet, though.

## Screenshots
![Recommended](https://user-images.githubusercontent.com/37495396/235842010-1fe2adf3-f098-49f4-ad38-10af6d25d020.png)
![updates](https://user-images.githubusercontent.com/37495396/235842013-f1a01680-5bd0-47d4-8341-96aa996de597.png)


## TODO

- [x] Search for available updates
- [x] Install updates
- [x] Search for new packages
- [x] Tested on Windows 11
- [x] Install new packages
- [x] Build pipeline
- [ ] Add tests
- [ ] Installer
- [ ] Publish
