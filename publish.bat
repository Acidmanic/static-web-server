


rmdir publish /s /q
set RUNTIME=win10-x64
set OUTPUT=publish\%RUNTIME%


dotnet publish --output %OUTPUT% --self-contained true --runtime %RUNTIME% --framework netcoreapp3.1 -p:PublishReadyToRun=true -p:PublishSingleFile=false
cd %OUTPUT% && 7z a -tzip ..\%RUNTIME%.zip . 
cd ..
cd ..
rmdir %OUTPUT% /s /q  

