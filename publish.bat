docker-compose -f docker-compose-local.yml --env-file .\config\FaiqHassan\env -p "faiqhassan" up -d --build
docker image tag faiqhassan-autosearch hungnguyen991995/autosearch
docker image push hungnguyen991995/autosearch

docker compose down -v
set /p DUMMY=Hit ENTER to continue...