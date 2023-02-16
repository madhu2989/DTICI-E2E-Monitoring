#!/usr/bin/env bash

if [[ ! -f "$K8S_CONFIG_FILE" ]]; then
  rm -f "$K8S_CONFIG_FILE"
fi