[![License](https://img.shields.io/badge/Licence-MIT-blue.svg?style=plastic)](https://github.com/Si13n7/CoD2MPWindowed/blob/master/LICENSE.txt)

***

#CALL OF DUTY 2 WINDOWED

A friend told me about the missing windowed mode for Call of Duty 2 and that he tries to fix it with reverse engineering.

It was a good reason to check out what we can do. I'm always interested in finding solutions... even for a creepy old game...

<h1 align="center"><sub><img  src="https://raw.githubusercontent.com/Si13n7/CoD2MPWindowed/master/PREVIEW.png"></sub></h1>


##How does it work?

It's very simple for Windows 10. I was really wondering but it's enough to create a form application and change the parent of the game. Then you only have to handle the resize functions for great flexibility.

The fix is similiar for older Windows versions. But before we can do that we have to change 2 bytes in 'gfx_d3d_mp_x86_s.dll' to force windowed mode.

To prevent md5 mismatch issues in PunkBuster it creates copies for 'CoD2MP_s.exe' and 'gfx_d3d_mp_x86_s.dll' for patching. The copied 'CoD2MP_s.exe' gets a patch to load the patched copy of 'gfx_d3d_mp_x86_s.dll' instead of the original.

That's the trick for PunkBuster... Looks like the best anti cheat protection ever! Do you understand sarcasm? If not: PunkBuster is no anti cheat protection it's only a simulator for ingame lags!


##Requirements:
- Microsoft Windows Vista or higher
- Microsoft .NET Framework Version 4.5+


<i>Would you like to help support me? [Donate by PayPal](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=GSCTSX46UPCDW) or [Report a Problem](https://support.si13n7.com/)! Please also feel free to report suggestions as well!</i>