ver=1.0.0-stable
docker build -t scholtz2/address2map:1.0.0-stable -f Dockerfile --progress=plain ..
docker push scholtz2/address2map:1.0.0-stable