#!/usr/bin/env bash
# add_whitelist.sh
# Place on router (root), make executable: chmod +x /usr/local/bin/add_whitelist.sh
# Usage: add_whitelist.sh <MAC> <TTL_SECONDS>

MAC="$1"
TTL="$2"

if [ -z "$MAC" ] || [ -z "$TTL" ]; then
  echo "usage: $0 <MAC> <TTL>"
  exit 2
fi

# Example: add an iptables rule matching source MAC and accept forwarding
iptables -I FORWARD 1 -m mac --mac-source "$MAC" -j ACCEPT
echo "$(date): added $MAC for $TTL seconds" >> /var/log/whitelist.log

# schedule removal
(
  sleep "$TTL"
  iptables -D FORWARD -m mac --mac-source "$MAC" -j ACCEPT 2>/dev/null && echo "$(date): removed $MAC" >> /var/log/whitelist.log
) &

exit 0

