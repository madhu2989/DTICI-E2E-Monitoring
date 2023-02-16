#!/usr/bin/env bash
echo -e "Inside k8s Deploy"

namespace=${VARS_AKSNAMESPACENAMEFLINK}-${GETCONFIGIDLEJOB_COMPUTEDIDLEENVIRONMENTIDENTIFIER}
exitCode=0

# shellcheck disable=SC1090
. "${SYSTEM_DEFAULTWORKINGDIRECTORY}"/ci/shared/scripts/get-k8s-credentials.sh

function evaluate() {
  if [[ $result == *"Error"* ]];
  then
      echo -e "\nError during kubectl execution:\n"
      echo "$result" 1>&2 #send the error content to stderr
      exitCode=1
  else
    echo "$result"
  fi
}

echo -e "AKS Deploy App"
#result="$(kubectl --kubeconfig="$K8S_CONFIG_FILE" apply -f 20-rbac/3-signal-mapping-service-account.yaml -n "$namespace" 2>&1)"
#evaluate "$result"
#result="$(kubectl --kubeconfig="$K8S_CONFIG_FILE" apply -f 20-rbac/4-signal-mapping-role.yaml -n "$namespace" 2>&1)"
#evaluate "$result"
#result="$(kubectl --kubeconfig="$K8S_CONFIG_FILE" apply -f 20-rbac/5-signal-mapping-role-binding.yaml -n "$namespace" 2>&1)"
#evaluate "$result"
#result="$(kubectl --kubeconfig="$K8S_CONFIG_FILE" apply -f 30-services/6-signal-mapping-service.yaml -n "$namespace" 2>&1)"
#evaluate "$result"
#result="$(kubectl --kubeconfig="$K8S_CONFIG_FILE" apply -f 40-deployments/7-signal-mapping-service-deployment.yaml -n "$namespace" 2>&1)"
#evaluate "$result"

# shellcheck disable=SC1090
. "${SYSTEM_DEFAULTWORKINGDIRECTORY}"/ci/shared/scripts/remove-k8s-credentials.sh

sleep 1m

exit $exitCode