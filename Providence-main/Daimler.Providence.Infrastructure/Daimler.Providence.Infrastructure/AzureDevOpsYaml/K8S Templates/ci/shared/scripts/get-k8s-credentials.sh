#!/usr/bin/env bash

export K8S_CONFIG_FILE=${SYSTEM_DEFAULTWORKINGDIRECTORY}/ci/k8s/k8s.config

#if there isn't kubernetes credentials config file, download it.
if [[ ! -f "$K8S_CONFIG_FILE" ]]; then
  echo "get aks credentials"
  az aks get-credentials --name "$VARS_COMPUTEDAKSCLUSTERNAME" --resource-group "$VARS_COMPUTEDAKSRESOURCEGROUPNAME" --file "$K8S_CONFIG_FILE"
fi