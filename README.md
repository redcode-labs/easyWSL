# easyWSL - have your own custom WSL distro easily

This is a tutorial how to easily have custom WSL distro, for example Arch Linux.
Change distro and directories if you want.

> I recommend using Windows Terminal, remember to use preview version of App Installer in order to use `winget` functionality.

1. Change your ExecutionPolicy.
   1. Open Powershell as an administrator.
   2. `Set-ExecutionPolicy Unrestricted`
2. Clone and go to the dir. of this repository.
   1. `git clone https://github.com/Unrooted/easyWSL`
   2. `cd easyWSL`
3. Run Powershell script. Click `Yes` and install Docker just by doing what an installator says.
   1. `&"C:\PATH\TO\CLONED\REPO\easyWSL\easyWSLPowershellPart.ps1"`
4. If your Docker container boots up, run:
   1. `cd && curl https://github.com/Unrooted/easyWSL/easyWSLLinuxPart.sh && ./easyWSLLinuxPart.sh`
5. Docker container will exit itself. Let the Powershell finish the job.

> You can also run first three steps commands in one line, but just to have a clarity in this README I decided to put them separatly. 

If you want to run your freshly created WSL, just go to your Powershell and type `wsl -d wslarchlinux` and enjoy it!