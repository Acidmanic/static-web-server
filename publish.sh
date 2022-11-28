


rm -rf publish


publish_for_runtime(){
  
  RUNTIME=$1
  
  mkdir -p "./publish/$RUNTIME"
  
  dotnet publish --output publish/"$RUNTIME" --self-contained true --runtime "$RUNTIME" --framework netcoreapp3.1 -p:PublishReadyToRun=true -p:PublishSingleFile=false && (cd publish/"$RUNTIME" && zip -r ../"$RUNTIME".zip ./) && rm -rf publish/"$RUNTIME"  
  
}


publish_for_runtime "debian.8-x64"
publish_for_runtime "linux-arm"
