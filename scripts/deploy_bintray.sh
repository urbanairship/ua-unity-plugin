#!/usr/bin/env bash
set -e
set -x

# Upload to bintray
VERSION=$1

upload() {
  echo -e "Uploading $1 into ${VERSION}"
  curl -T $1 -H "X-Bintray-Package:unity-plugin" -H "X-Bintray-Version:${VERSION}" -H "X-Bintray-Publish:0" -u $BINTRAY_AUTH https://api.bintray.com/content/urbanairship/unity/unity-plugin/${VERSION}/
}

upload "build/urbanairship-${VERSION}.unitypackage"

# Publish
curl -X POST -u $BINTRAY_AUTH https://api.bintray.com/content/urbanairship/unity/unity-plugin/${VERSION}/publish

# Needed or show in download list fails because bintray thinks the release is not published yet
sleep 30

# Show in download list
curl -X PUT -H "Content-Type: application/json" -u $BINTRAY_AUTH --data '{"list_in_downloads":true}' https://api.bintray.com/file_metadata/urbanairship/unity/unity-plugin/${VERSION}/urbanairship-${VERSION}.unitypackage