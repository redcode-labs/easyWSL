<h1 align="center"> easyWSL</h1> <br>
<p align="center">
  <a>
    <img src="easyWSL.png" width="450">
  </a>
</p>

<p align="center">
  Create WSL distros based on Docker Images.
</p>

> Made with ❤ by @wrobeljakub and @redcode-labs team.

## What does this project do?

There's a script inside which downloads a .tar or .tar.gz image from Docker Hub. In fact, it can be more than just one .tar/.tar.gz, that's why bsdtar.exe is included in this repo, because it is responsible for 'merging' all .tar/.tar.gz files together. Then, one big .tar/.tar.gz is created (if it's needed, we don't have to do this thing if the image contains just one layer) and can be easily exported as a WSL distro.

## How to use it?

Just go to our release page, download latest release and just run it. Done!

## Command List

`--help` / `-h` -> list all available commands
`--name` / `-n` -> name for your distro
`--distro` / `-d` -> id of the distro in the sources.json file
`--path` / `-p` -> path where you want to place your .vhd (it's basically how WSL distros work, they're .tar/.tar.gz files turned into .vhd with Linux filesystems)