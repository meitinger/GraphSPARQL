#!/bin/bash -eu

ROOT="$(dirname -- "$0")"
declare -a QUERY_FILES=("${ROOT}/queries_level1.dat" "${ROOT}/queries_level2.dat" "${ROOT}/queries_level3.dat" "${ROOT}/queries_level4.dat")
ROUNDS=5
case "$1" in
  h*)
    NAME='HyperGraphQL'
    EXECUTABLE='java -Dname=${NAME} -jar /home/student/HyperGraphQL/hypergraphql-2.0.0-exe.jar --config /home/student/HyperGraphQL/config.json'
    URI='http://localhost:8080/graphql'
    PREPARE='s/_[A-Z][A-Za-z]*/\0_GET/'
    ;;
  g*)
    EXECUTABLE='/home/student/GraphSPARQL/bin/Release/netcoreapp3.1/GraphSPARQL'
    URI='http://localhost:5000/graphql'
    NAME='GraphSPARQL'
    PREPARE='{}'
    ;;
  s*)
    EXECUTABLE='sleep infinity'
    URI='http://localhost:9300/benchmarking/graphql'
    NAME='stardog'
    PREPARE='s/_[a-z][A-Za-z]*/\0 @optional/g'
    ;;
  *)
    echo "USAGE: $0 h[ypergraphql]|g[raphql]|s[tardog]" >&2
    ;;
esac

QUERIES="${QUERY_FILES[$2]}"
echo "${QUERIES}"
RESULTS_NAME="${QUERIES%.dat}.csv"
RESULTS_NAME="${NAME}_${RESULTS_NAME#${ROOT}/}"
RESULTS="${ROOT}/results/${RESULTS_NAME}"
echo "Initialize ${NAME}..."
WORKDIR="${ROOT}/${NAME}"
WORKQUERIES="${WORKDIR}/queries.graphql"
rm -rf -- "${WORKDIR}"
mkdir -p -- "${WORKDIR}"
sed "${PREPARE}" -- "${QUERIES}" > "${WORKQUERIES}"
${EXECUTABLE} > "${WORKDIR}/stdout.log" 2> "${WORKDIR}/stderr.log" &
trap "kill $!" EXIT
sleep 5
touch "${RESULTS}"
echo -n "$(date '+%Y-%m-%d_%H-%M-%S'), " >> "${RESULTS}"
for ((ROUND=1; ROUND<=${ROUNDS}; ROUND++))
do
  echo -n "Round ${ROUND}: "
  TOTALTIME=0
  TOTALSIZE=0
  QUERYNUMBER=1
  while IFS= read -r QUERY
  do
    RESULT="${WORKDIR}/query${QUERYNUMBER}-round${ROUND}.json"
    RESULTTIME=$(curl --location "$URI" --user admin:admin --request 'POST' --header 'Content-Type: application/json' --data-raw "{\"query\":\"${QUERY}\"}" --output "${RESULT}" --silent --show-error --write-out '%{time_total}-%{time_pretransfer}')
    RESULTSIZE=$(stat -c '%s' -- "${RESULT}")
    TOTALTIME=$(bc <<< "${TOTALTIME}+${RESULTTIME}")
    TOTALSIZE=$((TOTALSIZE+RESULTSIZE))
    QUERYNUMBER=$((QUERYNUMBER+1))
  done < "${WORKQUERIES}"
  echo -n "${TOTALTIME}, " >> "${RESULTS}"
  echo "duration=${TOTALTIME}s size=${TOTALSIZE}b"
done
echo >> "${RESULTS}"
echo "Done."
