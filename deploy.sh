#!/bin/bash

function die_with() { echo "$*" >&2; exit 1; }

echo "Checking if commit is a pull request"
if [ $TRAVIS_PULL_REQUEST == true ]; then die_with "Skipping deployment for pull request!"; fi

echo "Configuring git credentials"
git config --global user.email "travis@travis-ci.org" && git config --global user.name "Travis" || die_with "Failed to configure git credentials!"

echo "Cloning snapshots branch using token"
git clone -q https://$GITHUB_TOKEN@github.com/OxideMod/Snapshots.git $HOME/snapshots >/dev/null || die_with "Failed to clone existing snapshots branch!"

cd $HOME/build/$TRAVIS_REPO_SLUG || die_with "Failed to change to build directory!"

echo "Copying target files to snapshots directory"
cp -f OxidePatcher/bin/Release/OxidePatcher.exe $HOME/snapshots || die_with "Failed to copy OxidePatcher.exe to snapshots!"

echo "Adding, committing, and pushing to snapshots branch"
cd $HOME/snapshots || die_with "Failed to change to snapshots directory!"
git add . && git commit -m "Patcher build $TRAVIS_BUILD_NUMBER from https://github.com/$TRAVIS_REPO_SLUG/commit/${TRAVIS_COMMIT:0:7}" || die_with "Failed to add and commit files with git!"
git push -q origin master >/dev/null || die_with "Failed to push snapshot to GitHub!"

echo "Deployment cycle completed. Happy developing!"
