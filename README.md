# Tarkov AprilOx Monitor

**Unofficial fork of [the-hideout/TarkovMonitor](https://github.com/the-hideout/TarkovMonitor). It is not affiliated with, endorsed by or supported by the-hideout or tarkov.dev.** Please do not report problems of this fork to the original project; use the [issues of this repository](https://github.com/ez4ence256/TarkovAprilOxMonitor/issues).

Tarkov AprilOx Monitor keeps every feature of TarkovMonitor (audio notifications, TarkovTracker sync, tarkov.dev remote, timers, groups...) and additionally sends your quest and position events to [Tarkov Quest Planner](https://tarkov.aprilox.fr).

Original authors and contributors: the-hideout and the TarkovMonitor contributors. All original copyright notices are kept.

## License

This program is free software licensed under the GNU General Public License v3.0 (see [LICENSE.txt](LICENSE.txt)). It comes with ABSOLUTELY NO WARRANTY. You may redistribute and modify it under the terms of the GPL-3.0; modified versions must stay under the same license and ship their source.

## Modifications (2026-10-20)

Changes made to the original TarkovMonitor (GPL-3.0 section 5a):

- Renamed the product and the assembly to `TarkovAprilOxMonitor` (window and tray titles, executable name, release archive `TarkovAprilOxMonitor.zip`). The C# root namespace stays `TarkovMonitor` to ease merging upstream.
- The update checker and the in-app updater now look at the releases of `ez4ence256/TarkovAprilOxMonitor` instead of `the-hideout/TarkovMonitor`; the "Report" button opens this fork's issue tracker.
- HTTP User-Agent is now `TarkovAprilOxMonitor/<version>` (derived from the assembly name); tarkov.dev calls are unchanged.
- Local diagnostics now live in `%LOCALAPPDATA%\TarkovAprilOxMonitor\Diagnostics`. The application has no telemetry: diagnostics are only written locally and copied by the user.
- New settings `aprilOxEnabled`, `aprilOxSiteUrl`, `aprilOxCode` and a "Tarkov Quest Planner (AprilOx)" card in Settings (enable switch, site URL, pairing code with Copy / Regenerate, "Open connected site").
- New `AprilOxClient` that POSTs task (started / finished / failed), map and position events, plus a `hello` event, to `{siteUrl}/api/monitor/{code}`. Events are sent only for live game activity (never while old logs are read at startup) and only when the game mode (regular or PVE) is known.
- The "Read past log files" dialog can now send the past quest statuses to Tarkov Quest Planner (default when connected, no TarkovTracker token needed) as one batched `tasks` event; the TarkovTracker.org target is unchanged.
- New setting `aprilOxDeleteScreenshots` (on by default): screenshots taken while the app is running, in the EFT screenshots folder and matching EFT's file name pattern, are sent to the Recycle Bin 30 minutes after creation. Pending deletions are kept in memory only, so screenshots still waiting when the app closes are not deleted.
- GitHub workflows and this README adapted to the new names.

## Connect to Tarkov Quest Planner

1. Start Tarkov AprilOx Monitor and open **Settings**, card **Tarkov Quest Planner (AprilOx)**.
2. Click **Open connected site**; the site asks you to confirm the pairing code. Or copy the pairing code with the copy button and enter it on the site.
3. Play: started / finished / failed quests, the map you load into and your position (take an in-game screenshot) show up on the site in real time.

The pairing code is the only secret protecting your feed: do not share it. Use **Regenerate code** if it leaks.

## Building

Requires the .NET 10 SDK on Windows.

```
dotnet build TarkovMonitor/TarkovMonitor.csproj -c Release
```

The executable is written to `TarkovMonitor/bin/Release/net10.0-windows10.0.17763.0/TarkovAprilOxMonitor.exe`. The `TarkovMonitor.Package` MSIX project keeps the original publisher identity and is not part of this fork's build.

---

The text below comes from the original README.

## Features

- Audio notifications
    - Match found
    - Raid starting
    - Restart failed tasks
    - Runthrough time elapsed
    - Turn air filter on/off
    - Scav cooldown
    - Quest Items Reminder (does not check for actual quest items, just a friendly reminder to allow you to back out of matching)
    - Customizable sounds for all of the above
- Goon tracking
    - Submit reports when you see the Goons to help other players find them
- Connect to the Tarkov.dev website via remote code
    - Automatically load the website map for the map you're playing on
    - Take an in-game screenshot and show your position on the website map
- Connect to Tarkov Tracker via API token
    - Automatically mark quests as complete as you complete them
- Statistics (all stored locally on your computer)
    - Track your total sales on the flea market
    - Track how many times you play on each map
- Visual Timers (have that friend that never heard the audio and asks "has the runthrough timer happened yet?")
   - Displays "Time in Raid"
   - Displays countdown for "Runthrough time"
   - Display countdown for Scav cooldown time

## Installation

Head on over to the [latest release](https://github.com/ez4ence256/TarkovAprilOxMonitor/releases/latest) page for this project. Once you are there, you'll see an `Assets` section and you'll want to download the `TarkovAprilOxMonitor.zip` link:

<img width="845" alt="Screenshot 2023-08-10 at 7 58 36 PM" src="https://github.com/the-hideout/TarkovMonitor/assets/23362539/86fbb000-25a3-4d71-bf39-45d622d61e8e">

Once downloaded, extract the zip and run the `TarkovAprilOxMonitor.exe` executable included within the bundle. Enjoy!

## Setup

On its own, TarkovMonitor will play audio notifications (e.g., when you match into a raid and when the raid begins). But its most useful features are unlocked when used in conjunction with other tools.

### Quest Tracking with TarkovTracker

[Tarkov Tracker](https://tarkovtracker.org) is a free website that allows you to track your quest progress. Once you log in to create a Tarkov Tracker account, you can share your quest progress with other tools (including TarkovMonitor) by creating an API token. Navigate to the [Tarkov Tracker settings page](https://tarkovtracker.org/settings), click the `create a token` button, and create a token that has permissions to `get progression` and `write progression`. You can give the token any name you want, but if you're creating it for Tarkov Monitor, it makes sense to name it `Tarkov Monitor`. Then click the `create token` button and click the token's copy button. Do not try to manually highlight the displayed token and copy it; some of the displayed token's characters are obfuscated with asterisks (*). Once you've copied the token, paste it in the Tarkov Tracker API token box in Tarkov Monitor settings and click the `Test Token` button. If you see a pop up indicating success, Tarkov Tracker is ready to start automatically updating your progress on Tarkov Tracker.

Tarkov Tracker only automatically marks a quest as complete if it's running when the quest is completed. If you already completed a bunch of quests prior to running Tarkov Monitor, see [the below section on how to read past progress](#ive-installed-and-run-tarkovmonitor-why-hasnt-it-marked-all-my-completed-quests-as-complete).

### Tarkov.dev Website Integration

The [Tarkov.dev website](https://tarkov.dev) has a "remote control" feature that allows the user to navigate to different pages in a browser window by using a different device. The original use case for this was to have the Tarkov.dev website open on a second monitor as you're playing the game and then using your cellphone as the remote control to load different pages on the website shown on the monitor without having to alt+tab out of the game.

TarkovMonitor can act as the "control" device, which allows it to do things like opening the corresponding map page on the website when you're loading into a raid and show your position (and rotation) on the map when you take a screenshot. To enable this integration, open the Tarkov.dev website, click the `Click to connect` button in the lower left, copy the `ID for remote control` shown in that box, and paste it in the Tarkov Monitor remote id settings. If you keep your browser window open, Tarkov Monitor should be set to control the Tarkov.dev site. Note that if you reload the Tarkov.dev site (including by restarting your browser), you'll need to click the `Click to connect` button again, but the remote code should remain the same.

## FAQ

### How does TarkovMonitor work?

TarkovMonitor simply watches the log files that the game creates as it's running. Certain log messages correspond with particular events, so it's possible to automatically read some game events from these log files.

### I've installed and run TarkovMonitor, why hasn't it marked all my completed quests as complete?

TarkovMonitor only monitors new logs as they are being written while the app is running. Therefore, it doesn't automatically update quest progress that was made prior to the app running. It will, however, still mark quests as complete going forward while the app is running.

If you want to automatically update your progress from previous logs, open the Settings page, scroll down to the Initial Setup section, and click the Read Past Logs button. Tarkov Monitor will then present you with a list of breakpoints to choose the starting point to read logs from. The breakpoints are determined by the game's version number and your player profile id as written into each set of logs. Select the breakpoint corresponding with the start of the wipe for the correct account, click OK, and Tarkov Monitor will process all logs starting from that point through the present for the selected profile and update your quest progress accordingly.

### Is TarkovMonitor a cheat?

We don't have any official word from BSG, but it would be silly for TarkovMonitor to be considered a cheat. It doesn't do anything while players are in-raid because the logs aren't updated while a raid is in-progress. Moreover, the application is simply reading the logs that are written to your computer.

### Can TarkovMonitor update my hideout build progress on Tarkov Tracker?

Unfortunately, there are no log events for when you build hideout stations, so TarkovMonitor cannot automatically mark them as built.

### Does TarkovMonitor update my PMC level on Tarkov Tracker?

PMC level information is not logged by the game, so Tarkov Monitor cannot update it in Tarkov Tracker.

### What is the "Tarkov.dev Website Remote" option for?

The Tarkov.dev website has a feature that allows the user to "control" the website using another device. The typical use case is for someone to have the Tarkov.dev website loaded in a browser on their second monitor and then use their phone as the second device to load pages on the website without having to alt+tab out of the game. TarkovMonitor can act as the remote device and do things like load the Tarkov.dev map page for the map you're loading into a raid on. Linking the remote also enables showing your position on the Tarkov.dev map when you take a screenshot. You have two options for connecting Tarkov Monitor to the tarkov.dev website: 
- If you don't have any other device acting as the remote for the tarkov.dev website, you can simply click the "connect" button in Tarkov Monitor settings to generate a remote code, launch the tarkov.dev website in a browser, and connect the tarkov.dev website to that remote code.
- If you want to use the remote code already generated by the tarkov.dev website, just open the Tarkov.dev website in your browser, click the "Click to connect" box in the lower left, and then copy and paste that code into the Remote ID setting box in Tarkov Monitor. You can click on the code to copy it to your clipboard.

### What is the "Submit Queue Time Data" option for?

When enabled, TarkovMonitor will submit the amount of time it takes to queue for a raid to Tarkov.dev. The information is sent anonymously and only the following pieces of information are sent and saved: the map, the time it took to find a raid, and whether the raid was for PMC or scav.

## Trouble Shooting

If the app won't launch and you see an "invalid WebView2 installation" error (sometimes accompanied by "The system cannot find the file specified"), you're likely hitting a known WebView2 issue.

Follow the solution here: [Error on launching program (invalid WebView2 installation) #22 — solution comment](https://github.com/the-hideout/TarkovMonitor/issues/22#issuecomment-3443766675)
