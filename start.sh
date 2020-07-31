#!/bin/bash
echo 'Checking dependencies....'

if ! command -v youtube-dl &> /dev/null
then
    echo "Oh no! Ranka needs youtube-dl for her to work. Please install it using your package manager!"
    exit
fi

if ! (dpkg --status "libopus0" &>/dev/null);
then
    echo "Oh no! Ranka needs libopus0 for her to work. Please install it using your package manager!"
    exit
fi

if ! command -v ffmpeg &> /dev/null
then
    echo "Oh no! Ranka needs ffmpeg for her to work. Please install it using your package manager!"
    exit
fi

echo 'Your dependencies are a-ok! Starting Ranka...'

./Ranka