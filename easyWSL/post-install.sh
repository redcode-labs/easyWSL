echo "Upgrading the system ..."
if command -v apt &> /dev/null; then
  apt-get -y update &> /dev/null
  apt-get -y install sudo &> /dev/null
fi

if command -v pacman &> /dev/null; then
  pacman -Syu --noconfirm &> /dev/null
  pacman -S --noconfirm sudo &> /dev/null
fi

if command -v apk &> /dev/null; then
  apk update &> /dev/null
  apk add sudo &> /dev/null
fi

if command -v rpm &> /dev/null; then
  rpm -U &> /dev/null
  rpm -i sudo &> /dev/null
fi

if command -v xbps &> /dev/null; then
  xbps-install -Su &> /dev/null
  xbps-install sudo &> /dev/null
fi

if command -v emerge &> /dev/null; then
  emaint -a sync &> /dev/null
  emerge-webrsync &> /dev/null
  eix-sync &> /dev/null
  emerge -a sudo &> /dev/null
fi

add_user()
{
  echo "Creating the user account ..."
  read -p "Username: " USERNAME
  useradd -m -s /bin/bash $USERNAME
  while ! passwd $USERNAME; do
    echo Try again.
  done
  printf "$USERNAME ALL=(ALL:ALL) ALL" >> /etc/sudoers
  printf "%s\n" "[user]" "default=$USERNAME" >> /etc/wsl.conf 
}

set_root_passwd()
{
  if ! passwd; then
    set_root_passwd
  fi
}

echo "Configuring the distribution ..."
pwconv
grpconv
chmod 0744 /etc/shadow
chmod 0744 /etc/gshadow
chown -R root:root /bin/su
chmod 755 /bin/su
chmod u+s /bin/su

while true; do
  read -p "Do you want to create a new user with administrator privilages? [y/n]: " yn
  case $yn in
    [Yy]* ) add_user; break;;
    [Nn]* ) set_root_passwd; break;;
  esac
done

