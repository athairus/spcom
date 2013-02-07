# spcom
spcom stands for spc-o-matic. It was made back in 2007 to correct .it (Impulse Tracker) files that an old DOS program called OpenSPC (SPC files are essentially Super Nintendo music dumps) would output. It was originally programmed in C. I did this C# port in 2012 as an exercise in both the language and in parsing binary files. 

## Features
* Corrects sample pitch (old spc players would play songs with the incorrect pitch, this limitation found its way into the it rips OpenSPC does)
* Mixes 16-channel songs into 8 (algorithm prefers to keep pitch shifting commands at the expense of pan commands)