Import-Module PSWorkflow
function Show-Menu {
    Clear-Host
    Write-Host "                          _       _______ __ "
    Write-Host "    ___  ____ ________  _| |     / / ___// / "
    Write-Host "   / _ \/ __ `/ ___/  / / / | /| / /\__ \/ /  "
    Write-Host "  /  __/ /_/ (__  ) /_/ /| |/ |/ /___/ / /___"
    Write-Host "  \___/\__,_/____/\__, / |__/|__//____/_____/"
    Write-Host "                 /____/                      "

    Write-Host "1: Install Distro"
    Write-Host "2: Show currently installed distros"
    Write-Host "3: Unregister Distro"
    Write-Host "H: Help"
    Write-Host "Q: Quit"
}

Show-Menu
$selection = Read-Host "Choose what you want to do"
switch ($selection) {

    "1" {
        $DistroName = Read-Host "Name of your distro"
        if ($DistroName) {
            Write-Host "You've choosen [$DistroName] as the name for your distro"
        } else {
            Write-Warning -Message "No name input."
        }
        $DockerContainer = Read-Host "Docker Hub repo name (for example: archlinux:latest)"
        if ($DockerContainer) {
            Write-Host "You've choosen [$DockerContainer] as your Docker container"
        } else {
            Write-Warning -Message "No Docker container name input."
        }
    }

    "2" {
        wsl.exe -l -v
    }

    "3" {
        wsl.exe -l
        Read-Host "Name of your distro that you want to unregister"
        if ($UnregisterDistro) {
            Write-Host "You've choosen [$UnregisterDistro] as the name for your distro"
        } else {
            Write-Warning -Message "No name input."
        }
    }

    "H" {
        Start-Process "https://github.com/redcode-labs/easyWSL/blob/master/README.md"
        exit
    }

    "q" {
        exit
    }
    "Q" {
        exit
    }
    default {
        Write-Host "Not a correct command"
        Show-Menu
        $selection = Read-Host "Choose what you want to do"
        switch ($selection) {

            "1" {
                $DistroName = Read-Host "Name of your distro"
                if ($DistroName) {
                    Write-Host "You've choosen [$DistroName] as the name for your distro"
                } else {
                    Write-Warning -Message "No name input."
                }
                $DockerContainer = Read-Host "Docker Hub repo name (for example: archlinux:latest)"
                if ($DockerContainer) {
                    Write-Host "You've choosen [$DockerContainer] as your Docker container"
                } else {
                    Write-Warning -Message "No Docker container name input."
                }
            }

            "2" {
                wsl.exe -l -v
            }

            "3" {
                wsl.exe -l
                Read-Host "Name of your distro that you want to unregister"
                if ($UnregisterDistro) {
                    Write-Host "You've choosen [$UnregisterDistro] as the name for your distro"
                } else {
                    Write-Warning -Message "No name input."
                }
            }

            "H" {
                Start-Process "https://github.com/redcode-labs/easyWSL/blob/master/README.md"
                exit
            }

            "q" {
                exit
            }
            "Q" {
                exit
            }
            default {
                exit
            }
        }
    }
}
function ChooseInstallationDirectory {
    do 
    {
        $script:DistroInstallationDirectory = Read-Host -Prompt "Type the path to directory where you want to store all your custom made distros "
        if (($script:DistroInstallationDirectory.Substring($script:DistroInstallationDirectory.Length-1) -eq "\")) {
            $script:DistroInstallationDirectory = $script:DistroInstallationDirectory.Substring(0,$script:DistroInstallationDirectory.Length-1)            
        }

        $script:DistroDir =  $script:DistroInstallationDirectory+"\"+$script:DistroName
        if (-Not (Test-Path -Path $script:DistroInstallationDirectory)) {write-Host "This is not a correct path!"}
    } while (-Not (Test-Path -Path $script:DistroInstallationDirectory))

}


ChooseInstallationDirectory


$confirmation = Read-Host "Are you sure you want to install" $DistroName "to path" $DistroDir "[y/n]"
while($confirmation -eq "n")
{
    if ($confirmation -eq 'y') {break}
    ChooseInstallationDirectory
    $confirmation = Read-Host "Are you sure you want to install" $DistroName "to path" $DistroDir"? [y/n]"
}

$confirmation = Read-Host "Do you want to set" $DistroName "as a default wsl distro? [y/n]"
if ($confirmation -eq 'y') {
    $DefaultDistro = "true"
}
else {
    $DefaultDistro = "false"
}


$DockerWingetPackageName = "Docker.DockerDesktop"
$DockerContainerLocal = "wsl"+$DistroName
$DockerExportTarName = "install.tar"


workflow Install-easyWSL {
    param (
        $DockerWingetPackageName,
        $DistroInstallationDirectory,
        $DistroName,
        $DockerExportTarName,
        $DistroDir,
        $DockerContainerLocal,
        $DockerContainer
    )
    
    Set-ExecutionPolicy Unrestricted

    # check if Docker is installed on a machine, if not install it 
    if (-Not (Get-Command docker -errorAction SilentlyContinue))
    {
        if (Get-Command winget -errorAction SilentlyContinue)
        {
            winget install -e --id Docker.DockerDesktop
        } else {
            Invoke-WebRequest -Uri "https://desktop.docker.com/win/stable/Docker%20Desktop%20Installer.exe" -OutFile $env:USERPROFILE"\Downloads"
            Start-Process -Wait "c:\Users\$env:USERNAME\Downloads\Docker Desktop Installer.exe"
        }
    }

    # ask user to confirm rebooting machine
    $confirmation = Read-Host "Your computer will have to be rebooted. Are you ready to do that now? [y/n]"
    do
    {
        if (-Not ($confirmation -eq 'y')) {
          $confirmation = Read-Host "Maybe now? [y/n]"
        } 
    } while($confirmation -eq "n")

    Restart-Computer -Wait
    
    # create a directory for the distribution inside a user specified directory
    New-Item -Path $DistroInstallationDirectory -Name $DistroName -ItemType "directory"

    # run the docker container with a chosen distro and extract the rootfs to the .tar file located in a distribution directory
    docker run -it --name $DockerContainerLocal $DockerContainer
    docker export --output=$DistroDir+"\"+$DockerExportTarName $DockerContainerLocal

    # register the distribution in wsl
    wsl.exe --import $DistroName $DistroDir $DistroDir+"\"+$DockerExportTarName

    #set the distro default in wsl if the user specified that he want do it
    if($DefaultDistro -eq "true") {
        wsl.exe --set-default $DistroName
    }

    # post setup, linux commands in here are specified to be for Arch-based systems
    wsl.exe --distro $DistroName "pacman -Syu -y && pacman -S sudo -y && pacman -S vim -y && echo export EDITOR=/usr/bin/vim >> ~/.bashrc && pwconv && grpconv && chmod 0744 /etc/shadow && chmod 0744 /etc/gshadow && exit"
}

Install-easyWSL $DockerWingetPackageName $DistroInstallationDirectory $DistroName $DockerExportTarName $DistroDir $DockerContainerLocal $DockerContainer