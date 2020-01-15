[![License](https://img.shields.io/badge/Licence-MIT-blue.svg?style=plastic)](https://github.com/Si13n7/CoD2MPWindowed/blob/master/LICENSE.txt)

***

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