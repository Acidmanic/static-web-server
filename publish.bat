


rmdir publish /s /q
set RUNTIME="win10-x64"
dotnet publish --output publish\"%RUNTIME%" --self-contained true --runtime "%RUNTIME%" --framework netcoreapp3.1 -p:PublishReadyToRun=true -p:PublishSingleFile=false \
&& cd publish\"%RUNTIME%" && 7z a -tzip ..\"%RUNTIME%".zip . 
cd .. \
&& rmdir publish\"%RUNTIME%" /s /q  

