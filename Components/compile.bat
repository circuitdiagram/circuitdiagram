@ECHO off

Pushd %2

%1 -i "XML" -o "Output" --icondir "Icons"

popd