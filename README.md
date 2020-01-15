<p align="center"><a href="https://github.com/Si13n7/CoD2MPWindowed/releases/latest"><img src="https://img.shields.io/github/tag/Si13n7/CoD2MPWindowed.svg?style=flat&label=release&logoWidth=14&logo=windows" alt="release"></a> &nbsp; <a href="https://www.microsoft.com/download/details.aspx?id=55170"><img src="https://img.shields.io/badge/platform->=%20v4.7-lightgrey.svg?style=flat&logo=.net&logoColor=white" alt="platform"></a> &nbsp; <a href="https://github.com/Si13n7/CoD2MPWindowed/blob/master/LICENSE.txt"><img src="https://img.shields.io/github/license/Si13n7/CoD2MPWindowed.svg?style=flat" alt="license"></a></p>
<p align="center"><a href="https://github.com/Si13n7/CoD2MPWindowed/commits/master"><img src="https://img.shields.io/github/last-commit/Si13n7/CoD2MPWindowed.svg?style=flat&logo=github&logoColor=white" alt="last-commit"></a> &nbsp; <a href="https://github.com/Si13n7/CoD2MPWindowed/commits/master"><img src="https://img.shields.io/github/commits-since/Si13n7/CoD2MPWindowed/latest.svg?style=flat&logo=github&logoColor=white" alt="commits-since"></a> &nbsp; <a href="https://github.com/Si13n7/CoD2MPWindowed/issues"><img src="https://img.shields.io/github/issues/Si13n7/CoD2MPWindowed.svg?style=flat&logo=github&logoColor=white" alt="issues-open"></a> &nbsp; <a href="https://github.com/Si13n7/CoD2MPWindowed/issues?q=is%3Aissue+is%3Aclosed"><img src="https://img.shields.io/github/issues-closed/Si13n7/CoD2MPWindowed.svg?style=flat&logo=github&logoColor=white" alt="issues-closed"></a></p>
<p align="center"><a href="https://dl.si13n7.de"><img src="https://img.shields.io/website/https/dl.si13n7.de.svg?style=flat&down_color=red&down_message=offline&up_color=limegreen&up_message=online&logo=data%3Aimage%2Fpng%3Bbase64%2CiVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEwSURBVDhPxZJNSgNBEIXnCp5AcCO4CmaTRRaKBhdCFkGCCKLgz2Y2RiQgCiqZzmi3CG4COj0X8ApewSt4Ba%2FQ9leZGpyVG8GComtq3qv3qmeS%2Fw9nikHMd5sVn3bqLx7zom1NcW8z%2F6G9CjoPm722rPEv45EJ21vD0O30AvX12IWDvTRsrPXrnjPlUYO0u3McVpZXhch5cnguZ7vVDWfpjRAZgPqc%2BIMEgKQe9Pfr0xn%2FBqZJjAUNQKilp5cC1gHYYz8Usc3OQsTz9HZWK5BMJwFDwrbWbuIXhfhg%2FDpWuE2mK5lEgQtiz4baU14u3V09i5peiipy6qVAxFWtZiflJiq8AAiIZx1CnxpStGmEpEHDZf4r2pUd%2BMjYxomoxJofo4L%2FHqyR57OF6vEvIkm%2BAYRc%2BWd4P97CAAAAAElFTkSuQmCC" alt="website"></a> &nbsp; <a href="https://dl.si13n7.com"><img src="https://img.shields.io/website/https/dl.si13n7.com.svg?style=flat&down_color=red&down_message=offline&label=mirror&up_color=limegreen&up_message=online&logo=data%3Aimage%2Fpng%3Bbase64%2CiVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEwSURBVDhPxZJNSgNBEIXnCp5AcCO4CmaTRRaKBhdCFkGCCKLgz2Y2RiQgCiqZzmi3CG4COj0X8ApewSt4Ba%2FQ9leZGpyVG8GComtq3qv3qmeS%2Fw9nikHMd5sVn3bqLx7zom1NcW8z%2F6G9CjoPm722rPEv45EJ21vD0O30AvX12IWDvTRsrPXrnjPlUYO0u3McVpZXhch5cnguZ7vVDWfpjRAZgPqc%2BIMEgKQe9Pfr0xn%2FBqZJjAUNQKilp5cC1gHYYz8Usc3OQsTz9HZWK5BMJwFDwrbWbuIXhfhg%2FDpWuE2mK5lEgQtiz4baU14u3V09i5peiipy6qVAxFWtZiflJiq8AAiIZx1CnxpStGmEpEHDZf4r2pUd%2BMjYxomoxJofo4L%2FHqyR57OF6vEvIkm%2BAYRc%2BWd4P97CAAAAAElFTkSuQmCC" alt="mirror"></a></p>

# [Call of Duty 2 Multiplayer (Windowed)](https://www.si13n7.com/Downloads/Gaming%20Tools/Call%20of%20Duty%202%20Windowed/)

A friend told me about the missing windowed mode for Call of Duty 2 and that he tries to fix it with reverse engineering.

It was a good reason to check out what we can do. I'm always interested in finding solutions... even for a creepy old game...

<h1 align="center"><sub><img  src="https://raw.githubusercontent.com/Si13n7/CoD2MPWindowed/master/PREVIEW.png"></sub></h1>


## How does it work?

~~It's very simple for Windows 10. I was really wondering but it's enough to create a form application and~~ change the parent of the game. Then you only have to handle the resize functions for great flexibility.

~~The fix is similiar for older Windows versions.~~ But before we can do that we have to change 2 bytes in 'gfx_d3d_mp_x86_s.dll' to force windowed mode.

To prevent md5 mismatch issues in PunkBuster it creates copies for 'CoD2MP_s.exe' and 'gfx_d3d_mp_x86_s.dll' for patching. The copied 'CoD2MP_s.exe' gets a patch to load the patched copy of 'gfx_d3d_mp_x86_s.dll' instead of the original.


### Requirements:
- Microsoft Windows 7 or higher
- [Microsoft .NET Framework Version 4.7+](https://www.microsoft.com/download/details.aspx?id=55170)
- [Call of Duty 2](https://en.wikipedia.org/wiki/Call_of_Duty_2)

### Requirements (Developer):
- [Microsoft Windows 10 (64-bit recommended)](https://www.microsoft.com/software-download/windows10)
   - _Microsoft Windows 7, 8 and 8.1 should work well, but is no longer tested_
- [Microsoft Visual Studio 2019 + .NET Framework 4.7.2 SDK](https://www.visualstudio.com/downloads/)
- [Si13n7 Dev.™ CSharp Library Binaries](https://github.com/Si13n7/SilDev.CSharpLib/)

## Would you like to help?

- [Star this Project](https://github.com/Si13n7/CoD2MPWindowed/stargazers) :star: and show me that this project interests you :hugs:
- [Open an Issue](https://github.com/Si13n7/CoD2MPWindowed/issues/new) :coffee: to give me your feedback and tell me your ideas and wishes for the future :sunglasses:
- [Open a Ticket](https://support.si13n7.de/) :mailbox: if you don't have a GitHub account, you can contact me directly on my website :wink:
- [Donate by PayPal](http://donate.si13n7.com/) :money_with_wings: to buy me some cookies :cookie: