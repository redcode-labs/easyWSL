#!/usr/bin/env sh
echo "Upgrading the system ..."
if command -v apt > /dev/null 2>&1; then
  apt-get -y update > /dev/null 2>&1
  apt-get -y install sudo > /dev/null 2>&1
fi

if command -v pacman > /dev/null 2>&1; then
  pacman -Syu --noconfirm > /dev/null 2>&1
  pacman -S --noconfirm sudo > /dev/null 2>&1
fi

if command -v apk > /dev/null 2>&1; then
  apk update > /dev/null 2>&1
  apk add sudo shadow > /dev/null 2>&1
fi

if command -v rpm > /dev/null 2>&1; then
  rpm -U > /dev/null 2>&1
  rpm -i sudo > /dev/null 2>&1
fi

if command -v xbps > /dev/null 2>&1; then
  xbps-install -Su > /dev/null 2>&1
  xbps-install sudo > /dev/null 2>&1
fi

if command -v emerge > /dev/null 2>&1; then
  emaint -a sync > /dev/null 2>&1
  emerge-webrsync > /dev/null 2>&1
  eix-sync > /dev/null 2>&1
  emerge -a sudo > /dev/null 2>&1
fi

add_user()
{
  echo "Creating the user account ..."
  printf "Username: "
  read -r USERNAME
  if command -v bash > /dev/null 2>&1
  then
    USER_SHELL="/bin/bash"
  else
    USER_SHELL="/bin/sh"
  fi
  useradd -m -s "$USER_SHELL" "$USERNAME"
  while ! passwd "$USERNAME"; do
    echo Try again.
  done
  printf "%s ALL=(ALL:ALL) ALL" "$USERNAME" >> /etc/sudoers
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
  printf "Do you want to create a new user with administrator privileges? [y/n]: "
  read -r answer
  if [ "$answer" != "${answer#[Yy]}" ]; then
    add_user
    break
  else
    set_root_passwd
    break
  fi
done
