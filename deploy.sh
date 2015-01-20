#!/bin/bash

function die_with() { echo "$*" >&2; exit 1; }

echo "Checking if commit is a pull request"
if [ $TRAVIS_PULL_REQUEST == true ]; then die_with "Skipping deployment for pull request!"; fi

echo "Configuring git credentials"
git config --global user.email "travis@travis-ci.org" && git config --global user.name "Travis" || die_with "Failed to configure git credentials!"

echo "Changing directory to $HOME and configuring git"
cd $HOME || die_with "Failed to change to home directory!"

echo "Cloning snapshots branch using token"
git clone -q --branch=snapshots https://$GITHUB_TOKEN@github.com/$TRAVIS_REPO_SLUG.git snapshots >/dev/null || die_with "Failed to clone existing snapshots branch!"

echo "Copying target files to temp directory"
mkdir $TRAVIS_COMMIT || die_with "Failed to create temp directory!"
cd build/$TRAVIS_REPO_SLUG || die_with "Failed to change to build directory!"
cp -vf RustExperimental.opj OxidePatcher/bin/Release/OxidePatcher.exe $HOME/$TRAVIS_COMMIT || die_with "Failed to copy RustExperimental.opj and OxidePatcher.exe!"

echo "Archiving and compressing target files"
cd $HOME/$TRAVIS_COMMIT || die_with "Failed to change to temp directory!"
zip -vFS9 $HOME/snapshots/OxidePatcher.zip * || die_with "Failed to archive snapshot files!"

echo "Adding, committing, and pushing to snapshots branch"
cd $HOME/snapshots || die_with "Failed to change to snapshots directory!"
git add -f . && git commit -m "Oxide 2 Patcher build $TRAVIS_BUILD_NUMBER" || die_with "Failed to add and commit files with git!"
git push -qf origin snapshots >/dev/null || die_with "Failed to push snapshot to GitHub!"

echo "Deployment cycle completed. Happy developing!"
