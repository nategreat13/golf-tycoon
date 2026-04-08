# Golf Tycoon - Windows Setup Guide

This guide will walk you through everything you need to get the game running. No developer experience required!

## Step 1: Install Git

1. Go to https://git-scm.com/download/win
2. Download and run the installer
3. Use all the default settings — just keep clicking **Next**, then **Install**
4. When it's done, open **Git Bash** from your Start menu to make sure it works

## Step 2: Download the Project

1. In Git Bash, paste this and press Enter:
   ```
   cd /c/Users/$USER/Desktop
   ```
2. Then paste this and press Enter:
   ```
   git clone https://github.com/nategreat13/golf-tycoon.git
   ```
3. Wait for the download to complete. You'll see a `golf-tycoon` folder on your Desktop.

## Step 3: Install Unity Hub

1. Go to https://unity.com/download
2. Click **Download Unity Hub**
3. Run the downloaded installer and follow the prompts
4. Open Unity Hub
5. Create a free Unity account (or sign in if you have one)

## Step 4: Install the Correct Unity Version

The project requires **Unity 6 (version 6000.4.1f1)** — it won't work with other versions.

1. In Unity Hub, click **Installs** on the left sidebar
2. Click **Install Editor**
3. Look for version **6000.4.1f1**
   - If you don't see it, click **Archive** at the top, which will take you to Unity's download archive — find 6000.4.1f1 there
4. When selecting modules, check:
   - **Android Build Support** (check both "Android SDK & NDK Tools" and "OpenJDK")
5. Click **Install** and wait — this takes a while (several GB download)

## Step 5: Open the Project in Unity

1. In Unity Hub, click **Projects** on the left sidebar
2. Click **Add** and navigate to `Desktop > golf-tycoon`
3. Select the folder and click **Open**
4. Unity will import everything — **this takes several minutes the first time**. Just let it run.
5. If Unity asks about enabling the new Input System, click **Yes** and let it restart

## Step 6: Run the Game

1. In the **Project** panel (bottom of the Unity window), navigate to **Assets > Scenes**
2. Double-click **Boot.unity**
3. Go to the top menu: **Golf Tycoon > Setup Entire Project** (this creates all the game assets)
4. Click the **Play** button (triangle at the top center) to run the game!

## Getting Updates

When I push new changes, you just need to pull them:

1. Open Git Bash
2. Run:
   ```
   cd /c/Users/$USER/Desktop/golf-tycoon
   git pull
   ```
3. Switch back to Unity — it will re-import any changed files automatically

## Troubleshooting

**Unity says the version doesn't match** — Make sure you installed exactly **6000.4.1f1**. Unity Hub should offer to install it for you.

**Errors in the Console** — Go to **Golf Tycoon > Setup Entire Project** in the top menu to regenerate everything.

**Green screen but no text** — TextMeshPro might not be imported. Unity usually prompts you; if not, try **Golf Tycoon > Setup Entire Project** again.

**"Unsafe code" error** — Go to **Edit > Project Settings > Player > Other Settings** and check **Allow 'unsafe' Code**.
