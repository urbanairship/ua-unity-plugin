
#!/usr/bin/env bash

set -ex

./gradlew clean

IMAGE_NAME="gableroux/unity3d:$(./gradlew -q getUnityVersion)-android"

export UNITY_CLASSES_JAR=/opt/Unity/Editor/Data/PlaybackEngines/AndroidPlayer/Variations/mono/Development/Classes/classes.jar
export UNITY_EXE=/opt/Unity/Editor/Unity

docker pull $IMAGE_NAME

docker run \
  -e UNITY_USERNAME \
  -e UNITY_PASSWORD \
  -e UNITY_SERIAL \
  -e UNITY_CLASSES_JAR \
  -e UNITY_EXE \
  -w /project/ \
  -v $(pwd):/project/ \
  $IMAGE_NAME \
  /bin/bash -c "./scripts/docker_activate_license.sh && ./gradlew build -x docs:build"

