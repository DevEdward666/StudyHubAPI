#!/usr/bin/env bash
# remove_whitelist.sh
# Place on router (root), make executable: chmod +x /usr/local/bin/remove_whitelist.sh
# Usage: remove_whitelist.sh <MAC>

MAC="$1"

if [ -z "$MAC" ]; then
  echo "usage: $0 <MAC>"
  exit 2
fi

iptables -D FORWARD -m mac --mac-source "$MAC" -j ACCEPT 2>/dev/null && echo "$(date): removed $MAC" >> /var/log/whitelist.log
exit 0

