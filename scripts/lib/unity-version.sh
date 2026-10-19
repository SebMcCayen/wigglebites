#!/usr/bin/env bash

wigglebites_unity_version_matches() {
  local required_version="$1"
  local reported_version="$2"

  [[ "$reported_version" == "$required_version" || "$reported_version" == "$required_version".* ]]
}
