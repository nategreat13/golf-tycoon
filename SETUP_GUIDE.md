# Golf Tycoon - Setup Guide

This guide will walk you through everything you need to install and do to get the project running on your machine. No developer experience required!

## Step 1: Install Unity Hub

1. Go to https://unity.com/download
2. Click **Download Unity Hub**
3. Run the installer and follow the prompts
4. Open Unity Hub and create a free Unity account (or sign in if you have one)

## Step 2: Install the Correct Unity Version

The project uses **Unity 6 (version 6000.4.1f1)**. You need this exact version.

1. In Unity Hub, click **Installs** on the left sidebar
2. Click **Install Editor**
3. Look for version **6000.4.1f1** in the list
   - If you don't see it, click **Archive** at the top, then find it on the Unity download archive page
4. When selecting modules, make sure these are checked:
   - **iOS Build Support** (if you have a Mac)
   - **Android Build Support** (check both "Android SDK & NDK Tools" and "OpenJDK")
5. Click **Install** and wait for it to finish (this can take a while)

## Step 3: Install Git

You need Git to download the project code.

### On Mac:
1. Open **Terminal** (search for it in Spotlight with Cmd+Space)
2. Type `git --version` and press Enter
3. If Git isn't installed, macOS will prompt you to install the Command Line Tools - click **Install**
4. Wait for the installation to complete

### On Windows:
1. Go to https://git-scm.com/download/win
2. Download and run the installer
3. Use all the default settings, just keep clicking **Next** then **Install**
4. After install, open **Git Bash** from your Start menu to verify it works

## Step 4: Download the Project

1. Open **Terminal** (Mac) or **Git Bash** (Windows)
2. Navigate to where you want the project. For example:
   ```
   cd ~/Desktop
   ```
3. Clone the repository:
   ```
   git clone https://github.com/nategreat13/golf-tycoon.git
   ```
4. Wait for the download to complete

## Step 5: Open the Project in Unity

1. Open **Unity Hub**
2. Click **Projects** on the left sidebar
3. Click **Add** (or **Open**) and navigate to the `golf-tycoon` folder you just downloaded
4. Select the folder and click **Open**
5. Unity will import the project - **this will take several minutes the first time** (it's processing all the assets). Be patient!
6. If Unity asks about enabling the new Input System, click **Yes** and let it restart

## Step 6: Run the Game

1. Once the project is open in Unity, go to the **Project** panel (usually at the bottom)
2. Navigate to **Assets > Scenes**
3. Double-click **Boot.unity** to open the boot scene
4. If the scenes aren't set up yet, go to the top menu bar: **Golf Tycoon > Setup Entire Project**
5. Click the **Play** button (triangle icon at the top center of Unity) to run the game!

## Troubleshooting

### "Unity version not found"
Make sure you installed exactly **6000.4.1f1**. Unity Hub may offer to install the correct version when you try to open the project.

### "Missing scripts" or errors in Console
Go to **Golf Tycoon > Setup Entire Project** in the top menu to regenerate all the scenes and assets.

### UI text is invisible
This is a known issue. If you see a green screen but no text:
1. Make sure you have TextMeshPro imported (Unity should prompt you automatically)
2. Try running **Golf Tycoon > Setup Entire Project** again

### "Unsafe code not allowed" error
1. Go to **Edit > Project Settings > Player**
2. Under **Other Settings**, check **Allow 'unsafe' Code**

### General issues
- Make sure you're opening **Boot.unity**, not another scene
- Check the **Console** window (Window > General > Console) for red error messages
- Try closing and reopening Unity

## Pulling Updates

When new changes are pushed to the project:

1. Open **Terminal** (Mac) or **Git Bash** (Windows)
2. Navigate to the project folder:
   ```
   cd ~/Desktop/golf-tycoon
   ```
3. Pull the latest changes:
   ```
   git pull
   ```
4. Open the project in Unity again - it will re-import any changed files
